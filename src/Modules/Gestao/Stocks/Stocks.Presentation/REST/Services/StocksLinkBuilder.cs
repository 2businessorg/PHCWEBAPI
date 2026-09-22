using Shared.Kernel.Responses;

namespace Stocks.Presentation.REST.Services;

/// <summary>
/// Implementação do builder de links HATEOAS para Stocks.
/// </summary>
public sealed class StocksLinkBuilder : IStocksLinkBuilder
{
    private const string BaseRoute = "/api/stocks";

    public HATEOASLink GetAllLink(int page, int pageSize, string? filterSuffix = null)
    {
        var href = $"{BaseRoute}?page={page}&pageSize={pageSize}{filterSuffix}";
        return new("self", href, "GET");
    }

    public HATEOASLink GetNextLink(int nextPage, int pageSize, string? filterSuffix = null)
    {
        var href = $"{BaseRoute}?page={nextPage}&pageSize={pageSize}{filterSuffix}";
        return new("next", href, "GET");
    }

    public HATEOASLink GetLastLink(int lastPage, int pageSize, string? filterSuffix = null)
    {
        var href = $"{BaseRoute}?page={lastPage}&pageSize={pageSize}{filterSuffix}";
        return new("last", href, "GET");
    }

    public HATEOASLink GetCreateLink() => new("create", BaseRoute, "POST");

    public HATEOASLink GetDetailLink(string referencia)
        => new("self", $"{BaseRoute}/{Uri.EscapeDataString(referencia)}", "GET");

    public HATEOASLink GetUpdateLink(string referencia)
        => new("update", $"{BaseRoute}/{Uri.EscapeDataString(referencia)}", "PUT");

    public HATEOASLink GetDeleteLink(string referencia)
        => new("delete", $"{BaseRoute}/{Uri.EscapeDataString(referencia)}", "DELETE");

    public HATEOASLink GetListLink() => new("list", BaseRoute, "GET");

    public HATEOASLink GetSelfPaginationLink(int page, int pageSize, string? filterSuffix = null)
    {
        var href = $"{BaseRoute}?page={page}&pageSize={pageSize}{filterSuffix}";
        return new("self", href, "GET");
    }
}
