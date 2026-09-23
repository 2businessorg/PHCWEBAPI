using System.Text.Json;
using Agent.Application.Abstractions;
using Agent.Application.Chat;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Recruitment.Application.DTOs;
using Recruitment.Application.Jobs;
using Recruitment.Application.Options;
using Recruitment.Application.Privacy;
using Recruitment.Application.Scoring;
using Recruitment.Application.Services;
using Recruitment.Domain.Constants;
using Recruitment.Domain.Entities;
using Recruitment.Domain.Repositories;
using Recruitment.Infrastructure.Options;
using Recruitment.Infrastructure.Scoring;
using Shared.Abstractions.DocumentTextExtraction;
using Shared.Abstractions.Privacy.Pseudonymization;
using Xunit;

namespace Recruitment.Application.Tests.Scoring;

public class QwenCloudScoreTests
{
    private const string RawSecret = "SEGREDO-RAW-911 maria@example.com";
    private const string Pseudo = "Candidato {{PERSON_ab12}} com SQL e PHC em Maputo.";
    private const string Quote = "SQL e PHC";

    [Fact]
    public void Selector_CloudOn_ResolvesQwen_CloudOff_ResolvesRubric()
    {
        var llm = new StubLlm("qwen3-coder-next");
        var qwen = new QwenCloudScoreEngine(llm);
        var rubric = new RubricCandidateScoreEngine(new RubricEvidenceScorer());

        var on = new CandidateScoreEngineSelector(
            Microsoft.Extensions.Options.Options.Create(new RecruitmentIaOptions { EnableCloudLlm = true }),
            rubric,
            qwen);
        var off = new CandidateScoreEngineSelector(
            Microsoft.Extensions.Options.Options.Create(new RecruitmentIaOptions { EnableCloudLlm = false }),
            rubric,
            qwen);

        on.CloudEnabled.Should().BeTrue();
        on.Resolve().Should().BeSameAs(qwen);
        off.CloudEnabled.Should().BeFalse();
        off.Resolve().Should().BeSameAs(rubric);
    }

    [Fact]
    public async Task QwenEngine_SetsUsedLlm_AndQuoteMustBelongToPseudonymizedText()
    {
        var llm = new StubLlm("qwen3-coder-next")
        {
            Response = CloudJson(
                """{"code":"CRT-STACK","note":20,"weight":99,"quote":"SQL e PHC","justificationPt":"Evidencia de stack no texto.","conflito":false}""")
        };
        var engine = new QwenCloudScoreEngine(llm);
        var criteria = new List<RctCriterion>
        {
            new() { Code = "CRT-STACK", Label = "Stack", Weight = 25, EvidenceHints = ["PHC"] }
        };

        var score = await engine.ScoreAsync(Pseudo, criteria, CancellationToken.None);

        score.UsedLlm.Should().BeTrue();
        score.EngineName.Should().Be("qwen3-coder-next");
        score.PromptVer.Should().Be(QwenCloudScoreEngine.PromptVer);
        score.AssistedDecision.Should().Be(AssistedDecisions.Avancar);
        score.Breakdown.Single().Quote.Should().Be(Quote);
        score.Breakdown.Single().Weight.Should().Be(25);
        score.RationalePt.Should().Be("Ajuste assistido com base nas citacoes.");
        score.StrengthsPt.Should().ContainSingle().Which.Should().Be("SQL e PHC citados no texto.");
        score.InterviewValidationQuestionPt.Should().Be("Que modulos PHC usou no ultimo projecto?");
        llm.LastUser.Should().Contain(Pseudo);
        llm.LastUser.Should().NotContain(RawSecret);
    }

