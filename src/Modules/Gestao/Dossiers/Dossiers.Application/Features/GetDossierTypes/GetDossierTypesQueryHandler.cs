using Dossiers.Application.DTOs;
using Dossiers.Application.Mappings;
using Dossiers.Domain.Repositories;
using MediatR;

namespace Dossiers.Application.Features.GetDossierTypes;

/// <summary>
/// Handler para obter tipos de dossier
/// </summary>
public class GetDossierTypesQueryHandler : IRequestHandler<GetDossierTypesQuery, IReadOnlyList<DossierTypeOutputDTO>>
{
    private readonly ITipoDossierRepository _repository;

    public GetDossierTypesQueryHandler(ITipoDossierRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<DossierTypeOutputDTO>> Handle(GetDossierTypesQuery request, CancellationToken cancellationToken)
    {
        var rows = await _repository.GetAllAsync(cancellationToken);

        return rows
            .Select(x =>
            {
                var entityType = (x.Bdempresas ?? string.Empty).Trim();
                return new DossierTypeOutputDTO
                {
                    Ndos = x.Ndos,
                    Nmdos = (x.Nmdos ?? string.Empty).Trim(),
                    Name = DossierTypeNameMapper.GetEnglishEntityTypeName(entityType),
                    Table = entityType
                };
            })
            .ToList();
    }
}
