using Invoices.Application.DTOs;
using Invoices.Application.Errors;
using Invoices.Application.ExternalServices;
using Invoices.Domain.Repositories;
using MediatR;
using Shared.Kernel.MultiTenancy;
using Shared.Kernel.UserFields;

namespace Invoices.Application.Features.CreateFatura;

/// <summary>
/// Handler para criação de fatura via PHC WEB
/// </summary>
public class CreateFaturaCommandHandler : IRequestHandler<CreateFaturaCommand, InvoiceOutputDTO>
{
    private readonly IFtService _ftService;
    private readonly IFaturaRepository _repository;
    private readonly ITDRepository _tdRepository;
    private readonly IClientRepository _clientRepository;
    private readonly IStockRepository _stockRepository;
    private readonly IUserFieldService? _userFieldService;
    private readonly ITenantContext? _tenantContext;

    public CreateFaturaCommandHandler(
        IFtService ftService,
        IFaturaRepository repository,
        ITDRepository tdRepository,
        IClientRepository clientRepository,
        IStockRepository stockRepository,
        IUserFieldService? userFieldService = null,
        ITenantContext? tenantContext = null)
    {
        _ftService = ftService ?? throw new ArgumentNullException(nameof(ftService));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _tdRepository = tdRepository ?? throw new ArgumentNullException(nameof(tdRepository));
        _clientRepository = clientRepository ?? throw new ArgumentNullException(nameof(clientRepository));
        _stockRepository = stockRepository ?? throw new ArgumentNullException(nameof(stockRepository));
        _userFieldService = userFieldService;
        _tenantContext = tenantContext;
    }

    public async Task<InvoiceOutputDTO> Handle(CreateFaturaCommand request, CancellationToken cancellationToken)
    {
        if (request?.Request == null)
            throw new ArgumentNullException(nameof(request));

        var dto = request.Request;

        try
        {
            // 1. Validar tipo de documento
            var docTypeExists = await _tdRepository.ExistsByIdAsync(dto.Ndoc, cancellationToken);
            if (!docTypeExists)
                throw new InvoicesModuleException(
                    InvoicesErrorCatalog.InvalidDocType.Code,
                    string.Format(InvoicesErrorCatalog.InvalidDocType.Description, dto.Ndoc));

            // 2. Validar cliente
            var estab = dto.Estab ?? 0;
            var clientExists = await _clientRepository.ExistsByNoAndEstabAsync(dto.No, estab, cancellationToken);
            if (!clientExists)
                throw new InvoicesModuleException(
                    InvoicesErrorCatalog.ClientNotFound.Code,
                    string.Format(InvoicesErrorCatalog.ClientNotFound.Description, dto.No, estab));

            // 3. Validar produtos
            if (dto.Linhas != null && dto.Linhas.Count > 0)
            {
                foreach (var linha in dto.Linhas)
                {
                    var productExists = await _stockRepository.ExistsByRefAsync(linha.Ref, cancellationToken);
                    if (!productExists)
                        throw new InvoicesModuleException(
                            InvoicesErrorCatalog.ProductNotFound.Code,
                            string.Format(InvoicesErrorCatalog.ProductNotFound.Description, linha.Ref));
                }
            }

            // 4. Resolver addFields (alias → columnName) e popular AddFieldsByTable no DTO
            await ApplyDynamicFieldsAsync(dto, cancellationToken);

            // 4b. Propagar utilizador de auditoria
            dto.CreatedBy = request.CreatedBy ?? "PHCAPI";

            // 5. Chamar PHC Web
            var output = await _ftService.CreateFaturaAsync(dto);

            // 6. Traduzir addFieldsByTable da resposta PHC (columnName → alias)
            await PopulateAddFieldsFromPhcResponseAsync(output, cancellationToken);

            return output;
        }
        catch (InvoicesModuleException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvoicesModuleException(
                InvoicesErrorCatalog.PhcWebError.Code,
                string.Format(InvoicesErrorCatalog.PhcWebError.Description, ex.Message));
        }
    }

