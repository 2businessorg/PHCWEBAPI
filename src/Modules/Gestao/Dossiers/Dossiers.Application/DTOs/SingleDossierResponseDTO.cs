using global::Shared.Kernel.DTOs;

namespace Dossiers.Application.DTOs;

/// <summary>
/// Response DTO for a single dossier item with HATEOAS links
/// Uses the standard SingleItemResponseDTO&lt;T&gt; pattern
/// </summary>
public class SingleDossierResponseDTO : global::Shared.Kernel.DTOs.SingleItemResponseDTO<DossierOutputDTO>
{
}
