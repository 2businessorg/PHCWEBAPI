using Shared.Kernel.Responses;

namespace Receipts.Presentation.REST.Services;

/// <summary>
/// Contrato para construção de links HATEOAS do módulo Receipts
/// </summary>
public interface IReceiptsLinkBuilder
{
    HATEOASLink GetListLink(int page, int pageSize, string? filterSuffix = null);
    HATEOASLink GetNextLink(int nextPage, int pageSize, string? filterSuffix = null);
    HATEOASLink GetLastLink(int lastPage, int pageSize, string? filterSuffix = null);
    HATEOASLink GetCreateLink();
    HATEOASLink GetTypesLink();
    HATEOASLink GetDetailByKeyLink(decimal ndoc, decimal rno, decimal reano);
}
