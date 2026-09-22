using Dossiers.Application.DTOs;
using Dossiers.Application.Errors;
using Dossiers.Application.Mappings;
using Dossiers.Domain.ExternalServices;
using Dossiers.Domain.Models;
using Dossiers.Domain.Repositories;
using MediatR;
using Shared.Kernel.MultiTenancy;
using Shared.Kernel.UserFields;

namespace Dossiers.Application.Features.CreateDossier;

/// <summary>
/// Handler para criação de dossier via PHC WEB
/// </summary>
public class CreateDossierCommandHandler : IRequestHandler<CreateDossierCommand, DossierOutputDTO>
{
    private readonly IPhcWebServiceDossiers _phcWebService;
    private readonly ITipoDossierRepository _tipoDossierRepository;
    private readonly IEntityRepository _entityRepository;
    private readonly ISupplierRepository _supplierRepository;
    private readonly IStockRepository _stockRepository;
    private readonly IDossierRepository? _dossierRepository;
    private readonly IUserFieldService? _userFieldService;
    private readonly ITenantContext? _tenantContext;

    /// <summary>
    /// Inicializa o handler de criação de dossiers via PHC WEB.
    /// </summary>
    public CreateDossierCommandHandler(
        IPhcWebServiceDossiers phcWebService,
        ITipoDossierRepository tipoDossierRepository,
        IEntityRepository entityRepository,
        ISupplierRepository supplierRepository,
        IStockRepository stockRepository,
        IDossierRepository? dossierRepository = null,
        IUserFieldService? userFieldService = null,
        ITenantContext? tenantContext = null)
    {
        _phcWebService = phcWebService;
        _tipoDossierRepository = tipoDossierRepository;
        _entityRepository = entityRepository;
        _supplierRepository = supplierRepository;
        _stockRepository = stockRepository;
        _dossierRepository = dossierRepository;
        _userFieldService = userFieldService;
        _tenantContext = tenantContext;
    }

    /// <summary>
    /// Processa a criação de um dossier via script PHC WEB.
    /// Validações:
    /// - Tipo de dossier deve existir
    /// - Entidade (CL/FL/AG/EM) deve existir de acordo com bdempresa do tipo
    /// - Todos os produtos/referências devem existir no stock
    /// 
    /// O PHC WEB é responsável por:
    /// - Resolução de preços, moedas e IVA
    /// - Cálculo de totais e impostos
    /// - Persistência na base de dados
    /// </summary>
    public async Task<DossierOutputDTO> Handle(CreateDossierCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;

        // 1. Validar se o tipo de dossier existe e obter informações
        var tipo = await _tipoDossierRepository.GetByIdAsync(dto.Ndos, cancellationToken);
        if (tipo is null)
        {
            throw new DossiersModuleException(
                DossiersErrorCatalog.InvalidTipoDossier.Code,
                $"Número do Tipo de Dossier inválido: {dto.Ndos}");
        }

        // 2. Determinar qual tabela usar e validar a entidade
        // tipo.Bdempresas contém a(s) tabela(s) a usar (CL, FL, AG, EM, etc)
        var tableCode = tipo.Bdempresas.Trim().ToUpperInvariant();
        var tableName = DossiersErrorCatalog.GetTableName(tableCode);
        var estab = dto.Estab ?? 0;

        // Validar se a entidade existe
        bool entityExists = tableCode switch
        {
            "FL" or "CL" => await _supplierRepository.ExistsByNoEstabAsync(dto.No, estab, cancellationToken),
            _ => await _entityRepository.ExistsByNoAsync(dto.No, cancellationToken)
        };

        if (!entityExists)
        {
            throw new DossiersModuleException(
                DossiersErrorCatalog.ClientNotFound.Code,
                string.Format(DossiersErrorCatalog.ClientNotFound.Description, tableName, dto.No, estab));
        }

        // 3. Validar se todas as referências de produtos existem no stock
        if (dto.Linhas != null && dto.Linhas.Count > 0)
        {
            foreach (var linha in dto.Linhas)
            {
                var stockExists = await _stockRepository.ExistsByRefAsync(linha.Referencia, cancellationToken);
                if (!stockExists)
                {
                    throw new DossiersModuleException(
                        DossiersErrorCatalog.ReferenceNotFound.Code,
                        string.Format(DossiersErrorCatalog.ReferenceNotFound.Description, linha.Referencia));
                }
            }
        }

        // 4. Converter para requisição PHC (JSON mínimo)
        var phcRequest = request.ToPhcRequest();

        await ApplyPhcDynamicFieldsAsync(dto, phcRequest, cancellationToken);

        // 5. Chamar script PHC WEB
        var phcResponse = await _phcWebService.InsertBoAsync(phcRequest, cancellationToken);

        // 6. Converter resposta PHC para DTO de resposta
        var outputDto = phcResponse.ToOutputDTO();

        // 7. Traduzir addFieldsByTable (columnName → alias) a partir da resposta PHC
        await PopulateAddFieldsFromPhcResponseAsync(phcResponse, outputDto, cancellationToken);

        return outputDto;
    }

