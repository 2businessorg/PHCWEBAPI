namespace Treasury.Application.DTOs;

/// <summary>
/// Treasury account summary included in reconciliation responses.
/// </summary>
public record TreasuryAccountOutputDTO
{
    /// <summary>Account display name.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Internal PHC account code (<c>noconta</c>).</summary>
    public decimal Code { get; init; }

    /// <summary>Bank / cash account number.</summary>
    public string AccountNumber { get; init; } = string.Empty;

    /// <summary>Whether the account is inactive.</summary>
    public bool Inactive { get; init; }

    /// <summary>Account currency code.</summary>
    public string Currency { get; init; } = string.Empty;

    /// <summary>Current PHC balance.</summary>
    public decimal Balance { get; init; }
}
