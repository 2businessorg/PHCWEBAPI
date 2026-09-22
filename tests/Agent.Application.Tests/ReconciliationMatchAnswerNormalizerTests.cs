using Agent.Application;
using FluentAssertions;
using System.Text.Json;
using Xunit;

namespace Agent.Application.Tests;

public class ReconciliationMatchAnswerNormalizerTests
{
    private const string Candidates = """
        {
          "bankMovements": [
            { "refId": "B25", "date": "2023-02-10", "amount": 14016.28 },
            { "refId": "B64", "date": "2023-01-10", "amount": 10391.86 },
            { "refId": "B1", "date": "2022-01-01", "amount": 1 }
          ],
          "treasuryMovements": [
            { "refId": "T67", "date": "2023-02-15", "amount": 14016.28 },
            { "refId": "T37", "date": "2025-07-04", "amount": 10391.86 },
            { "refId": "T1", "date": "2025-01-01", "amount": 2 }
          ]
        }
        """;

    [Fact]
    public void Normalize_MatchedWithFarDates_ShouldBecomePartialMatched()
    {
        const string answer = """
            ```json
            {
              "combinations": [
                { "status": "MATCHED", "reason": "PROMAR", "bankRefs": ["B64"], "treasuryRefs": ["T37"] },
                { "status": "MATCHED", "reason": "GEOSYSTEMS", "bankRefs": ["B25"], "treasuryRefs": ["T67"] },
                { "status": "UNMATCHED", "reason": "x", "bankRefs": ["B1"], "treasuryRefs": [] },
                { "status": "UNMATCHED", "reason": "x", "bankRefs": [], "treasuryRefs": ["T1"] }
              ]
            }
            ```
            """;

        var normalized = ReconciliationMatchAnswerNormalizer.Normalize(answer, Candidates);
        using var doc = JsonDocument.Parse(normalized);
        var combinations = doc.RootElement.GetProperty("combinations");

        combinations.GetArrayLength().Should().Be(4);

        var partial = combinations.EnumerateArray()
            .Single(c => c.GetProperty("bankRefs").GetArrayLength() == 1
                && c.GetProperty("bankRefs")[0].GetString() == "B64");
        partial.GetProperty("status").GetString().Should().Be("PARTIAL MATCHED");
        partial.GetProperty("reason").GetString().Should().Contain("2023-01-10");
        partial.GetProperty("reason").GetString().Should().Contain("2025-07-04");

        combinations.EnumerateArray()
            .Single(c => c.GetProperty("bankRefs").GetArrayLength() == 1
                && c.GetProperty("bankRefs")[0].GetString() == "B25")
            .GetProperty("status").GetString().Should().Be("MATCHED");

        combinations.EnumerateArray()
            .Single(c => c.GetProperty("status").GetString() == "UNMATCHED"
                && c.GetProperty("treasuryRefs").GetArrayLength() == 0)
            .GetProperty("bankRefs").EnumerateArray().Select(x => x.GetString())
            .Should().Equal("B1");
        combinations.EnumerateArray()
            .Single(c => c.GetProperty("status").GetString() == "UNMATCHED"
                && c.GetProperty("bankRefs").GetArrayLength() == 0)
            .GetProperty("treasuryRefs").EnumerateArray().Select(x => x.GetString())
            .Should().Equal("T1");
    }

