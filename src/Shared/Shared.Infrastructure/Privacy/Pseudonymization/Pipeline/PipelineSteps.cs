using Shared.Abstractions.Privacy.Pseudonymization;

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

        context.PseudonymizedText = ApplyReplacements(context.NormalizedText, replacements);
    }

    private static string ApplyReplacements(string text, List<(int Start, int End, string Token)> replacements)
    {
        // Drop overlapping mentions (keep longest / first by start then length).
        var ordered = replacements
            .OrderBy(r => r.Start)
            .ThenByDescending(r => r.End - r.Start)
            .ToList();

        var accepted = new List<(int Start, int End, string Token)>();
        var cursor = 0;
        foreach (var r in ordered)
        {
            if (r.Start < cursor)
                continue;
            accepted.Add(r);
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