    /// <summary>
    /// Resolve aliases → columnNames via IUserFieldService e popula AddFieldsByTable
    /// nos DTOs de input para o FtService enviar ao PHC.
    /// </summary>
    private async Task ApplyDynamicFieldsAsync(
        CreateInvoiceInputDTO input,
        CancellationToken cancellationToken)
    {
        if (_userFieldService is null || _tenantContext?.AppLicenseStamp is null)
            return;

        var tenant = _tenantContext.AppLicenseStamp;

        var ftDefs  = await _userFieldService.GetFieldsAsync(tenant, "Invoices", "ft",  cancellationToken);
        var ft2Defs = await _userFieldService.GetFieldsAsync(tenant, "Invoices", "ft2", cancellationToken);
        var ft3Defs = await _userFieldService.GetFieldsAsync(tenant, "Invoices", "ft3", cancellationToken);
        var fiDefs  = await _userFieldService.GetFieldsAsync(tenant, "Invoices", "fi",  cancellationToken);
        var fi2Defs = await _userFieldService.GetFieldsAsync(tenant, "Invoices", "fi2", cancellationToken);

        // Cabeçalho
        if (input.AddFields is { Count: > 0 })
        {
            var ft  = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            var ft2 = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            var ft3 = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

            foreach (var entry in input.AddFields)
            {
                if (string.IsNullOrWhiteSpace(entry.Key)) continue;
                var value = UnwrapValue(entry.Value);

                var def = ftDefs.FirstOrDefault(f => MatchesAlias(f, entry.Key));
                if (def is not null) { ft[def.ColumnName] = value; continue; }

                def = ft2Defs.FirstOrDefault(f => MatchesAlias(f, entry.Key));
                if (def is not null) { ft2[def.ColumnName] = value; continue; }

                def = ft3Defs.FirstOrDefault(f => MatchesAlias(f, entry.Key));
                if (def is not null) { ft3[def.ColumnName] = value; }
            }

            var byTable = new Dictionary<string, Dictionary<string, object?>>(StringComparer.OrdinalIgnoreCase);
            if (ft.Count  > 0) byTable["ft"]  = ft;
            if (ft2.Count > 0) byTable["ft2"] = ft2;
            if (ft3.Count > 0) byTable["ft3"] = ft3;
            if (byTable.Count > 0) input.AddFieldsByTable = byTable;
        }

        // Linhas
        if (input.Linhas is { Count: > 0 })
        {
            foreach (var linha in input.Linhas)
            {
                if (linha.AddFields is not { Count: > 0 }) continue;

                var fi  = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                var fi2 = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

                foreach (var entry in linha.AddFields)
                {
                    if (string.IsNullOrWhiteSpace(entry.Key)) continue;
                    var value = UnwrapValue(entry.Value);

                    var def = fiDefs.FirstOrDefault(f => MatchesAlias(f, entry.Key));
                    if (def is not null) { fi[def.ColumnName] = value; continue; }

                    def = fi2Defs.FirstOrDefault(f => MatchesAlias(f, entry.Key));
                    if (def is not null) { fi2[def.ColumnName] = value; }
                }

                var byTable = new Dictionary<string, Dictionary<string, object?>>(StringComparer.OrdinalIgnoreCase);
                if (fi.Count  > 0) byTable["fi"]  = fi;
                if (fi2.Count > 0) byTable["fi2"] = fi2;
                if (byTable.Count > 0) linha.AddFieldsByTable = byTable;
            }
        }
    }

    /// <summary>
    /// Traduz addFieldsByTableRaw da resposta PHC (columnName → alias) e popula AddFields.
    /// </summary>
    private async Task PopulateAddFieldsFromPhcResponseAsync(
        InvoiceOutputDTO output,
        CancellationToken cancellationToken)
    {
        if (_userFieldService is null || _tenantContext?.AppLicenseStamp is null)
            return;

        var hasHeader = output.AddFieldsByTableRaw is { Count: > 0 };
        var hasLines  = output.Linhas.Any(l => l.AddFieldsByTableRaw is { Count: > 0 });
        if (!hasHeader && !hasLines)
            return;

        var tenant = _tenantContext.AppLicenseStamp;

        if (hasHeader)
        {
            var defs = new List<UserFieldDefinition>();
            defs.AddRange(await _userFieldService.GetFieldsAsync(tenant, "Invoices", "ft",  cancellationToken));
            defs.AddRange(await _userFieldService.GetFieldsAsync(tenant, "Invoices", "ft2", cancellationToken));
            defs.AddRange(await _userFieldService.GetFieldsAsync(tenant, "Invoices", "ft3", cancellationToken));

            var flat = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            foreach (var colValues in output.AddFieldsByTableRaw!.Values)
                foreach (var (col, val) in colValues)
                {
                    var def = defs.FirstOrDefault(d => string.Equals(d.ColumnName, col, StringComparison.OrdinalIgnoreCase));
                    if (def is not null) flat[def.Alias] = val;
                }
            if (flat.Count > 0) output.AddFields = flat;
        }

        if (hasLines)
        {
            var defs = new List<UserFieldDefinition>();
            defs.AddRange(await _userFieldService.GetFieldsAsync(tenant, "Invoices", "fi",  cancellationToken));
            defs.AddRange(await _userFieldService.GetFieldsAsync(tenant, "Invoices", "fi2", cancellationToken));

            foreach (var linha in output.Linhas)
            {
                if (linha.AddFieldsByTableRaw is not { Count: > 0 }) continue;
                var flat = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                foreach (var colValues in linha.AddFieldsByTableRaw.Values)
                    foreach (var (col, val) in colValues)
                    {
                        var def = defs.FirstOrDefault(d => string.Equals(d.ColumnName, col, StringComparison.OrdinalIgnoreCase));
                        if (def is not null) flat[def.Alias] = val;
                    }
                if (flat.Count > 0) linha.AddFields = flat;
            }
        }
    }

    private static bool MatchesAlias(UserFieldDefinition def, string key) =>
        string.Equals(def.Alias, key, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(def.ColumnName, key, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Converte JsonElement (STJ) para tipo CLR primitivo para evitar serialização incorrecta pelo Newtonsoft.
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
                je.TryGetInt64(out var l)   ? (object?)l :
                je.TryGetDecimal(out var d) ? d : je.GetDouble(),
            _ => je.GetRawText()
        };
    }
}
