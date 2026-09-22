namespace Invoices.Application.DTOs;

/// <summary>
/// Resultado paginado de faturas.
/// </summary>
public record GetAllInvoicesResultDTO(
    int TotalItems,
    int CurrentPage,
    int PageSize,
    IReadOnlyList<InvoiceOutputDTO> Items
);
