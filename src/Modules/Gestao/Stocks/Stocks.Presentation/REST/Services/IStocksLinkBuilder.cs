using Shared.Kernel.Responses;

namespace Stocks.Presentation.REST.Services;

/// <summary>
/// Builder para centralizar geração de links HATEOAS do módulo Stocks.
/// </summary>
public interface IStocksLinkBuilder
{
    HATEOASLink GetAllLink(int page, int pageSize, string? filterSuffix = null);
    HATEOASLink GetNextLink(int nextPage, int pageSize, string? filterSuffix = null);
    HATEOASLink GetLastLink(int lastPage, int pageSize, string? filterSuffix = null);
    HATEOASLink GetCreateLink();
    HATEOASLink GetDetailLink(string referencia);
    HATEOASLink GetUpdateLink(string referencia);
    HATEOASLink GetDeleteLink(string referencia);
    HATEOASLink GetListLink();
    HATEOASLink GetSelfPaginationLink(int page, int pageSize, string? filterSuffix = null);
}
