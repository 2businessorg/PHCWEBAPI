using System.Text.RegularExpressions;

namespace Recruitment.Application.Scoring;

/// <summary>
/// Deterministic product-name casing for human-facing pt-MZ prose.
/// Does not rewrite sentences or invent facts.
/// </summary>
public static class PtMzProse
{
    private static readonly (string Pattern, string Canonical)[] Products =
    [
        (@"sql\s+server", "SQL Server"),
        (@"\.net", ".NET"),
        (@"primavera", "Primavera"),
        (@"\bsap\b", "SAP"),
        (@"\bphc\b", "PHC"),
        (@"\bsql\b", "SQL")
    ];

    public static string Apply(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return text ?? string.Empty;

        var current = text;
        foreach (var (pattern, canonical) in Products)
            current = Regex.Replace(current, pattern, canonical, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        return current;
    }
}
