using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace Admin.UI.Pages.Users;

[Authorize(Roles = "Administrator")]
public class EditModel : PageModel
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<EditModel> _logger;

    public EditModel(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ILogger<EditModel> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public List<RoleViewModel> AvailableRoles { get; set; } = new();
    public UserDetailsViewModel UserDetails { get; set; } = new();

    [TempData]
    public string? SuccessMessage { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public class InputModel
    {
        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; } = string.Empty;

        public bool EmailConfirmed { get; set; }
        public bool LockoutEnabled { get; set; }
        public List<string> SelectedRoles { get; set; } = new();
    }

    public class RoleViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
    }

    public class UserDetailsViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool EmailConfirmed { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public bool LockoutEnabled { get; set; }
        public int AccessFailedCount { get; set; }
        public DateTimeOffset? LastLogin { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return NotFound();
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        // Load user details
        var userRoles = await _userManager.GetRolesAsync(user);
        
        UserDetails = new UserDetailsViewModel
        {
            Id = user.Id,
            UserName = user.UserName!,
            Email = user.Email!,
            EmailConfirmed = user.EmailConfirmed,
            LockoutEnd = user.LockoutEnd,
            LockoutEnabled = user.LockoutEnabled,
            AccessFailedCount = user.AccessFailedCount
        };

        // Populate input model
        Input = new InputModel
        {
            Email = user.Email!,
            EmailConfirmed = user.EmailConfirmed,
            LockoutEnabled = user.LockoutEnabled,
            SelectedRoles = userRoles.ToList()
        };

        await LoadRolesAsync(userRoles);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return NotFound();
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        // Reload user details for display
        var currentRoles = await _userManager.GetRolesAsync(user);
        UserDetails = new UserDetailsViewModel
        {
            Id = user.Id,
            UserName = user.UserName!,
            Email = user.Email!,
            EmailConfirmed = user.EmailConfirmed,
            LockoutEnd = user.LockoutEnd,
            LockoutEnabled = user.LockoutEnabled,
            AccessFailedCount = user.AccessFailedCount
        };

        await LoadRolesAsync(currentRoles);

        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Update email
        if (user.Email != Input.Email)
        {
            user.Email = Input.Email;
            user.NormalizedEmail = Input.Email.ToUpper();
        }

        // Update email confirmed
        user.EmailConfirmed = Input.EmailConfirmed;

        // Update lockout
        user.LockoutEnabled = Input.LockoutEnabled;
        if (!Input.LockoutEnabled && user.LockoutEnd.HasValue)
        {
            user.LockoutEnd = null; // Remove lockout
        }

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            foreach (var error in updateResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return Page();
        }

        // Update roles
        var rolesToAdd = Input.SelectedRoles?.Except(currentRoles).ToList() ?? new List<string>();
        var rolesToRemove = currentRoles.Except(Input.SelectedRoles ?? new List<string>()).ToList();

        if (rolesToRemove.Any())
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
            if (!removeResult.Succeeded)
            {
                _logger.LogWarning("Failed to remove roles from user {UserName}", user.UserName);
            }
        }

        if (rolesToAdd.Any())
        {
            var addResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
            if (!addResult.Succeeded)
            {
                _logger.LogWarning("Failed to add roles to user {UserName}", user.UserName);
            }
        }

        _logger.LogInformation("User {UserName} updated successfully by {Admin}", 
            user.UserName, User.Identity?.Name);

        SuccessMessage = $"Utilizador '{user.UserName}' atualizado com sucesso!";
        return RedirectToPage("./Index");
    }

    private async Task LoadRolesAsync(IList<string> userRoles)
    {
        AvailableRoles = _roleManager.Roles
            .Select(r => new RoleViewModel
            {
                Name = r.Name!,
                Description = GetRoleDescription(r.Name!),
                IsSelected = userRoles.Contains(r.Name!)
            })
            .ToList();
    }

    private static string GetRoleDescription(string roleName)
    {
        return roleName switch
        {
            "Administrator" => "Acesso total ao sistema",
            "InternalUser" => "Utilizador interno da empresa",
            "ApiUser" => "Acesso via API",
            "AuditViewer" => "Visualizar logs de auditoria",
            _ => "Role personalizada"
        };
    }
}
