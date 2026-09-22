using Auth.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Repositories;

/// <summary>
/// User repository implementation using ASP.NET Identity
/// </summary>
public class IdentityUserRepository : IUserRepository
{
    private readonly UserManager<IdentityUser> _userManager;

    public IdentityUserRepository(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<UserInfo?> FindByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByNameAsync(username);
        return user == null ? null : MapToUserInfo(user);
    }

    public async Task<UserInfo?> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        return user == null ? null : MapToUserInfo(user);
    }

    public async Task<bool> CheckPasswordAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByNameAsync(username);
        if (user == null) return false;
        
        return await _userManager.CheckPasswordAsync(user, password);
    }

    public async Task<(bool Success, string UserId, IEnumerable<string> Errors)> CreateUserAsync(
        string username, 
        string email, 
        string password, 
        CancellationToken cancellationToken = default)
    {
        var user = new IdentityUser
        {
            UserName = username,
            Email = email,
            EmailConfirmed = false
        };

        var result = await _userManager.CreateAsync(user, password);
        
        return (
            result.Succeeded,
            result.Succeeded ? user.Id : string.Empty,
            result.Errors.Select(e => e.Description)
        );
    }

    public async Task<IEnumerable<string>> GetUserRolesAsync(string username, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByNameAsync(username);
        if (user == null) return new List<string>();
        
        return await _userManager.GetRolesAsync(user);
    }

    public async Task<bool> AddToRoleAsync(string username, string role, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByNameAsync(username);
        if (user == null) return false;
        
        var result = await _userManager.AddToRoleAsync(user, role);
        return result.Succeeded;
    }

    public async Task<bool> RemoveFromRoleAsync(string username, string role, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByNameAsync(username);
        if (user == null) return false;
        
        var result = await _userManager.RemoveFromRoleAsync(user, role);
        return result.Succeeded;
    }

    private static UserInfo MapToUserInfo(IdentityUser user)
    {
        return new UserInfo
        {
            Id = user.Id,
            Username = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            EmailConfirmed = user.EmailConfirmed,
            IsLockedOut = user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow
        };
    }
}
