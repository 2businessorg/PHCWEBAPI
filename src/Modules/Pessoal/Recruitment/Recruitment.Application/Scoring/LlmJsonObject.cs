using System.Text;
using System.Text.Json;

namespace Recruitment.Application.Scoring;

/// <summary>
/// Pulls the first JSON object out of a chat completion.
/// Fences, leading prose, and a later brace do not widen the slice.
/// </summary>
public static class LlmJsonObject
{
    public static bool TryExtract(string? content, out string json)
    {
        json = string.Empty;
        if (string.IsNullOrWhiteSpace(content))
            return false;

        var text = content.Trim().TrimStart('\uFEFF');
        for (var i = 0; i < text.Length; i++)
        {
            if (text[i] != '{')
                continue;
            if (!TrySliceBalanced(text, i, out var slice))
                continue;
            if (TryParseObject(slice, out json))
                return true;
            if (TryParseObject(StripTrailingCommas(slice), out json))
                return true;
        }

        return false;
    }

    private static bool TryParseObject(string slice, out string json)
    {
        json = string.Empty;
        var folded = FoldWhitespaceOutsideStrings(slice);
        try
        {
            using var doc = JsonDocument.Parse(folded);
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
                return false;
            json = folded;
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static bool TrySliceBalanced(string text, int start, out string slice)
    {
        slice = string.Empty;
        var depth = 0;
        var inString = false;
        var escape = false;
        for (var i = start; i < text.Length; i++)
        {
            var c = text[i];
            if (inString)
            {
                if (escape)
                {
                    escape = false;
                    continue;
                }

                if (c == '\\')
                {
                    escape = true;
                    continue;
                }

                if (c == '"')
                    inString = false;
                continue;
            }

            if (c == '"')
            {
                inString = true;
                continue;
            }

            if (c == '{')
                depth++;
            else if (c == '}')
            {
                depth--;
                if (depth == 0)
                {
                    slice = text[start..(i + 1)];
                    return true;
                }
            }
        }

        return false;
    }

    private static string FoldWhitespaceOutsideStrings(string json)
    {
        var sb = new StringBuilder(json.Length);
        var inString = false;
        var escape = false;
        foreach (var c in json)
        {
            if (inString)
            {
                sb.Append(c);
                if (escape)
                {
                    escape = false;
                    continue;
                }

                if (c == '\\')
                {
                    escape = true;
                    continue;
                }

                if (c == '"')
                    inString = false;
                continue;
            }

            if (c == '"')
            {
                inString = true;
                sb.Append(c);
                continue;
            }

            if (c is '\u00A0' or '\u2000' or '\u2001' or '\u2002' or '\u2003' or '\u2009' or '\u202F' or '\uFEFF')
                sb.Append(' ');
            else
                sb.Append(c);
        }

        return sb.ToString();
    }

    private static string StripTrailingCommas(string json)
    {
        var sb = new StringBuilder(json.Length);
        var inString = false;
        var escape = false;
        for (var i = 0; i < json.Length; i++)
        {
            var c = json[i];
            if (inString)
            {
                sb.Append(c);
                if (escape)
                {
                    escape = false;
                    continue;
                }

                if (c == '\\')
                {
                    escape = true;
                    continue;
                }

                if (c == '"')
                    inString = false;
                continue;
            }

            if (c == '"')
            {
                inString = true;
                sb.Append(c);
                continue;
            }

            if (c == ',')
            {
                var j = i + 1;
                while (j < json.Length && char.IsWhiteSpace(json[j]))
                    j++;
                if (j < json.Length && json[j] is '}' or ']')
                    continue;
            }

            sb.Append(c);
        }

        return sb.ToString();
    }
}
