using MediatR;
using Recruitment.Application.DTOs;
using Recruitment.Application.Features.AnalyzeVacancy;
using Recruitment.Application.Features.GetRctRanking;
using Recruitment.Application.Scoring;
using Recruitment.Domain.Repositories;

namespace Recruitment.Application.Features.GetVacancyRanking;

/// <summary>
/// HITL ranking by public vacancy id. Read-only: does not write selection or condp (AH-04 / BR-09).
/// </summary>
public sealed record GetVacancyRankingQuery(string IdRct) : IRequest<RctRankingDto>;

public sealed class GetVacancyRankingQueryHandler : IRequestHandler<GetVacancyRankingQuery, RctRankingDto>
{
    private readonly IRctVacancyRepository _vacancies;
    private readonly IMediator _mediator;

    public GetVacancyRankingQueryHandler(IRctVacancyRepository vacancies, IMediator mediator)
    {
        _vacancies = vacancies;
        _mediator = mediator;
    }

    public async Task<RctRankingDto> Handle(
        GetVacancyRankingQuery request,
        CancellationToken cancellationToken)
    {
        var idRct = AnalyzeVacancyCommandHandler.RequireId(request.IdRct);
        var rctStamp = await _vacancies.ResolveStampByIdAsync(idRct, cancellationToken);
        if (string.IsNullOrWhiteSpace(rctStamp))
            throw new KeyNotFoundException($"Vaga idrct={idRct} nao encontrada.");

        var ranking = await _mediator.Send(new GetRctRankingQuery(rctStamp), cancellationToken);
        var dto = new RctRankingDto
        {
            Title = ranking.Title,
            Footer = ranking.Footer,
            PreSelectionNote = ranking.PreSelectionNote,
            IdRct = idRct,
            RctStamp = ranking.RctStamp,
            Candidates = ranking.Candidates
        };

        ForbiddenCopyGuard.ThrowIfForbidden(dto.Title, "title");
        ForbiddenCopyGuard.ThrowIfForbidden(dto.Footer, "footer");
        ForbiddenCopyGuard.ThrowIfForbidden(dto.PreSelectionNote, "preSelectionNote");
        return dto;
    }
}