    [Fact]
    public void Normalize_LeftoverAmountAndParty_ShouldNotInventPairs()
    {
        const string candidates = """
            {
              "bankMovements": [
                { "refId": "B25", "date": "2023-02-10", "valueDate": "2023-02-10", "amount": 14016.28, "party": "GEOSYSTEMS 2BUSINESS" },
                { "refId": "B1", "date": "2022-01-01", "amount": 1, "party": "OUTRO" }
              ],
              "treasuryMovements": [
                { "refId": "T67", "date": "2023-02-15", "valueDate": "2023-02-15", "amount": 14016.28, "party": "GEOSYSTEMS INSTRUMENTOS" },
                { "refId": "T1", "date": "2025-01-01", "amount": 2, "party": "CAIXA" }
              ]
            }
            """;

        const string answer = """
            {
              "combinations": [
                { "status": "UNMATCHED", "reason": "x", "bankRefs": ["B25", "B1"], "treasuryRefs": [] },
                { "status": "UNMATCHED", "reason": "x", "bankRefs": [], "treasuryRefs": ["T67", "T1"] }
              ]
            }
            """;

        var normalized = ReconciliationMatchAnswerNormalizer.Normalize(answer, candidates);
        using var doc = JsonDocument.Parse(normalized);
        var combinations = doc.RootElement.GetProperty("combinations");

        combinations.EnumerateArray()
            .Should()
            .OnlyContain(c => c.GetProperty("status").GetString() == "UNMATCHED");
        combinations.EnumerateArray()
            .Should()
            .NotContain(c =>
                c.GetProperty("status").GetString() == "MATCHED"
                || c.GetProperty("status").GetString() == "PARTIAL MATCHED");
    }

    [Fact]
    public void Normalize_RepeatedBankRef_ShouldKeepClosestPairOnly()
    {
        const string candidates = """
            {
              "bankMovements": [
                { "refId": "B64", "date": "2023-01-10", "amount": 10391.86, "party": "PROMAR" }
              ],
              "treasuryMovements": [
                { "refId": "T37", "date": "2025-07-04", "amount": 10391.86, "party": "PROMAR" },
                { "refId": "T51", "date": "2026-01-13", "amount": 10391.86, "party": "PROMAR" },
                { "refId": "T70", "date": "2026-01-27", "amount": 10391.86, "party": "SOPERFIS" }
              ]
            }
            """;

        const string answer = """
            {
              "combinations": [
                { "status": "PARTIAL MATCHED", "reason": "a", "bankRefs": ["B64"], "treasuryRefs": ["T37"] },
                { "status": "PARTIAL MATCHED", "reason": "b", "bankRefs": ["B64"], "treasuryRefs": ["T37"] },
                { "status": "PARTIAL MATCHED", "reason": "c", "bankRefs": ["B64"], "treasuryRefs": ["T51"] },
                { "status": "PARTIAL MATCHED", "reason": "d", "bankRefs": ["B64"], "treasuryRefs": ["T70"] }
              ]
            }
            """;

        var normalized = ReconciliationMatchAnswerNormalizer.Normalize(answer, candidates);
        using var doc = JsonDocument.Parse(normalized);
        var combinations = doc.RootElement.GetProperty("combinations");

        combinations.EnumerateArray()
            .Count(c => c.GetProperty("status").GetString() == "PARTIAL MATCHED")
            .Should().Be(1);
        combinations[0].GetProperty("treasuryRefs")[0].GetString().Should().Be("T37");

        var unmatchedTreasury = combinations.EnumerateArray()
            .Single(c => c.GetProperty("status").GetString() == "UNMATCHED"
                && c.GetProperty("bankRefs").GetArrayLength() == 0)
            .GetProperty("treasuryRefs")
            .EnumerateArray()
            .Select(x => x.GetString())
            .ToList();

        unmatchedTreasury.Should().BeEquivalentTo("T51", "T70");
    }

    [Fact]
    public void Normalize_EmptyPartialCombination_ShouldDropIt()
    {
        const string candidates = """
            {
              "bankMovements": [
                { "refId": "B64", "date": "2023-01-10", "amount": 10391.86, "party": "PROMAR" }
              ],
              "treasuryMovements": [
                { "refId": "T37", "date": "2025-07-04", "amount": 10391.86, "party": "PROMAR" }
              ]
            }
            """;

        const string answer = """
            {
              "combinations": [
                { "status": "PARTIAL MATCHED", "reason": "noise", "bankRefs": [], "treasuryRefs": [] },
                { "status": "PARTIAL MATCHED", "reason": "ok", "bankRefs": ["B64"], "treasuryRefs": ["T37"] }
              ]
            }
            """;

        var normalized = ReconciliationMatchAnswerNormalizer.Normalize(answer, candidates);
        using var doc = JsonDocument.Parse(normalized);
        var combinations = doc.RootElement.GetProperty("combinations");

        combinations.GetArrayLength().Should().Be(1);
        combinations[0].GetProperty("bankRefs")[0].GetString().Should().Be("B64");
        combinations[0].GetProperty("treasuryRefs")[0].GetString().Should().Be("T37");
    }

