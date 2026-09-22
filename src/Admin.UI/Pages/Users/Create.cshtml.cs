using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace Admin.UI.Pages.Users;

[Authorize(Roles = "Administrator")]
public class CreateModel : PageModel
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<CreateModel> _logger;

    public CreateModel(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ILogger<CreateModel> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public List<RoleViewModel> AvailableRoles { get; set; } = new();

    [TempData]
    public string? SuccessMessage { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public class InputModel
    {
        [Required(ErrorMessage = "Nome de utilizador é obrigatório")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Nome de utilizador deve ter entre 3 e 20 caracteres")]
        [RegularExpression(@"^[a-zA-Z0-9._-]+$", ErrorMessage = "Nome de utilizador só pode conter letras, números, pontos, hífens e underscores")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password é obrigatória")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password deve ter no mínimo 6 caracteres")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirmação de password é obrigatória")]
        [Compare("Password", ErrorMessage = "Password e confirmação não coincidem")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;

        public bool EmailConfirmed { get; set; }

        public List<string> SelectedRoles { get; set; } = new();
    }

    public class RoleViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public async Task OnGetAsync()
    {
        await LoadRolesAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadRolesAsync();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Create user
        var user = new IdentityUser
        {
            UserName = Input.UserName,
            Email = Input.Email,
            EmailConfirmed = Input.EmailConfirmed
        };

        var result = await _userManager.CreateAsync(user, Input.Password);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return Page();
        }

        _logger.LogInformation("User {UserName} created successfully by {Admin}", 
            Input.UserName, User.Identity?.Name);

        // Add roles
        if (Input.SelectedRoles?.Any() == true)
        {
            foreach (var roleName in Input.SelectedRoles)
            {
                if (await _roleManager.RoleExistsAsync(roleName))
                {
                    await _userManager.AddToRoleAsync(user, roleName);
                    _logger.LogInformation("Role {RoleName} assigned to user {UserName}", 
                        roleName, Input.UserName);
                }
            }
        }

        SuccessMessage = $"Utilizador '{Input.UserName}' criado com sucesso!";
        return RedirectToPage("./Index");
    }

    private async Task LoadRolesAsync()
    {
        AvailableRoles = _roleManager.Roles
            .Select(r => new RoleViewModel
            {
                Name = r.Name!,
                Description = GetRoleDescription(r.Name!)
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
