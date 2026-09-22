namespace Shared.Abstractions.ExternalServices;

/// <summary>
/// Provides PHC WEB credentials (URL, username, password) resolved from current tenant context
/// Shared interface for all modules that need to integrate with PHC Web
/// </summary>
public interface IPhcWebCredentialsProvider
{
    /// <summary>
    /// Gets the PHC WEB credentials for the current tenant
    /// </summary>
    Task<PhcWebCredentials> GetCredentialsAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// PHC Web credentials data transfer object
/// </summary>
public class PhcWebCredentials
{
    /// <summary>
    /// PHC WEB service URL
    /// </summary>
    public string PhcWebUrl { get; set; } = string.Empty;

    /// <summary>
    /// PHC WEB service username
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// PHC WEB service password
    /// </summary>
    public string Password { get; set; } = string.Empty;
}
