using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Shared.Infrastructure.Privacy.Pseudonymization;

/// <summary>
/// ASCII token grammar {{TYPE_HEX}} - Windows-1252 safe; Unicode delimiters deferred.
/// </summary>
public static partial class TokenGrammar
{
    public const string Pattern = @"\{\{([A-Z][A-Z0-9_]*)_([a-f0-9]{4,16})\}\}";

    [GeneratedRegex(Pattern, RegexOptions.CultureInvariant)]
    public static partial Regex TokenRegex { get; }

    public static string Format(string entityType, string shortId) =>
        $"{{{{{entityType.ToUpperInvariant()}_{shortId.ToLowerInvariant()}}}}}";

    public static string IssueShortId(string sessionId, string entityType, string normalizedValue)
    {
        var material = Encoding.UTF8.GetBytes($"{sessionId}|{entityType}|{normalizedValue}");
        var hash = SHA256.HashData(material);
        return Convert.ToHexString(hash).ToLowerInvariant()[..8];
    }

    public static bool TryParse(string token, out string entityType, out string shortId)
    {
        entityType = string.Empty;
        shortId = string.Empty;
        var m = TokenRegex.Match(token);
        if (!m.Success)
            return false;
        entityType = m.Groups[1].Value;
        shortId = m.Groups[2].Value;
        return true;
    }
}

public static partial class TextNormalizer
{
    public static string NormalizeDocument(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;
        var nfc = text.Normalize(NormalizationForm.FormC);
        return WhitespaceRegex().Replace(nfc, " ").Trim();
    }

    public static string NormalizeEntityValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;
        var form = value.Trim().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(form.Length);
        foreach (var c in form)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(char.ToLowerInvariant(c));
        }
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    [GeneratedRegex(@"\s+", RegexOptions.CultureInvariant)]
    private static partial Regex WhitespaceRegex();
}