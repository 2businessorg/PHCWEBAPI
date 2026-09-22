using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Admin.UI.Pages.Roles;

[Authorize(Roles = "Administrator")]
public class IndexModel : PageModel
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(
        RoleManager<IdentityRole> roleManager, 
        UserManager<IdentityUser> userManager,
        ILogger<IndexModel> logger)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _logger = logger;
    }

    public List<RoleViewModel> Roles { get; set; } = new();

    [TempData]
    public string? SuccessMessage { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        var roles = await _roleManager.Roles.ToListAsync();
        
        foreach (var role in roles)
        {
            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
            
            Roles.Add(new RoleViewModel
            {
                Id = role.Id,
                Name = role.Name!,
                UserCount = usersInRole.Count,
                IsSystemRole = IsSystemRole(role.Name!)
            });
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(string roleId)
    {
        var role = await _roleManager.FindByIdAsync(roleId);
        
        if (role == null)
        {
            ErrorMessage = "Role não encontrada.";
            return RedirectToPage();
        }

        if (IsSystemRole(role.Name!))
        {
            ErrorMessage = "Não é possível eliminar roles do sistema.";
            return RedirectToPage();
        }

        var result = await _roleManager.DeleteAsync(role);
        
        if (result.Succeeded)
        {
            _logger.LogInformation("Role {RoleName} eliminada pelo utilizador {UserName}", role.Name, User.Identity!.Name);
            SuccessMessage = $"Role '{role.Name}' eliminada com sucesso!";
        }
        else
        {
            _logger.LogError("Erro ao eliminar role {RoleName}: {Errors}", role.Name, string.Join(", ", result.Errors.Select(e => e.Description)));
            ErrorMessage = $"Erro ao eliminar role: {string.Join(", ", result.Errors.Select(e => e.Description))}";
        }

        return RedirectToPage();
    }

    private bool IsSystemRole(string roleName)
    {
        var systemRoles = new[] { "Administrator", "InternalUser", "ApiUser", "AuditViewer", "ExternalStakeholder" };
        return systemRoles.Contains(roleName);
    }
}

public class RoleViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int UserCount { get; set; }
    public bool IsSystemRole { get; set; }
    
    public string Icon => Name switch
    {
        "Administrator" => "fa-crown",
        "InternalUser" => "fa-user-tie",
        "ApiUser" => "fa-code",
        "AuditViewer" => "fa-eye",
        "ExternalStakeholder" => "fa-handshake",
        _ => "fa-shield-alt"
    };
    
    public string Description => Name switch
    {
        "Administrator" => "Acesso total ao sistema. Pode gerir utilizadores, roles e configurações.",
        "InternalUser" => "Utilizador interno com acesso às funcionalidades operacionais.",
        "ApiUser" => "Acesso via API para integrações e sistemas externos.",
        "AuditViewer" => "Visualização de logs e auditorias do sistema.",
        "ExternalStakeholder" => "Role personalizada",
        _ => "Role personalizada do sistema"
    };
}
