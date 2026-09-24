using System.Text.RegularExpressions;
using Shared.Abstractions.Privacy.Pseudonymization;
using Shared.Infrastructure.Privacy.Pseudonymization.Detection;

namespace Shared.Infrastructure.Privacy.Pseudonymization.Leak;

/// <summary>
/// Independent second-pass leak checker (fail-closed for CloudEgress).
/// Diversifies signals: PT/MZ contact regex + session original plaintext presence + skill allowlist.
/// </summary>
public sealed class IndependentLeakChecker : ILeakChecker
{
    private static readonly Regex EmailRx = ContactSignals.Email;

    private static readonly Regex PhoneRx = ContactSignals.Phone;

    private static readonly Regex NifRx = ContactSignals.Nif;

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
            findings.Add(new LeakFinding("regex.phone", m.Value, m.Index, m.Index + m.Length));

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

            if (string.IsNullOrWhiteSpace(original) || original.Length < ContactSignals.MinPlaintextLength)
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
