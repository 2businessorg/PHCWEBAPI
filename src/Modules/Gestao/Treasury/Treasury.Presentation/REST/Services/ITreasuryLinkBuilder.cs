using Shared.Kernel.Responses;

namespace Treasury.Presentation.REST.Services;

/// <summary>
/// HATEOAS links for treasury endpoints.
/// </summary>
public interface ITreasuryLinkBuilder
{
    HATEOASLink GetMovementsLink(
        string accountName,
        DateOnly dateFrom,
        DateOnly dateTo,
        int page = 1,
        int pageSize = 500);

    HATEOASLink GetSummaryLink(string accountName, DateOnly dateFrom, DateOnly dateTo);

    HATEOASLink GetMatchesLink(string accountName, DateOnly dateFrom, DateOnly dateTo);

    HATEOASLink GetAccountsLink(
        string? nameContains = null,
        bool includeInactive = false,
        int page = 1,
        int pageSize = 50);

    HATEOASLink GetAccountLink(string accountName);
}
