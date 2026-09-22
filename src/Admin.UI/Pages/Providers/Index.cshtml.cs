using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Admin.UI.Pages.Providers;

[Authorize(Roles = "Administrator")]
public class IndexModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<IndexModel> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public List<ProviderViewModel> Providers { get; set; } = new();

    [TempData]
    public string? SuccessMessage { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
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
            
            var response = await client.GetAsync($"{apiBaseUrl}/api/providers");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<ProviderViewModel>>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (apiResponse?.Data != null)
                {
                    Providers = apiResponse.Data;
                }
            }
            else
            {
                _logger.LogWarning("Failed to load providers. Status: {StatusCode}", response.StatusCode);
                ErrorMessage = $"Erro ao carregar providers: {response.StatusCode}";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading providers");
            ErrorMessage = "Erro ao carregar providers. Verifique se a API está acessível.";
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(string stamp)
    {
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

            var response = await client.DeleteAsync($"{apiBaseUrl}/api/providers/{stamp}");

            if (response.IsSuccessStatusCode)
            {
                SuccessMessage = "✅ Provider eliminado com sucesso!";
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Failed to delete provider {Stamp}. Status: {StatusCode}, Error: {Error}", 
                    stamp, response.StatusCode, errorContent);
                ErrorMessage = $"Erro ao eliminar provider: {response.StatusCode}";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting provider {Stamp}", stamp);
            ErrorMessage = "Erro ao eliminar provider. Tente novamente.";
        }

        return RedirectToPage();
    }
}

// ViewModels
public class ProviderViewModel
{
    public string UProviderStamp { get; set; } = string.Empty;
    public int Codigo { get; set; }
    public string Provedor { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public bool Ativo { get; set; }
    public List<ProviderValueItemViewModel> Values { get; set; } = new();
}

public class ProviderValueItemViewModel
{
    public string OperationCode { get; set; } = string.Empty;
    public string Chave { get; set; } = string.Empty;
    public string Valor { get; set; } = string.Empty;
    public bool Encriptado { get; set; }
    public int Ordem { get; set; }
    public bool Ativo { get; set; }
}

public class ApiResponse<T>
{
    public T? Data { get; set; }
    public ResponseInfo? Response { get; set; }
}

public class ResponseInfo
{
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
