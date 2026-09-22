using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Recruitment.Application.Jobs;
using Recruitment.Application.Options;
using Recruitment.Application.Scoring;
using Recruitment.Application.Services;
using Recruitment.Domain.Constants;
using Recruitment.Domain.Entities;
using Recruitment.Domain.Repositories;
using Shared.Abstractions.DocumentTextExtraction;
using Xunit;

namespace Recruitment.Application.Tests.Services;

public class AnalyzeCandidateJobTests
{
    [Fact]
    public async Task Execute_OcrFailure_SetsErro_NoScorePersisted()
    {
        var outboxId = 42L;
        var item = new RecruitmentOutboxItem
        {
            Id = outboxId,
            CveStamp = "CVE1",
            RctStamp = "RCT1",
            SrtStamp = "SRT1",
            AnexoStamp = "ANX1",
            Estado = OutboxEstados.Pending,
            CreatedAtUtc = DateTime.UtcNow
        };

        var outbox = new Mock<IRecruitmentOutboxRepository>();
        outbox.Setup(x => x.GetAsync(outboxId, It.IsAny<CancellationToken>())).ReturnsAsync(item);

        var anexos = new Mock<ICvAnexoRepository>();
        anexos.Setup(x => x.GetCvAnexoAsync("CVE1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CvAnexo
            {
                AnexoStamp = "ANX1",
                CveStamp = "CVE1",
                Bytes = [1, 2, 3],
                FileName = "cv.pdf"
            });

        var srt = new Mock<ISrtScoreRepository>();
        srt.Setup(x => x.GetByStampAsync("SRT1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SrtCandidateRow
            {
                SrtStamp = "SRT1",
                CveStamp = "CVE1",
                RctStamp = "RCT1",
                Condp = "nativo",
                SelectionStateSnapshot = "aberto|0|0"
            });

        var cve = new Mock<ICveEstadoIaRepository>();
        var criteria = new Mock<IRctCriteriaRepository>();
        var intervenientes = new Mock<IRctIntervenienteRepository>();
        intervenientes.Setup(x => x.GetIntervenientesAsync("RCT1", It.IsAny<CancellationToken>()))
            .ReturnsAsync([new RctInterveniente { UserId = "1", DisplayName = "RH" }]);

        var extractor = new Mock<IDocumentTextExtractor>();
        extractor.Setup(x => x.ExtractAsync(
                It.IsAny<Stream>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(DocumentTextExtractionResult.Failure("OCR engine down"));

        var avisos = new Mock<IPhcAvisoService>();
        var scorer = new Mock<IRubricEvidenceScorer>();

        var job = CreateJob(outbox, anexos, criteria, intervenientes, srt, cve, extractor, scorer.Object, avisos);

        await job.ExecuteAsync(outboxId, CancellationToken.None);

        cve.Verify(x => x.SetEstadoIaAsync("CVE1", IaEstados.Erro, It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        srt.Verify(x => x.ClearScoreOnOcrErrorAsync("SRT1", It.IsAny<CancellationToken>()), Times.Once);
        srt.Verify(x => x.SaveScoreAsync(
            It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>(),
            It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<DateTime>(), It.IsAny<string?>(),
            It.IsAny<CancellationToken>()), Times.Never);
        scorer.Verify(x => x.Score(It.IsAny<string?>(), It.IsAny<IReadOnlyList<RctCriterion>>()), Times.Never);
        avisos.Verify(x => x.EmitAnalisarCompletedAsync(
            "RCT1", "CVE1", IaEstados.Erro, It.IsAny<IReadOnlyList<RctInterveniente>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Execute_Success_WritesScoreToSrt_LeavesSelectionAndCondpUnchanged()
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

        var cvText = "Suporte PHC SQL Excel Maputo 5 anos.";
        var criteriaList = new List<RctCriterion>
        {
            new()
            {
                Code = "CRT-STACK",
                Label = "Stack",
                Weight = 25,
                EvidenceHints = ["PHC", "SQL"]
            }
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
                FileName = "cv.pdf"
            });

        var row = new SrtCandidateRow
        {
            SrtStamp = "SRT2",
            CveStamp = "CVE2",
            RctStamp = "RCT2",
            Condp = "condp-nativo",
            SelectionStateSnapshot = "em-analise|0|nao"
        };

        var srt = new Mock<ISrtScoreRepository>();
        srt.Setup(x => x.GetByStampAsync("SRT2", It.IsAny<CancellationToken>())).ReturnsAsync(row);

        var cve = new Mock<ICveEstadoIaRepository>();
        var criteria = new Mock<IRctCriteriaRepository>();
        criteria.Setup(x => x.GetUsableCriteriaAsync("RCT2", It.IsAny<CancellationToken>()))
            .ReturnsAsync(criteriaList);

        var intervenientes = new Mock<IRctIntervenienteRepository>();
        intervenientes.Setup(x => x.GetIntervenientesAsync("RCT2", It.IsAny<CancellationToken>()))
            .ReturnsAsync([new RctInterveniente { UserId = "9" }]);

        var extractor = new Mock<IDocumentTextExtractor>();
        extractor.Setup(x => x.ExtractAsync(
                It.IsAny<Stream>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(DocumentTextExtractionResult.FromNative(cvText, "test"));

        var realScorer = new RubricEvidenceScorer();
        var avisos = new Mock<IPhcAvisoService>();

        var job = CreateJob(outbox, anexos, criteria, intervenientes, srt, cve, extractor, realScorer, avisos);

        await job.ExecuteAsync(outboxId, CancellationToken.None);

        srt.Verify(x => x.SaveScoreAsync(
            "SRT2",
            It.Is<decimal>(d => d > 0),
            It.Is<string>(j => j.Contains("CRT-STACK") && !ForbiddenCopyGuard.ContainsForbiddenPhrase(j)),
            RubricEvidenceScorer.EngineName,
            RubricEvidenceScorer.PromptVer,
            It.IsAny<DateTime>(),
            It.IsAny<string?>(),
            It.IsAny<CancellationToken>()), Times.Once);

        cve.Verify(x => x.SetEstadoIaAsync("CVE2", IaEstados.Ok, It.IsAny<CancellationToken>()), Times.AtLeastOnce);

        // BR-03/04: repository SaveScore is the only write; GetByStamp used for assert keeps same snapshot
        srt.Verify(x => x.GetByStampAsync("SRT2", It.IsAny<CancellationToken>()), Times.AtLeast(2));
        row.Condp.Should().Be("condp-nativo");
        row.SelectionStateSnapshot.Should().Be("em-analise|0|nao");
    }

    private static AnalyzeCandidateJob CreateJob(
        Mock<IRecruitmentOutboxRepository> outbox,
        Mock<ICvAnexoRepository> anexos,
        Mock<IRctCriteriaRepository> criteria,
        Mock<IRctIntervenienteRepository> intervenientes,
        Mock<ISrtScoreRepository> srt,
        Mock<ICveEstadoIaRepository> cve,
        Mock<IDocumentTextExtractor> extractor,
        IRubricEvidenceScorer scorer,
        Mock<IPhcAvisoService> avisos)
    {
        return new AnalyzeCandidateJob(
            outbox.Object,
            anexos.Object,
            criteria.Object,
            intervenientes.Object,
            srt.Object,
            cve.Object,
            extractor.Object,
            scorer,
            avisos.Object,
            Microsoft.Extensions.Options.Options.Create(new RecruitmentIaOptions { EnableCloudLlm = false }),
            NullLogger<AnalyzeCandidateJob>.Instance);
    }
}

public class RecruitmentEnqueueServiceTests
{
    [Fact]
    public async Task TryEnqueue_NoCriteria_DoesNotCreateOutbox()
    {
        var criteria = new Mock<IRctCriteriaRepository>();
        criteria.Setup(x => x.CountUsableCriteriaAsync("RCT", It.IsAny<CancellationToken>())).ReturnsAsync(0);

        var intervenientes = new Mock<IRctIntervenienteRepository>();
        intervenientes.Setup(x => x.CountIntervenientesAsync("RCT", It.IsAny<CancellationToken>())).ReturnsAsync(2);

        var outbox = new Mock<IRecruitmentOutboxRepository>(MockBehavior.Strict);
        var sut = CreateSut(criteria, intervenientes, outbox);

        var result = await sut.TryEnqueueAsync("CVE", "RCT", "SRT");

        result.Enqueued.Should().BeFalse();
        result.CriteriaCount.Should().Be(0);
    }

    [Fact]
    public async Task TryEnqueue_NoRctclb_DoesNotCreateOutbox()
    {
        var criteria = new Mock<IRctCriteriaRepository>();
        criteria.Setup(x => x.CountUsableCriteriaAsync("RCT", It.IsAny<CancellationToken>())).ReturnsAsync(3);

        var intervenientes = new Mock<IRctIntervenienteRepository>();
        intervenientes.Setup(x => x.CountIntervenientesAsync("RCT", It.IsAny<CancellationToken>())).ReturnsAsync(0);

        var outbox = new Mock<IRecruitmentOutboxRepository>(MockBehavior.Strict);
        var sut = CreateSut(criteria, intervenientes, outbox);

        var result = await sut.TryEnqueueAsync("CVE", "RCT", "SRT");

        result.Enqueued.Should().BeFalse();
        result.IntervenienteCount.Should().Be(0);
    }

    private static RecruitmentEnqueueService CreateSut(
        Mock<IRctCriteriaRepository> criteria,
        Mock<IRctIntervenienteRepository> intervenientes,
        Mock<IRecruitmentOutboxRepository> outbox)
    {
        return new RecruitmentEnqueueService(
            criteria.Object,
            intervenientes.Object,
            Mock.Of<ICvAnexoRepository>(),
            outbox.Object,
            Mock.Of<ICveEstadoIaRepository>(),
            Microsoft.Extensions.Options.Options.Create(new RecruitmentIaOptions()),
            NullLogger<RecruitmentEnqueueService>.Instance);
    }
}
