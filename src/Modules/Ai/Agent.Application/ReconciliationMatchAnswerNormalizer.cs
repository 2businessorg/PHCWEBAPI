using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Agent.Application;

/// <summary>
/// Host-side guardrails on the model's match JSON: date-gap relabel and leftover grouping.
/// Does not invent MATCHED or PARTIAL pairs — the model decides those.
/// </summary>
public static class ReconciliationMatchAnswerNormalizer
{
    public const int MatchedMaxDateGapDays = 30;

    private static readonly JsonSerializerOptions WriteOptions = new()
    {
        WriteIndented = true
    };

    public static string Normalize(string answer, string? matchCandidatesJson)
    {
        if (!TryExtractJsonObject(answer, out var json))
            return answer;

        JsonNode? root;
        try
        {
            root = JsonNode.Parse(json);
        }
        catch (JsonException)
        {
            return answer;
        }

        if (root is not JsonObject rootObject
            || rootObject["combinations"] is not JsonArray combinations)
        {
            return json;
        }

        var lines = ParseLines(matchCandidatesJson);
        ApplyDateGapStatus(combinations, lines);

        var leftoverBank = new List<string>();
        var leftoverTreasury = new List<string>();
        SplitUnmatched(combinations, leftoverBank, leftoverTreasury);
        DeduplicateExclusiveRefs(combinations, leftoverBank, leftoverTreasury, lines);
        CompleteLeftoversFromCandidates(combinations, leftoverBank, leftoverTreasury, lines);
        AppendLeftoverGroups(combinations, leftoverBank, leftoverTreasury);

        return rootObject.ToJsonString(WriteOptions);
    }

    public static bool TryExtractJsonObject(string answer, out string json)
    {
        json = string.Empty;
        if (string.IsNullOrWhiteSpace(answer))
            return false;

        var text = answer.Trim();
        if (text.StartsWith("```", StringComparison.Ordinal))
        {
            var firstNewline = text.IndexOf('\n');
            if (firstNewline < 0)
                return false;

            text = text[(firstNewline + 1)..];
            var fence = text.LastIndexOf("```", StringComparison.Ordinal);
            if (fence >= 0)
                text = text[..fence];
            text = text.Trim();
        }

        var start = text.IndexOf('{');
        var end = text.LastIndexOf('}');
        if (start < 0 || end <= start)
            return false;

        json = text[start..(end + 1)];
        return true;
    }

    private static Dictionary<string, LineInfo> ParseLines(string? matchCandidatesJson)
    {
        var lines = new Dictionary<string, LineInfo>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(matchCandidatesJson))
            return lines;

        try
        {
            using var doc = JsonDocument.Parse(matchCandidatesJson);
            ReadLines(doc.RootElement, "bankMovements", lines);
            ReadLines(doc.RootElement, "treasuryMovements", lines);
        }
        catch (JsonException)
        {
            return lines;
        }

