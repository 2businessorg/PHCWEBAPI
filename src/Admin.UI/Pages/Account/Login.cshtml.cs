using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace Admin.UI.Pages.Account;

public class LoginModel : PageModel
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly ILogger<LoginModel> _logger;

    public LoginModel(SignInManager<IdentityUser> signInManager, ILogger<LoginModel> logger)
    {
        _signInManager = signInManager;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ReturnUrl { get; set; }

    public string? ErrorMessage { get; set; }

    public class InputModel
    {
        [Required(ErrorMessage = "Nome de utilizador é obrigatório")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password é obrigatória")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Lembrar-me")]
        public bool RememberMe { get; set; }
    }

    public async Task OnGetAsync(string? returnUrl = null)
    {
        if (!string.IsNullOrEmpty(ErrorMessage))
        {
            ModelState.AddModelError(string.Empty, ErrorMessage);
        }

        // Garante que sempre redireciona para Admin após login, não para raiz
        returnUrl = string.IsNullOrEmpty(returnUrl) || returnUrl == "/" ? "/Admin/Users" : returnUrl;

        // Clear the existing external cookie to ensure a clean login process
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        ReturnUrl = returnUrl;
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        // Garante que sempre redireciona para /Admin/Users após login
        returnUrl = string.IsNullOrEmpty(returnUrl) || returnUrl == "/" ? "/Admin/Users" : returnUrl;

        if (ModelState.IsValid)
        {
            // 🛡️ SECURITY: Enable lockout on password failures (VULN-007 Fixed)
            // After 3 failed attempts, user will be locked out for 15 minutes
            var result = await _signInManager.PasswordSignInAsync(
                Input.Username, 
                Input.Password, 
                Input.RememberMe, 
                lockoutOnFailure: true);

            if (result.Succeeded)
            {
                _logger.LogInformation("User {Username} logged in.", Input.Username);
                return LocalRedirect(returnUrl);
            }
            if (result.RequiresTwoFactor)
            {
                return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, Input.RememberMe });
            }
            if (result.IsLockedOut)
            {
                _logger.LogWarning("User {Username} account locked out.", Input.Username);
                ErrorMessage = "Conta bloqueada. Tente novamente mais tarde.";
            }
            else
            {
                ErrorMessage = "Credenciais inválidas. Verifique o nome de utilizador e password.";
            }
        }

        // If we got this far, something failed, redisplay form
        return Page();
    }
}
