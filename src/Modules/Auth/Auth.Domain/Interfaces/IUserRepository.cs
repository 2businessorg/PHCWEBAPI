namespace Auth.Domain.Interfaces;

/// <summary>
/// User repository interface for user management operations.
/// This abstraction decouples from specific identity provider implementation.
/// </summary>
public interface IUserRepository
{
    Task<UserInfo?> FindByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<UserInfo?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> CheckPasswordAsync(string username, string password, CancellationToken cancellationToken = default);
    Task<(bool Success, string UserId, IEnumerable<string> Errors)> CreateUserAsync(string username, string email, string password, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> GetUserRolesAsync(string username, CancellationToken cancellationToken = default);
    Task<bool> AddToRoleAsync(string username, string role, CancellationToken cancellationToken = default);
    Task<bool> RemoveFromRoleAsync(string username, string role, CancellationToken cancellationToken = default);
}

/// <summary>
/// User information DTO for domain layer
/// </summary>
public class UserInfo
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool EmailConfirmed { get; set; }
    public bool IsLockedOut { get; set; }
}
