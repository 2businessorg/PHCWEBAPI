using FluentAssertions;
using Recruitment.Application.Scoring;
using Recruitment.Domain.Constants;
using Recruitment.Domain.Entities;
using Xunit;

namespace Recruitment.Application.Tests.Scoring;

public class RubricEvidenceScorerTests
{
    private readonly RubricEvidenceScorer _sut = new();

    private static IReadOnlyList<RctCriterion> LabCriteria() =>
    [
        new()
        {
            Code = "CRT-STACK",
            Label = "Stack",
            Weight = 25,
            EvidenceHints = ["PHC", "SQL", "Excel"]
        },
        new()
        {
            Code = "CRT-EXP",
            Label = "Experiencia",
            Weight = 25,
            EvidenceHints = ["suporte", "ERP", "anos"]
        },
        new()
        {
            Code = "CRT-MZ",
            Label = "Maputo",
            Weight = 10,
            EvidenceHints = ["Maputo", "presencial"]
        }
    ];

    [Fact]
    public void Score_NoCitationForCriterion_ContributionIsZero()
    {
        // Alfredo DTTest: CV without skill X must not score high on X
        var cv = "Candidato com experiencia em vendas e marketing digital em Lisboa.";

        var result = _sut.Score(cv, LabCriteria());

        var stack = result.Breakdown.Single(b => b.Code == "CRT-STACK");
        stack.Note.Should().Be(0);
        stack.SemEvidencia.Should().BeTrue();
        stack.Quote.Should().BeNull();
        stack.JustificationPt.Should().Be(HitlCopy.SemEvidencia);
    }

    [Fact]
    public void Score_WithEvidence_QuoteIsSubsetOfUTexto()
    {
        var cv = "Tecnico de suporte ERP PHC com 5 anos. SQL e Excel diarios. Reside em Maputo.";

        var result = _sut.Score(cv, LabCriteria());

        foreach (var row in result.Breakdown.Where(b => b.Note > 0))
        {
            row.Quote.Should().NotBeNullOrWhiteSpace();
            RubricEvidenceScorer.AssertQuoteIsSubset(cv, row.Quote);
            cv.Should().Contain(row.Quote!);
        }

        result.TotalScore.Should().BeGreaterThan(0);
        result.EngineName.Should().Be(RubricEvidenceScorer.EngineName);
        result.PromptVer.Should().Be(RubricEvidenceScorer.PromptVer);
        result.UsedLlm.Should().BeFalse();
    }

    [Fact]
    public void Score_EmptyCriteria_Throws()
    {
        var act = () => _sut.Score("qualquer texto", Array.Empty<RctCriterion>());
        act.Should().Throw<InvalidOperationException>().WithMessage("*crt*");
    }

    [Fact]
    public void Score_ConflictYears_FlagsConflito_NoSilentAverageCleanScore()
    {
        var cv =
            "Experiencia de 3 anos em suporte ERP. " +
            "Mais tarde refere 10 anos de suporte ERP sÃ©nior.";

        var criteria = new[]
        {
            new RctCriterion
            {
                Code = "CRT-EXP",
                Label = "Experiencia",
                Weight = 25,
                EvidenceHints = ["anos", "suporte", "ERP"]
            }
        };

        var result = _sut.Score(cv, criteria);
        var exp = result.Breakdown.Single();

        // AH-05: conflict flagged; not a silent clean full weight
        exp.Conflito.Should().BeTrue();
        exp.ConflictQuotes.Should().NotBeEmpty();
        exp.Note.Should().BeLessThan(25);
    }

    [Fact]
    public void AssertQuoteIsSubset_InventedQuote_ThrowsAh02()
    {
        var act = () => RubricEvidenceScorer.AssertQuoteIsSubset(
            "texto real do CV",
            "frase inventada que nao existe");

        act.Should().Throw<InvalidOperationException>().WithMessage("*AH-02*");
    }
}

public class ForbiddenCopyGuardTests
{
    [Theory]
    [InlineData("Candidato seleccionado pela IA")]
    [InlineData("Rejeitado pela IA automaticamente")]
    [InlineData("Rejeitado by AI automaticamente")]
    [InlineData("Avancado pela IA para entrevista")]
    [InlineData("Candidato avãnçado pela IA")]
    [InlineData("Candidato avãnçado by AI")]
    public void ContainsForbiddenPhrase_DetectsAh04(string text)
    {
        ForbiddenCopyGuard.ContainsForbiddenPhrase(text).Should().BeTrue();
    }

    [Fact]
    public void ContainsForbiddenPhrase_AccentedAvancado_FailsIfGuardAsciiOnly()
    {
        // Dinis CONDICIONA / Alfredo DTTest: weak ASCII-only guard must fail this case.
        var text = "Candidato avãnçado pela IA";
        ForbiddenCopyGuard.ContainsForbiddenPhrase(text).Should().BeTrue();
        ForbiddenCopyGuard.Normalize(text).Should().Contain("avancado pela ia");
    }

    [Fact]
    public void RankingCopy_IsAllowed()
    {
        ForbiddenCopyGuard.ContainsForbiddenPhrase(HitlCopy.RankingTitle).Should().BeFalse();
        ForbiddenCopyGuard.ContainsForbiddenPhrase(HitlCopy.Footer).Should().BeFalse();
        ForbiddenCopyGuard.ContainsForbiddenPhrase(HitlCopy.PreSelectionNote).Should().BeFalse();
        ForbiddenCopyGuard.ContainsForbiddenPhrase(HitlCopy.AssistedDisclaimerPt).Should().BeFalse();
        HitlCopy.Footer.Should().Contain("Decisão").And.Contain("só humana");
        HitlCopy.RankingTitle.Should().Contain("Ordenação").And.Contain("não é decisão");
        HitlCopy.PreSelectionNote.Should().Contain("pré-selecção").And.Contain("Não é decisão automática");
    }
}