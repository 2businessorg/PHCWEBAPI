namespace Stocks.Application.Features.GetStockByWarehouse;

using MediatR;
using Stocks.Domain.Repositories;
using Stocks.Application.DTOs;

/// <summary>
/// Handler for GetStockByWarehouseQuery that retrieves stock data grouped by warehouse
/// </summary>
public class GetStockByWarehouseQueryHandler : IRequestHandler<GetStockByWarehouseQuery, GetStockByWarehouseResultDTO>
{
    private readonly IStockRepository _repository;

    public GetStockByWarehouseQueryHandler(IStockRepository repository)
    {
        _repository = repository;
    }

    public async Task<GetStockByWarehouseResultDTO> Handle(GetStockByWarehouseQuery request, CancellationToken cancellationToken)
    {
        var domainDtos = await _repository.GetStockByWarehouseAsync(request.Referencia, cancellationToken);

        var applicationDtos = domainDtos.Select(d => new StockByWarehouseDetailsDTO
        {
            Armazem = d.Armazem,
            NomeArmazem = d.NomeArmazem,
            Stock = d.Stock,
            CustoStock = d.CustoStock,
            Localizacao = d.Localizacao,
            EncomendadoPorClientes = d.EncomendadoPorClientes,
            EncomendadoAFornecedores = d.EncomendadoAFornecedores,
            StockMinimo = d.StockMinimo,
            QuantidadeEmRecepcao = d.QuantidadeEmRecepcao,
            QuantidadeCativada = d.QuantidadeCativada
        }).ToList();

        return new GetStockByWarehouseResultDTO
        {
            Items = applicationDtos,
            Metadata = new Meta
            {
                TotalItems = applicationDtos.Count,
                ItemCount = applicationDtos.Count
            }
        };
    }
}
