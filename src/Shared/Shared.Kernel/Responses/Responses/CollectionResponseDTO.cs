using System.Text.Json.Serialization;

namespace Shared.Kernel.Responses;

public sealed record CollectionResponseDTO
{
    [JsonPropertyName("items")]
    public IEnumerable<object> Items { get; init; } = Enumerable.Empty<object>();

    [JsonPropertyName("meta")]
    public PaginationMeta Meta { get; init; } = new(0,0,0,0,0);

    [JsonPropertyName("links")]
    public IEnumerable<HATEOASLink> Links { get; init; } = Enumerable.Empty<HATEOASLink>();

    public CollectionResponseDTO(IEnumerable<object> items, PaginationMeta meta, IEnumerable<HATEOASLink>? links = null)
    {
        Items = items ?? Enumerable.Empty<object>();
        Meta = meta;
        Links = links ?? Enumerable.Empty<HATEOASLink>();
    }

    public static CollectionResponseDTO FromPaged<T>(IEnumerable<T> items, int totalItems, int page, int pageSize, IEnumerable<HATEOASLink>? links = null)
    {
        var list = items?.Cast<object>().ToList() ?? new List<object>();
        var itemCount = list.Count;
        var totalPages = pageSize > 0 ? (int)Math.Ceiling(totalItems / (double)pageSize) : 1;
        var meta = new PaginationMeta(totalItems, itemCount, pageSize, totalPages, page);
        return new CollectionResponseDTO(list, meta, links);
    }
}