        return lines;
    }

    private static void ReadLines(JsonElement root, string property, Dictionary<string, LineInfo> lines)
    {
        if (!root.TryGetProperty(property, out var items) || items.ValueKind != JsonValueKind.Array)
            return;

        foreach (var item in items.EnumerateArray())
        {
            if (!item.TryGetProperty("refId", out var refIdProp))
                continue;

            var refId = refIdProp.GetString();
            if (string.IsNullOrWhiteSpace(refId))
                continue;

            var date = ReadDate(item, "date");
            if (date is null)
                continue;

            var valueDate = ReadDate(item, "valueDate") ?? date.Value;
            var amount = ReadAmount(item);
            var party = ReadString(item, "party") ?? ReadString(item, "description") ?? string.Empty;
            var cheque = DigitsOnly(ReadString(item, "cheque"));
            var invoice = ReadString(item, "invoice") ?? ReadString(item, "document") ?? string.Empty;

            lines[refId] = new LineInfo(refId, amount, date.Value, valueDate, party, cheque, invoice);
        }
    }

    private static void ApplyDateGapStatus(JsonArray combinations, IReadOnlyDictionary<string, LineInfo> lines)
    {
        if (lines.Count == 0)
            return;

        foreach (var node in combinations)
        {
            if (node is not JsonObject combination)
                continue;

            var status = combination["status"]?.GetValue<string>();
            if (!string.Equals(status, "MATCHED", StringComparison.OrdinalIgnoreCase))
                continue;

            var bankRefs = ReadRefs(combination["bankRefs"]);
            var treasuryRefs = ReadRefs(combination["treasuryRefs"]);
            if (bankRefs.Count == 0 || treasuryRefs.Count == 0)
                continue;

            if (!TryDateGap(bankRefs, treasuryRefs, lines, out var gap, out var minDate, out var maxDate))
                continue;

            if (gap <= MatchedMaxDateGapDays)
                continue;

            combination["status"] = "PARTIAL MATCHED";
            var reason = combination["reason"]?.GetValue<string>() ?? string.Empty;
            var suffix = $"datas {minDate:yyyy-MM-dd} vs {maxDate:yyyy-MM-dd} ({gap} dias > {MatchedMaxDateGapDays})";
            combination["reason"] = string.IsNullOrWhiteSpace(reason)
                ? suffix
                : $"{reason.Trim().TrimEnd('.')}. {suffix}";
        }
    }

    private static void SplitUnmatched(JsonArray combinations, List<string> leftoverBank, List<string> leftoverTreasury)
    {
        var kept = new List<JsonNode?>();

        foreach (var node in combinations)
        {
            if (node is not JsonObject combination)
            {
                kept.Add(node?.DeepClone());
                continue;
            }

            var status = combination["status"]?.GetValue<string>() ?? string.Empty;
            var bankRefs = ReadRefs(combination["bankRefs"]);
            var treasuryRefs = ReadRefs(combination["treasuryRefs"]);
            var isUnmatched = string.Equals(status, "UNMATCHED", StringComparison.OrdinalIgnoreCase);

            if (isUnmatched && bankRefs.Count > 0 && treasuryRefs.Count == 0)
            {
                leftoverBank.AddRange(bankRefs);
                continue;
            }

            if (isUnmatched && treasuryRefs.Count > 0 && bankRefs.Count == 0)
            {
                leftoverTreasury.AddRange(treasuryRefs);
                continue;
            }

            kept.Add(combination.DeepClone());
        }

        combinations.Clear();
        foreach (var item in kept)
            combinations.Add(item);
    }

    private static void DeduplicateExclusiveRefs(
        JsonArray combinations,
        List<string> leftoverBank,
        List<string> leftoverTreasury,
        IReadOnlyDictionary<string, LineInfo> lines)
    {
        var ranked = combinations
            .OfType<JsonObject>()
            .Select(combination =>
            {
                var bankRefs = ReadRefs(combination["bankRefs"]);
                var treasuryRefs = ReadRefs(combination["treasuryRefs"]);
                var status = combination["status"]?.GetValue<string>() ?? string.Empty;
                var hasGap = TryDateGap(bankRefs, treasuryRefs, lines, out var gap, out _, out _);
                var rank = string.Equals(status, "MATCHED", StringComparison.OrdinalIgnoreCase) ? 0
                    : string.Equals(status, "PARTIAL MATCHED", StringComparison.OrdinalIgnoreCase) ? 1
                    : 2;
                return (combination, bankRefs, treasuryRefs, rank, DateGap: hasGap ? gap : int.MaxValue);
            })
            .OrderBy(x => x.rank)
            .ThenBy(x => x.DateGap)
            .ToList();

        var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var kept = new List<JsonNode>();

        foreach (var (combination, bankRefs, treasuryRefs, _, _) in ranked)
        {
            if (bankRefs.Count == 0 && treasuryRefs.Count == 0)
                continue;

            var isPair = !string.Equals(
                combination["status"]?.GetValue<string>(),
                "UNMATCHED",
                StringComparison.OrdinalIgnoreCase);
            if (isPair && (bankRefs.Count == 0 || treasuryRefs.Count == 0))
            {
                leftoverBank.AddRange(bankRefs.Where(id => !used.Contains(id)));
                leftoverTreasury.AddRange(treasuryRefs.Where(id => !used.Contains(id)));
                continue;
            }

            if (bankRefs.Any(used.Contains) || treasuryRefs.Any(used.Contains))
            {
                foreach (var id in bankRefs.Where(id => !used.Contains(id)))
                    leftoverBank.Add(id);
                foreach (var id in treasuryRefs.Where(id => !used.Contains(id)))
                    leftoverTreasury.Add(id);
                continue;
            }

            foreach (var id in bankRefs.Concat(treasuryRefs))
                used.Add(id);

            kept.Add(combination.DeepClone());
        }

        leftoverBank.RemoveAll(used.Contains);
        leftoverTreasury.RemoveAll(used.Contains);

        combinations.Clear();
        foreach (var item in kept)
            combinations.Add(item);
    }

    private static void CompleteLeftoversFromCandidates(
        JsonArray combinations,
        List<string> leftoverBank,
        List<string> leftoverTreasury,
        IReadOnlyDictionary<string, LineInfo> lines)
    {
        if (lines.Count == 0)
            return;

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var node in combinations)
        {
            if (node is not JsonObject combination)
                continue;

            foreach (var id in ReadRefs(combination["bankRefs"]).Concat(ReadRefs(combination["treasuryRefs"])))
                seen.Add(id);
        }

        foreach (var id in leftoverBank.Concat(leftoverTreasury))
            seen.Add(id);

        foreach (var id in lines.Keys)
        {
            if (!seen.Add(id))
                continue;

            if (id.StartsWith('B') || id.StartsWith('b'))
                leftoverBank.Add(id);
            else if (id.StartsWith('T') || id.StartsWith('t'))
                leftoverTreasury.Add(id);
        }
    }

    private static void AppendLeftoverGroups(
        JsonArray combinations,
        IReadOnlyList<string> leftoverBank,
        IReadOnlyList<string> leftoverTreasury)
    {
        if (leftoverBank.Count > 0)
            combinations.Add(UnmatchedGroup("Sem correspondencia na tesouraria", leftoverBank, []));

        if (leftoverTreasury.Count > 0)
            combinations.Add(UnmatchedGroup("Sem correspondencia no banco", [], leftoverTreasury));
    }

    private static bool TryDateGap(
        IReadOnlyList<string> bankRefs,
        IReadOnlyList<string> treasuryRefs,
        IReadOnlyDictionary<string, LineInfo> lines,
        out int gap,
        out DateOnly minDate,
        out DateOnly maxDate)
    {
        gap = 0;
        minDate = default;
        maxDate = default;
        var best = int.MaxValue;
        DateOnly? bestMin = null;
        DateOnly? bestMax = null;

        foreach (var bankRef in bankRefs)
        {
            if (!lines.TryGetValue(bankRef, out var bank))
                continue;

            foreach (var treasuryRef in treasuryRefs)
            {
                if (!lines.TryGetValue(treasuryRef, out var treasury))
                    continue;

                foreach (var bankDate in new[] { bank.Date, bank.ValueDate })
                {
                    foreach (var treasuryDate in new[] { treasury.Date, treasury.ValueDate })
                    {
                        var current = Math.Abs(bankDate.DayNumber - treasuryDate.DayNumber);
                        if (current >= best)
                            continue;

                        best = current;
                        bestMin = bankDate < treasuryDate ? bankDate : treasuryDate;
                        bestMax = bankDate < treasuryDate ? treasuryDate : bankDate;
                    }
                }
            }
        }

        if (bestMin is null || bestMax is null)
            return false;

        gap = best;
        minDate = bestMin.Value;
        maxDate = bestMax.Value;
        return true;
    }

    private static JsonObject UnmatchedGroup(string reason, IReadOnlyList<string> bankRefs, IReadOnlyList<string> treasuryRefs)
        => new()
        {
            ["status"] = "UNMATCHED",
            ["reason"] = reason,
            ["bankRefs"] = ToArray(bankRefs),
            ["treasuryRefs"] = ToArray(treasuryRefs)
        };

    private static JsonArray ToArray(IReadOnlyList<string> values)
    {
        var array = new JsonArray();
        foreach (var value in values)
            array.Add(value);
        return array;
    }

    private static List<string> ReadRefs(JsonNode? node)
    {
        var refs = new List<string>();
        if (node is not JsonArray array)
            return refs;

        foreach (var item in array)
        {
            var value = item?.GetValue<string>();
            if (!string.IsNullOrWhiteSpace(value))
                refs.Add(value);
        }

        return refs;
    }

    private static DateOnly? ReadDate(JsonElement item, string name)
    {
        if (!item.TryGetProperty(name, out var dateProp)
            || dateProp.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return null;
        }

        var raw = dateProp.ValueKind == JsonValueKind.String
            ? dateProp.GetString()
            : dateProp.ToString();

        if (DateOnly.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            return date;

        if (DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dateTime))
            return DateOnly.FromDateTime(dateTime);

        return null;
    }

    private static decimal ReadAmount(JsonElement item)
    {
        if (!item.TryGetProperty("amount", out var amount) || amount.ValueKind != JsonValueKind.Number)
            return 0m;

        return amount.GetDecimal();
    }

    private static string? ReadString(JsonElement item, string name)
    {
        if (!item.TryGetProperty(name, out var value) || value.ValueKind != JsonValueKind.String)
            return null;

        var text = value.GetString()?.Trim();
        return string.IsNullOrWhiteSpace(text) ? null : text;
    }

    private static string DigitsOnly(string? value)
        => string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : new string(value.Where(char.IsDigit).ToArray());

    private sealed record LineInfo(
        string RefId,
        decimal Amount,
        DateOnly Date,
        DateOnly ValueDate,
        string Party,
        string ChequeDigits,
        string Invoice);
}
