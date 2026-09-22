using System.Globalization;
using System.Text;
using Treasury.Application.DTOs;

namespace Treasury.Application.Matching;

/// <summary>
/// Deterministic greedy matcher: 1:1 by amount+date, then 1:N / N:1 sums, then amount-only partials.
/// </summary>
public sealed class ReconciliationMatcher : IReconciliationMatcher
{
    public const int MatchedMaxDateDays = 7;
    public const int CombinationMaxDateDays = 14;
    public const decimal AmountTolerance = 0.01m;

    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "TRF", "RCB", "CHQ", "CHEQUE", "TRANSF", "NCHE", "ADIANT", "VENC", "FT", "LDA", "SA",
        "COMP", "ORD", "BCI", "USD", "EUR", "THE", "FROM", "PARA", "DE", "DO", "DA", "E",
        "REF", "PAG", "PAGTO", "TRANSFER", "CREDITO", "DEBITO"
    };

    public IReadOnlyList<ReconciliationMatchCombinationDTO> Match(
        IReadOnlyList<ImportedBankMovementOutputDTO> bankMovements,
        IReadOnlyList<TreasuryAccountMovementOutputDTO> treasuryMovements)
    {
        var bank = bankMovements.Select((m, i) => BankItem.From(m, i)).ToList();
        var treasury = treasuryMovements.Select((m, i) => TreasuryItem.From(m, i)).ToList();
        var usedBank = new HashSet<int>();
        var usedTreasury = new HashSet<int>();
        var combinations = new List<ReconciliationMatchCombinationDTO>();

        MatchOneToOne(bank, treasury, usedBank, usedTreasury, combinations, requireCloseDate: true);
        MatchCombinations(bank, treasury, usedBank, usedTreasury, combinations);
        MatchOneToOne(bank, treasury, usedBank, usedTreasury, combinations, requireCloseDate: false);
        AppendUnmatched(bank, treasury, usedBank, usedTreasury, combinations);

        return combinations;
    }

    private static void MatchOneToOne(
        List<BankItem> bank,
        List<TreasuryItem> treasury,
        HashSet<int> usedBank,
        HashSet<int> usedTreasury,
        List<ReconciliationMatchCombinationDTO> combinations,
        bool requireCloseDate)
    {
        foreach (var bankItem in bank.Where(b => !usedBank.Contains(b.Index)))
        {
            TreasuryItem? best = null;
            var bestScore = int.MinValue;

            foreach (var treasuryItem in treasury.Where(t => !usedTreasury.Contains(t.Index)))
            {
                if (!AmountEquals(bankItem.Amount, treasuryItem.SignedAmount))
                    continue;

                var days = DateGapDays(bankItem.Date, treasuryItem.Date);
                var closeDate = days <= MatchedMaxDateDays;
                if (requireCloseDate && !closeDate)
                    continue;

                var tokens = SharedTokenCount(bankItem.Tokens, treasuryItem.Tokens);
                var cheque = ChequeEquals(bankItem.ChequeDigits, treasuryItem.ChequeDigits);
                var score = (closeDate ? 1000 : 0) + (cheque ? 200 : 0) + (tokens * 10) - days;

                if (score > bestScore)
                {
                    bestScore = score;
                    best = treasuryItem;
                }
            }

            if (best is null)
                continue;

            usedBank.Add(bankItem.Index);
            usedTreasury.Add(best.Index);

            var gap = DateGapDays(bankItem.Date, best.Date);
            var matched = gap <= MatchedMaxDateDays;
            combinations.Add(CreateCombination(
                matched ? ReconciliationMatchStatuses.Matched : ReconciliationMatchStatuses.PartialMatched,
                matched
                    ? $"Valor igual e {gap} dia(s) de diferenca."
                    : $"Valor igual mas {gap} dia(s) de diferenca.",
                new[] { bankItem },
                new[] { best }));
        }
    }

    private static void MatchCombinations(
        List<BankItem> bank,
        List<TreasuryItem> treasury,
        HashSet<int> usedBank,
        HashSet<int> usedTreasury,
        List<ReconciliationMatchCombinationDTO> combinations)
    {
        foreach (var bankItem in bank.Where(b => !usedBank.Contains(b.Index)))
        {
            var unusedTreasury = treasury.Where(t => !usedTreasury.Contains(t.Index)).ToList();
            var subset = FindSubset(unusedTreasury, t => t.SignedAmount, bankItem.Amount, bankItem.Date);
            if (subset is null)
                continue;

            usedBank.Add(bankItem.Index);
            foreach (var item in subset)
                usedTreasury.Add(item.Index);

            var maxGap = subset.Max(t => DateGapDays(bankItem.Date, t.Date));
            var matched = maxGap <= MatchedMaxDateDays;
            combinations.Add(CreateCombination(
                matched ? ReconciliationMatchStatuses.Matched : ReconciliationMatchStatuses.PartialMatched,
                matched
                    ? $"1 movimento bancario = {subset.Count} de tesouraria (soma igual, max {maxGap} dia(s))."
                    : $"1 movimento bancario = {subset.Count} de tesouraria (soma igual, datas ate {maxGap} dia(s)).",
                new[] { bankItem },
                subset));
        }

        foreach (var treasuryItem in treasury.Where(t => !usedTreasury.Contains(t.Index)))
        {
            var unusedBank = bank.Where(b => !usedBank.Contains(b.Index)).ToList();
            var subset = FindSubset(unusedBank, b => b.Amount, treasuryItem.SignedAmount, treasuryItem.Date);
            if (subset is null)
                continue;

            usedTreasury.Add(treasuryItem.Index);
            foreach (var item in subset)
                usedBank.Add(item.Index);

            var maxGap = subset.Max(b => DateGapDays(b.Date, treasuryItem.Date));
            var matched = maxGap <= MatchedMaxDateDays;
            combinations.Add(CreateCombination(
                matched ? ReconciliationMatchStatuses.Matched : ReconciliationMatchStatuses.PartialMatched,
                matched
                    ? $"{subset.Count} movimentos bancarios = 1 de tesouraria (soma igual, max {maxGap} dia(s))."
                    : $"{subset.Count} movimentos bancarios = 1 de tesouraria (soma igual, datas ate {maxGap} dia(s)).",
                subset,
                new[] { treasuryItem }));
        }
    }

    private static List<T>? FindSubset<T>(
        List<T> items,
        Func<T, decimal> amount,
        decimal target,
        DateOnly anchorDate)
        where T : class
    {
        var candidates = items
            .Select((item, i) => (item, i, amount: amount(item), days: DateGapDays(anchorDate, GetDate(item))))
            .Where(x => x.days <= CombinationMaxDateDays)
            .OrderBy(x => x.days)
            .ToList();

        for (var i = 0; i < candidates.Count; i++)
        {
            for (var j = i + 1; j < candidates.Count; j++)
            {
                if (AmountEquals(candidates[i].amount + candidates[j].amount, target))
                    return new List<T> { candidates[i].item, candidates[j].item };

                for (var k = j + 1; k < candidates.Count; k++)
                {
                    if (AmountEquals(candidates[i].amount + candidates[j].amount + candidates[k].amount, target))
                        return new List<T> { candidates[i].item, candidates[j].item, candidates[k].item };
                }
            }
        }

        return null;
    }

    private static DateOnly GetDate(object item)
        => item switch
        {
            BankItem bank => bank.Date,
            TreasuryItem treasury => treasury.Date,
            _ => throw new InvalidOperationException("Unexpected match item.")
        };

    private static void AppendUnmatched(
        List<BankItem> bank,
        List<TreasuryItem> treasury,
        HashSet<int> usedBank,
        HashSet<int> usedTreasury,
        List<ReconciliationMatchCombinationDTO> combinations)
    {
        foreach (var bankItem in bank.Where(b => !usedBank.Contains(b.Index)))
        {
            combinations.Add(CreateCombination(
                ReconciliationMatchStatuses.Unmatched,
                "Sem movimento de tesouraria com o mesmo valor.",
                new[] { bankItem },
                Array.Empty<TreasuryItem>()));
        }

        foreach (var treasuryItem in treasury.Where(t => !usedTreasury.Contains(t.Index)))
        {
            combinations.Add(CreateCombination(
                ReconciliationMatchStatuses.Unmatched,
                "Sem movimento bancario com o mesmo valor.",
                Array.Empty<BankItem>(),
                new[] { treasuryItem }));
        }
    }

    private static ReconciliationMatchCombinationDTO CreateCombination(
        string status,
        string reason,
        IReadOnlyList<BankItem> bankItems,
        IReadOnlyList<TreasuryItem> treasuryItems)
    {
        var bankLines = bankItems.Select(b => b.ToLine()).ToList();
        var treasuryLines = treasuryItems.Select(t => t.ToLine()).ToList();
        var dates = bankLines.Select(l => l.Date).Concat(treasuryLines.Select(l => l.Date)).ToList();
        int? gap = dates.Count >= 2
            ? dates.Max().DayNumber - dates.Min().DayNumber
            : null;

        return new ReconciliationMatchCombinationDTO
        {
            Status = status,
            Reason = reason,
            BankCount = bankLines.Count,
            TreasuryCount = treasuryLines.Count,
            BankAmountTotal = bankLines.Sum(l => l.Amount),
            TreasuryAmountTotal = treasuryLines.Sum(l => l.Amount),
            DateGapDays = gap,
            Bank = bankLines,
            Treasury = treasuryLines
        };
    }

    internal static bool AmountEquals(decimal left, decimal right)
        => Math.Abs(left - right) <= AmountTolerance;

    internal static int DateGapDays(DateOnly left, DateOnly right)
        => Math.Abs(left.DayNumber - right.DayNumber);

    internal static bool ChequeEquals(string left, string right)
        => left.Length >= 4 && right.Length >= 4 && (left == right || left.Contains(right) || right.Contains(left));

    internal static int SharedTokenCount(IReadOnlySet<string> left, IReadOnlySet<string> right)
        => left.Count(right.Contains);

    internal static HashSet<string> Tokenize(params string[] texts)
    {
        var tokens = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var text in texts)
        {
            if (string.IsNullOrWhiteSpace(text))
                continue;

            var normalized = RemoveDiacritics(text);
            var current = new StringBuilder();
            foreach (var ch in normalized)
            {
                if (char.IsLetterOrDigit(ch))
                {
                    current.Append(char.ToUpperInvariant(ch));
                    continue;
                }

                AddToken(tokens, current);
            }

            AddToken(tokens, current);
        }

        return tokens;
    }

    private static void AddToken(HashSet<string> tokens, StringBuilder current)
    {
        if (current.Length >= 4 && !StopWords.Contains(current.ToString()))
            tokens.Add(current.ToString());

        current.Clear();
    }

    internal static string DigitsOnly(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var chars = value.Where(char.IsDigit).ToArray();
        return new string(chars);
    }

    private static string RemoveDiacritics(string text)
    {
        var formD = text.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(formD.Length);
        foreach (var ch in formD)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                builder.Append(ch);
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }

    private sealed record BankItem(
        int Index,
        string Id,
        DateOnly Date,
        decimal Amount,
        string Description,
        string Cheque,
        string ChequeDigits,
        HashSet<string> Tokens)
    {
        public static BankItem From(ImportedBankMovementOutputDTO movement, int index)
            => new(
                index,
                movement.Id,
                movement.Date,
                movement.Amount,
                movement.Description,
                movement.Cheque,
                DigitsOnly(movement.Cheque),
                Tokenize(movement.Description, movement.Document, movement.Cheque));

        public ReconciliationMatchLineDTO ToLine()
            => new()
            {
                Id = Id,
                Side = "BANK",
                Date = Date,
                Amount = Amount,
                Description = Description,
                Cheque = Cheque
            };
    }

    private sealed record TreasuryItem(
        int Index,
        string Id,
        DateOnly Date,
        decimal SignedAmount,
        string Description,
        string Cheque,
        string ChequeDigits,
        HashSet<string> Tokens)
    {
        public static TreasuryItem From(TreasuryAccountMovementOutputDTO movement, int index)
            => new(
                index,
                movement.Id,
                movement.Date,
                movement.Inflow - movement.Outflow,
                movement.Description,
                movement.Cheque,
                DigitsOnly(movement.Cheque),
                Tokenize(movement.Description, movement.Document, movement.Cheque));

        public ReconciliationMatchLineDTO ToLine()
            => new()
            {
                Id = Id,
                Side = "TREASURY",
                Date = Date,
                Amount = SignedAmount,
                Description = Description,
                Cheque = Cheque
            };
    }
}
