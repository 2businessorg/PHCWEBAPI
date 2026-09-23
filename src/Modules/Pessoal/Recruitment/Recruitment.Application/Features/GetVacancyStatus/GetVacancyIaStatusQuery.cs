using MediatR;
using Recruitment.Application.DTOs;
using Recruitment.Application.Features.AnalyzeVacancy;
using Recruitment.Domain.Repositories;

namespace Recruitment.Application.Features.GetVacancyStatus;

public sealed record GetVacancyIaStatusQuery(string IdRct) : IRequest<VacancyIaStatusDto>;

public sealed class GetVacancyIaStatusQueryHandler
    : IRequestHandler<GetVacancyIaStatusQuery, VacancyIaStatusDto>
{
    private readonly IRctVacancyRepository _vacancies;
    private readonly ISrtScoreRepository _srt;

    public GetVacancyIaStatusQueryHandler(IRctVacancyRepository vacancies, ISrtScoreRepository srt)
    {
        _vacancies = vacancies;
        _srt = srt;
    }

    public async Task<VacancyIaStatusDto> Handle(
        GetVacancyIaStatusQuery request,
        CancellationToken cancellationToken)
    {
        var idRct = AnalyzeVacancyCommandHandler.RequireId(request.IdRct);
        var rctStamp = await _vacancies.ResolveStampByIdAsync(idRct, cancellationToken);
        if (string.IsNullOrWhiteSpace(rctStamp))
            throw new KeyNotFoundException($"Vaga idrct={idRct} nao encontrada.");

        var counts = await _srt.CountEstadosByRctAsync(rctStamp, cancellationToken);
        return new VacancyIaStatusDto
        {
            IdRct = idRct,
            RctStamp = rctStamp,
            Pendente = counts.Pendente,
            Ok = counts.Ok,
            Erro = counts.Erro,
            SemEstado = counts.SemEstado,
            Total = counts.Total
        };
    }
}
