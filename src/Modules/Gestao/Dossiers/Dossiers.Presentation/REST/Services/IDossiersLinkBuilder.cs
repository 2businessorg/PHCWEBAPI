using Shared.Kernel.Responses;

namespace Dossiers.Presentation.REST.Services;

public interface IDossiersLinkBuilder
{
    HATEOASLink GetListLink(int page, int pageSize, string? filterSuffix = null);
    HATEOASLink GetNextLink(int nextPage, int pageSize, string? filterSuffix = null);
    HATEOASLink GetLastLink(int lastPage, int pageSize, string? filterSuffix = null);
    HATEOASLink GetCreateLink();
    HATEOASLink GetTypesLink();
    HATEOASLink GetClientsListLink();
    HATEOASLink GetCurrenciesListLink();
    HATEOASLink GetVatTablesListLink();
    HATEOASLink GetDetailByKeyLink(decimal ndos, decimal obrano, decimal boano);
    HATEOASLink GetDeleteByKeyLink(decimal ndos, decimal obrano, decimal boano);
    HATEOASLink GetStocksListLink();
}
