using System.Text.RegularExpressions;
using Shared.Abstractions.Privacy.Pseudonymization;

namespace Shared.Infrastructure.Privacy.Pseudonymization.Leak;

/// <summary>
/// Independent second-pass leak checker (fail-closed for CloudEgress).
/// Diversifies signals: PT regex + session original plaintext presence + skill allowlist.
/// </summary>
public sealed class IndependentLeakChecker : ILeakChecker
{
    private static readonly Regex EmailRx = new(
        @"\b[A-Z0-9._%+\-]+@[A-Z0-9.\-]+\.[A-Z]{2,}\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly Regex PhoneRx = new(
        @"(?:\+351\s?)?(?:9\d{2}[\s\-]?\d{3}[\s\-]?\d{3})",
        RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly Regex NifRx = new(
        @"\b[123568]\d{8}\b",
        RegexOptions.CultureInvariant | RegexOptions.Compiled);

    public Task<LeakCheckResult> CheckAsync(
        string candidateEgressText,
        PseudonymizationPolicy policy,
        IReadOnlyList<TokenMapEntry> sessionMap,
        Func<TokenMapEntry, string> decryptOriginal,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var findings = new List<LeakFinding>();

        foreach (Match m in EmailRx.Matches(candidateEgressText))
            findings.Add(new LeakFinding("regex.email", m.Value, m.Index, m.Index + m.Length));

        foreach (Match m in PhoneRx.Matches(candidateEgressText))
            findings.Add(new LeakFinding("regex.phone_pt", m.Value, m.Index, m.Index + m.Length));

        foreach (Match m in NifRx.Matches(candidateEgressText))
            findings.Add(new LeakFinding("regex.nif_pt", m.Value, m.Index, m.Index + m.Length));

        foreach (var entry in sessionMap)
        {
            string original;
            try
            {
                original = decryptOriginal(entry);
            }
            catch
            {
                findings.Add(new LeakFinding("map.decrypt", $"Failed decrypt for token {entry.Token}"));
                continue;
            }

            if (string.IsNullOrWhiteSpace(original) || original.Length < 3)
                continue;

            if (policy.PreserveProfessionalSkills
                && ProfessionalSkillAllowlist.IsSkill(original, policy.ExtraAllowlist))
                continue;

            if (candidateEgressText.Contains(original, StringComparison.OrdinalIgnoreCase))
            {
                findings.Add(new LeakFinding(
                    "session.plaintext_residual",
                    $"Mapped value for {entry.Token} still present in egress text"));
            }
        }

        // Filter allowlisted skill false-positives that look like emails etc. — skills never match email/phone.
        if (findings.Count == 0)
            return Task.FromResult(LeakCheckResult.Ok());

        return Task.FromResult(LeakCheckResult.Fail(findings.ToArray()));
    }
}
