using Shared.Kernel.Responses;

namespace Clients.Presentation.REST.Services;

/// <summary>
/// Implementação do builder de links HATEOAS para Clients
/// </summary>
public sealed class ClientsLinkBuilder : IClientsLinkBuilder
{
    private const string BaseRoute = "/api/clients";

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

    public HATEOASLink GetCreateLink()
    {
        return new("create", BaseRoute, "POST");
    }

    public HATEOASLink GetDetailLink(decimal no, decimal estab = 0)
    {
        var href = $"{BaseRoute}/{no}/{estab}";
        return new("self", href, "GET");
    }

    public HATEOASLink GetUpdateLink(decimal no, decimal estab = 0)
    {
        var href = $"{BaseRoute}/{no}/{estab}";
        return new("update", href, "PUT");
    }

    public HATEOASLink GetDeleteLink(decimal no, decimal estab = 0)
    {
        var href = $"{BaseRoute}/{no}/{estab}";
        return new("delete", href, "DELETE");
    }

    public HATEOASLink GetListLink()
    {
        return new("list", BaseRoute, "GET");
    }

    public HATEOASLink GetSelfPaginationLink(int page, int pageSize, string? filterSuffix = null)
    {
        var href = $"{BaseRoute}?page={page}&pageSize={pageSize}{filterSuffix}";
        return new("self", href, "GET");
    }
}
