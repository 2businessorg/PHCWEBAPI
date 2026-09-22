using System.Text.Json.Serialization;

namespace Shared.Kernel.Responses;

/// <summary>
/// DTO para resposta com um único item (não paginado).
/// Retorna um objeto individual em vez de uma coleção.
/// </summary>
public sealed record SingleResponseDTO
{
    [JsonPropertyName("item")]
    public object Item { get; init; } = new();

    [JsonPropertyName("meta")]
    public PaginationMeta Meta { get; init; } = new(1, 1, 1, 1, 1);

    [JsonPropertyName("links")]
    public IEnumerable<HATEOASLink> Links { get; init; } = Enumerable.Empty<HATEOASLink>();

    public SingleResponseDTO(object item, PaginationMeta meta, IEnumerable<HATEOASLink>? links = null)
    {
        Item = item ?? new();
        Meta = meta;
        Links = links ?? Enumerable.Empty<HATEOASLink>();
    }

    /// <summary>
    /// Cria uma resposta com um único item.
    /// </summary>
    public static SingleResponseDTO FromItem<T>(T item, IEnumerable<HATEOASLink>? links = null)
    {
        var meta = new PaginationMeta(1, 1, 1, 1, 1);
        return new SingleResponseDTO(item as object ?? new(), meta, links);
    }
}
