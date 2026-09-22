namespace VatTaxes.Application.DTOs;

/// <summary>
/// Resultado paginado de taxas de IVA
/// </summary>
public record GetAllVatTaxesResultDTO(
    int TotalItems,
    int CurrentPage,
    int PageSize,
    IReadOnlyList<VatTaxOutputDTO> Items
);
