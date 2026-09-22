using MediatR;
using Shared.Kernel.MultiTenancy;
using Shared.Kernel.UserFields;
using Stocks.Application.DTOs;
using Stocks.Application.Mappers;
using Stocks.Domain.Repositories;

namespace Stocks.Application.Features.GetAllStocks;

public class GetAllStocksQueryHandler : IRequestHandler<GetAllStocksQuery, GetAllStocksResultDTO>
{
    private readonly IStockRepository _repository;
    private readonly IUserFieldService? _userFieldService;
    private readonly ITenantContext? _tenantContext;

    public GetAllStocksQueryHandler(
        IStockRepository repository,
        IUserFieldService? userFieldService = null,
        ITenantContext? tenantContext = null)
    {
        _repository = repository;
        _userFieldService = userFieldService;
        _tenantContext = tenantContext;
    }

    public async Task<GetAllStocksResultDTO> Handle(GetAllStocksQuery request, CancellationToken cancellationToken)
    {
        var result = await _repository.GetPagedAsync(
            request.Referencia,
            request.Descricao,
            request.Familia,
            request.Inactivo,
            request.Page,
            request.PageSize,
            cancellationToken);

        var stFieldDefs = await GetStockFieldDefinitionsAsync(cancellationToken);
        var userFieldValuesByStamp = new Dictionary<string, Dictionary<string, object?>>(StringComparer.OrdinalIgnoreCase);

        if (stFieldDefs.Count > 0 && result.Items.Count > 0)
        {
            var columns = stFieldDefs.Select(f => f.ColumnName).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            userFieldValuesByStamp = await _repository.GetUserFieldValuesBatchAsync(
                result.Items.Select(x => x.Ststamp),
                columns,
                cancellationToken);
        }

        return new GetAllStocksResultDTO
        {
            TotalItems = result.TotalItems,
            CurrentPage = result.CurrentPage,
            PageSize = result.PageSize,
            Items = result.Items.Select(stock =>
            {
                userFieldValuesByStamp.TryGetValue(stock.Ststamp, out var rawValues);
                return MapToOutput(stock, stFieldDefs, rawValues);
            }).ToList()
        };
    }

    private async Task<IReadOnlyList<UserFieldDefinition>> GetStockFieldDefinitionsAsync(CancellationToken cancellationToken)
    {
        if (_tenantContext?.AppLicenseStamp is null || _userFieldService is null)
            return [];

        return await _userFieldService.GetFieldsAsync(
            _tenantContext.AppLicenseStamp,
            "Stocks",
            "st",
            cancellationToken);
    }

    private static StockOutputDTO MapToOutput(
        Stocks.Domain.Entities.St stock,
        IReadOnlyList<UserFieldDefinition> stFieldDefs,
        IReadOnlyDictionary<string, object?>? rawValues)
    {
        // Converter campos individuais para array de preços
        var precosArray = StockMapper.ConvertFieldsToPrecosArray(
            stock.Pv1, stock.Iva1incl,
            stock.Pv2, stock.Iva2incl,
            stock.Pv3, stock.Iva3incl,
            stock.Pv4, stock.Iva4incl,
            stock.Pv5, stock.Iva5incl);

        var output = new StockOutputDTO
        {
            Referencia = stock.Ref,
            Descricao = stock.Design,
            Eservico = stock.Stns,
            Precos = precosArray,
            Stock = stock.Stock,
            TabIva = (int)stock.Tabiva,
            FamiliaRef = stock.Familia,
            FamiliaNome = stock.Faminome,
            Obs = stock.Obs,
            Inactivo = stock.Inactivo,
            AddFields = MapColumnsToAliases(rawValues, stFieldDefs)
        };

        return StockMapper.NormalizeOutput(output);
    }

    private static Dictionary<string, object?>? MapColumnsToAliases(
        IReadOnlyDictionary<string, object?>? rawValues,
        IReadOnlyList<UserFieldDefinition> stFieldDefs)
    {
        if (rawValues is null || stFieldDefs.Count == 0)
            return null;

        var result = new Dictionary<string, object?>();

        foreach (var field in stFieldDefs)
        {
            if (!result.ContainsKey(field.Alias))
                result[field.Alias] = rawValues.GetValueOrDefault(field.ColumnName);
        }

        return result.Count > 0 ? result : null;
    }
}
