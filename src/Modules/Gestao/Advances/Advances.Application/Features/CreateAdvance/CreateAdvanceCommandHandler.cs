using Advances.Application.DTOs;
using Advances.Application.Errors;
using Advances.Application.Mappings;
using Advances.Domain.ExternalServices;
using Advances.Domain.Repositories;
using MediatR;
using Shared.Kernel.MultiTenancy;
using Shared.Kernel.UserFields;
using System.Text.Json;

namespace Advances.Application.Features.CreateAdvance;

/// <summary>
/// Handler para criação de Adiantamento via PHC WEB (script insertRdAPI).
///
/// Validações de negócio:
/// - Série do adiantamento (ndoc) deve existir em tsrd
/// - Cliente (no) deve existir em cl
///
/// O PHC WEB é responsável por:
/// - Lookup do banco, olcodigo e cm contabilístico
/// - Geração da numeração e stamp do RD
/// - Persistência na tabela rd
/// </summary>
public class CreateAdvanceCommandHandler : IRequestHandler<CreateAdvanceCommand, CreateAdvanceOutputDTO>
{
    private readonly IPhcWebServiceAdvances _phcWebService;
    private readonly IAdvanceTypeRepository _advanceTypeRepository;
    private readonly IClientRepository _clientRepository;
    private readonly ITenantContext? _tenantContext;
    private readonly IUserFieldService? _userFieldService;

    public CreateAdvanceCommandHandler(
        IPhcWebServiceAdvances phcWebService,
        IAdvanceTypeRepository advanceTypeRepository,
        IClientRepository clientRepository,
        ITenantContext? tenantContext = null,
        IUserFieldService? userFieldService = null)
    {
        _phcWebService = phcWebService;
        _advanceTypeRepository = advanceTypeRepository;
        _clientRepository = clientRepository;
        _tenantContext = tenantContext;
        _userFieldService = userFieldService;
    }

    /// <inheritdoc />
    public async Task<CreateAdvanceOutputDTO> Handle(CreateAdvanceCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;

        // 1. Validar série do adiantamento
        var serieExists = await _advanceTypeRepository.ExistsByNdocAsync(dto.Ndoc, cancellationToken);
        if (!serieExists)
        {
            throw new AdvancesModuleException(
                AdvancesErrorCatalog.InvalidAdvanceType.Code,
                string.Format(AdvancesErrorCatalog.InvalidAdvanceType.Description, dto.Ndoc));
        }

        // 2. Validar cliente
        var clientExists = await _clientRepository.ExistsByNoAsync(dto.No, cancellationToken);
        if (!clientExists)
        {
            throw new AdvancesModuleException(
                AdvancesErrorCatalog.ClientNotFound.Code,
                string.Format(AdvancesErrorCatalog.ClientNotFound.Description, dto.No));
        }

        // 3. Resolver aliases de addFields para nomes de colunas reais
        Dictionary<string, object?>? resolvedAddFields = null;

        if (_userFieldService is not null && _tenantContext?.AppLicenseStamp is not null)
        {
            var rdDefs = await _userFieldService.GetFieldsAsync(
                _tenantContext.AppLicenseStamp, "Advances", "rd", cancellationToken);

            if (dto.AddFields is { Count: > 0 })
                resolvedAddFields = ResolveAliases(dto.AddFields, rdDefs);
        }

        // 4. Construir payload para o PHC WEB
        var phcPayload = AdvancePhcMapper.ToPhcPayload(dto, request.CreatedBy, resolvedAddFields);
        var payloadJson = JsonSerializer.Serialize(phcPayload);

        // 5. Invocar script no PHC WEB
        string rawResponse;
        try
        {
            rawResponse = await _phcWebService.CreateAdvanceAsync(payloadJson, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new AdvancesModuleException(
                AdvancesErrorCatalog.PhcWebIntegrationError.Code,
                string.Format(AdvancesErrorCatalog.PhcWebIntegrationError.Description, ex.Message));
        }

        // 6. Mapear resposta do PHC WEB para DTO de saída
        return AdvanceMapper.FromPhcCreateResponse(rawResponse, dto);
    }

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
