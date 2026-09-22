namespace Receipts.Application.DTOs;

/// <summary>
/// Resultado paginado de consulta de Recibos
/// </summary>
public record GetAllReceiptsResultDTO(
    int TotalItems,
    int CurrentPage,
    int PageSize,
    IReadOnlyList<ReceiptOutputDTO> Items);
