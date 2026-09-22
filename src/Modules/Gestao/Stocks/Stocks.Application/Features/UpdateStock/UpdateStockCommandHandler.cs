using MediatR;
using Stocks.Application.DTOs;
using Stocks.Application.Mappers;
using Stocks.Domain.Repositories;
using Shared.Kernel.MultiTenancy;
using Shared.Kernel.UserFields;

namespace Stocks.Application.Features.UpdateStock;

public class UpdateStockCommandHandler : IRequestHandler<UpdateStockCommand, StockOutputDTO?>
{
    private readonly IStockRepository _repository;
    private readonly IUserFieldService? _userFieldService;
    private readonly ITenantContext? _tenantContext;

    public UpdateStockCommandHandler(
        IStockRepository repository,
        IUserFieldService? userFieldService = null,
        ITenantContext? tenantContext = null)
    {
        _repository = repository;
        _userFieldService = userFieldService;
        _tenantContext = tenantContext;
    }

    public async Task<StockOutputDTO?> Handle(UpdateStockCommand request, CancellationToken cancellationToken)
    {
        var stock = await _repository.GetByRefAsync(request.Referencia, cancellationToken);
        if (stock is null) return null;

        if (request.Dto.Descricao is not null)
            stock.Design = StockMapper.NormalizeString(request.Dto.Descricao);

        if (request.Dto.Eservico.HasValue)
            stock.Stns = request.Dto.Eservico.Value;

        // Processar array de preços se fornecido
        if (request.Dto.Precos is not null && request.Dto.Precos.Count > 0)
        {
            var (pv1, iva1, pv2, iva2, pv3, iva3, pv4, iva4, pv5, iva5) =
                StockMapper.ConvertPrecosArrayToFields(request.Dto.Precos);

            stock.Pv1 = pv1;
            stock.Epv1 = pv1;
            stock.Iva1incl = iva1;

            stock.Pv2 = pv2;
            stock.Epv2 = pv2;
            stock.Iva2incl = iva2;

            stock.Pv3 = pv3;
            stock.Epv3 = pv3;
            stock.Iva3incl = iva3;

            stock.Pv4 = pv4;
            stock.Epv4 = pv4;
            stock.Iva4incl = iva4;

            stock.Pv5 = pv5;
            stock.Epv5 = pv5;
            stock.Iva5incl = iva5;
        }

        if (request.Dto.FamiliaRef is not null)
            stock.Familia = StockMapper.NormalizeString(request.Dto.FamiliaRef);

        if (request.Dto.TabIva.HasValue)
            stock.Tabiva = (int)request.Dto.TabIva.Value;

        if (request.Dto.Obs is not null)
            stock.Obs = StockMapper.NormalizeString(request.Dto.Obs);

        if (request.Dto.Inactivo.HasValue)
            stock.Inactivo = request.Dto.Inactivo.Value;

        var stFieldDefs = await GetStockFieldDefinitionsAsync(cancellationToken);

        if (request.Dto.AddFields is { Count: > 0 })
        {
            var updates = MapAddFieldsToColumns(request.Dto.AddFields, stFieldDefs);
            if (updates.Count > 0)
                await _repository.UpdateUserFieldsAsync(stock.Ststamp, updates, cancellationToken);
        }

        var now = DateTime.Now;
        stock.Usrinis = request.UpdatedBy ?? "PHCAPI";
        stock.Usrdata = now.Date;
        stock.Usrhora = now.ToString("HH:mm:ss");

        await _repository.UpdateAsync(stock, cancellationToken);

        // Converter campos individuais de volta para array de preços no retorno
        var precosArray = StockMapper.ConvertFieldsToPrecosArray(
            stock.Pv1, stock.Iva1incl,
            stock.Pv2, stock.Iva2incl,
            stock.Pv3, stock.Iva3incl,
            stock.Pv4, stock.Iva4incl,
            stock.Pv5, stock.Iva5incl);

        Dictionary<string, object?>? addFields = null;
        if (stFieldDefs.Count > 0)
        {
            var columns = stFieldDefs.Select(f => f.ColumnName).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            var rawValues = await _repository.GetUserFieldValuesAsync(stock.Ststamp, columns, cancellationToken);
            addFields = MapColumnsToAliases(rawValues, stFieldDefs);
        }

        return new StockOutputDTO
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
            AddFields = addFields
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

    private static Dictionary<string, object?> MapAddFieldsToColumns(
        IReadOnlyDictionary<string, object?> addFields,
        IReadOnlyList<UserFieldDefinition> stFieldDefs)
    {
        var updates = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        var hasDefinitions = stFieldDefs.Count > 0;

        foreach (var entry in addFields)
        {
            if (string.IsNullOrWhiteSpace(entry.Key))
                continue;

            var definition = stFieldDefs.FirstOrDefault(f =>
                string.Equals(f.Alias, entry.Key, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(f.ColumnName, entry.Key, StringComparison.OrdinalIgnoreCase));

            if (definition is not null)
            {
                if (!definition.IsReadOnly)
                    updates[definition.ColumnName] = entry.Value;
                continue;
            }

            if (!hasDefinitions)
                updates[entry.Key] = entry.Value;
        }

        return updates;
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