    private async Task ApplyPhcDynamicFieldsAsync(
        CreateDossierInputDTO input,
        PhcInsertBoRequest request,
        CancellationToken cancellationToken)
    {
        if (_userFieldService is null || _tenantContext?.AppLicenseStamp is null)
            return;

        var tenant = _tenantContext.AppLicenseStamp;

        var boDefs = await _userFieldService.GetFieldsAsync(tenant, "Dossiers", "bo", cancellationToken);
        var bo2Defs = await _userFieldService.GetFieldsAsync(tenant, "Dossiers", "bo2", cancellationToken);
        var bo3Defs = await _userFieldService.GetFieldsAsync(tenant, "Dossiers", "bo3", cancellationToken);
        var biDefs = await _userFieldService.GetFieldsAsync(tenant, "Dossiers", "bi", cancellationToken);
        var bi2Defs = await _userFieldService.GetFieldsAsync(tenant, "Dossiers", "bi2", cancellationToken);

        if (input.AddFields is { Count: > 0 })
        {
            var headerUpdates = MapAddFieldsByTable(input.AddFields, boDefs, bo2Defs, bo3Defs);
            var headerByTable = new Dictionary<string, Dictionary<string, object?>>(StringComparer.OrdinalIgnoreCase);

            if (headerUpdates.Bo.Count > 0)
                headerByTable["bo"] = headerUpdates.Bo;
            if (headerUpdates.Bo2.Count > 0)
                headerByTable["bo2"] = headerUpdates.Bo2;
            if (headerUpdates.Bo3.Count > 0)
                headerByTable["bo3"] = headerUpdates.Bo3;

            if (headerByTable.Count > 0)
                request.AddFieldsByTable = headerByTable;
        }

        if (input.Linhas is { Count: > 0 } && request.LstBi.Count > 0)
        {
            for (var i = 0; i < input.Linhas.Count && i < request.LstBi.Count; i++)
            {
                var lineInput = input.Linhas[i];
                if (lineInput.AddFields is not { Count: > 0 })
                    continue;

                var lineUpdates = MapAddFieldsByTable(lineInput.AddFields, biDefs, bi2Defs);
                var lineByTable = new Dictionary<string, Dictionary<string, object?>>(StringComparer.OrdinalIgnoreCase);

                if (lineUpdates.Primary.Count > 0)
                    lineByTable["bi"] = lineUpdates.Primary;
                if (lineUpdates.Secondary.Count > 0)
                    lineByTable["bi2"] = lineUpdates.Secondary;

                if (lineByTable.Count > 0)
                    request.LstBi[i].AddFieldsByTable = lineByTable;
            }
        }
    }

    /// <summary>
    /// Traduz os addFieldsByTable da resposta PHC (columnName → alias) e popula o output DTO.
    /// Evita leituras à BD — usa diretamente os valores devolvidos pelo script PHC.
    /// </summary>
    private async Task PopulateAddFieldsFromPhcResponseAsync(
        PhcInsertBoResponse phcResponse,
        DossierOutputDTO output,
        CancellationToken cancellationToken)
    {
        if (_userFieldService is null || _tenantContext?.AppLicenseStamp is null)
            return;

        var hasHeaderFields = phcResponse.AddFieldsByTable is { Count: > 0 };
        var hasLineFields = phcResponse.Linhas.Any(l => l.AddFieldsByTable is { Count: > 0 });

        if (!hasHeaderFields && !hasLineFields)
            return;

        var tenant = _tenantContext.AppLicenseStamp;

        if (hasHeaderFields)
        {
            var headerDefs = new List<UserFieldDefinition>();
            headerDefs.AddRange(await _userFieldService.GetFieldsAsync(tenant, "Dossiers", "bo", cancellationToken));
            headerDefs.AddRange(await _userFieldService.GetFieldsAsync(tenant, "Dossiers", "bo2", cancellationToken));
            headerDefs.AddRange(await _userFieldService.GetFieldsAsync(tenant, "Dossiers", "bo3", cancellationToken));

            var flat = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            foreach (var colValues in phcResponse.AddFieldsByTable!.Values)
            {
                foreach (var (colName, value) in colValues)
                {
                    var def = headerDefs.FirstOrDefault(d => string.Equals(d.ColumnName, colName, StringComparison.OrdinalIgnoreCase));
                    if (def is not null)
                        flat[def.Alias] = value;
                }
            }
            if (flat.Count > 0)
                output.AddFields = flat;
        }

        if (hasLineFields)
        {
            var lineDefs = new List<UserFieldDefinition>();
            lineDefs.AddRange(await _userFieldService.GetFieldsAsync(tenant, "Dossiers", "bi", cancellationToken));
            lineDefs.AddRange(await _userFieldService.GetFieldsAsync(tenant, "Dossiers", "bi2", cancellationToken));

            for (var i = 0; i < phcResponse.Linhas.Count && i < output.Linhas.Count; i++)
            {
                var linhaPhc = phcResponse.Linhas[i];
                if (linhaPhc.AddFieldsByTable is not { Count: > 0 })
                    continue;

                var flat = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                foreach (var colValues in linhaPhc.AddFieldsByTable.Values)
                {
                    foreach (var (colName, value) in colValues)
                    {
                        var def = lineDefs.FirstOrDefault(d => string.Equals(d.ColumnName, colName, StringComparison.OrdinalIgnoreCase));
                        if (def is not null)
                            flat[def.Alias] = value;
                    }
                }
                if (flat.Count > 0)
                    output.Linhas[i].AddFields = flat;
            }
        }
    }

