using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;

namespace Admin.UI.Pages.Providers;

[Authorize(Roles = "Administrator")]
public class CreateModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CreateModel> _logger;

    public CreateModel(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<CreateModel> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    [BindProperty]
    public CreateProviderInputModel Input { get; set; } = new();

    public void OnGet()
    {
        // Initialize with defaults
        Input.Ativo = true;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var client = _httpClientFactory.CreateClient();
            var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7000";
            
            // ✅ Add authentication cookie to the request
            var cookies = Request.Headers.Cookie.ToString();
            if (!string.IsNullOrEmpty(cookies))
            {
                client.DefaultRequestHeaders.Add("Cookie", cookies);
            }

            var jsonContent = JsonSerializer.Serialize(Input, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{apiBaseUrl}/api/providers", httpContent);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = $"✅ Provider '{Input.Provedor}' criado com sucesso!";
                return RedirectToPage("./Index");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Failed to create provider. Status: {StatusCode}, Error: {Error}", 
                    response.StatusCode, errorContent);
                
                ModelState.AddModelError(string.Empty, $"Erro ao criar provider: {response.StatusCode}");
                return Page();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating provider");
            ModelState.AddModelError(string.Empty, "Erro ao criar provider. Verifique se a API está acessível.");
            return Page();
        }
    }
}

// Input Models
public class CreateProviderInputModel
{
    [Required(ErrorMessage = "Código é obrigatório")]
    [Range(1, int.MaxValue, ErrorMessage = "Código deve ser maior que 0")]
    public int Codigo { get; set; }

    [Required(ErrorMessage = "Nome do Provider é obrigatório")]
    [StringLength(50, ErrorMessage = "Nome do Provider não pode exceder 50 caracteres")]
    public string Provedor { get; set; } = string.Empty;

    [Required(ErrorMessage = "Environment é obrigatório")]
    [StringLength(50, ErrorMessage = "Environment não pode exceder 50 caracteres")]
    public string Environment { get; set; } = string.Empty;

    [Required(ErrorMessage = "Descrição é obrigatória")]
    [StringLength(500, ErrorMessage = "Descrição não pode exceder 500 caracteres")]
    public string Descricao { get; set; } = string.Empty;

    public bool Ativo { get; set; } = true;

    public List<CreateProviderValueItemModel> Values { get; set; } = new();
}

public class CreateProviderValueItemModel
{
    [Required(ErrorMessage = "Operation Code é obrigatório")]
    public string OperationCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Chave é obrigatória")]
    public string Chave { get; set; } = string.Empty;

    [Required(ErrorMessage = "Valor é obrigatório")]
    public string Valor { get; set; } = string.Empty;

    public bool Encriptado { get; set; }

    [Required(ErrorMessage = "Ordem é obrigatória")]
    [Range(1, int.MaxValue, ErrorMessage = "Ordem deve ser maior que 0")]
    public int Ordem { get; set; } = 1;

    public bool Ativo { get; set; } = true;
}
