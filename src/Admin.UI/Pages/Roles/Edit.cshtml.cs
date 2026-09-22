using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace Admin.UI.Pages.Roles;

[Authorize(Roles = "Administrator")]
public class EditModel : PageModel
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ILogger<EditModel> _logger;

    public EditModel(
        RoleManager<IdentityRole> roleManager,
        UserManager<IdentityUser> userManager,
        ILogger<EditModel> logger)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public RoleDetailsViewModel RoleDetails { get; set; } = new();

    [TempData]
    public string? SuccessMessage { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public class InputModel
    {
        [Required(ErrorMessage = "Nome da role é obrigatório")]
        [Display(Name = "Nome da Role")]
        [RegularExpression(@"^[A-Z][a-zA-Z0-9]*$", ErrorMessage = "Use PascalCase (ex: ContentEditor)")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 50 caracteres")]
        public string Name { get; set; } = string.Empty;
    }

    public class RoleDetailsViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int UserCount { get; set; }
        public bool IsSystemRole { get; set; }
        public List<string> Users { get; set; } = new();
    }

    public async Task<IActionResult> OnGetAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return NotFound();
        }

        var role = await _roleManager.FindByIdAsync(id);
        if (role == null)
        {
            return NotFound();
        }

        var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
        var isSystemRole = IsSystemRole(role.Name!);

        RoleDetails = new RoleDetailsViewModel
        {
            Id = role.Id,
            Name = role.Name!,
            UserCount = usersInRole.Count,
            IsSystemRole = isSystemRole,
            Users = usersInRole.Select(u => u.UserName!).ToList()
        };

        Input = new InputModel
        {
            Name = role.Name!
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return NotFound();
        }

        var role = await _roleManager.FindByIdAsync(id);
        if (role == null)
        {
            ErrorMessage = "Role não encontrada.";
            return RedirectToPage("./Index");
        }

        // Reload role details for display in case of validation errors
        var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
        var isSystemRole = IsSystemRole(role.Name!);

        RoleDetails = new RoleDetailsViewModel
        {
            Id = role.Id,
            Name = role.Name!,
            UserCount = usersInRole.Count,
            IsSystemRole = isSystemRole,
            Users = usersInRole.Select(u => u.UserName!).ToList()
        };

        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Cannot edit system roles
        if (isSystemRole)
        {
            ErrorMessage = "Roles do sistema não podem ser editadas.";
            return RedirectToPage("./Index");
        }

        // Check if new name already exists (if name was changed)
        if (role.Name != Input.Name)
        {
            var existingRole = await _roleManager.FindByNameAsync(Input.Name);
            if (existingRole != null)
            {
                ModelState.AddModelError("Input.Name", $"A role '{Input.Name}' já existe.");
                return Page();
            }

            // Update role name
            role.Name = Input.Name;
            role.NormalizedName = Input.Name.ToUpperInvariant();
            
            var result = await _roleManager.UpdateAsync(role);

            if (result.Succeeded)
            {
                _logger.LogInformation("Role ID {RoleId} renomeada para {NewName} pelo utilizador {UserName}", 
                    id, Input.Name, User.Identity!.Name);
                SuccessMessage = $"Role atualizada para '{Input.Name}' com sucesso!";
                return RedirectToPage("./Index");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return Page();
        }

        // No changes made
        SuccessMessage = "Nenhuma alteração foi feita.";
        return RedirectToPage("./Index");
    }

    private bool IsSystemRole(string roleName)
    {
        var systemRoles = new[] { "Administrator", "InternalUser", "ApiUser", "AuditViewer", "ExternalStakeholder" };
        return systemRoles.Contains(roleName);
    }
}