    [Theory]
    [InlineData("avancar", "avancar")]
    [InlineData("em_duvida", "em_duvida")]
    [InlineData("nao_avancar", "nao_avancar")]
    [InlineData("shortlist_suggest", null)]
    [InlineData("interview_suggest", null)]
    [InlineData("weak_fit_suggest", null)]
    [InlineData("insufficient_evidence", null)]
    [InlineData("conflict_review", null)]
    [InlineData("hire", null)]
    [InlineData("reject", null)]
    [InlineData("selected", null)]
    [InlineData("rejected", null)]
    [InlineData("advanced", null)]
    [InlineData("hired", null)]
    [InlineData("approved", null)]
    [InlineData("auto_advance", null)]
    [InlineData("pass", null)]
    [InlineData("fail", null)]
    [InlineData("entrevista", null)]
    [InlineData("mais_info", null)]
    [InlineData("nao_recomendar_assistido", null)]
    public void Parser_KeepsOnlyThreeDecisionValues(string raw, string? expected)
    {
        var json = $$"""{"recommendation":{"decision":"{{raw}}"},"criteria":[]}""";
        QwenCloudScoreEngine.ReadAllowedDecision(json).Should().Be(expected);
    }

    [Fact]
    public async Task QwenEngine_KilledDecisionToken_IsNotThePersistedSuggestion()
    {
        var llm = new StubLlm("qwen3-coder-next")
        {
            Response = CloudJson(
                """{"code":"CRT-STACK","note":20,"quote":"SQL e PHC","justificationPt":"Evidencia de stack no texto.","conflito":false}""",
                "hire")
        };
        var engine = new QwenCloudScoreEngine(llm);
        var score = await engine.ScoreAsync(
            Pseudo,
            [new RctCriterion { Code = "CRT-STACK", Label = "Stack", Weight = 25 }],
            CancellationToken.None);

        score.AssistedDecision.Should().Be(AssistedDecisions.Avancar);
        AssistedDecisions.IsAllowed(score.AssistedDecision).Should().BeTrue();
    }

    [Fact]
    public void Recommendation_ConflictOrNoEvidence_IsEmDuvida_WeakFit_IsNaoAvancar()
    {
        var conflict = AssistedRecommendationBuilder.Build(
        [
            Row("CRT-A", 25, 20, Quote, conflito: true)
        ]);
        conflict.Decision.Should().Be(AssistedDecisions.EmDuvida);
        conflict.Criteria.Single().Status.Should().Be(AssistedDecisions.StatusConflict);
        conflict.Conflicts.Single().Code.Should().Be("CRT-A");
        conflict.Conflicts.Single().Quotes.Should().Contain(Quote);
        conflict.LabelPt.Should().Be("Sugestão: em dúvida — decisão humana obrigatória");
        conflict.DecisionNote.Should().Be(HitlCopy.AssistedDisclaimerPt);

        var missing = AssistedRecommendationBuilder.Build(
        [
            Row("CRT-A", 25, 0, null, conflito: false)
        ]);
        missing.Decision.Should().Be(AssistedDecisions.EmDuvida);
        missing.Criteria.Single().Status.Should().Be(AssistedDecisions.StatusNoEvidence);
        missing.GapsPt.Should().NotBeEmpty();

        var weak = AssistedRecommendationBuilder.Build(
        [
            Row("CRT-A", 25, 8, Quote, conflito: false)
        ]);
        weak.Decision.Should().Be(AssistedDecisions.NaoAvancar);
        weak.Criteria.Single().Status.Should().Be(AssistedDecisions.StatusEvidenced);
        weak.LabelPt.Should().Be("Sugestão: não avançar — decisão humana obrigatória");
        weak.Criteria.Single().WeightSource.Should().Be("rct");

        var strong = AssistedRecommendationBuilder.Build(
        [
            Row("CRT-A", 25, 20, Quote, conflito: false)
        ]);
        strong.Decision.Should().Be(AssistedDecisions.Avancar);
        strong.LabelPt.Should().Be("Sugestão: avançar — decisão humana obrigatória");
    }

    private static CriterionScoreBreakdown Row(string code, decimal weight, decimal note, string? quote, bool conflito) =>
        new()
        {
            Code = code,
            Label = code,
            Weight = weight,
            Note = note,
            Quote = quote,
            SemEvidencia = note <= 0,
            Conflito = conflito,
            ConflictQuotes = conflito && quote is not null ? new[] { quote } : Array.Empty<string>(),
            JustificationPt = note <= 0 ? HitlCopy.SemEvidencia : "Evidencia citada."
        };

