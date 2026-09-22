using Shared.Kernel.DTOs;

namespace Invoices.Application.DTOs;

/// <summary>
/// Response DTO para retorno de uma fatura única com HATEOAS links.
/// </summary>
public class SingleInvoiceResponseDTO : SingleItemResponseDTO<InvoiceOutputDTO>
{
}
