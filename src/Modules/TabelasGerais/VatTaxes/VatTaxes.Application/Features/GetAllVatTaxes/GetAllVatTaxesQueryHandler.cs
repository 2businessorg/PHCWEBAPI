using MediatR;
using VatTaxes.Application.DTOs;
using VatTaxes.Domain;

namespace VatTaxes.Application.Features.GetAllVatTaxes;

/// <summary>
/// Handler para consultar todas as taxas de IVA com suporte a paginação e filtros
/// </summary>
public sealed class GetAllVatTaxesQueryHandler : IRequestHandler<GetAllVatTaxesQuery, GetAllVatTaxesResultDTO>
{
    private readonly IVatTaxRepository _repository;

    /// <summary>
    /// Inicializa uma nova instância de GetAllVatTaxesQueryHandler
    /// </summary>
    /// <param name="repository">Repository para acesso aos dados de taxas de IVA</param>
    public GetAllVatTaxesQueryHandler(IVatTaxRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Manipula a query para obter todas as taxas de IVA
    /// </summary>
    /// <param name="request">Query com parâmetros de paginação e filtros</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resultado paginado com as taxas de IVA</returns>
    public async Task<GetAllVatTaxesResultDTO> Handle(
        GetAllVatTaxesQuery request,
        CancellationToken cancellationToken)
    {
        // Validar paginação
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 50 : request.PageSize;

        // Obter total de registros
        var total = await _repository.GetTotalCountAsync(cancellationToken);

        // Obter registros paginados
        var items = await _repository.GetPagedAsync(page, pageSize, cancellationToken);

        // Aplicar filtros na memória (se necessário)
        if (request.Code.HasValue)
        {
            items = items.Where(x => x.Codigo == request.Code.Value).ToList();
        }

        if (request.Rate.HasValue)
        {
            items = items.Where(x => x.Taxa == request.Rate.Value).ToList();
        }

        // Mapear para DTOs
        var result = items.Select(x => x.ToOutput()).ToList();

        return new GetAllVatTaxesResultDTO(
            TotalItems: total,
            CurrentPage: page,
            PageSize: pageSize,
            Items: result
        );
    }
}
