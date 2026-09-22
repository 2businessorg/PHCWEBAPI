using System.Text.Json.Serialization;

namespace Shared.Kernel.Responses;

public sealed record PaginationMeta
{
    [JsonPropertyName("totalItems")]
    public int TotalItems { get; init; }

    [JsonPropertyName("itemCount")]
    public int ItemCount { get; init; }

    [JsonPropertyName("pageSize")]
    public int PageSize { get; init; }

    [JsonPropertyName("totalPages")]
    public int TotalPages { get; init; }

    [JsonPropertyName("currentPage")]
    public int CurrentPage { get; init; }

    public PaginationMeta(int totalItems, int itemCount, int pageSize, int totalPages, int currentPage)
    {
        TotalItems = totalItems;
        ItemCount = itemCount;
        PageSize = pageSize;
        TotalPages = totalPages;
        CurrentPage = currentPage;
    }
}
