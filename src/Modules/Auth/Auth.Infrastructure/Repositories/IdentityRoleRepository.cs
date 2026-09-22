using Auth.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Repositories;

/// <summary>
/// Role repository implementation using ASP.NET Identity
/// </summary>
public class IdentityRoleRepository : IRoleRepository
{
    private readonly RoleManager<IdentityRole> _roleManager;

    public IdentityRoleRepository(RoleManager<IdentityRole> roleManager)
    {
        _roleManager = roleManager;
    }

    public async Task<bool> RoleExistsAsync(string roleName, CancellationToken cancellationToken = default)
    {
        return await _roleManager.RoleExistsAsync(roleName);
    }

    public async Task<bool> CreateRoleAsync(string roleName, CancellationToken cancellationToken = default)
    {
        var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
        return result.Succeeded;
    }

    public async Task<bool> DeleteRoleAsync(string roleName, CancellationToken cancellationToken = default)
    {
        var role = await _roleManager.FindByNameAsync(roleName);
        if (role == null) return false;
        
        var result = await _roleManager.DeleteAsync(role);
        return result.Succeeded;
    }

    public async Task<IEnumerable<string>> GetAllRolesAsync(CancellationToken cancellationToken = default)
    {
        return await _roleManager.Roles
            .Select(r => r.Name!)
            .ToListAsync(cancellationToken);
    }
}