    [Fact]
    public void Normalize_OmittedTreasuryLeftovers_ShouldCompleteFromCandidates()
    {
        const string candidates = """
            {
              "bankMovements": [
                { "refId": "B1", "date": "2026-01-06", "amount": 221153.51, "party": "NEDBANK" },
                { "refId": "B2", "date": "2026-01-06", "amount": 1102.00, "party": "IDELSON" },
                { "refId": "B3", "date": "2026-01-06", "amount": 60693.10, "party": "NEDBANK" },
                { "refId": "B4", "date": "2026-01-06", "amount": 46048.52, "party": "OIC" }
              ],
              "treasuryMovements": [
                { "refId": "T1", "date": "2022-03-01", "amount": 50, "party": "CAIXA" },
                { "refId": "T2", "date": "2023-06-01", "amount": 80, "party": "FORNECEDOR" }
              ]
            }
            """;

        const string answer = """
            {
              "combinations": [
                { "status": "UNMATCHED", "reason": "Sem correspondencia na tesouraria", "bankRefs": ["B1", "B2", "B3", "B4"], "treasuryRefs": [] }
              ]
            }
            """;

        var normalized = ReconciliationMatchAnswerNormalizer.Normalize(answer, candidates);
        using var doc = JsonDocument.Parse(normalized);
        var combinations = doc.RootElement.GetProperty("combinations");

        combinations.GetArrayLength().Should().Be(2);
        combinations[0].GetProperty("bankRefs").EnumerateArray().Select(x => x.GetString())
            .Should().Equal("B1", "B2", "B3", "B4");
        combinations[1].GetProperty("treasuryRefs").EnumerateArray().Select(x => x.GetString())
            .Should().Equal("T1", "T2");
    }

    [Fact]
    public void Normalize_OmittedPairInLeftovers_ShouldStayUnmatched()
    {
        const string candidates = """
            {
              "bankMovements": [
                { "refId": "B1", "date": "2026-01-06", "amount": 1102.00, "party": "IDELSON" },
                { "refId": "B25", "date": "2023-01-04", "amount": 14016.28, "party": "GEOSYSTEMS" }
              ],
              "treasuryMovements": [
                { "refId": "T67", "date": "2026-01-27", "amount": 14016.28, "party": "GEOSYSTEMS" },
                { "refId": "T1", "date": "2022-03-01", "amount": 50, "party": "CAIXA" }
              ]
            }
            """;

        const string answer = """
            {
              "combinations": [
                { "status": "UNMATCHED", "reason": "x", "bankRefs": ["B1", "B25"], "treasuryRefs": [] }
              ]
            }
            """;

        var normalized = ReconciliationMatchAnswerNormalizer.Normalize(answer, candidates);
        using var doc = JsonDocument.Parse(normalized);
        var combinations = doc.RootElement.GetProperty("combinations");

        combinations.EnumerateArray()
            .Should()
            .OnlyContain(c => c.GetProperty("status").GetString() == "UNMATCHED");

        combinations.EnumerateArray()
            .Single(c => c.GetProperty("treasuryRefs").GetArrayLength() == 0)
            .GetProperty("bankRefs").EnumerateArray().Select(x => x.GetString())
            .Should().BeEquivalentTo("B1", "B25");
        combinations.EnumerateArray()
            .Single(c => c.GetProperty("bankRefs").GetArrayLength() == 0)
            .GetProperty("treasuryRefs").EnumerateArray().Select(x => x.GetString())
            .Should().BeEquivalentTo("T67", "T1");
    }

    [Fact]
    public void Normalize_PlainText_ShouldLeaveUnchanged()
    {
        const string answer = "31 BR and 57 BA movements.";
        ReconciliationMatchAnswerNormalizer.Normalize(answer, Candidates).Should().Be(answer);
    }
}
