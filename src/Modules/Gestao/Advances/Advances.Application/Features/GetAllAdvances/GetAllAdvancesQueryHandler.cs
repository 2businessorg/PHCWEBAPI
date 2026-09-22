using Advances.Application.DTOs;
using Advances.Application.Mappings;
using Advances.Domain.Repositories;
using MediatR;
using Shared.Kernel.MultiTenancy;
using Shared.Kernel.UserFields;

namespace Advances.Application.Features.GetAllAdvances;

/// <summary>
/// Handler para listagem paginada de Adiantamentos
/// </summary>
public class GetAllAdvancesQueryHandler : IRequestHandler<GetAllAdvancesQuery, GetAllAdvancesResultDTO>
{
    private readonly IAdvanceRepository _advanceRepository;
    private readonly IUserFieldService? _userFieldService;
    private readonly ITenantContext? _tenantContext;

    public GetAllAdvancesQueryHandler(
        IAdvanceRepository advanceRepository,
        IUserFieldService? userFieldService = null,
        ITenantContext? tenantContext = null)
    {
        _advanceRepository = advanceRepository;
        _userFieldService = userFieldService;
        _tenantContext = tenantContext;
    }

    /// <inheritdoc />
    public async Task<GetAllAdvancesResultDTO> Handle(GetAllAdvancesQuery request, CancellationToken cancellationToken)
    {
        var (totalItems, items) = await _advanceRepository.GetAllAsync(
            request.Ndoc,
            request.Rno,
            request.Rdano,
            request.No,
            request.Page,
            request.PageSize,
            cancellationToken);

        // Carregar field defs para a tabela rd
        var rdFieldDefs = await GetFieldDefsAsync("rd", cancellationToken);

        // Batch addFields para os cabeçalhos
        Dictionary<string, Dictionary<string, object?>> rdFieldsBatch = [];
        if (rdFieldDefs.Count > 0)
        {
            var rdstamps = items.Select(r => r.Rdstamp).Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
            var cols = rdFieldDefs.Select(f => f.ColumnName).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            rdFieldsBatch = await _advanceRepository.GetUserFieldValuesBatchAsync(rdstamps, cols, cancellationToken);
        }

        var dtos = new List<AdvanceOutputDTO>();
        foreach (var rd in items)
        {
            var bankAccountName = await _advanceRepository.GetBankAccountNameAsync(rd.Contado, cancellationToken);
            var dto = AdvanceMapper.FromRd(rd, bankAccountName);

            if (rdFieldDefs.Count > 0 && rdFieldsBatch.TryGetValue(rd.Rdstamp.Trim(), out var rdRaw))
                dto.AddFields = MapToAliases(rdRaw, rdFieldDefs);

            dtos.Add(dto);
        }

        return new GetAllAdvancesResultDTO(totalItems, request.Page, request.PageSize, dtos);
    }

    private async Task<IReadOnlyList<UserFieldDefinition>> GetFieldDefsAsync(string tableName, CancellationToken ct)
    {
        if (_tenantContext?.AppLicenseStamp is null || _userFieldService is null)
            return [];
        return await _userFieldService.GetFieldsAsync(_tenantContext.AppLicenseStamp, "Advances", tableName, ct);
    }

    private static Dictionary<string, object?>? MapToAliases(
        Dictionary<string, object?> raw,
        IReadOnlyList<UserFieldDefinition> fieldDefs)
    {
        if (raw.Count == 0) return null;
        var result = new Dictionary<string, object?>(raw.Count);
        foreach (var def in fieldDefs)
            if (raw.TryGetValue(def.ColumnName, out var val))
                result[def.Alias] = UserFieldValueFormatter.FormatForOutput(val, def.FieldType);
        return result.Count > 0 ? result : null;
    }
}
