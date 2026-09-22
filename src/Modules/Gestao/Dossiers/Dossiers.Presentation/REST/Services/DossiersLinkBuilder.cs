using Shared.Kernel.Responses;

namespace Dossiers.Presentation.REST.Services;

public class DossiersLinkBuilder : IDossiersLinkBuilder
{
    private const string BaseRoute = "/api/dossiers";

    public HATEOASLink GetListLink(int page, int pageSize, string? filterSuffix = null)
        => new("self", $"{BaseRoute}?page={page}&pageSize={pageSize}{filterSuffix ?? string.Empty}", "GET");

    public HATEOASLink GetNextLink(int nextPage, int pageSize, string? filterSuffix = null)
        => new("next", $"{BaseRoute}?page={nextPage}&pageSize={pageSize}{filterSuffix ?? string.Empty}", "GET");

    public HATEOASLink GetLastLink(int lastPage, int pageSize, string? filterSuffix = null)
        => new("last", $"{BaseRoute}?page={lastPage}&pageSize={pageSize}{filterSuffix ?? string.Empty}", "GET");

    public HATEOASLink GetCreateLink()
        => new("create", BaseRoute, "POST");

    public HATEOASLink GetTypesLink()
        => new("types", $"{BaseRoute}/types", "GET");

    public HATEOASLink GetClientsListLink()
        => new("clients", "/api/clients", "GET");

    public HATEOASLink GetCurrenciesListLink()
        => new("currencies", "/api/tables/moedas", "GET");

    public HATEOASLink GetVatTablesListLink()
        => new("vatTables", "/api/taxasIva", "GET");

    public HATEOASLink GetDetailByKeyLink(decimal ndos, decimal obrano, decimal boano)
        => new("self", $"{BaseRoute}/{ndos}/{obrano}/{boano}", "GET");

    public HATEOASLink GetDeleteByKeyLink(decimal ndos, decimal obrano, decimal boano)
        => new("delete", $"{BaseRoute}/{ndos}/{obrano}/{boano}", "DELETE");

    public HATEOASLink GetStocksListLink()
        => new("stocks", "/api/stocks", "GET");
}
