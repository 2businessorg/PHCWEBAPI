using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Shared.Abstractions.ExternalServices;
using Stocks.Domain.ExternalServices;
using System.Security;
using System.Text;
using System.Xml;

namespace Stocks.Infrastructure.ExternalServices;

/// <summary>
/// Implementacao do servico de integracao com PHC WEB para o modulo Stocks.
/// </summary>
public class PhcWebService : IPhcWebServiceStocks
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

    /// <inheritdoc />
    public async Task<string> CreateStockAsync(string parameter, CancellationToken ct = default)
    {
        var code = _configuration["PhcWeb:Scripts:InsertSt:Code"]
            ?? throw new InvalidOperationException("PhcWeb:Scripts:InsertSt:Code nao configurado");

        try
        {
            _logger.LogInformation("Starting PHC WEB integration for Stocks script: {Code}", code);

            var credentials = await _credentialsProvider.GetCredentialsAsync(ct);
            var soapRequest = BuildSoapEnvelope(credentials.Username, credentials.Password, code, parameter);

            var content = new StringContent(soapRequest, Encoding.UTF8, "text/xml");
            content.Headers.Add("SOAPAction", "http://www.phc.pt/RunCode");

            var response = await _httpClient.PostAsync(credentials.PhcWebUrl, content, ct);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(ct);
                throw new InvalidOperationException($"Erro ao chamar PHC WEB: {response.StatusCode} - {errorContent}");
            }

            var soapResponse = await response.Content.ReadAsStringAsync(ct);
            return ExtractResultFromSoapResponse(soapResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing PHC WEB Stocks script {Code}: {Message}", code, ex.Message);
            throw;
        }
    }

    private static string BuildSoapEnvelope(string username, string password, string code, string parameter)
    {
        return $"""
            <?xml version="1.0" encoding="utf-8"?>
            <soap:Envelope xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
                           xmlns:xsd="http://www.w3.org/2001/XMLSchema"
                           xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
              <soap:Body>
                <RunCode xmlns="http://www.phc.pt/">
                  <userName>{SecurityElement.Escape(username)}</userName>
                  <password>{SecurityElement.Escape(password)}</password>
                  <code>{SecurityElement.Escape(code)}</code>
                  <parameter><val>{parameter}</val></parameter>
                </RunCode>
              </soap:Body>
            </soap:Envelope>
            """;
    }

    private static string ExtractResultFromSoapResponse(string soapXml)
    {
        try
        {
            var doc = new XmlDocument();
            doc.LoadXml(soapXml);

            var ns = new XmlNamespaceManager(doc.NameTable);
            ns.AddNamespace("phc", "http://www.phc.pt/");

            var resultNode = doc.SelectSingleNode("//phc:RunCodeResult", ns)
                ?? doc.SelectSingleNode("//*[local-name()='RunCodeResult']");

            return resultNode?.InnerText ?? soapXml;
        }
        catch
        {
            return soapXml;
        }
    }
}
