using System.Text.Json.Serialization;
using Shared.Kernel.Responses;

namespace Shared.Kernel.DTOs;

/// <summary>
/// Generic response DTO for single item endpoints with HATEOAS links.
/// Standard pattern for all singular GET endpoints across modules.
/// 
/// Usage example:
/// GET /api/stocks/{referencia} returns SingleItemResponseDTO&lt;StockOutputDTO&gt;
/// GET /api/dossiers/{doctypeId}/{year} returns SingleItemResponseDTO&lt;DossierOutputDTO&gt;
/// </summary>
/// <typeparam name="T">The type of the item being returned</typeparam>
public class SingleItemResponseDTO<T> where T : class
{
    /// <summary>
    /// The main item data
    /// </summary>
    [JsonPropertyName("item")]
    public T Item { get; set; } = null!;

    /// <summary>
    /// HATEOAS links for navigation and actions
    /// </summary>
    [JsonPropertyName("links")]
    public List<HATEOASLink> Links { get; set; } = new();
}
