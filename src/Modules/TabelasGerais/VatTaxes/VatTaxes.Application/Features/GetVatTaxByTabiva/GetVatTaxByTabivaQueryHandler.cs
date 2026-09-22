using MediatR;
using VatTaxes.Application.DTOs;
using VatTaxes.Domain;

namespace VatTaxes.Application.Features.GetVatTaxByTabiva;

/// <summary>
/// Handler para consultar uma taxa de IVA específica por código (Tabiva)
/// </summary>
public sealed class GetVatTaxByTabivaQueryHandler : IRequestHandler<GetVatTaxByTabivaQuery, VatTaxOutputDTO?>
{
    private readonly IVatTaxRepository _repository;

    /// <summary>
    /// Inicializa uma nova instância de GetVatTaxByTabivaQueryHandler
    /// </summary>
    /// <param name="repository">Repository para acesso aos dados de taxas de IVA</param>
    public GetVatTaxByTabivaQueryHandler(IVatTaxRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Manipula a query para obter uma taxa de IVA por código
    /// </summary>
    /// <param name="request">Query com o código da taxa</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>DTO da taxa de IVA ou null se não encontrada</returns>
    public async Task<VatTaxOutputDTO?> Handle(
        GetVatTaxByTabivaQuery request,
        CancellationToken cancellationToken)
    {
        // Validar código
        if (request.Code <= 0)
            return null;

        // Buscar taxa no repositório
        var taxasIva = await _repository.GetByCodeAsync(request.Code, cancellationToken);

        // Retornar DTO ou null
        return taxasIva?.ToOutput();
    }
}
