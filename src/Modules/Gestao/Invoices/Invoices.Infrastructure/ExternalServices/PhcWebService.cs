using Invoices.Domain.ExternalServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Shared.Abstractions.ExternalServices;
using System.Net.Http.Headers;
using System.Text;
using System.Xml;

namespace Invoices.Infrastructure.ExternalServices;

/// <summary>
/// Implementação do serviço de integração com PHC WEB para Invoices
/// Executa scripts no PHC Web utilizando credenciais do tenant
/// </summary>
public class PhcWebService : IPhcWebService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly IPhcWebCredentialsProvider _credentialsProvider;
    private readonly ILogger<PhcWebService> _logger;

    public PhcWebService(
        HttpClient httpClient,
        IConfiguration configuration,
        IPhcWebCredentialsProvider credentialsProvider,
        ILogger<PhcWebService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _credentialsProvider = credentialsProvider;
        _logger = logger;
    }

    /// <summary>
    /// Executes a script on PHC WEB using provided credentials
    /// </summary>
    /// <param name="url">URL do web service PHC</param>
    /// <param name="username">Utilizador para autenticação</param>
    /// <param name="password">Password para autenticação</param>
    /// <param name="code">Código do script a executar</param>
    /// <param name="parameter">Parâmetro JSON a enviar</param>
    /// <returns>Resposta JSON do PHC Web</returns>
    public async Task<string> ExecuteAsync(
        string url,
        string username,
        string password,
        string code,
        string parameter)
    {
        try
        {
            _logger.LogInformation("Starting PHC WEB integration for script execution");

            // Converter parâmetro para JSON se necessário
            var requestJson = parameter;
            _logger.LogDebug("Request JSON prepared: {JsonLength} characters", requestJson.Length);

            // Construir envelope SOAP
            var soapRequest = BuildSoapEnvelope(username, password, code, requestJson);

            _logger.LogDebug("SOAP envelope constructed for URL: {Url}", url);

            // Criar requisição HTTP
            var content = new StringContent(soapRequest, Encoding.UTF8, "text/xml");
            content.Headers.Add("SOAPAction", "http://www.phc.pt/RunCode");

            // Enviar requisição
            var response = await _httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("PHC WEB returned error status: {StatusCode}", response.StatusCode);
                throw new InvalidOperationException(
                    $"Erro ao chamar PHC WEB: {response.StatusCode} - {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("PHC WEB response received: {ContentLength} characters", responseContent.Length);

            // Extrair JSON da resposta SOAP
            var jsonResponse = ExtractJsonFromSoapResponse(responseContent);

            _logger.LogInformation("PHC WEB integration completed successfully");

            return jsonResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing PHC WEB script: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Constrói o envelope SOAP para chamar o script do PHC
    /// </summary>
    private string BuildSoapEnvelope(string userName, string password, string code, string parameter)
    {
        var escapedJson = System.Security.SecurityElement.Escape(parameter);

        var soapEnvelope = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
               xmlns:xsd=""http://www.w3.org/2001/XMLSchema""
               xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soap:Body>
    <RunCode xmlns=""http://www.phc.pt/"">
      <userName>{System.Security.SecurityElement.Escape(userName)}</userName>
      <password>{System.Security.SecurityElement.Escape(password)}</password>
      <code>{System.Security.SecurityElement.Escape(code)}</code>
      <parameter><val>{escapedJson}</val></parameter>
    </RunCode>
  </soap:Body>
</soap:Envelope>";

        return soapEnvelope;
    }

    /// <summary>
    /// Extrai o JSON da resposta SOAP
    /// </summary>
    private string ExtractJsonFromSoapResponse(string soapResponse)
    {
        try
        {
            _logger.LogDebug("Parsing SOAP response");

            // Parsear XML SOAP
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(soapResponse);

            // Definir namespaces
            var nsManager = new XmlNamespaceManager(xmlDoc.NameTable);
            nsManager.AddNamespace("soap", "http://schemas.xmlsoap.org/soap/envelope/");
            nsManager.AddNamespace("phc", "http://www.phc.pt/");

            // Tentar encontrar o nó RunCodeResult com namespace phc
            var resultNode = xmlDoc.SelectSingleNode("//soap:Body/phc:RunCodeResponse/phc:RunCodeResult", nsManager);

            // Se não encontrar, tentar sem namespace
            if (resultNode == null)
            {
                _logger.LogWarning("RunCodeResult with namespace not found, trying without namespace");
                resultNode = xmlDoc.SelectSingleNode("//soap:Body/RunCodeResponse/RunCodeResult", nsManager);
            }

            if (resultNode == null)
            {
                _logger.LogError("RunCodeResult node not found in SOAP response");
                throw new InvalidOperationException("Resposta SOAP inválida: RunCodeResult não encontrado");
            }

            var jsonString = resultNode.InnerText;

            _logger.LogDebug("JSON extracted from SOAP: {JsonLength} characters", jsonString.Length);

            // Validações adicionais
            if (string.IsNullOrWhiteSpace(jsonString))
            {
                _logger.LogError("Extracted JSON is empty");
                throw new InvalidOperationException("JSON extraído do SOAP está vazio");
            }

            return jsonString.Trim();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Critical error extracting SOAP response: {Message}", ex.Message);
            throw;
        }
    }
}
