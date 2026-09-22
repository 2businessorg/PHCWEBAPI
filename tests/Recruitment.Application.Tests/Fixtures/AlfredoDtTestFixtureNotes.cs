using FluentAssertions;
using Recruitment.Application.DTOs;
using Recruitment.Application.Features.CompareCandidates;
using Recruitment.Domain.Constants;
using Xunit;

namespace Recruitment.Application.Tests.Fixtures;

/// <summary>
/// Notes for Alfredo DTTest fixtures (demo OnTS_2BusinessIA):
/// 1. CV without skill X must not score high on X (RubricEvidenceScorerTests covers unit).
/// 2. Justification quote must be contiguous subset of anexos.u_texto (AH-02 falsifier).
/// 3. After OCR fail: cve.u_estadoia='erro' and srt.u_scoreia IS NULL.
/// 4. After success: srt.u_scoreia set; cve has NO canonical multi-RCT score column written.
/// 5. Diff srt.condp / estado / apurado / entrevista pre vs post job = unchanged.
/// </summary>
public class AlfredoDtTestFixtureNotes
{
    [Fact]
    public void Compare_TopDiffs_UsesQuotesNotMoralNarrative()
    {
        var first = new RankedCandidateDto
        {
            Position = 1,
            SrtStamp = "S1",
            CveStamp = "C1",
            ScoreTotal = 80,
            Breakdown =
            [
                new CriterionBreakdownDto
                {
                    Code = "CRT-STACK",
                    Label = "Stack",
                    Weight = 25,
                    Note = 25,
                    Quote = "experiencia PHC e SQL"
                },
                new CriterionBreakdownDto
                {
                    Code = "CRT-MZ",
                    Label = "MZ",
                    Weight = 10,
                    Note = 10,
                    Quote = "Maputo presencial"
                }
            ]
        };

        var other = new RankedCandidateDto
        {
            Position = 2,
            SrtStamp = "S2",
            CveStamp = "C2",
            ScoreTotal = 40,
            Breakdown =
            [
                new CriterionBreakdownDto
                {
                    Code = "CRT-STACK",
                    Label = "Stack",
                    Weight = 25,
                    Note = 10,
                    Quote = "Excel basico"
                },
                new CriterionBreakdownDto
                {
                    Code = "CRT-MZ",
                    Label = "MZ",
                    Weight = 10,
                    Note = 0,
                    SemEvidencia = true
                }
            ]
        };

        var diffs = CompareCandidatesQueryHandler.BuildTopDiffs(first, other, 3);

        diffs.Should().NotBeEmpty();
        diffs[0].FirstQuote.Should().NotBeNullOrWhiteSpace();
        diffs.Should().OnlyContain(d =>
            d.OtherQuote != null
            && !d.FirstQuote!.Contains("melhor pessoa", StringComparison.OrdinalIgnoreCase));
        HitlCopy.Footer.Should().Contain("humana");
    }
}
