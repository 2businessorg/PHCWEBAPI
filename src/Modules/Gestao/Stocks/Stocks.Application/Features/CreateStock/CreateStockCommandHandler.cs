using MediatR;
using Shared.Kernel.MultiTenancy;
using Shared.Kernel.UserFields;
using System.Text.Json;
using Stocks.Application.DTOs;
using Stocks.Application.Errors;
using Stocks.Application.Mappers;
using Stocks.Domain.ExternalServices;
using Stocks.Domain.Repositories;

namespace Stocks.Application.Features.CreateStock;

public class CreateStockCommandHandler : IRequestHandler<CreateStockCommand, StockOutputDTO>
{
    private readonly IStockRepository _repository;
    private readonly IPhcWebServiceStocks _phcWebService;
    private readonly IUserFieldService? _userFieldService;
    private readonly ITenantContext? _tenantContext;

    public CreateStockCommandHandler(
        IStockRepository repository,
        IPhcWebServiceStocks phcWebService,
        IUserFieldService? userFieldService = null,
        ITenantContext? tenantContext = null)
    {
        _repository = repository;
        _phcWebService = phcWebService;
        _userFieldService = userFieldService;
        _tenantContext = tenantContext;
    }

    public async Task<StockOutputDTO> Handle(CreateStockCommand request, CancellationToken cancellationToken)
    {
        var normalizedRef = StockMapper.NormalizeString(request.Dto.Referencia);
        var normalizedDesc = StockMapper.NormalizeString(request.Dto.Descricao);

        var exists = await _repository.ExistsByRefAsync(normalizedRef, cancellationToken);
        if (exists)
        {
            throw new StocksModuleException(
                StocksErrorCatalog.StockRefAlreadyExists.Code,
                string.Format(StocksErrorCatalog.StockRefAlreadyExists.Description, normalizedRef));
        }

        // Traduzir addFields: alias → columnName usando definições de campos de utilizador
        // O script VB usa as chaves de addFields directamente como nomes de colunas no INSERT SQL
        Dictionary<string, object?>? translatedAddFields = null;
        if (request.Dto.AddFields is { Count: > 0 })
        {
            translatedAddFields = await TranslateAddFieldsAsync(request.Dto.AddFields, cancellationToken);
        }

        // A insercao na base de dados e gerida pelo script VB em PHC Web.
        // Ver: PHC Web/Scripts/ST/Criar ST via API.vb
        var phcPayload = new
        {
            reference = normalizedRef,
            description = normalizedDesc,
            isService = request.Dto.Eservico,
            prices = request.Dto.Precos,
            taxTableId = request.Dto.TabIva,
            familyRef = StockMapper.NormalizeString(request.Dto.FamiliaRef),
            observations = StockMapper.NormalizeString(request.Dto.Obs),
            isInactive = request.Dto.Inactivo,
            createdBy = request.CreatedBy ?? "PHCAPI",
            addFields = translatedAddFields
        };

        var payloadJson = JsonSerializer.Serialize(phcPayload);
        string rawResponse;

        try
        {
            rawResponse = await _phcWebService.CreateStockAsync(payloadJson, cancellationToken);
            ValidatePhcResponse(rawResponse);
        }
        catch (Exception ex)
        {
            throw new StocksModuleException(
                StocksErrorCatalog.DatabaseUpdateError.Code,
                $"Erro na integracao com PHC WEB ao criar stock: {ex.Message}");
        }

        var (pv1, iva1, pv2, iva2, pv3, iva3, pv4, iva4, pv5, iva5) =
            StockMapper.ConvertPrecosArrayToFields(request.Dto.Precos);

        var precosArray = StockMapper.ConvertFieldsToPrecosArray(
            pv1, iva1, pv2, iva2, pv3, iva3, pv4, iva4, pv5, iva5);

        return new StockOutputDTO
        {
            Referencia = normalizedRef,
            Descricao = normalizedDesc,
            Eservico = request.Dto.Eservico,
            Precos = precosArray,
            Stock = 0m,
            TabIva = request.Dto.TabIva > 0 ? (int)request.Dto.TabIva : 1,
            FamiliaRef = StockMapper.NormalizeString(request.Dto.FamiliaRef),
            FamiliaNome = string.Empty,
            Obs = StockMapper.NormalizeString(request.Dto.Obs),
            Inactivo = request.Dto.Inactivo,
            AddFields = request.Dto.AddFields
        };
    }

