using MediatR;
using Receipts.Application.DTOs;
using Receipts.Application.Mappings;
using Receipts.Domain.Repositories;
using Shared.Kernel.MultiTenancy;
using Shared.Kernel.UserFields;

namespace Receipts.Application.Features.GetReceiptById;

/// <summary>
/// Handler para obter um Recibo por chave composta, incluindo as linhas
/// </summary>
public class GetReceiptByIdQueryHandler : IRequestHandler<GetReceiptByIdQuery, ReceiptOutputDTO?>
{
    private readonly IReceiptRepository _receiptRepository;
    private readonly IUserFieldService? _userFieldService;
    private readonly ITenantContext? _tenantContext;

    /// <summary>
    /// Inicializa o handler com as dependências necessárias.
    /// </summary>
    public GetReceiptByIdQueryHandler(
        IReceiptRepository receiptRepository,
        IUserFieldService? userFieldService = null,
        ITenantContext? tenantContext = null)
    {
        _receiptRepository = receiptRepository;
        _userFieldService = userFieldService;
        _tenantContext = tenantContext;
    }

    /// <inheritdoc />
    public async Task<ReceiptOutputDTO?> Handle(GetReceiptByIdQuery request, CancellationToken cancellationToken)
    {
        var aggregate = await _receiptRepository.GetByKeyAsync(
            request.Ndoc, request.Rno, request.Reano, cancellationToken);

        if (aggregate is null)
            return null;

        var bankAccountName = await _receiptRepository.GetBankAccountNameAsync(aggregate.Header.Contado, cancellationToken);
        var dto = ReceiptMapper.FromRe(aggregate.Header, bankAccountName);

        // AddFields — cabeçalho (re)
        var reFieldDefs = await GetFieldDefsAsync("re", cancellationToken);
        if (reFieldDefs.Count > 0)
        {
            var cols = reFieldDefs.Select(f => f.ColumnName).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            var raw = await _receiptRepository.GetUserFieldValuesAsync(aggregate.Header.Restamp, cols, cancellationToken);
            dto.AddFields = MapToAliases(raw, reFieldDefs);
        }

        // Linhas (rl)
        var rlFieldDefs = await GetFieldDefsAsync("rl", cancellationToken);
        dto.Lines = new List<ReceiptLineOutputDTO>();
        foreach (var rl in aggregate.Lines)
        {
            var invoiceTypeId = await _receiptRepository.GetInvoiceSeriesIdAsync(rl.Ccstamp, cancellationToken);
            var lineDto = ReceiptMapper.FromRl(rl, invoiceTypeId);

            if (rlFieldDefs.Count > 0)
            {
                var cols = rlFieldDefs.Select(f => f.ColumnName).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                var raw = await _receiptRepository.GetUserFieldValuesRlAsync(rl.Rlstamp, cols, cancellationToken);
                lineDto.AddFields = MapToAliases(raw, rlFieldDefs);
            }

            dto.Lines.Add(lineDto);
        }

        return dto;
    }

    private async Task<IReadOnlyList<UserFieldDefinition>> GetFieldDefsAsync(string tableName, CancellationToken ct)
    {
        if (_tenantContext?.AppLicenseStamp is null || _userFieldService is null)
            return [];
        return await _userFieldService.GetFieldsAsync(_tenantContext.AppLicenseStamp, "Receipts", tableName, ct);
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