    private static (Dictionary<string, object?> Bo, Dictionary<string, object?> Bo2, Dictionary<string, object?> Bo3) MapAddFieldsByTable(
        IReadOnlyDictionary<string, object?> addFields,
        IReadOnlyList<UserFieldDefinition> boDefs,
        IReadOnlyList<UserFieldDefinition> bo2Defs,
        IReadOnlyList<UserFieldDefinition> bo3Defs)
    {
        var bo = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        var bo2 = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        var bo3 = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        foreach (var entry in addFields)
        {
            if (string.IsNullOrWhiteSpace(entry.Key))
                continue;

            var value = UnwrapValue(entry.Value);

            var boDef = boDefs.FirstOrDefault(f => string.Equals(f.Alias, entry.Key, StringComparison.OrdinalIgnoreCase) || string.Equals(f.ColumnName, entry.Key, StringComparison.OrdinalIgnoreCase));
            if (boDef is not null)
            {
                bo[boDef.ColumnName] = value;
                continue;
            }

            var bo2Def = bo2Defs.FirstOrDefault(f => string.Equals(f.Alias, entry.Key, StringComparison.OrdinalIgnoreCase) || string.Equals(f.ColumnName, entry.Key, StringComparison.OrdinalIgnoreCase));
            if (bo2Def is not null)
            {
                bo2[bo2Def.ColumnName] = value;
                continue;
            }

            var bo3Def = bo3Defs.FirstOrDefault(f => string.Equals(f.Alias, entry.Key, StringComparison.OrdinalIgnoreCase) || string.Equals(f.ColumnName, entry.Key, StringComparison.OrdinalIgnoreCase));
            if (bo3Def is not null)
                bo3[bo3Def.ColumnName] = value;
        }

        return (bo, bo2, bo3);
    }

    private static (Dictionary<string, object?> Primary, Dictionary<string, object?> Secondary) MapAddFieldsByTable(
        IReadOnlyDictionary<string, object?> addFields,
        IReadOnlyList<UserFieldDefinition> primaryDefs,
        IReadOnlyList<UserFieldDefinition> secondaryDefs)
    {
        var primary = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        var secondary = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        foreach (var entry in addFields)
        {
            if (string.IsNullOrWhiteSpace(entry.Key))
                continue;

            var value = UnwrapValue(entry.Value);

            var primaryDef = primaryDefs.FirstOrDefault(f => string.Equals(f.Alias, entry.Key, StringComparison.OrdinalIgnoreCase) || string.Equals(f.ColumnName, entry.Key, StringComparison.OrdinalIgnoreCase));
            if (primaryDef is not null)
            {
                primary[primaryDef.ColumnName] = value;
                continue;
            }

            var secondaryDef = secondaryDefs.FirstOrDefault(f => string.Equals(f.Alias, entry.Key, StringComparison.OrdinalIgnoreCase) || string.Equals(f.ColumnName, entry.Key, StringComparison.OrdinalIgnoreCase));
            if (secondaryDef is not null)
                secondary[secondaryDef.ColumnName] = value;
        }

        return (primary, secondary);
    }

    /// <summary>
    /// Converte JsonElement (resultado de deserialização STJ de object?) para tipo CLR primitivo.
    /// Necessário porque o Newtonsoft.Json serializa JsonElement como objeto complexo em vez de valor primitivo.
    /// </summary>
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
                je.TryGetInt64(out var l)    ? (object?)l :
                je.TryGetDecimal(out var d)  ? d : je.GetDouble(),
            _ => je.GetRawText()
        };
    }
}

