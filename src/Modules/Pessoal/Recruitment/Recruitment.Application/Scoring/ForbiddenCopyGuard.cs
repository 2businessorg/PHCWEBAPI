using System.Globalization;
using System.Text;
using Recruitment.Domain.Constants;

namespace Recruitment.Application.Scoring;

/// <summary>
/// Guards all outbound copy against AH-04 forbidden decision phrases.
/// Accent-insensitive normalize OR both forms; catches pela IA / by AI.
/// </summary>
public static class ForbiddenCopyGuard
{
    public static bool ContainsForbiddenPhrase(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;

        var normalized = Normalize(text);
        foreach (var phrase in HitlCopy.ForbiddenPhrases)
        {
            if (normalized.Contains(Normalize(phrase), StringComparison.Ordinal))
                return true;
        }

        return false;
    }

    public static void ThrowIfForbidden(string? text, string context)
    {
        if (ContainsForbiddenPhrase(text))
            throw new InvalidOperationException($"AH-04: texto proibido em {context}.");
    }

    /// <summary>Lowercase + strip combining marks (AH-04 Unicode/diacritics).</summary>
    public static string Normalize(string text)
    {
        var lower = text.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(lower.Length);
        foreach (var ch in lower)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                sb.Append(ch);
        }

        return sb.ToString().Normalize(NormalizationForm.FormC);
    }
}
