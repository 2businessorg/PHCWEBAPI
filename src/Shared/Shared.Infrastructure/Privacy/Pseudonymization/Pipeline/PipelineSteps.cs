using System.Text.RegularExpressions;
using Shared.Abstractions.Privacy.Pseudonymization;
using Shared.Infrastructure.Privacy.Pseudonymization.Detection;

namespace Shared.Infrastructure.Privacy.Pseudonymization.Pipeline;

public sealed class NormalizeTextStep : IPseudonymizationStep
{
    public string Name => "Normalize";

    public Task ExecuteAsync(PseudonymizationPipelineContext context, CancellationToken cancellationToken)
    {
        context.NormalizedText = TextNormalizer.NormalizeDocument(context.Request.PlainText);
        return Task.CompletedTask;
    }
}

public sealed class DetectEntitiesStep : IPseudonymizationStep
{
    private readonly IEntityDetector _detector;

    public DetectEntitiesStep(IEntityDetector detector) => _detector = detector;

    public string Name => "Detect";

    public async Task ExecuteAsync(PseudonymizationPipelineContext context, CancellationToken cancellationToken)
    {
        var spans = await _detector.DetectAsync(
            context.NormalizedText,
            context.Request.Policy,
            cancellationToken);
        context.RawSpans.AddRange(spans);
    }
}

public sealed class ClassifyAndScoreStep : IPseudonymizationStep
{
    public string Name => "Classify";

    public Task ExecuteAsync(PseudonymizationPipelineContext context, CancellationToken cancellationToken)
    {
        var policy = context.Request.Policy;
        foreach (var span in context.RawSpans)
        {
            if (policy.PreserveProfessionalSkills
                && ProfessionalSkillAllowlist.IsSkill(span.Text, policy.ExtraAllowlist))
                continue;

            if (!policy.PseudonymizeEducation
                && EntityTypes.EducationTypes.Contains(span.EntityType))
                continue;

            var score = span.Score;
            if (!policy.ConservativeOnLowConfidence && score < 0.55f)
                continue;

            context.Scored.Add(new ScoredEntity(
                span.EntityType,
                span.Text,
                TextNormalizer.NormalizeEntityValue(span.Text),
                span.Start,
                span.End,
                score,
                span.SourceRecognizer));
        }

        return Task.CompletedTask;
    }
}

public sealed class ResolveEntitiesStep : IPseudonymizationStep
{
    private readonly IEntityResolver _resolver;

    public ResolveEntitiesStep(IEntityResolver resolver) => _resolver = resolver;

    public string Name => "Resolve";

    public Task ExecuteAsync(PseudonymizationPipelineContext context, CancellationToken cancellationToken)
    {
        context.Resolved.AddRange(_resolver.Resolve(context.Scored));
        return Task.CompletedTask;
    }
}

public sealed class TokenizeAndPersistStep : IPseudonymizationStep
{
    private readonly ITokenMapStore _store;
    private readonly ITokenValueProtector _protector;

    public TokenizeAndPersistStep(ITokenMapStore store, ITokenValueProtector protector)
    {
        _store = store;
        _protector = protector;
    }

    public string Name => "TokenizePersist";

    public async Task ExecuteAsync(PseudonymizationPipelineContext context, CancellationToken cancellationToken)
    {
        var req = context.Request;
        var ttl = req.Policy.EffectiveMapTtl;
        var now = DateTimeOffset.UtcNow;
        var expires = now.Add(ttl);

        // Build replacements from end to start to keep offsets valid.
        var replacements = new List<(int Start, int End, string Token)>();

        foreach (var entity in context.Resolved)
        {
            var shortId = TokenGrammar.IssueShortId(req.SessionId, entity.EntityType, entity.NormalizedValue);
            var token = TokenGrammar.Format(entity.EntityType, shortId);
            var cipher = _protector.Protect(entity.CanonicalValue);
            var normCipher = _protector.Protect(entity.NormalizedValue);
            var hmac = _protector.ComputeHmac(entity.NormalizedValue);

            var entry = new TokenMapEntry(
                req.SessionId,
                req.DocumentId,
                token,
                entity.EntityType,
                cipher,
                normCipher,
                hmac,
                entity.Confidence,
                entity.SourceRecognizer,
                now,
                expires);

            await _store.UpsertAsync(entry, cancellationToken);
            context.MapEntries.Add(entry);
            context.Summaries.Add(new DetectedEntitySummary(
                entity.EntityType,
                token,
                entity.Confidence,
                entity.SourceRecognizer,
                entity.Mentions[0].Start,
                entity.Mentions[0].End));

            foreach (var mention in entity.Mentions)
                replacements.Add((mention.Start, mention.End, token));
        }

        var redacted = ApplyReplacements(context.NormalizedText, replacements);
        context.PseudonymizedText = ScrubResidualPlaintext(redacted, context.MapEntries, _protector, req.Policy);
    }