    private static void ValidatePhcResponse(string rawResponse)
    {
        if (string.IsNullOrWhiteSpace(rawResponse))
            throw new InvalidOperationException("Resposta vazia do PHC WEB");

        using var doc = JsonDocument.Parse(rawResponse);
        var root = doc.RootElement;

        if (root.TryGetProperty("success", out var successElement) &&
            successElement.ValueKind is JsonValueKind.False)
        {
            var code = root.TryGetProperty("code", out var codeElement)
                ? codeElement.GetString()
                : null;
            var message = root.TryGetProperty("message", out var messageElement)
                ? messageElement.GetString()
                : "Operacao devolveu sucesso=false";

            throw new InvalidOperationException($"{code ?? "ST-PHC"}: {message}");
        }

        if (root.TryGetProperty("code", out var codeEl) &&
            codeEl.ValueKind == JsonValueKind.String)
        {
            var code = codeEl.GetString();
            if (!string.IsNullOrWhiteSpace(code) && !string.Equals(code, "0000", StringComparison.OrdinalIgnoreCase))
            {
                var message = root.TryGetProperty("message", out var msgEl)
                    ? msgEl.GetString()
                    : "Erro desconhecido na resposta PHC";
                throw new InvalidOperationException($"{code}: {message}");
            }
        }
    }

    /// <summary>
    /// Traduz os aliases de addFields para os nomes de colunas reais usando IUserFieldService.
    /// O script VB usa os nomes das chaves directamente em SQL, por isso devem ser os columnNames (e.g. us_descript1).
    /// Se o serviço não estiver disponível ou o alias não for encontrado, a chave original é mantida.
    /// </summary>
    private async Task<Dictionary<string, object?>> TranslateAddFieldsAsync(
        IReadOnlyDictionary<string, object?> addFields,
        CancellationToken cancellationToken)
    {
        var result = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        IReadOnlyList<UserFieldDefinition>? stDefs = null;
        if (_userFieldService is not null && _tenantContext?.AppLicenseStamp is not null)
        {
            stDefs = await _userFieldService.GetFieldsAsync(
                _tenantContext.AppLicenseStamp, "Stocks", "st", cancellationToken);
        }

        foreach (var entry in addFields)
        {
            if (string.IsNullOrWhiteSpace(entry.Key))
                continue;

            var value = UnwrapValue(entry.Value);

            if (stDefs is not null)
            {
                var def = stDefs.FirstOrDefault(f =>
                    string.Equals(f.Alias, entry.Key, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(f.ColumnName, entry.Key, StringComparison.OrdinalIgnoreCase));

                if (def is not null)
                {
                    result[def.ColumnName] = value;
                    continue;
                }
            }

            // Sem definição encontrada: mantém a chave original
            result[entry.Key] = value;
        }

        return result;
    }

    private static object? UnwrapValue(object? value)
    {
        if (value is not System.Text.Json.JsonElement je)
            return value;

        return je.ValueKind switch
        {
            System.Text.Json.JsonValueKind.String  => je.GetString(),
            System.Text.Json.JsonValueKind.True    => true,
            System.Text.Json.JsonValueKind.False   => false,
            System.Text.Json.JsonValueKind.Null    => null,
            System.Text.Json.JsonValueKind.Number  =>
                je.TryGetInt64(out var l)   ? (object?)l :
                je.TryGetDecimal(out var d) ? d : je.GetDouble(),
            _ => je.GetRawText()
        };
    }
}
