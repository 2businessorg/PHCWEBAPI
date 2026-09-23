using MediatR;
using Microsoft.Extensions.Logging;
using Recruitment.Application.DTOs;
using Recruitment.Application.Features.EnqueueAnalysis;
using Recruitment.Domain.Constants;
using Recruitment.Domain.Repositories;

namespace Recruitment.Application.Features.AnalyzeVacancy;

/// <summary>
/// Explicit batch analyze for one vacancy (idrct). Never called from candidature save.
/// Each SRT with a CV goes through <see cref="EnqueueAnalysisCommand"/> (BR-01 + Hangfire).
/// </summary>
public sealed record AnalyzeVacancyCommand(
    string IdRct,
    string? RequestedBy = null,
    string? LabGoRef = null) : IRequest<AnalyzeVacancyResultDto>;

public sealed class AnalyzeVacancyCommandHandler
    : IRequestHandler<AnalyzeVacancyCommand, AnalyzeVacancyResultDto>
{
    private readonly IRctVacancyRepository _vacancies;
    private readonly ISrtScoreRepository _srt;
    private readonly IMediator _mediator;
    private readonly ILogger<AnalyzeVacancyCommandHandler> _logger;

    public AnalyzeVacancyCommandHandler(
        IRctVacancyRepository vacancies,
        ISrtScoreRepository srt,
        IMediator mediator,
        ILogger<AnalyzeVacancyCommandHandler> logger)
    {
        _vacancies = vacancies;
        _srt = srt;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<AnalyzeVacancyResultDto> Handle(
        AnalyzeVacancyCommand request,
        CancellationToken cancellationToken)
    {
        var idRct = RequireId(request.IdRct);
        var rctStamp = await ResolveStampAsync(idRct, cancellationToken);
        var rows = await _srt.ListWithCvByRctAsync(rctStamp, cancellationToken);

        var items = new List<CandidateEnqueueItemDto>(rows.Count);
        var enqueued = 0;
        foreach (var row in rows)
        {
            var result = await _mediator.Send(
                new EnqueueAnalysisCommand(row.CveStamp, rctStamp, row.SrtStamp, request.LabGoRef),
                cancellationToken);

            if (result.Enqueued)
                enqueued++;

            items.Add(new CandidateEnqueueItemDto
            {
                SrtStamp = row.SrtStamp,
                CveStamp = row.CveStamp,
                Enqueued = result.Enqueued,
                OutboxId = result.OutboxId,
                Message = result.Message,
                CriteriaCount = result.CriteriaCount,
                IntervenienteCount = result.IntervenienteCount
            });
        }

        var requestedBy = string.IsNullOrWhiteSpace(request.RequestedBy)
            ? null
            : request.RequestedBy.Trim();
        var skipped = rows.Count - enqueued;
        var message = rows.Count == 0
            ? "Nenhuma candidatura com CV em anexos para esta vaga."
            : $"Candidaturas com CV: {rows.Count}. Enfileiradas: {enqueued}. Ignoradas: {skipped}.";
        if (requestedBy is not null)
            message = $"{message} Pedido por {requestedBy}.";

        _logger.LogInformation(
            "Analyze vacancy idrct={IdRct} rct={Rct} withCv={WithCv} enqueued={Enqueued}",
            idRct, rctStamp, rows.Count, enqueued);

        return new AnalyzeVacancyResultDto
        {
            IdRct = idRct,
            RctStamp = rctStamp,
            RequestedBy = requestedBy,
            WithCv = rows.Count,
            Enqueued = enqueued,
            Skipped = skipped,
            Message = message,
            Candidates = items
        };
    }

    internal static string RequireId(string? idRct)
    {
        if (string.IsNullOrWhiteSpace(idRct))
            throw new ArgumentException("idrct obrigatorio.");

        var trimmed = idRct.Trim();
        if (trimmed.Length > RctIds.MaxLength)
            throw new ArgumentException($"idrct excede {RctIds.MaxLength} caracteres.");

        return trimmed;
    }

    private async Task<string> ResolveStampAsync(string idRct, CancellationToken ct)
    {
        var rctStamp = await _vacancies.ResolveStampByIdAsync(idRct, ct);
        if (string.IsNullOrWhiteSpace(rctStamp))
            throw new KeyNotFoundException($"Vaga idrct={idRct} nao encontrada.");

        return rctStamp;
    }
}
