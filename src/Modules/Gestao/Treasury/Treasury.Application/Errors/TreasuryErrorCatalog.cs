namespace Treasury.Application.Errors;

/// <summary>
/// Standardized error codes for the Treasury module.
/// </summary>
public static class TreasuryErrorCatalog
{
    public sealed record ErrorInfo(string Code, string Description);

    public static readonly ErrorInfo Success =
        new("0000", "Operation completed successfully");

    public static readonly ErrorInfo AccountNotFound =
        new("TR001", "Treasury account '{0}' was not found");

    public static readonly ErrorInfo InvalidDateRange =
        new("TR002", "dateFrom must be less than or equal to dateTo");

    public static readonly ErrorInfo ValidationError =
        new("TR003", "Validation error");
}
