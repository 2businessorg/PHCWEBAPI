using System.Text.RegularExpressions;

namespace Treasury.Application.Matching;

/// <summary>
/// Extracts match hints from PHC/BCI description noise (TRF :, FT, LDA, pipe refs).
/// </summary>
public static partial class ReconciliationTextHints
{
    [GeneratedRegex(@"FT\s*\d+\s*/\s*\d{2,4}", RegexOptions.IgnoreCase)]
    private static partial Regex InvoiceRegex();

    /// <summary>Significant name tokens from BCI/PHC description noise.</summary>
    public static string? Party(string? description, string? document, string? cheque)
    {
        var tokens = ReconciliationMatcher.Tokenize(
            description ?? string.Empty,
            document ?? string.Empty,
            cheque ?? string.Empty);

        var names = tokens
            .Where(t => t.Any(char.IsLetter) && t.Length >= 4)
            .OrderByDescending(t => t.Length)
            .Take(3)
            .ToList();

        return names.Count == 0 ? null : string.Join(' ', names);
    }

    /// <summary>Invoice token such as FT3/2023 when present.</summary>
    public static string? Invoice(string? description, string? document)
    {
        var match = InvoiceRegex().Match($"{description} {document}");
        return match.Success
            ? match.Value.Replace(" ", string.Empty, StringComparison.Ordinal).ToUpperInvariant()
            : null;
    }

    /// <summary>True when party tokens overlap (PROMAR vs PROMAR | 271).</summary>
    public static bool PartiesOverlap(string? left, string? right)
    {
        var leftTokens = SplitParty(left);
        var rightTokens = SplitParty(right);
        if (leftTokens.Count == 0 || rightTokens.Count == 0)
            return false;

        return leftTokens.Any(l => rightTokens.Any(r => l.Contains(r, StringComparison.Ordinal) || r.Contains(l, StringComparison.Ordinal)));
    }

    private static List<string> SplitParty(string? party)
    {
        if (string.IsNullOrWhiteSpace(party))
            return [];

        return party
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(t => t.Length >= 4)
            .Select(t => t.ToUpperInvariant())
            .ToList();
    }
}
