using MediatR;
using Recruitment.Application.DTOs;
using Recruitment.Application.Features.AnalyzeVacancy;
using Recruitment.Application.Features.ReprocessAnalysis;
using Recruitment.Domain.Repositories;

namespace Recruitment.Application.Features.ReprocessVacancyCandidate;

/// <summary>
/// Reprocess one candidature of the vacancy. Resolves idrct and checks the SRT belongs to it.
/// Does not write selection or condp (AH-04 / BR-09).
/// </summary>
public sealed record ReprocessVacancyCandidateCommand(
    string IdRct,
    string SrtStamp,
    string RequestedBy,
    string? LabGoRef = null) : IRequest<EnqueueAnalysisResultDto>;

public sealed class ReprocessVacancyCandidateCommandHandler
    : IRequestHandler<ReprocessVacancyCandidateCommand, EnqueueAnalysisResultDto>
{
    private readonly IRctVacancyRepository _vacancies;
    private readonly ISrtScoreRepository _srt;
    private readonly IMediator _mediator;

    public ReprocessVacancyCandidateCommandHandler(
        IRctVacancyRepository vacancies,
        ISrtScoreRepository srt,
        IMediator mediator)
    {
        _vacancies = vacancies;
        _srt = srt;
        _mediator = mediator;
    }

    public async Task<EnqueueAnalysisResultDto> Handle(
        ReprocessVacancyCandidateCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RequestedBy))
            throw new ArgumentException("BR-06: RequestedBy obrigatorio para auditoria.");

        if (string.IsNullOrWhiteSpace(request.SrtStamp))
            throw new ArgumentException("srtStamp obrigatorio.");

        var idRct = AnalyzeVacancyCommandHandler.RequireId(request.IdRct);
        var rctStamp = await _vacancies.ResolveStampByIdAsync(idRct, cancellationToken);
        if (string.IsNullOrWhiteSpace(rctStamp))
            throw new KeyNotFoundException($"Vaga idrct={idRct} nao encontrada.");

        var row = await _srt.GetByStampAsync(request.SrtStamp.Trim(), cancellationToken);
        if (row is null
            || !string.Equals(row.RctStamp.Trim(), rctStamp.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            throw new KeyNotFoundException("Candidatura nao pertence a esta vaga.");
        }

        return await _mediator.Send(
            new ReprocessAnalysisCommand(
                row.CveStamp,
                rctStamp,
                row.SrtStamp,
                request.RequestedBy.Trim(),
                request.LabGoRef),
            cancellationToken);
    }
}
