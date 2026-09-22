using System.Security.Claims;

namespace Auth.Domain.Interfaces;

/// <summary>
/// Token generation and validation service.
/// This abstraction allows for different token providers (JWT, OAuth, etc.)
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generates a token for the given user claims
    /// </summary>
    string GenerateToken(IEnumerable<Claim> claims);
    
    /// <summary>
    /// Validates a token and returns claims if valid
    /// </summary>
    Task<ClaimsPrincipal?> ValidateTokenAsync(string token);
    
    /// <summary>
    /// Gets token expiration time
    /// </summary>
    DateTime GetTokenExpiration();
}
