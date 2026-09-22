using Advances.Application.DTOs;
using Advances.Domain.Repositories;
using MediatR;

namespace Advances.Application.Features.GetAdvanceTypes;

/// <summary>
/// Handler para listagem dos tipos/séries de adiantamento
/// </summary>
public class GetAdvanceTypesQueryHandler : IRequestHandler<GetAdvanceTypesQuery, IReadOnlyList<AdvanceTypeOutputDTO>>
{
    private readonly IAdvanceTypeRepository _advanceTypeRepository;

    public GetAdvanceTypesQueryHandler(IAdvanceTypeRepository advanceTypeRepository)
    {
        _advanceTypeRepository = advanceTypeRepository;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<AdvanceTypeOutputDTO>> Handle(
        GetAdvanceTypesQuery request,
        CancellationToken cancellationToken)
    {
        var types = await _advanceTypeRepository.GetAllAsync(cancellationToken);
        return types.Select(t => new AdvanceTypeOutputDTO
        {
            Ndoc = t.Ndoc,
            Nmdoc = t.Nmdoc.Trim()
        }).ToList();
    }
}
