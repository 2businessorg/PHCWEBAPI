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
public class EditModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<EditModel> _logger;

    public EditModel(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<EditModel> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    [BindProperty(SupportsGet = true)]
    public string Stamp { get; set; } = string.Empty;

    [BindProperty]
    public UpdateProviderInputModel Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        if (string.IsNullOrEmpty(Stamp))
        {
            return RedirectToPage("./Index");
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

            var response = await client.GetAsync($"{apiBaseUrl}/api/providers/{Stamp}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<ProviderViewModel>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (apiResponse?.Data != null)
                {
                    var provider = apiResponse.Data;
                    Input = new UpdateProviderInputModel
                    {
                        Codigo = provider.Codigo,
                        Provedor = provider.Provedor,
                        Environment = provider.Environment,
                        Descricao = provider.Descricao,
                        Ativo = provider.Ativo,
                        Values = provider.Values.Select(v => new UpdateProviderValueItemModel
                        {
                            OperationCode = v.OperationCode,
                            Chave = v.Chave,
                            Valor = v.Valor,
                            Encriptado = v.Encriptado,
                            Ordem = v.Ordem,
                            Ativo = v.Ativo
                        }).ToList()
                    };
                }
                else
                {
                    TempData["ErrorMessage"] = "Provider não encontrado.";
                    return RedirectToPage("./Index");
                }
            }
            else
            {
                _logger.LogWarning("Failed to load provider {Stamp}. Status: {StatusCode}", Stamp, response.StatusCode);
                TempData["ErrorMessage"] = "Erro ao carregar provider.";
                return RedirectToPage("./Index");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading provider {Stamp}", Stamp);
            TempData["ErrorMessage"] = "Erro ao carregar provider. Verifique se a API está acessível.";
            return RedirectToPage("./Index");
        }

        return Page();
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

            var response = await client.PutAsync($"{apiBaseUrl}/api/providers/{Stamp}", httpContent);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = $"✅ Provider '{Input.Provedor}' atualizado com sucesso!";
                return RedirectToPage("./Index");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Failed to update provider {Stamp}. Status: {StatusCode}, Error: {Error}", 
                    Stamp, response.StatusCode, errorContent);
                
                ModelState.AddModelError(string.Empty, $"Erro ao atualizar provider: {response.StatusCode}");
                return Page();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating provider {Stamp}", Stamp);
            ModelState.AddModelError(string.Empty, "Erro ao atualizar provider. Verifique se a API está acessível.");
            return Page();
        }
    }
}

// Input Models
public class UpdateProviderInputModel
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

    public List<UpdateProviderValueItemModel> Values { get; set; } = new();
}

public class UpdateProviderValueItemModel
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
