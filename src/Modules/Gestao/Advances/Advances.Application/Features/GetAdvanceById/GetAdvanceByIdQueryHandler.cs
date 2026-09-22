using Advances.Application.DTOs;
using Advances.Application.Mappings;
using Advances.Domain.Repositories;
using MediatR;
using Shared.Kernel.MultiTenancy;
using Shared.Kernel.UserFields;

namespace Advances.Application.Features.GetAdvanceById;

/// <summary>
/// Handler para obter um Adiantamento por chave composta
/// </summary>
public class GetAdvanceByIdQueryHandler : IRequestHandler<GetAdvanceByIdQuery, AdvanceOutputDTO?>
{
    private readonly IAdvanceRepository _advanceRepository;
    private readonly IUserFieldService? _userFieldService;
    private readonly ITenantContext? _tenantContext;

    public GetAdvanceByIdQueryHandler(
        IAdvanceRepository advanceRepository,
        IUserFieldService? userFieldService = null,
        ITenantContext? tenantContext = null)
    {
        _advanceRepository = advanceRepository;
        _userFieldService = userFieldService;
        _tenantContext = tenantContext;
    }

    /// <inheritdoc />
    public async Task<AdvanceOutputDTO?> Handle(GetAdvanceByIdQuery request, CancellationToken cancellationToken)
    {
        var rd = await _advanceRepository.GetByKeyAsync(request.Ndoc, request.Rno, request.Rdano, cancellationToken);
        if (rd is null) return null;

        var bankAccountName = await _advanceRepository.GetBankAccountNameAsync(rd.Contado, cancellationToken);
        var dto = AdvanceMapper.FromRd(rd, bankAccountName);

        // Carregar addFields do rd
        if (_userFieldService is not null && _tenantContext?.AppLicenseStamp is not null)
        {
            var defs = await _userFieldService.GetFieldsAsync(
                _tenantContext.AppLicenseStamp, "Advances", "rd", cancellationToken);

            if (defs.Count > 0)
            {
                var cols = defs.Select(f => f.ColumnName).ToList();
                var raw = await _advanceRepository.GetUserFieldValuesAsync(rd.Rdstamp, cols, cancellationToken);
                if (raw.Count > 0)
                {
                    var result = new Dictionary<string, object?>(raw.Count);
                    foreach (var def in defs)
                        if (raw.TryGetValue(def.ColumnName, out var val))
                            result[def.Alias] = UserFieldValueFormatter.FormatForOutput(val, def.FieldType);
                    dto.AddFields = result.Count > 0 ? result : null;
                }
            }
        }

        return dto;
    }
}
