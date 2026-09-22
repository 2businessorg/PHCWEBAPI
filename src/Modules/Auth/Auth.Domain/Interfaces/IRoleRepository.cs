namespace Auth.Domain.Interfaces;

/// <summary>
/// Role management repository interface
/// </summary>
public interface IRoleRepository
{
    Task<bool> RoleExistsAsync(string roleName, CancellationToken cancellationToken = default);
    Task<bool> CreateRoleAsync(string roleName, CancellationToken cancellationToken = default);
    Task<bool> DeleteRoleAsync(string roleName, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> GetAllRolesAsync(CancellationToken cancellationToken = default);
}
