using Shared.Kernel.Responses;

namespace Receipts.Presentation.REST.Services;

/// <summary>
/// Implementação de links HATEOAS para o módulo Receipts
/// </summary>
public class ReceiptsLinkBuilder : IReceiptsLinkBuilder
{
    private const string BaseRoute = "/api/receipts";

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

    public HATEOASLink GetDetailByKeyLink(decimal ndoc, decimal rno, decimal reano)
        => new("self", $"{BaseRoute}/{ndoc}/{rno}/{reano}", "GET");
}
