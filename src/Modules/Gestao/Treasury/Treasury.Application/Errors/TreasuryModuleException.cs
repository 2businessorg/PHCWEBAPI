namespace Treasury.Application.Errors;

/// <summary>
/// Structured Treasury module exception.
/// </summary>
public class TreasuryModuleException : Exception
{
    public string ErrorCode { get; }

    public TreasuryModuleException(string errorCode, string message)
        : base(message)
    {
        ErrorCode = errorCode;
    }
}
