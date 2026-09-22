using MediatR;
using Parameters.Application.DTOs.Parameters;
using Parameters.Application.Mappings;
using Parameters.Domain.Repositories;

namespace Parameters.Application.Features.GetMoedas;

/// <summary>
/// Handler para listar moedas disponíveis.
/// Resultado: union entre para1 (ge_pteoueuro) e moedas distintas da tabela cb.
/// </summary>
public class GetMoedasQueryHandler : IRequestHandler<GetMoedasQuery, IEnumerable<MoedaOutputDTO>>
{
    private readonly ICbRepository _cbRepository;

    public GetMoedasQueryHandler(ICbRepository cbRepository)
    {
        _cbRepository = cbRepository;
    }

    public async Task<IEnumerable<MoedaOutputDTO>> Handle(GetMoedasQuery request, CancellationToken cancellationToken)
    {
        var moedas = await _cbRepository.GetMoedasAsync(cancellationToken);
        return moedas.ToOutputDtos();
    }
}
