using MediatR;
using System.Text.Json;
using Clients.Application.DTOs;
using Clients.Application.Errors;
using Clients.Domain.ExternalServices;
using Clients.Domain.Repositories;
using Shared.Kernel.MultiTenancy;
using Shared.Kernel.UserFields;

namespace Clients.Application.Features.CreateClient;

/// <summary>
/// Handler para criar um novo cliente via script PHC WEB.
/// O INSERT nas tabelas cl/cl2 é da responsabilidade do script VB em PHC Web.
/// </summary>
public class CreateClientCommandHandler : IRequestHandler<CreateClientCommand, ClientOutputDTO>
{
    private readonly IClientRepository _repository;
    private readonly IPhcWebServiceClients _phcWebService;
    private readonly IUserFieldService? _userFieldService;
    private readonly ITenantContext? _tenantContext;

    public CreateClientCommandHandler(
        IClientRepository repository,
        IPhcWebServiceClients phcWebService,
        IUserFieldService? userFieldService = null,
        ITenantContext? tenantContext = null)
    {
        _repository = repository;
        _phcWebService = phcWebService;
        _userFieldService = userFieldService;
        _tenantContext = tenantContext;
    }

    public async Task<ClientOutputDTO> Handle(
        CreateClientCommand request,
        CancellationToken cancellationToken)
    {
        var requestedNo = request.Dto.No.GetValueOrDefault();
        var requestedEstab = request.Dto.Estab.GetValueOrDefault();
        var usesAutomaticNo = requestedNo == 0 && requestedEstab == 0;

        // 1. Validações de negócio (feitas no lado .NET para respostas de erro estruturadas)
        var existingByNcont = await _repository.GetByNcontAsync(request.Dto.Ncont, cancellationToken);

        if (usesAutomaticNo)
        {
            if (existingByNcont is not null)
            {
                throw new ClientsModuleException(
                    ClientsErrorCatalog.ClientNcontAlreadyExists.Code,
                    string.Format(ClientsErrorCatalog.ClientNcontAlreadyExists.Description, request.Dto.Ncont));
            }
        }
        else
        {
            var existsSameNoAndEstab = await _repository.ExistsByNoAndEstabAsync(
                requestedNo, requestedEstab, cancellationToken);

            if (existsSameNoAndEstab)
            {
                throw new ClientsModuleException(
                    ClientsErrorCatalog.ClientNoEstabAlreadyExists.Code,
                    string.Format(ClientsErrorCatalog.ClientNoEstabAlreadyExists.Description, requestedNo, requestedEstab));
            }

            if (existingByNcont is not null && existingByNcont.No != requestedNo)
            {
                throw new ClientsModuleException(
                    ClientsErrorCatalog.ClientNcontLinkedToAnotherNo.Code,
                    string.Format(ClientsErrorCatalog.ClientNcontLinkedToAnotherNo.Description, request.Dto.Ncont, existingByNcont.No));
            }
        }

        // 2. Traduzir addFields: alias → columnName (cl e cl2), para o script VB usar directamente em SQL
        var addFieldsByTable = await TranslateAddFieldsAsync(request.Dto.AddFields, cancellationToken);

        // 3. Montar payload para o script PHC
        // O script VB é responsável por: gerar stamp, determinar No automático, inserir cl + cl2, aplicar addFields
        var phcPayload = new
        {
            id        = requestedNo,
            branch    = requestedEstab,
            name      = request.Dto.Nome?.Trim(),
            nuit      = request.Dto.Ncont?.Trim(),
            phone     = request.Dto.Telefone?.Trim() ?? string.Empty,
            address   = request.Dto.Morada?.Trim() ?? string.Empty,
            email     = request.Dto.Email?.Trim() ?? string.Empty,
            inactive  = false,
            createdBy = request.CreatedBy ?? "PHCAPI",
            addFieldsByTable
        };

        var payloadJson = JsonSerializer.Serialize(phcPayload);
        string rawResponse;

        try
        {
            rawResponse = await _phcWebService.CreateClientAsync(payloadJson, cancellationToken);
            ValidatePhcResponse(rawResponse);
        }
        catch (Exception ex)
        {
            throw new ClientsModuleException(
                ClientsErrorCatalog.DatabaseUpdateError.Code,
                $"Erro na integração com PHC WEB ao criar cliente: {ex.Message}");
        }

        // 4. Converter resposta PHC → DTO de saída
        return ParsePhcResponse(rawResponse, request.Dto);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Traduz addFields (alias → columnName) separando por tabela cl / cl2.
    /// O resultado é enviado ao PHC como addFieldsByTable: { "cl": {...}, "cl2": {...} }.
    /// </summary>
    private async Task<Dictionary<string, Dictionary<string, object?>>?> TranslateAddFieldsAsync(
        Dictionary<string, object?>? addFields,
        CancellationToken cancellationToken)
    {
        if (addFields is not { Count: > 0 })
            return null;

        IReadOnlyList<UserFieldDefinition> clDefs = [];
        IReadOnlyList<UserFieldDefinition> cl2Defs = [];

        if (_userFieldService is not null && _tenantContext?.AppLicenseStamp is not null)
        {
            clDefs  = await _userFieldService.GetFieldsAsync(_tenantContext.AppLicenseStamp, "Clients", "cl",  cancellationToken);
            cl2Defs = await _userFieldService.GetFieldsAsync(_tenantContext.AppLicenseStamp, "Clients", "cl2", cancellationToken);
        }

        var cl  = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        var cl2 = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        foreach (var entry in addFields)
        {
            if (string.IsNullOrWhiteSpace(entry.Key))
                continue;

            var value = UnwrapValue(entry.Value);

            var clDef = clDefs.FirstOrDefault(f =>
                string.Equals(f.Alias, entry.Key, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(f.ColumnName, entry.Key, StringComparison.OrdinalIgnoreCase));

            if (clDef is not null) { cl[clDef.ColumnName] = value; continue; }

            var cl2Def = cl2Defs.FirstOrDefault(f =>
                string.Equals(f.Alias, entry.Key, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(f.ColumnName, entry.Key, StringComparison.OrdinalIgnoreCase));

            if (cl2Def is not null) { cl2[cl2Def.ColumnName] = value; continue; }

            // Sem definição encontrada: assume cl (tabela principal)
            cl[entry.Key] = value;
        }

        var result = new Dictionary<string, Dictionary<string, object?>>(StringComparer.OrdinalIgnoreCase);
        if (cl.Count  > 0) result["cl"]  = cl;
        if (cl2.Count > 0) result["cl2"] = cl2;
        return result.Count > 0 ? result : null;
    }

    private static void ValidatePhcResponse(string rawResponse)
    {
        if (string.IsNullOrWhiteSpace(rawResponse))
            throw new InvalidOperationException("Resposta vazia do PHC WEB");

        using var doc = JsonDocument.Parse(rawResponse);
        var root = doc.RootElement;

        if (root.TryGetProperty("success", out var success) && success.ValueKind is JsonValueKind.False)
        {
            var code    = root.TryGetProperty("code",    out var c) ? c.GetString() : null;
            var message = root.TryGetProperty("message", out var m) ? m.GetString() : "Operação devolveu success=false";
            throw new InvalidOperationException($"{code ?? "CL-PHC"}: {message}");
        }

        if (root.TryGetProperty("code", out var codeEl) && codeEl.ValueKind == JsonValueKind.String)
        {
            var code = codeEl.GetString();
            if (!string.IsNullOrWhiteSpace(code) && !string.Equals(code, "0000", StringComparison.OrdinalIgnoreCase))
            {
                var message = root.TryGetProperty("message", out var m) ? m.GetString() : "Erro desconhecido na resposta PHC";
                throw new InvalidOperationException($"{code}: {message}");
            }
        }
    }

    private static ClientOutputDTO ParsePhcResponse(string rawResponse, CreateClientInputDTO input)
    {
        using var doc  = JsonDocument.Parse(rawResponse);
        var root = doc.RootElement;

        if (!root.TryGetProperty("item", out var item))
        {
            // Fallback: devolver com dados do input (script antigo sem "item")
            return new ClientOutputDTO
            {
                Nome     = input.Nome,
                Ncont    = input.Ncont,
                Telefone = input.Telefone ?? string.Empty,
                Morada   = input.Morada   ?? string.Empty,
                Email    = input.Email    ?? string.Empty,
                No       = input.No.GetValueOrDefault(),
                Estab    = input.Estab.GetValueOrDefault(),
                Inactivo = false,
            };
        }

        var dto = new ClientOutputDTO
        {
            No       = item.TryGetProperty("id",       out var id)    ? id.GetDecimal()       : input.No.GetValueOrDefault(),
            Estab    = item.TryGetProperty("branch",   out var br)    ? br.GetDecimal()       : input.Estab.GetValueOrDefault(),
            Nome     = item.TryGetProperty("name",     out var name)  ? name.GetString()!     : input.Nome,
            Ncont    = item.TryGetProperty("nuit",     out var nuit)  ? nuit.GetString()!     : input.Ncont,
            Telefone = item.TryGetProperty("phone",    out var ph)    ? ph.GetString()!       : input.Telefone ?? string.Empty,
            Morada   = item.TryGetProperty("address",  out var addr)  ? addr.GetString()!     : input.Morada   ?? string.Empty,
            Email    = item.TryGetProperty("email",    out var em)    ? em.GetString()!       : input.Email    ?? string.Empty,
            Inactivo = item.TryGetProperty("inactive", out var inact) && inact.GetBoolean(),
        };

        // addFields da resposta PHC (script devolve os valores já gravados)
        if (item.TryGetProperty("addFields", out var af) && af.ValueKind == JsonValueKind.Object)
        {
            var fields = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            foreach (var prop in af.EnumerateObject())
                fields[prop.Name] = UnwrapJsonElement(prop.Value);
            if (fields.Count > 0)
                dto.AddFields = fields;
        }

        return dto;
    }

    private static object? UnwrapValue(object? value)
    {
        if (value is not JsonElement je) return value;
        return UnwrapJsonElement(je);
    }

    private static object? UnwrapJsonElement(JsonElement je) => je.ValueKind switch
    {
        JsonValueKind.String  => je.GetString(),
        JsonValueKind.True    => true,
        JsonValueKind.False   => false,
        JsonValueKind.Null    => null,
        JsonValueKind.Number  =>
            je.TryGetInt64(out var l)   ? (object?)l :
            je.TryGetDecimal(out var d) ? d : je.GetDouble(),
        _ => je.GetRawText()
    };
}
