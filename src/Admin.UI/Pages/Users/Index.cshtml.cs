using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Admin.UI.Pages.Users;

[Authorize(Roles = "Administrator")]
public class IndexModel : PageModel
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public IndexModel(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public List<UserViewModel> Users { get; set; } = new();
    public List<string> AllRoles { get; set; } = new();

    [TempData]
    public string? SuccessMessage { get; set; }

    public async Task OnGetAsync()
    {
        // Load all users
        var users = _userManager.Users.ToList();
        
        // Load all roles
        AllRoles = _roleManager.Roles.Select(r => r.Name!).ToList();

        // Map to view models
        Users = new List<UserViewModel>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            Users.Add(new UserViewModel
            {
                Id = user.Id,
                UserName = user.UserName!,
                Email = user.Email!,
                EmailConfirmed = user.EmailConfirmed,
                LockoutEnd = user.LockoutEnd,
                AccessFailedCount = user.AccessFailedCount,
                Roles = roles.ToList()
            });
        }
    }

    /// <summary>
    /// 🔓 UNLOCK USER: Reset lockout and failed login attempts
    /// </summary>
    public async Task<IActionResult> OnPostUnlockAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            SuccessMessage = "❌ Erro: Utilizador não especificado.";
            return RedirectToPage();
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            SuccessMessage = "❌ Utilizador não encontrado.";
            return RedirectToPage();
        }

        // Reset lockout
        var unlockResult = await _userManager.SetLockoutEndDateAsync(user, null);
        
        // Reset failed login count
        var resetResult = await _userManager.ResetAccessFailedCountAsync(user);

        if (unlockResult.Succeeded && resetResult.Succeeded)
        {
            SuccessMessage = $"✅ Utilizador '{user.UserName}' desbloqueado com sucesso!";
        }
        else
        {
            SuccessMessage = $"❌ Erro ao desbloquear '{user.UserName}'.";
        }

        return RedirectToPage();
    }
}

public class UserViewModel
{
    public string Id { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool EmailConfirmed { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }
    public int AccessFailedCount { get; set; }
    public List<string> Roles { get; set; } = new();
}
