using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Kernel.MultiTenancy;
using Shared.Kernel.UserFields;
using Stocks.Application.DTOs;
using Stocks.Application.Mappers;
using Stocks.Domain.Repositories;

namespace Stocks.Application.Features.GetStockByRef;

public class GetStockByRefQueryHandler : IRequestHandler<GetStockByRefQuery, StockOutputDTO?>
{
    private readonly IStockRepository _repository;
    private readonly IUserFieldService? _userFieldService;
    private readonly ITenantContext? _tenantContext;
    private readonly ILogger<GetStockByRefQueryHandler> _logger;

    public GetStockByRefQueryHandler(
        IStockRepository repository,
        ILogger<GetStockByRefQueryHandler> logger,
        IUserFieldService? userFieldService = null,
        ITenantContext? tenantContext = null)
    {
        _repository = repository;
        _logger = logger;
        _userFieldService = userFieldService;
        _tenantContext = tenantContext;
    }

    public async Task<StockOutputDTO?> Handle(GetStockByRefQuery request, CancellationToken cancellationToken)
    {
        var stock = await _repository.GetByRefAsync(request.Referencia, cancellationToken);
        if (stock is null) return null;

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
            Inactivo = stock.Inactivo
        };

        var stFieldDefs = await GetStockFieldDefinitionsAsync(cancellationToken);
        if (stFieldDefs.Count > 0)
        {
            var columns = stFieldDefs.Select(f => f.ColumnName).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            var rawValues = await _repository.GetUserFieldValuesAsync(stock.Ststamp, columns, cancellationToken);
            output.AddFields = MapColumnsToAliases(rawValues, stFieldDefs);
        }

        return StockMapper.NormalizeOutput(output);
    }

    private async Task<IReadOnlyList<UserFieldDefinition>> GetStockFieldDefinitionsAsync(CancellationToken cancellationToken)
    {
        if (_userFieldService is null)
        {
            _logger.LogWarning("[AddFields] IUserFieldService is null — user fields disabled");
            return [];
        }

        var stamp = _tenantContext?.AppLicenseStamp;
        if (stamp is null)
        {
            _logger.LogWarning("[AddFields] AppLicenseStamp is null — check JWT claim 'applicense_stamp'");
            return [];
        }

        _logger.LogInformation("[AddFields] Lookup: stamp='{Stamp}', module='Stocks', table='st'", stamp);
        var defs = await _userFieldService.GetFieldsAsync(stamp, "Stocks", "st", cancellationToken);
        _logger.LogInformation("[AddFields] Found {Count} field definition(s) for stamp='{Stamp}'", defs.Count, stamp);
        return defs;
    }

    private static Dictionary<string, object?>? MapColumnsToAliases(
        IReadOnlyDictionary<string, object?> rawValues,
        IReadOnlyList<UserFieldDefinition> stFieldDefs)
    {
        var result = new Dictionary<string, object?>();

        foreach (var field in stFieldDefs)
        {
            if (!result.ContainsKey(field.Alias))
                result[field.Alias] = rawValues.GetValueOrDefault(field.ColumnName);
        }

        return result.Count > 0 ? result : null;
    }
}
