using System.Text.RegularExpressions;
using Shared.Abstractions.Privacy.Pseudonymization;

namespace Shared.Infrastructure.Privacy.Pseudonymization.Detection;

/// <summary>
/// v1 local ensemble: regex + dictionary + context rules (no GLiNER/spaCy).
/// </summary>
public sealed class RegexDictContextEntityDetector : IEntityDetector
{
    private static readonly (string Type, Regex Rx, string Source)[] Patterns =
    {
        (EntityTypes.EmailAddress,
            new Regex(@"\b[A-Z0-9._%+\-]+@[A-Z0-9.\-]+\.[A-Z]{2,}\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled),
            "regex.email"),
        (EntityTypes.PhoneNumber,
            new Regex(@"(?:\+351\s?)?(?:9\d{2}[\s\-]?\d{3}[\s\-]?\d{3}|\d{3}[\s\-]?\d{3}[\s\-]?\d{3})", RegexOptions.CultureInvariant | RegexOptions.Compiled),
            "regex.phone_pt"),
        (EntityTypes.Nif,
            new Regex(@"\b[123568]\d{8}\b", RegexOptions.CultureInvariant | RegexOptions.Compiled),
            "regex.nif_pt"),
        (EntityTypes.Niss,
            new Regex(@"\b\d{11}\b", RegexOptions.CultureInvariant | RegexOptions.Compiled),
            "regex.niss_pt"),
        (EntityTypes.IbanCode,
            new Regex(@"\b[A-Z]{2}\d{2}[A-Z0-9]{10,30}\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled),
            "regex.iban"),
    };

    private static readonly (string Type, Regex Rx, string Source)[] ContextPatterns =
    {
        (EntityTypes.Project,
            new Regex(@"(?i)\b(?:projecto|projeto|project)\s+(?<name>[A-Z][\w\-]{2,40})", RegexOptions.CultureInvariant | RegexOptions.Compiled),
            "context.project"),
        (EntityTypes.Client,
            new Regex(@"(?i)\bcliente\s+(?<name>[A-Z][\w\-]{2,40})", RegexOptions.CultureInvariant | RegexOptions.Compiled),
            "context.client"),
        (EntityTypes.InternalSystem,
            new Regex(@"(?i)\b(?:sistema|erp|plataforma)\s+(?<name>[A-Z][\w\-]{2,40})", RegexOptions.CultureInvariant | RegexOptions.Compiled),
            "context.system"),
        (EntityTypes.Employer,
            new Regex(@"(?i)\b(?:empresa|empregador|employer)\s+(?<name>[A-Z][\w\-]{2,40})", RegexOptions.CultureInvariant | RegexOptions.Compiled),
            "context.employer"),
    };

    private readonly IReadOnlyList<string> _denyDict;

    public RegexDictContextEntityDetector(IEnumerable<string>? denyDictionary = null)
    {
        _denyDict = (denyDictionary ?? DefaultDenyDictionary()).ToList();
    }

    public Task<IReadOnlyList<RawEntitySpan>> DetectAsync(
        string text,
        PseudonymizationPolicy policy,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var spans = new List<RawEntitySpan>();

        foreach (var (type, rx, source) in Patterns)
        {
            foreach (Match m in rx.Matches(text))
            {
                if (!m.Success)
                    continue;
                if (ShouldSuppress(m.Value, policy))
                    continue;
                spans.Add(new RawEntitySpan(type, m.Value, m.Index, m.Index + m.Length, 0.9f, source));
            }
        }

        foreach (var (type, rx, source) in ContextPatterns)
        {
            foreach (Match m in rx.Matches(text))
            {
                if (!m.Success)
                    continue;
                var name = m.Groups["name"];
                if (!name.Success || ShouldSuppress(name.Value, policy))
                    continue;
                spans.Add(new RawEntitySpan(type, name.Value, name.Index, name.Index + name.Length, 0.8f, source));
            }
        }

        var deny = MergeDeny(policy);
        foreach (var term in deny)
        {
            if (string.IsNullOrWhiteSpace(term))
                continue;
            if (policy.PreserveProfessionalSkills && ProfessionalSkillAllowlist.IsSkill(term, policy.ExtraAllowlist))
                continue;

            var idx = 0;
            while (idx < text.Length)
            {
                var found = text.IndexOf(term, idx, StringComparison.OrdinalIgnoreCase);
                if (found < 0)
                    break;
                spans.Add(new RawEntitySpan(
                    EntityTypes.Organization,
                    text.Substring(found, term.Length),
                    found,
                    found + term.Length,
                    0.95f,
                    "dict.deny"));
                idx = found + term.Length;
            }
        }

        IReadOnlyList<RawEntitySpan> result = spans
            .OrderBy(s => s.Start)
            .ThenByDescending(s => s.End - s.Start)
            .ToList();
        return Task.FromResult(result);
    }

    private bool ShouldSuppress(string value, PseudonymizationPolicy policy) =>
        policy.PreserveProfessionalSkills
        && ProfessionalSkillAllowlist.IsSkill(value, policy.ExtraAllowlist);

    private IEnumerable<string> MergeDeny(PseudonymizationPolicy policy)
    {
        if (policy.ExtraDenyDict is null || policy.ExtraDenyDict.Count == 0)
            return _denyDict;
        return _denyDict.Concat(policy.ExtraDenyDict);
    }

    /// <summary>Synthetic / placeholder org names only — never real client data in repo.</summary>
    private static IEnumerable<string> DefaultDenyDictionary() =>
        new[]
        {
            "Acme Corp",
            "ExampleEmployer SA",
            "FakeClient Lda",
            "LabInternalERP"
        };
}
