using MediatR;
using Receipts.Application.DTOs;
using Receipts.Application.Errors;
using Receipts.Application.Mappings;
using Receipts.Domain.ExternalServices;
using Receipts.Domain.Repositories;
using Shared.Kernel.MultiTenancy;
using Shared.Kernel.UserFields;
using System.Text.Json;

namespace Receipts.Application.Features.CreateReceipt;

/// <summary>
/// Handler para criação de Recibo via PHC WEB (script insertReAPI).
///
/// Validações de negócio:
/// - Série do recibo (ndoc) deve existir em tsre
/// - Cliente (no) deve existir em cl
///
/// O PHC WEB é responsável por:
/// - Resolver o ccstamp internamente
/// - Resolver o código de tesouraria e data de processamento
/// - Cálculo dos totais e geração do número do recibo
/// - Persistência nas tabelas re e rl
/// - Processamento contabilístico e eventual criação de adiantamentos (RD)
/// </summary>
public class CreateReceiptCommandHandler : IRequestHandler<CreateReceiptCommand, CreateReceiptOutputDTO>
{
    private readonly IPhcWebServiceReceipts _phcWebService;
    private readonly IReceiptTypeRepository _receiptTypeRepository;
    private readonly IClientRepository _clientRepository;
    private readonly ITenantContext? _tenantContext;
    private readonly IUserFieldService? _userFieldService;

    /// <summary>
    /// Inicializa o handler com os serviços necessários.
    /// </summary>
    public CreateReceiptCommandHandler(
        IPhcWebServiceReceipts phcWebService,
        IReceiptTypeRepository receiptTypeRepository,
        IClientRepository clientRepository,
        ITenantContext? tenantContext = null,
        IUserFieldService? userFieldService = null)
    {
        _phcWebService = phcWebService;
        _receiptTypeRepository = receiptTypeRepository;
        _clientRepository = clientRepository;
        _tenantContext = tenantContext;
        _userFieldService = userFieldService;
    }

    /// <inheritdoc />
    public async Task<CreateReceiptOutputDTO> Handle(CreateReceiptCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;

        // 1. Validar série do recibo
        var serie = await _receiptTypeRepository.GetByNdocAsync(dto.Ndoc, cancellationToken);
        if (serie is null)
        {
            throw new ReceiptsModuleException(
                ReceiptsErrorCatalog.InvalidReceiptType.Code,
                string.Format(ReceiptsErrorCatalog.InvalidReceiptType.Description, dto.Ndoc));
        }

        // 2. Validar cliente
        var clientExists = await _clientRepository.ExistsByNoAsync(dto.No, cancellationToken);
        if (!clientExists)
        {
            throw new ReceiptsModuleException(
                ReceiptsErrorCatalog.ClientNotFound.Code,
                string.Format(ReceiptsErrorCatalog.ClientNotFound.Description, dto.No));
        }

        // 3. Resolver aliases de addFields para nomes de colunas reais
        Dictionary<string, object?>? resolvedHeaderFields = null;
        List<Dictionary<string, object?>?>? resolvedLineFields = null;

        if (_userFieldService is not null && _tenantContext?.AppLicenseStamp is not null)
        {
            var tenant = _tenantContext.AppLicenseStamp;
            var reDefs = await _userFieldService.GetFieldsAsync(tenant, "Receipts", "re", cancellationToken);
            var rlDefs = await _userFieldService.GetFieldsAsync(tenant, "Receipts", "rl", cancellationToken);

            if (dto.AddFields is { Count: > 0 })
                resolvedHeaderFields = ResolveAliases(dto.AddFields, reDefs);

            if (dto.Linhas.Any(l => l.AddFields is { Count: > 0 }))
                resolvedLineFields = dto.Linhas
                    .Select(l => l.AddFields is { Count: > 0 } ? ResolveAliases(l.AddFields, rlDefs) : null)
                    .ToList();
        }

        // 4. Construir payload para o PHC WEB
        // O script insertReAPI resolve internamente: ccstamp, tesouraria e data de processamento
        var phcPayload = ReceiptPhcMapper.ToPhcPayload(dto, request.CreatedBy, resolvedHeaderFields, resolvedLineFields);
        var payloadJson = JsonSerializer.Serialize(phcPayload);

        // 5. Invocar script no PHC WEB
        string rawResponse;
        try
        {
            rawResponse = await _phcWebService.CreateReceiptAsync(
                payloadJson,
                cancellationToken);
        }
        catch (Exception ex)
        {
            throw new ReceiptsModuleException(
                ReceiptsErrorCatalog.PhcWebIntegrationError.Code,
                string.Format(ReceiptsErrorCatalog.PhcWebIntegrationError.Description, ex.Message));
        }

        // 6. Mapear resposta do PHC WEB para DTO de saída
        var output = ReceiptMapper.FromPhcCreateResponse(rawResponse, dto);
        return output;
    }

    /// <summary>
    /// Resolve aliases de addFields para nomes de colunas reais usando as definições de u_addfields.
    /// Se o alias não existir nas definições, o nome é enviado tal como está (fallback).
    /// </summary>
    private static Dictionary<string, object?> ResolveAliases(
        IReadOnlyDictionary<string, object?> addFields,
        IReadOnlyList<UserFieldDefinition> defs)
    {
        var result = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        foreach (var entry in addFields)
        {
            var def = defs.FirstOrDefault(d =>
                string.Equals(d.Alias, entry.Key, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(d.ColumnName, entry.Key, StringComparison.OrdinalIgnoreCase));

            result[def is not null ? def.ColumnName : entry.Key] = entry.Value;
        }
        return result;
    }
}


