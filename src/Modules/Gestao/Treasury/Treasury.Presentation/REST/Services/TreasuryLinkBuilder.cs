using Shared.Kernel.Responses;

namespace Treasury.Presentation.REST.Services;

/// <summary>
/// HATEOAS links for the Treasury API.
/// </summary>
public class TreasuryLinkBuilder : ITreasuryLinkBuilder
{
    private const string MovementsRoute = "/api/treasury/bank-reconciliation/movements";
    private const string SummaryRoute = "/api/treasury/bank-reconciliation/summary";
    private const string MatchesRoute = "/api/treasury/bank-reconciliation/matches";
    private const string AccountsRoute = "/api/treasury/accounts";

    public HATEOASLink GetMovementsLink(
        string accountName,
        DateOnly dateFrom,
        DateOnly dateTo,
        int page = 1,
        int pageSize = 500)
    {
        var href =
            $"{MovementsRoute}?accountName={Uri.EscapeDataString(accountName)}" +
            $"&dateFrom={dateFrom:yyyy-MM-dd}&dateTo={dateTo:yyyy-MM-dd}" +
            $"&page={page}&pageSize={pageSize}";

        return new("movements", href, "GET");
    }

    public HATEOASLink GetSummaryLink(string accountName, DateOnly dateFrom, DateOnly dateTo)
    {
        var href =
            $"{SummaryRoute}?accountName={Uri.EscapeDataString(accountName)}" +
            $"&dateFrom={dateFrom:yyyy-MM-dd}&dateTo={dateTo:yyyy-MM-dd}";

        return new("summary", href, "GET");
    }

    public HATEOASLink GetMatchesLink(string accountName, DateOnly dateFrom, DateOnly dateTo)
    {
        var href =
            $"{MatchesRoute}?accountName={Uri.EscapeDataString(accountName)}" +
            $"&dateFrom={dateFrom:yyyy-MM-dd}&dateTo={dateTo:yyyy-MM-dd}";

        return new("matches", href, "GET");
    }

    public HATEOASLink GetAccountsLink(
        string? nameContains = null,
        bool includeInactive = false,
        int page = 1,
        int pageSize = 50)
    {
        var href =
            $"{AccountsRoute}?includeInactive={includeInactive}&page={page}&pageSize={pageSize}";

        if (!string.IsNullOrWhiteSpace(nameContains))
        {
            href += $"&nameContains={Uri.EscapeDataString(nameContains)}";
        }

        return new("accounts", href, "GET");
    }

    public HATEOASLink GetAccountLink(string accountName)
        => new("account", $"{AccountsRoute}/{Uri.EscapeDataString(accountName)}", "GET");
}