    /// <summary>
    /// LeakCheck compares every mapped original of length &gt;= 3 and the contact regexes.
    /// Span replacement misses OCR duplicates and spacing variants, so scrub them here.
    /// </summary>
    private static string ScrubResidualPlaintext(
        string text,
        IReadOnlyList<TokenMapEntry> entries,
        ITokenValueProtector protector,
        PseudonymizationPolicy policy)
    {
        var scrubbed = text;
        foreach (var entry in entries)
        {
            string original;
            string normalized;
            try
            {
                original = protector.Unprotect(entry.OriginalValueCipher);
                normalized = entry.NormalizedValueCipher is null
                    ? original
                    : protector.Unprotect(entry.NormalizedValueCipher);
            }
            catch
            {
                continue;
            }

            scrubbed = RemoveMappedValue(scrubbed, original, policy);
            if (!string.Equals(original, normalized, StringComparison.OrdinalIgnoreCase))
                scrubbed = RemoveMappedValue(scrubbed, normalized, policy);
        }

        scrubbed = ContactSignals.Email.Replace(scrubbed, "{{CONTACT}}");
        scrubbed = ContactSignals.Phone.Replace(scrubbed, "{{CONTACT}}");
        scrubbed = ContactSignals.Nif.Replace(scrubbed, "{{CONTACT}}");
        return scrubbed;
    }

    private static string RemoveMappedValue(string text, string value, PseudonymizationPolicy policy)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length < ContactSignals.MinPlaintextLength)
            return text;
        if (policy.PreserveProfessionalSkills
            && ProfessionalSkillAllowlist.IsSkill(value, policy.ExtraAllowlist))
            return text;

        var flexible = FlexibleWhitespacePattern(value);
        return Regex.Replace(text, flexible, "{{CONTACT}}", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    }

    private static string FlexibleWhitespacePattern(string value)
    {
        var parts = value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
            return Regex.Escape(value);
        return string.Join(@"\s+", parts.Select(Regex.Escape));
    }

    private static string ApplyReplacements(string text, List<(int Start, int End, string Token)> replacements)
    {
        // Earliest start, then longest. A later span that still extends past the
        // accepted cursor is redacted from the cursor, so a short overlap cannot
        // leave a contact tail for the leak checker.
        var ordered = replacements
            .OrderBy(r => r.Start)
            .ThenByDescending(r => r.End - r.Start)
            .ToList();

        var accepted = new List<(int Start, int End, string Token)>();
        var cursor = 0;
        foreach (var r in ordered)
        {
            if (r.End <= cursor)
                continue;
            var start = Math.Max(r.Start, cursor);
            accepted.Add((start, r.End, r.Token));
            cursor = r.End;
        }

        var sb = new System.Text.StringBuilder(text.Length);
        var last = 0;
        foreach (var r in accepted.OrderBy(a => a.Start))
        {
            sb.Append(text, last, r.Start - last);
            sb.Append(r.Token);
            last = r.End;
        }
        sb.Append(text, last, text.Length - last);
        return sb.ToString();
    }
}

public sealed class LeakCheckStep : IPseudonymizationStep
{
    private readonly ILeakChecker _leakChecker;
    private readonly ITokenValueProtector _protector;

    public LeakCheckStep(ILeakChecker leakChecker, ITokenValueProtector protector)
    {
        _leakChecker = leakChecker;
        _protector = protector;
    }

    public string Name => "LeakCheck";

    public async Task ExecuteAsync(PseudonymizationPipelineContext context, CancellationToken cancellationToken)
    {
        var result = await _leakChecker.CheckAsync(
            context.PseudonymizedText,
            context.Request.Policy,
            context.MapEntries,
            e => _protector.Unprotect(e.OriginalValueCipher),
            cancellationToken);

        context.LeakCheck = result;
        context.EgressAllowed = result.Passed;

        if (!result.Passed)
            context.FailureReason = "Leak checker failed (fail-closed)";
    }
}
