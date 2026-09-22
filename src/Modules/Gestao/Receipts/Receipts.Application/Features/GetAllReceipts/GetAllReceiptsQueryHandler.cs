using MediatR;
using Receipts.Application.DTOs;
using Receipts.Application.Mappings;
using Receipts.Domain.Repositories;
using Shared.Kernel.MultiTenancy;
using Shared.Kernel.UserFields;

namespace Receipts.Application.Features.GetAllReceipts;

/// <summary>
/// Handler para listagem paginada de Recibos
/// </summary>
public class GetAllReceiptsQueryHandler : IRequestHandler<GetAllReceiptsQuery, GetAllReceiptsResultDTO>
{
    private readonly IReceiptRepository _receiptRepository;
    private readonly IUserFieldService? _userFieldService;
    private readonly ITenantContext? _tenantContext;

    /// <summary>
    /// Inicializa o handler com as dependências necessárias.
    /// </summary>
    public GetAllReceiptsQueryHandler(
        IReceiptRepository receiptRepository,
        IUserFieldService? userFieldService = null,
        ITenantContext? tenantContext = null)
    {
        _receiptRepository = receiptRepository;
        _userFieldService = userFieldService;
        _tenantContext = tenantContext;
    }

    /// <inheritdoc />
    public async Task<GetAllReceiptsResultDTO> Handle(GetAllReceiptsQuery request, CancellationToken cancellationToken)
    {
        var (totalItems, items) = await _receiptRepository.GetAllAsync(
            request.Ndoc,
            request.Rno,
            request.Reano,
            request.No,
            request.Page,
            request.PageSize,
            cancellationToken);

        // Carregar field defs uma vez
        var reFieldDefs = await GetFieldDefsAsync("re", cancellationToken);
        var rlFieldDefs = request.IncludeLines
            ? await GetFieldDefsAsync("rl", cancellationToken)
            : (IReadOnlyList<UserFieldDefinition>)[];

        // Batch addFields para os cabeçalhos
        Dictionary<string, Dictionary<string, object?>> reFieldsBatch = [];
        if (reFieldDefs.Count > 0)
        {
            var restamps = items.Select(r => r.Restamp).Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
            var cols = reFieldDefs.Select(f => f.ColumnName).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            reFieldsBatch = await _receiptRepository.GetUserFieldValuesBatchAsync(restamps, cols, cancellationToken);
        }

        var dtos = new List<ReceiptOutputDTO>();
        foreach (var re in items)
        {
            var bankAccountName = await _receiptRepository.GetBankAccountNameAsync(re.Contado, cancellationToken);
            var dto = ReceiptMapper.FromRe(re, bankAccountName);

            if (reFieldDefs.Count > 0 && reFieldsBatch.TryGetValue(re.Restamp.Trim(), out var reRaw))
                dto.AddFields = MapToAliases(reRaw, reFieldDefs);

            if (request.IncludeLines)
            {
                var lines = await _receiptRepository.GetLinesByRestampAsync(re.Restamp, cancellationToken);

                // Batch addFields para as linhas deste cabeçalho
                Dictionary<string, Dictionary<string, object?>> rlFieldsBatch = [];
                if (rlFieldDefs.Count > 0)
                {
                    var rlstamps = lines.Select(l => l.Rlstamp).Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
                    var cols = rlFieldDefs.Select(f => f.ColumnName).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                    rlFieldsBatch = await _receiptRepository.GetUserFieldValuesRlBatchAsync(rlstamps, cols, cancellationToken);
                }

                dto.Lines = new List<ReceiptLineOutputDTO>();
                foreach (var rl in lines)
                {
                    var invoiceTypeId = await _receiptRepository.GetInvoiceSeriesIdAsync(rl.Ccstamp, cancellationToken);
                    var lineDto = ReceiptMapper.FromRl(rl, invoiceTypeId);

                    if (rlFieldDefs.Count > 0 && rlFieldsBatch.TryGetValue(rl.Rlstamp.Trim(), out var rlRaw))
                        lineDto.AddFields = MapToAliases(rlRaw, rlFieldDefs);

                    dto.Lines.Add(lineDto);
                }
            }

            dtos.Add(dto);
        }

        return new GetAllReceiptsResultDTO(totalItems, request.Page, request.PageSize, dtos);
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
