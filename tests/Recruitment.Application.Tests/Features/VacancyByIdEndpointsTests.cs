using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Recruitment.Application.DTOs;
using Recruitment.Application.Features.AnalyzeVacancy;
using Recruitment.Application.Features.EnqueueAnalysis;
using Recruitment.Application.Features.GetRctRanking;
using Recruitment.Application.Features.GetVacancyRanking;
using Recruitment.Application.Features.ReprocessAnalysis;
using Recruitment.Application.Features.ReprocessVacancyCandidate;
using Recruitment.Application.Services;
using Recruitment.Domain.Constants;
using Recruitment.Domain.Entities;
using Recruitment.Domain.Repositories;
using Recruitment.Infrastructure.Options;
using Xunit;

namespace Recruitment.Application.Tests.Features;

public class VacancyByIdEndpointsTests
{
    [Fact]
    public async Task Analyze_ResolvesIdRct_EnqueuesEachSrtWithCv_OneHangfireJobEach()
    {
        var vacancies = new Mock<IRctVacancyRepository>();
        vacancies.Setup(x => x.ResolveStampByIdAsync("42", It.IsAny<CancellationToken>()))
            .ReturnsAsync("RCT-42");

        var withCv = new List<SrtCandidateRow>
        {
            Row("SRT-A", "CVE-A", "RCT-42", score: null),
            Row("SRT-B", "CVE-B", "RCT-42", score: null)
        };

        var srt = new Mock<ISrtScoreRepository>(MockBehavior.Strict);
        srt.Setup(x => x.ListWithCvByRctAsync("RCT-42", It.IsAny<CancellationToken>()))
            .ReturnsAsync(withCv);

        var enqueue = new Mock<IRecruitmentEnqueueService>();
        enqueue.Setup(x => x.TryEnqueueAsync(
                "CVE-A", "RCT-42", "SRT-A", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EnqueueDecision
            {
                Enqueued = true,
                OutboxId = 11,
                Message = "ok",
                CriteriaCount = 2,
                IntervenienteCount = 1
            });
        enqueue.Setup(x => x.TryEnqueueAsync(
                "CVE-B", "RCT-42", "SRT-B", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EnqueueDecision
            {
                Enqueued = false,
                Message = "Sem caracteristicas (crt) utilizaveis no RCT. IA nao entra em Analisar.",
                CriteriaCount = 0,
                IntervenienteCount = 1
            });

        var scheduler = new Mock<IRecruitmentJobScheduler>();
        var inner = new EnqueueAnalysisCommandHandler(enqueue.Object, scheduler.Object);
        var mediator = new DelegateMediator(enqueueRequest => inner.Handle(enqueueRequest, CancellationToken.None));

        var sut = new AnalyzeVacancyCommandHandler(
            vacancies.Object,
            srt.Object,
            mediator,
            NullLogger<AnalyzeVacancyCommandHandler>.Instance);

        var result = await sut.Handle(
            new AnalyzeVacancyCommand("42", "denilson"),
            CancellationToken.None);

        result.IdRct.Should().Be("42");
        result.RctStamp.Should().Be("RCT-42");
        result.WithCv.Should().Be(2);
        result.Enqueued.Should().Be(1);
        result.Skipped.Should().Be(1);
        result.RequestedBy.Should().Be("denilson");
        result.Candidates.Select(c => c.SrtStamp).Should().Equal("SRT-A", "SRT-B");
        result.Candidates[0].OutboxId.Should().Be(11);

        enqueue.Verify(x => x.TryEnqueueAsync(
            "CVE-A", "RCT-42", "SRT-A", null, It.IsAny<CancellationToken>()), Times.Once);
        enqueue.Verify(x => x.TryEnqueueAsync(
            "CVE-B", "RCT-42", "SRT-B", null, It.IsAny<CancellationToken>()), Times.Once);
        scheduler.Verify(x => x.EnqueueAnalyze(11), Times.Once);
        scheduler.VerifyNoOtherCalls();
        srt.Verify(x => x.ListWithCvByRctAsync("RCT-42", It.IsAny<CancellationToken>()), Times.Once);
        srt.Verify(x => x.SaveScoreAsync(
            It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>(),
            It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<DateTime>(), It.IsAny<string?>(),
            It.IsAny<CancellationToken>()), Times.Never);
        srt.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task RankingByIdRct_OrdersByScore_DoesNotWriteSelectionOrCondp()
    {
        var high = Row("SRT-HIGH", "CVE-H", "RCT-7", score: 30m, condp: "condp-humano", selection: "aberto|0|0");
        var low = Row("SRT-LOW", "CVE-L", "RCT-7", score: 5m, condp: "condp-outro", selection: "aberto|1|0");

        var srt = new Mock<ISrtScoreRepository>(MockBehavior.Strict);
        srt.Setup(x => x.GetByRctAsync("RCT-7", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { low, high });

        var anexos = new Mock<ICvAnexoRepository>(MockBehavior.Strict);
        var ranking = new GetRctRankingQueryHandler(srt.Object, anexos.Object);

        var vacancies = new Mock<IRctVacancyRepository>(MockBehavior.Strict);
        vacancies.Setup(x => x.ResolveStampByIdAsync("7", It.IsAny<CancellationToken>()))
            .ReturnsAsync("RCT-7");

        var mediator = new DelegateMediator(
            onRanking: query => ranking.Handle(query, CancellationToken.None));

        var sut = new GetVacancyRankingQueryHandler(vacancies.Object, mediator);
        var dto = await sut.Handle(new GetVacancyRankingQuery("7"), CancellationToken.None);

        dto.IdRct.Should().Be("7");
        dto.RctStamp.Should().Be("RCT-7");
        dto.Candidates.Select(c => c.SrtStamp).Should().Equal("SRT-HIGH", "SRT-LOW");
        dto.Candidates[0].Condp.Should().Be("condp-humano");
        dto.Candidates[1].Condp.Should().Be("condp-outro");
        dto.PreSelectionNote.Should().Be(HitlCopy.PreSelectionNote);
        ForbiddenCopyGuardDoesNotFlag(dto.Title);
        ForbiddenCopyGuardDoesNotFlag(dto.Footer);
        ForbiddenCopyGuardDoesNotFlag(dto.PreSelectionNote);

        high.Condp.Should().Be("condp-humano");
        high.SelectionStateSnapshot.Should().Be("aberto|0|0");
        low.Condp.Should().Be("condp-outro");
        low.SelectionStateSnapshot.Should().Be("aberto|1|0");

        srt.Verify(x => x.GetByRctAsync("RCT-7", It.IsAny<CancellationToken>()), Times.Once);
        srt.VerifyNoOtherCalls();
        anexos.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Reprocess_SrtFromAnotherVacancy_DoesNotEnqueue()
    {
        var vacancies = new Mock<IRctVacancyRepository>();
        vacancies.Setup(x => x.ResolveStampByIdAsync("9", It.IsAny<CancellationToken>()))
            .ReturnsAsync("RCT-9");

        var srt = new Mock<ISrtScoreRepository>(MockBehavior.Strict);
        srt.Setup(x => x.GetByStampAsync("SRT-X", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Row("SRT-X", "CVE-X", "RCT-OTHER", score: 1m));

        var mediator = new DelegateMediator(
            onReprocess: _ => throw new InvalidOperationException("reprocess must not run"));

        var sut = new ReprocessVacancyCandidateCommandHandler(vacancies.Object, srt.Object, mediator);

        var act = async () => await sut.Handle(
            new ReprocessVacancyCandidateCommand("9", "SRT-X", "denilson"),
            CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
        srt.Verify(x => x.GetByStampAsync("SRT-X", It.IsAny<CancellationToken>()), Times.Once);
        srt.Verify(x => x.SaveScoreAsync(
            It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>(),
            It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<DateTime>(), It.IsAny<string?>(),
            It.IsAny<CancellationToken>()), Times.Never);
        srt.VerifyNoOtherCalls();
    }

    [Fact]
    public void SchemaDefaults_UseShortScamposColumnNames()
    {
        var schema = new RecruitmentSchemaOptions();
        schema.RctIdColumn.Should().Be("idrct");
        schema.SrtPromptVerIaColumn.Should().Be("u_prmveria");
        schema.SrtAuditIaColumn.Should().Be("u_auditia");
        schema.CveEstadoIaColumn.Should().Be("u_estadoia");
        schema.AnexosTextoColumn.Should().Be("u_texto");
        schema.SrtScoreIaColumn.Should().Be("u_scoreia");
    }

    private static void ForbiddenCopyGuardDoesNotFlag(string text)
    {
        Recruitment.Application.Scoring.ForbiddenCopyGuard.ContainsForbiddenPhrase(text).Should().BeFalse();
    }

    private static SrtCandidateRow Row(
        string srtStamp,
        string cveStamp,
        string rctStamp,
        decimal? score,
        string? condp = null,
        string? selection = null) =>
        new()
        {
            SrtStamp = srtStamp,
            CveStamp = cveStamp,
            RctStamp = rctStamp,
            ScoreIa = score,
            Condp = condp,
            SelectionStateSnapshot = selection
        };

    private sealed class DelegateMediator : IMediator
    {
        private readonly Func<EnqueueAnalysisCommand, Task<EnqueueAnalysisResultDto>>? _onEnqueue;
        private readonly Func<GetRctRankingQuery, Task<RctRankingDto>>? _onRanking;
        private readonly Func<ReprocessAnalysisCommand, Task<EnqueueAnalysisResultDto>>? _onReprocess;

        public DelegateMediator(
            Func<EnqueueAnalysisCommand, Task<EnqueueAnalysisResultDto>>? onEnqueue = null,
            Func<GetRctRankingQuery, Task<RctRankingDto>>? onRanking = null,
            Func<ReprocessAnalysisCommand, Task<EnqueueAnalysisResultDto>>? onReprocess = null)
        {
            _onEnqueue = onEnqueue;
            _onRanking = onRanking;
            _onReprocess = onReprocess;
        }

        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request is EnqueueAnalysisCommand enqueue && _onEnqueue is not null)
                return (TResponse)(object)(await _onEnqueue(enqueue))!;

            if (request is GetRctRankingQuery ranking && _onRanking is not null)
                return (TResponse)(object)(await _onRanking(ranking))!;

            if (request is ReprocessAnalysisCommand reprocess && _onReprocess is not null)
                return (TResponse)(object)(await _onReprocess(reprocess))!;

            throw new NotSupportedException(request.GetType().Name);
        }

        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
            where TRequest : IRequest
            => throw new NotSupportedException();

        public Task<object?> Send(object request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(
            IStreamRequest<TResponse> request,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task Publish(object notification, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotification
            => Task.CompletedTask;
    }
}