    [Fact]
    public async Task QwenEngine_MissingStrengths_FailsClosed()
    {
        var llm = new StubLlm("qwen3-coder-next")
        {
            Response = """{"criteria":[{"code":"CRT-STACK","note":20,"quote":"SQL e PHC","justificationPt":"Evidencia de stack no texto."}],"recommendation":{"decision":"avancar","rationalePt":"Texto curto."},"interviewValidationQuestionPt":"Qual o modulo?"}"""
        };
        var engine = new QwenCloudScoreEngine(llm);
        var act = async () => await engine.ScoreAsync(
            Pseudo,
            [new RctCriterion { Code = "CRT-STACK", Label = "Stack", Weight = 25 }],
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*strengthsPt*");
    }

    [Fact]
    public async Task QwenJson_Roundtrip_KeepsScorecardAndAssistedDecision()
    {
        var llm = new StubLlm("qwen3-coder-next")
        {
            Response = CloudJson(
                """{"code":"CRT-STACK","note":20,"weight":99,"quote":"SQL e PHC","justificationPt":"Evidencia de stack no texto.","conflito":false}""")
        };
        var engine = new QwenCloudScoreEngine(llm);
        var score = await engine.ScoreAsync(
            Pseudo,
            [new RctCriterion { Code = "CRT-STACK", Label = "Stack", Weight = 25 }],
            CancellationToken.None);

        var payload = ScorecardComposer.Compose(
            score,
            "SRT2",
            DocumentTextExtractionResult.FromNative(Pseudo, "test"),
            new CloudEgressPreparation(true, true, Pseudo, null, new PseudonymizationResult(
                Pseudo,
                "SRT2",
                "ANX2",
                [new DetectedEntitySummary("PERSON", "{{PERSON_ab12}}", 0.9f, "presidio", 0, 8)],
                LeakCheckResult.Ok(),
                true,
                null)));

        var json = JsonSerializer.Serialize(payload);
        var back = JsonSerializer.Deserialize<JustificationPayloadDto>(json);

        back.Should().NotBeNull();
        back!.UsedLlm.Should().BeTrue();
        back.TotalScore.Should().Be(20);
        back.Recommendation!.Decision.Should().Be(AssistedDecisions.Avancar);
        back.ScoreIsInputNotDecision.Should().BeTrue();
        back.HumanDecisionRequired.Should().BeTrue();
        back.Recommendation.RationalePt.Should().Be("Ajuste assistido com base nas citacoes.");
        back.StrengthsPt.Should().ContainSingle();
        back.InterviewValidationQuestionPt.Should().NotBeNullOrWhiteSpace();
        back.Labels.HumanDecisionRequired.Should().BeTrue();
        back.Labels.PiiExcludedFromScore.Should().BeTrue();
        back.Criteria.Single().MaxWeight.Should().Be(25);
        back.Criteria.Single().WeightSource.Should().Be("rct");
        back.Readiness!.PiiTypesExcluded.Should().Contain("PERSON");
        back.CandidateAlias.Should().Be(ScorecardComposer.CandidateAlias("SRT2"));
        back.CandidateAlias.Should().NotContain("maria");
        json.Should().NotContain(RawSecret);
    }

    [Fact]
    public async Task QwenEngine_QuoteOutsidePseudonymizedText_ThrowsAh02()
    {
        var llm = new StubLlm("qwen3-coder-next")
        {
            Response = """{"criteria":[{"code":"CRT-STACK","note":10,"quote":"frase inventada","justificationPt":"ok"}]}"""
        };
        var engine = new QwenCloudScoreEngine(llm);

        var act = async () => await engine.ScoreAsync(
            Pseudo,
            [new RctCriterion { Code = "CRT-STACK", Label = "Stack", Weight = 25 }],
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*AH-02*");
    }

    [Fact]
    public async Task Adapter_MissingApiKey_DoesNotCallChatModel()
    {
        var chat = new Mock<ILocalChatModel>(MockBehavior.Strict);
        chat.SetupGet(x => x.ModelName).Returns("qwen3-coder-next");
        var client = new LocalChatRecruitmentLlmClient(
            chat.Object,
            Microsoft.Extensions.Options.Options.Create(new RecruitmentLocalAiOptions { ApiKey = "  ", Model = "qwen3-coder-next" }));

        var act = async () => await client.CompleteAsync("sys", "user", CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*ApiKey*");
        chat.Verify(x => x.SendAsync(
            It.IsAny<IReadOnlyCollection<ChatMessage>>(),
            It.IsAny<IReadOnlyCollection<ToolDefinition>>(),
            It.IsAny<CancellationToken>(),
            It.IsAny<ChatCompletionOptions?>()), Times.Never);
    }

    [Fact]
    public async Task Adapter_ForwardsPromptToILocalChatModel()
    {
        string? seen = null;
        var chat = new Mock<ILocalChatModel>();
        chat.SetupGet(x => x.ModelName).Returns("qwen3-coder-next");
        chat.Setup(x => x.SendAsync(
                It.IsAny<IReadOnlyCollection<ChatMessage>>(),
                It.IsAny<IReadOnlyCollection<ToolDefinition>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<ChatCompletionOptions?>()))
            .Callback<IReadOnlyCollection<ChatMessage>, IReadOnlyCollection<ToolDefinition>, CancellationToken, ChatCompletionOptions?>(
                (messages, _, _, _) => seen = string.Join("\n", messages.Select(m => m.Content)))
            .ReturnsAsync(new ChatModelResponse("{}", Array.Empty<ToolCall>()));

        var client = new LocalChatRecruitmentLlmClient(
            chat.Object,
            Microsoft.Extensions.Options.Options.Create(new RecruitmentLocalAiOptions { ApiKey = "unit-test-key", Model = "qwen3-coder-next" }));

        var content = await client.CompleteAsync("sistema", Pseudo, CancellationToken.None);

        content.Should().Be("{}");
        client.ModelName.Should().Be("qwen3-coder-next");
        seen.Should().Contain(Pseudo);
        seen.Should().NotContain(RawSecret);
    }

    [Fact]
    public async Task Job_CloudEgressRefused_FailsClosed_DoesNotScore()
    {
        var rubric = new Mock<IRubricEvidenceScorer>(MockBehavior.Strict);
        var cloud = new Mock<IRecruitmentLlmClient>(MockBehavior.Strict);
        var (job, srt, cve) = CreateCloudJob(
            rubric.Object,
            cloud.Object,
            egressAllowed: false,
            failure: "Leak checker failed (fail-closed)");

        await job.ExecuteAsync(7, CancellationToken.None);

        cve.Verify(x => x.SetEstadoIaAsync("CVE2", IaEstados.Erro, It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        srt.Verify(x => x.ClearScoreOnOcrErrorAsync("SRT2", It.IsAny<CancellationToken>()), Times.Once);
        srt.Verify(x => x.SaveScoreAsync(
            It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>(),
            It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<DateTime>(), It.IsAny<string?>(),
            It.IsAny<CancellationToken>()), Times.Never);
        rubric.Verify(x => x.Score(It.IsAny<string?>(), It.IsAny<IReadOnlyList<RctCriterion>>()), Times.Never);
        cloud.Verify(x => x.CompleteAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Job_CloudChatThrows_DoesNotFallBackToRubric()
    {
        var rubric = new Mock<IRubricEvidenceScorer>(MockBehavior.Strict);
        var cloud = new Mock<IRecruitmentLlmClient>();
        cloud.SetupGet(x => x.ModelName).Returns("qwen3-coder-next");
        cloud.Setup(x => x.CompleteAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Qwen returned 401: unauthorized"));

        var (job, srt, cve) = CreateCloudJob(rubric.Object, cloud.Object, egressAllowed: true, failure: null);

        await job.ExecuteAsync(7, CancellationToken.None);

        cve.Verify(x => x.SetEstadoIaAsync("CVE2", IaEstados.Erro, It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        srt.Verify(x => x.SaveScoreAsync(
            It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>(),
            It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<DateTime>(), It.IsAny<string?>(),
            It.IsAny<CancellationToken>()), Times.Never);
        rubric.Verify(x => x.Score(It.IsAny<string?>(), It.IsAny<IReadOnlyList<RctCriterion>>()), Times.Never);
    }

    [Fact]
    public async Task Job_CloudPath_PersistsUsedLlm_AndAnonymizationAuditWithoutCvBody()
    {
        var rubric = new Mock<IRubricEvidenceScorer>(MockBehavior.Strict);
        var cloud = new Mock<IRecruitmentLlmClient>();
        cloud.SetupGet(x => x.ModelName).Returns("qwen3-coder-next");
        string? userPrompt = null;
        cloud.Setup(x => x.CompleteAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<string, string, CancellationToken>((_, user, _) => userPrompt = user)
            .ReturnsAsync(CloudJson(
                """{"code":"CRT-STACK","note":18,"weight":99,"quote":"SQL e PHC","justificationPt":"Stack citada no texto pseudonimizado."}"""));

        string? audit = null;
        string? engine = null;
        string? justification = null;
        var (job, srt, _) = CreateCloudJob(
            rubric.Object,
            cloud.Object,
            egressAllowed: true,
            failure: null,
            onSave: (savedJson, savedEngine, savedAudit) =>
            {
                justification = savedJson;
                engine = savedEngine;
                audit = savedAudit;
            });

        await job.ExecuteAsync(7, CancellationToken.None);

        engine.Should().Be("qwen3-coder-next");
        audit.Should().NotBeNull();
        audit.Should().Contain("\"used_llm\":true");
        audit.Should().Contain("\"egress_allowed\":true");
        audit.Should().Contain("\"entity_count\":2");
        audit.Should().Contain("PERSON");
        audit.Should().Contain("EMAIL_ADDRESS");
        audit.Should().Contain("\"leak_check_passed\":true");
        audit.Should().Contain("qwen-cloud-v5");
        audit.Should().Contain("\"modelName\":\"qwen3-coder-next\"");
        audit.Should().Contain("\"usedLlm\":true");
        audit.Should().Contain("\"entityTypeCounts\"");
        audit.Should().Contain("job_stamp_utc");
        audit.Should().Contain("ocr_stamp_utc");
        audit.Should().NotContain(RawSecret);
        audit.Should().NotContain(Pseudo);
        audit.Should().NotContain("maria@example.com");
        userPrompt.Should().Contain(Pseudo);
        userPrompt.Should().NotContain(RawSecret);
        justification.Should().Contain("\"decision\":\"avancar\"");
        justification.Should().Contain("Sugestão: avançar — decisão humana obrigatória");
        justification.Should().Contain(HitlCopy.AssistedDisclaimerPt);
        justification.Should().NotContain("shortlist_suggest");
        justification.Should().NotContain("interview_suggest");
        justification.Should().NotContain("weak_fit_suggest");
        justification.Should().NotContain("insufficient_evidence");
        justification.Should().NotContain("conflict_review");
        justification.Should().NotContain("\"decision\":\"selected\"");
        justification.Should().NotContain("\"decision\":\"hired\"");
        justification.Should().Contain("\"humanDecisionRequired\":true");
        justification.Should().Contain("\"scoreIsInputNotDecision\":true");
        justification.Should().Contain("\"status\":\"evidenced\"");
        justification.Should().Contain("\"weightSource\":\"rct\"");
        justification.Should().Contain("perfil pseudonimizado");
        justification.Should().Contain("\"totalScore\":18");
        justification.Should().Contain("\"maxWeight\":25");
        justification.Should().Contain("\"criteriaWithEvidence\":\"1/1\"");
        justification.Should().Contain("\"piiExcludedFromScore\":true");
        justification.Should().Contain("SQL e PHC citados no texto.");
        justification.Should().Contain("Que modulos PHC usou no ultimo projecto?");
        justification.Should().Contain("Ajuste assistido com base nas citacoes.");
        justification.Should().NotContain("shortlist_suggest");
        justification.Should().NotContain("hire");
        justification.Should().NotContain("\"decision\":\"entrevista\"");
        justification.Should().NotContain("\"weight\":99");
        justification.Should().NotContain(RawSecret);
        rubric.Verify(x => x.Score(It.IsAny<string?>(), It.IsAny<IReadOnlyList<RctCriterion>>()), Times.Never);
    }

    [Fact]
    public async Task Ranking_LlmQuote_IsNotReCheckedAgainstRawUTexto()
    {
        var srt = new Mock<ISrtScoreRepository>();
        srt.Setup(x => x.GetByRctAsync("RCT-Q", It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new SrtCandidateRow
                {
                    SrtStamp = "SRT-Q",
                    CveStamp = "CVE-Q",
                    RctStamp = "RCT-Q",
                    CandidateName = "Ana",
                    ScoreIa = 18,
                    Condp = "humano",
                    SelectionStateSnapshot = "aberto|0|0",
                    JustificationJson =
                        $$"""{"used_llm":true,"breakdown":[{"code":"CRT-STACK","label":"Stack","note":18,"quote":"{{Quote}}"}]}"""
                }
            ]);
        var anexos = new Mock<ICvAnexoRepository>(MockBehavior.Strict);
        anexos.Setup(x => x.GetCvAnexoAsync("CVE-Q", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CvAnexo
            {
                AnexoStamp = "ANX",
                CveStamp = "CVE-Q",
                Bytes = [1],
                Texto = RawSecret
            });

        var handler = new Recruitment.Application.Features.GetRctRanking.GetRctRankingQueryHandler(srt.Object, anexos.Object);
        var dto = await handler.Handle(
            new Recruitment.Application.Features.GetRctRanking.GetRctRankingQuery("RCT-Q"),
            CancellationToken.None);

        dto.Candidates.Single().Breakdown.Single().Quote.Should().Be(Quote);
        dto.Candidates.Single().Condp.Should().Be("humano");
        anexos.Verify(x => x.GetCvAnexoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        srt.Verify(x => x.SaveScoreAsync(
            It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>(),
            It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<DateTime>(), It.IsAny<string?>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    private static (AnalyzeCandidateJob Job, Mock<ISrtScoreRepository> Srt, Mock<ICveEstadoIaRepository> Cve) CreateCloudJob(
        IRubricEvidenceScorer rubric,
        IRecruitmentLlmClient llm,
        bool egressAllowed,
        string? failure,
        Action<string, string?, string?>? onSave = null)
    {
        var outboxId = 7L;
        var item = new RecruitmentOutboxItem
        {
            Id = outboxId,
            CveStamp = "CVE2",
            RctStamp = "RCT2",
            SrtStamp = "SRT2",
            AnexoStamp = "ANX2",
            Estado = OutboxEstados.Pending,
            CreatedAtUtc = DateTime.UtcNow
        };

        var outbox = new Mock<IRecruitmentOutboxRepository>();
        outbox.Setup(x => x.GetAsync(outboxId, It.IsAny<CancellationToken>())).ReturnsAsync(item);

        var anexos = new Mock<ICvAnexoRepository>();
        anexos.Setup(x => x.GetCvAnexoAsync("CVE2", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CvAnexo
            {
                AnexoStamp = "ANX2",
                CveStamp = "CVE2",
                Bytes = [9, 9],
                FileName = "cv.pdf",
                Texto = RawSecret
            });

        var srt = new Mock<ISrtScoreRepository>();
        srt.Setup(x => x.GetByStampAsync("SRT2", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SrtCandidateRow
            {
                SrtStamp = "SRT2",
                CveStamp = "CVE2",
                RctStamp = "RCT2",
                Condp = "condp-nativo",
                SelectionStateSnapshot = "em-analise|0|nao"
            });
        if (onSave is not null)
        {
            srt.Setup(x => x.SaveScoreAsync(
                    "SRT2",
                    It.IsAny<decimal>(),
                    It.IsAny<string>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<string?>(),
                    It.IsAny<CancellationToken>()))
                .Callback<string, decimal, string, string?, string?, DateTime, string?, CancellationToken>(
                    (_, _, json, modelo, _, _, audit, _) => onSave(json, modelo, audit))
                .Returns(Task.CompletedTask);
        }

        var cve = new Mock<ICveEstadoIaRepository>();
        var criteria = new Mock<IRctCriteriaRepository>();
        criteria.Setup(x => x.GetUsableCriteriaAsync("RCT2", It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new RctCriterion { Code = "CRT-STACK", Label = "Stack", Weight = 25, EvidenceHints = ["PHC"] }
            ]);
        var intervenientes = new Mock<IRctIntervenienteRepository>();
        intervenientes.Setup(x => x.GetIntervenientesAsync("RCT2", It.IsAny<CancellationToken>()))
            .ReturnsAsync([new RctInterveniente { UserId = "9" }]);

        var extractor = new Mock<IDocumentTextExtractor>();
        extractor.Setup(x => x.ExtractAsync(
                It.IsAny<Stream>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(DocumentTextExtractionResult.FromNative(RawSecret, "test"));

        var entities = new DetectedEntitySummary[]
        {
            new("PERSON", "{{PERSON_ab12}}", 0.9f, "presidio", 0, 8),
            new("EMAIL_ADDRESS", "{{EMAIL_ADDRESS_ff}}", 0.95f, "presidio", 9, 20)
        };
        var result = new PseudonymizationResult(
            Pseudo,
            "SRT2",
            "ANX2",
            entities,
            LeakCheckResult.Ok(),
            egressAllowed,
            failure);
        var egress = new FixedEgress(new CloudEgressPreparation(
            EnableCloudLlm: true,
            EgressAllowed: egressAllowed,
            PseudonymizedText: egressAllowed ? Pseudo : null,
            FailureReason: failure,
            Result: result));

        var selector = new CandidateScoreEngineSelector(
            Microsoft.Extensions.Options.Options.Create(new RecruitmentIaOptions { EnableCloudLlm = true }),
            new RubricCandidateScoreEngine(rubric),
            new QwenCloudScoreEngine(llm));

        var job = new AnalyzeCandidateJob(
            outbox.Object,
            anexos.Object,
            criteria.Object,
            intervenientes.Object,
            srt.Object,
            cve.Object,
            extractor.Object,
            selector,
            Mock.Of<IPhcAvisoService>(),
            Microsoft.Extensions.Options.Options.Create(new RecruitmentIaOptions { EnableCloudLlm = true, QueueHostName = "test-host" }),
            egress,
            NullLogger<AnalyzeCandidateJob>.Instance);

        return (job, srt, cve);
    }

    private sealed class FixedEgress : IRecruitmentCloudEgressGuard
    {
        private readonly CloudEgressPreparation _prep;

        public FixedEgress(CloudEgressPreparation prep) => _prep = prep;

        public Task<CloudEgressPreparation> PrepareAsync(
            string sessionId,
            string documentId,
            string plainText,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(_prep);
    }

    private static string CloudJson(string criterionObject, string decision = "avancar") =>
        $$"""
        {"criteria":[{{criterionObject}}],"recommendation":{"decision":"{{decision}}","rationalePt":"Ajuste assistido com base nas citacoes."},"strengthsPt":["SQL e PHC citados no texto."],"interviewValidationQuestionPt":"Que modulos PHC usou no ultimo projecto?"}
        """;

    private sealed class StubLlm : IRecruitmentLlmClient
    {
        public StubLlm(string modelName) => ModelName = modelName;

        public string ModelName { get; }

        public string Response { get; init; } = "{}";

        public string? LastUser { get; private set; }

        public Task<string> CompleteAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken)
        {
            LastUser = userPrompt;
            return Task.FromResult(Response);
        }
    }
}
