using Receipts.Application.DTOs;

namespace Receipts.Application.Mappings;

/// <summary>
/// Constrói o payload enviado ao PHC WEB para emissão de recibo (script insertReAPI).
/// O script resolve internamente: ccstamp, tesouraria e data de processamento.
/// </summary>
public static class ReceiptPhcMapper
{
    /// <summary>
    /// Converte o DTO de input num objecto anónimo para o script insertReAPI do PHC WEB.
    /// </summary>
    public static object ToPhcPayload(
        CreateReceiptInputDTO dto,
        string? createdBy = null,
        Dictionary<string, object?>? resolvedHeaderAddFields = null,
        List<Dictionary<string, object?>?>? resolvedLineAddFields = null)
    {
        return new
        {
            docTypeId = dto.Ndoc,
            clientId = dto.No,
            bankAccountId = dto.Contado,
            createdBy = createdBy ?? "PHCAPI",
            addFields = resolvedHeaderAddFields ?? dto.AddFields,
            lines = dto.Linhas.Select((l, i) => new
            {
                invoiceNumber = l.InvoiceNumber,
                invoiceTypeId = l.InvoiceTypeId,
                invoiceYear = l.InvoiceYear,
                amount = l.Amount,
                addFields = resolvedLineAddFields?[i] ?? l.AddFields
            }).ToList()
        };
    }
}
