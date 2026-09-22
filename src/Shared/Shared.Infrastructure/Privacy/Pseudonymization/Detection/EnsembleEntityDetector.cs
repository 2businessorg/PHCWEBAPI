using Shared.Abstractions.Privacy.Pseudonymization;

namespace Shared.Infrastructure.Privacy.Pseudonymization.Detection;

/// <summary>
/// Merges Presidio sidecar spans (when available) with local regex/dict/context.
/// v2 GLiNER/spaCy stubbed via options.EnableHybridNerV2 (no-op in v1).
/// </summary>
public sealed class EnsembleEntityDetector : IEntityDetector
{
    private readonly IEntityDetector _local;
    private readonly IPresidioAnalyzerClient _presidio;
    private readonly bool _tryPresidio;

    public EnsembleEntityDetector(
        RegexDictContextEntityDetector local,
        IPresidioAnalyzerClient presidio,
        bool tryPresidio = true)
    {
        _local = local;
        _presidio = presidio;
        _tryPresidio = tryPresidio;
    }

    public async Task<IReadOnlyList<RawEntitySpan>> DetectAsync(
        string text,
        PseudonymizationPolicy policy,
        CancellationToken cancellationToken = default)
    {
        var local = await _local.DetectAsync(text, policy, cancellationToken);

        if (!_tryPresidio)
            return local;

        try
        {
            var remote = await _presidio.AnalyzeAsync(text, "pt", cancellationToken);
            return Merge(local, remote, policy);
        }
        catch
        {
            // Caller (CloudEgress) decides fail-closed when sidecar required; local still useful for OnPrem.
            return local;
        }
    }

    private static IReadOnlyList<RawEntitySpan> Merge(
        IReadOnlyList<RawEntitySpan> local,
        IReadOnlyList<RawEntitySpan> remote,
        PseudonymizationPolicy policy)
    {
        var merged = new List<RawEntitySpan>(local.Count + remote.Count);
        merged.AddRange(local);
        foreach (var r in remote)
        {
            if (policy.PreserveProfessionalSkills
                && ProfessionalSkillAllowlist.IsSkill(r.Text, policy.ExtraAllowlist))
                continue;
            merged.Add(r);
        }
        return merged
            .OrderBy(s => s.Start)
            .ThenByDescending(s => s.End - s.Start)
            .ToList();
    }
}
