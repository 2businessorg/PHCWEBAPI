namespace Treasury.Domain.Entities;

/// <summary>
/// Treasury account / cash account.
/// PHC table <c>bl</c> — dictionary name: "Contas de Tesouraria".
/// </summary>
public class TreasuryAccount
{
    /// <summary>Internal account code (<c>noconta</c>).</summary>
    public decimal AccountCode { get; set; }

    /// <summary>Account display name (<c>banco</c>).</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Bank / cash account number (<c>conta</c>).</summary>
    public string AccountNumber { get; set; } = string.Empty;

    /// <summary>Whether the account is inactive (<c>inactivo</c>).</summary>
    public bool Inactive { get; set; }

    /// <summary>Account currency (<c>moeda</c>).</summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>Current PHC balance (<c>saldo</c>).</summary>
    public decimal Balance { get; set; }
}
