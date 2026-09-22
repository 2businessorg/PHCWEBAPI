using Dossiers.Application.Errors;
using Dossiers.Domain.ExternalServices;
using Dossiers.Domain.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Shared.Abstractions.ExternalServices;
using System.Net.Http.Headers;
using System.Text;
using System.Xml;

namespace Dossiers.Infrastructure.ExternalServices;

/// <summary>
/// Implementação do serviço de integração com PHC WEB
/// Obtém credenciais dinamicamente do TenantContext e AppLicense
/// </summary>
public class PhcWebService : IPhcWebServiceDossiers
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
    public async Task<string> ExecuteAsync(string url, string username, string password, string code, string parameter)
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

            return responseContent;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing PHC WEB script: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Chama o script insertBoAPI do PHC WEB via SOAP
    /// </summary>
    public async Task<PhcInsertBoResponse> InsertBoAsync(
        PhcInsertBoRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting PHC WEB integration for InsertBo");

            // Converter request para DTO com JsonProperty
            var requestDto = PhcInsertBoRequestDto.FromDomain(request);
            
            // Converter DTO para JSON
            var requestJson = JsonConvert.SerializeObject(requestDto);
            _logger.LogDebug("Request JSON prepared: {JsonLength} characters", requestJson.Length);

            // Obter credenciais do tenant context e AppLicense
            var credentials = await _credentialsProvider.GetCredentialsAsync(cancellationToken);
            var phcUrl = credentials.PhcWebUrl;
            var phcUsername = credentials.Username;
            var phcPassword = credentials.Password;
            var phcScriptCode = _configuration["PhcWeb:Scripts:InsertBo:Code"]
                ?? throw new InvalidOperationException("PhcWeb:Scripts:InsertBo:Code não configurado");

            _logger.LogInformation("PHC WEB credentials resolved from tenant context. URL: {Url}", phcUrl);

            // Construir envelope SOAP
            var soapRequest = BuildSoapEnvelope(phcUsername, phcPassword, phcScriptCode, requestJson);

            _logger.LogDebug("SOAP envelope constructed for URL: {Url}", phcUrl);

            // Criar requisição HTTP
            var content = new StringContent(soapRequest, Encoding.UTF8, "text/xml");
            content.Headers.Add("SOAPAction", "http://www.phc.pt/RunCode");

            // Enviar requisição
            var response = await _httpClient.PostAsync(phcUrl, content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("PHC WEB returned error status: {StatusCode}", response.StatusCode);
                throw new InvalidOperationException(
                    $"Erro ao chamar PHC WEB: {response.StatusCode} - {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogDebug("PHC WEB response received: {ResponseLength} characters", responseContent.Length);

            // Parsear resposta SOAP
            var phcResponse = ExtractPhcResponseFromSoap(responseContent);

            _logger.LogInformation("PHC WEB integration completed successfully");

            return phcResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Critical error during PHC WEB integration");
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
    /// Extrai a resposta JSON da resposta SOAP
    /// </summary>
    private PhcInsertBoResponse ExtractPhcResponseFromSoap(string soapResponse)
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

            var trimmedJson = jsonString.Trim();
            
            // Desserializar JSON para envelope (que contém success flag)
            PhcScriptResponseEnvelope envelope;
            try
            {
                envelope = JsonConvert.DeserializeObject<PhcScriptResponseEnvelope>(trimmedJson)
                    ?? throw new InvalidOperationException("Desserialização retornou null");
                
                _logger.LogDebug("JSON parsed successfully from SOAP response");
            }
            catch (JsonReaderException jex)
            {
                _logger.LogError(jex, "JSON parsing error: {Message} at line {Line}, position {Position}", 
                    jex.Message, jex.LineNumber, jex.LinePosition);
                throw;
            }

            // Verificar se o script retornou erro
            if (!envelope.Success)
            {
                var errorMessage = $"PHC WEB script returned error: {envelope.Message}";
                _logger.LogError(errorMessage);
                
                // Lançar exceção DossiersModuleException com o código e mensagem do PHC
                throw new DossiersModuleException(
                    envelope.Code ?? "BO014",
                    $"Erro na integração com PHC WEB: {envelope.Message}");
            }

            if (envelope.Data == null)
            {
                _logger.LogError("PHC WEB response successful but no data returned");
                throw new InvalidOperationException("Resposta PHC WEB bem-sucedida mas sem dados");
            }

            // Converter DTO para modelo do Domain
            var phcResponse = envelope.Data.ToDomain();

            _logger.LogDebug("DTO converted to Domain successfully");

            return phcResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Critical error extracting SOAP response: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Generic implementation for executing PHC WEB scripts with type-safe request/response handling
    /// </summary>
    public async Task<TResponse> ExecuteAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : class
        where TResponse : class
    {
        // For now, route to specific implementations based on request type
        if (request is PhcInsertBoRequest insertBoRequest)
        {
            var response = await InsertBoAsync(insertBoRequest, cancellationToken);
            return response as TResponse ?? throw new InvalidOperationException("Response type mismatch");
        }

        throw new NotImplementedException($"No handler for request type {typeof(TRequest).Name}");
    }
}
