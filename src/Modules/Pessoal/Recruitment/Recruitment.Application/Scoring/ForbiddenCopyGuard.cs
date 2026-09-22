using Recruitment.Domain.Constants;

namespace Recruitment.Application.Scoring;

/// <summary>
/// Guards all outbound copy against AH-04 forbidden decision phrases.
/// </summary>
public static class ForbiddenCopyGuard
{
    public static bool ContainsForbiddenPhrase(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;

        var normalized = text.Trim().ToLowerInvariant();
        foreach (var phrase in HitlCopy.ForbiddenPhrases)
        {
            if (normalized.Contains(phrase, StringComparison.Ordinal))
                return true;
        }

        return false;
    }

    public static void ThrowIfForbidden(string? text, string context)
    {
        if (ContainsForbiddenPhrase(text))
            throw new InvalidOperationException($"AH-04: texto proibido em {context}.");
    }
}
