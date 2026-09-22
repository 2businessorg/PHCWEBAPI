using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace Admin.UI.Pages.Roles;

[Authorize(Roles = "Administrator")]
public class CreateModel : PageModel
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<CreateModel> _logger;

    public CreateModel(RoleManager<IdentityRole> roleManager, ILogger<CreateModel> logger)
    {
        _roleManager = roleManager;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

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

        [Display(Name = "Descrição")]
        [StringLength(200, ErrorMessage = "A descrição não pode ter mais de 200 caracteres")]
        public string? Description { get; set; }
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Check if role already exists
        var existingRole = await _roleManager.FindByNameAsync(Input.Name);
        if (existingRole != null)
        {
            ModelState.AddModelError(string.Empty, $"A role '{Input.Name}' já existe.");
            return Page();
        }

        // Create new role
        var role = new IdentityRole(Input.Name);
        var result = await _roleManager.CreateAsync(role);

        if (result.Succeeded)
        {
            _logger.LogInformation("Role {RoleName} criada pelo utilizador {UserName}", Input.Name, User.Identity!.Name);
            SuccessMessage = $"Role '{Input.Name}' criada com sucesso!";
            return RedirectToPage("./Index");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return Page();
    }
}
