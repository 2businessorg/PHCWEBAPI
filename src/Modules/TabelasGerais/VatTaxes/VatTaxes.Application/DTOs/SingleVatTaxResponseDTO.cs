using Shared.Kernel.DTOs;

namespace VatTaxes.Application.DTOs;

/// <summary>
/// Response DTO para single VAT Tax endpoint com HATEOAS links
/// </summary>
public class SingleVatTaxResponseDTO : SingleItemResponseDTO<VatTaxOutputDTO>
{
}
