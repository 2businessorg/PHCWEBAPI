using Invoices.Application.DTOs;
using Invoices.Application.ExternalServices;
using Invoices.Domain.ExternalServices;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Shared.Abstractions.ExternalServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Invoices.Infrastructure.ExternalServices
{
    /// <summary>
    /// Serviço para integração com PHC Web para criação de faturas (FT)
    /// Chama o método CreateFTDoc do web service
    /// </summary>
    public class FtService : IFtService
    {
        private readonly IPhcWebCredentialsProvider _credentialsProvider;
        private readonly IPhcWebService _phcWebService;
        private readonly IConfiguration _configuration;

        public FtService(
            IPhcWebCredentialsProvider credentialsProvider,
            IPhcWebService phcWebService,
            IConfiguration configuration)
        {
            _credentialsProvider = credentialsProvider ?? throw new ArgumentNullException(nameof(credentialsProvider));
            _phcWebService = phcWebService ?? throw new ArgumentNullException(nameof(phcWebService));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Cria uma fatura no PHC Web
        /// Serializa os dados em JSON e chama o script no PHC Web
        /// </summary>
        public async Task<InvoiceOutputDTO> CreateFaturaAsync(CreateInvoiceInputDTO request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            try
            {
                // 1. Obter credenciais do tenant
                var credentials = await _credentialsProvider.GetCredentialsAsync();
                if (credentials == null)
                    throw new InvalidOperationException("Não foi possível resolver credenciais do tenant para PHC Web");

                var scriptCode = _configuration["PhcWeb:Scripts:InsertFt:Code"]
                    ?? throw new InvalidOperationException("PhcWeb:Scripts:InsertFt:Code não configurado");

                // 2. Construir JSON para enviar ao PHC Web
                var jsonRequest = BuildJsonRequest(request);

                // 3. Chamar PHC Web
                var result = await _phcWebService.ExecuteAsync(
                    credentials.PhcWebUrl,
                    credentials.Username,
                    credentials.Password,
                    scriptCode,
                    jsonRequest);

                // 4. Parsear resposta
                var faturaResponse = ParseResponse(result);

                return faturaResponse;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Erro ao criar fatura no PHC Web: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Constrói o JSON a enviar para o PHC Web
        /// Estrutura: {"ndoc":..., "no":..., "ftano":..., "AddFieldsByTable":{...}, "lstFi":[...]}
        /// </summary>
        private string BuildJsonRequest(CreateInvoiceInputDTO request)
        {
            var json = new JObject();

            // Campos obrigatórios
            json["ndoc"] = request.Ndoc;
            json["no"] = request.No;

            // Campos opcionais
            json["ftano"] = request.Ftano.HasValue ? request.Ftano.Value : JValue.CreateNull();
            json["estab"] = request.Estab.HasValue ? request.Estab.Value : JValue.CreateNull();
            json["nome"] = !string.IsNullOrEmpty(request.Nome) ? request.Nome : JValue.CreateNull();
            json["moeda"] = !string.IsNullOrEmpty(request.Moeda) ? request.Moeda : JValue.CreateNull();

            // Utilizador de auditoria
            json["createdBy"] = !string.IsNullOrEmpty(request.CreatedBy) ? request.CreatedBy : "PHCAPI";

            // Data
            json["data"] = !string.IsNullOrEmpty(request.Data)
                ? request.Data
                : DateTime.Today.ToString("yyyy-MM-dd");

            // Campos adicionais do cabeçalho (ft/ft2/ft3) resolvidos pelo handler
            if (request.AddFieldsByTable is { Count: > 0 })
                json["AddFieldsByTable"] = JObject.FromObject(request.AddFieldsByTable);
            else
                json["AddFieldsByTable"] = JValue.CreateNull();

            // Array de linhas
            var linhasArray = new JArray();
            foreach (var linha in request.Linhas)
            {
                var linhaObj = new JObject();
                linhaObj["ref"] = linha.Ref;
                linhaObj["qtt"] = linha.Qtt;
                linhaObj["design"] = !string.IsNullOrEmpty(linha.Design) ? linha.Design : JValue.CreateNull();
                linhaObj["precoVenda"] = linha.PV.HasValue ? linha.PV.Value : JValue.CreateNull();
                linhaObj["tabIva"] = linha.TabIva.HasValue ? linha.TabIva.Value : JValue.CreateNull();
                linhaObj["ivaIncl"] = linha.IvaIncl.HasValue ? linha.IvaIncl.Value : JValue.CreateNull();
                linhaObj["lote"] = !string.IsNullOrEmpty(linha.Lote) ? linha.Lote : JValue.CreateNull();
                linhaObj["nroSerie"] = !string.IsNullOrEmpty(linha.NroSerie) ? linha.NroSerie : JValue.CreateNull();

                // Campos adicionais da linha (fi/fi2) resolvidos pelo handler
                if (linha.AddFieldsByTable is { Count: > 0 })
                    linhaObj["AddFieldsByTable"] = JObject.FromObject(linha.AddFieldsByTable);
                else
                    linhaObj["AddFieldsByTable"] = JValue.CreateNull();

                linhasArray.Add(linhaObj);
            }

            json["lstFi"] = linhasArray;

            json["observacoes"] = !string.IsNullOrEmpty(request.Observacoes)
                ? request.Observacoes : JValue.CreateNull();

            return json.ToString();
        }

        /// <summary>
        /// Parseia a resposta JSON do PHC Web
        /// Extrai dados da fatura criada e linhas
        /// </summary>
        private InvoiceOutputDTO ParseResponse(string responseJson)
        {
            if (string.IsNullOrEmpty(responseJson))
                throw new InvalidOperationException("Resposta vazia do PHC Web");

            try
            {
                var responseObj = JObject.Parse(responseJson);

                // Verificar se operação foi bem-sucedida
                if (responseObj["success"]?.Value<bool>() != true)
                {
                    var errorCode = responseObj["code"]?.Value<string>();
                    var errorMsg = responseObj["message"]?.Value<string>();
                    throw new InvalidOperationException(
                        $"PHC Web retornou erro ({errorCode}): {errorMsg}");
                }

                var data = responseObj["data"];
                if (data == null)
                    throw new InvalidOperationException("Resposta do PHC Web não contém dados de fatura");

                // Mapear resposta para InvoiceOutputDTO
                var response = new InvoiceOutputDTO
                {
                    Ndoc = data["ndoc"]?.Value<int>() ?? 0,
                    NmDoc = data["nmdoc"]?.Value<string>() ?? "",
                    Fno = data["fno"]?.Value<int>() ?? 0,
                    Ftano = data["ftano"]?.Value<int>() ?? DateTime.Now.Year,
                    No = data["no"]?.Value<int>() ?? 0,
                    Nome = data["nome"]?.Value<string>() ?? "",
                    Estab = data["estab"]?.Value<int>() ?? 0,
                    Data = data["data"]?.Value<string>() ?? DateTime.Today.ToString("yyyy-MM-dd"),
                    Moeda = data["moeda"]?.Value<string>() ?? "",
                    Ttiva = data["ttiva"]?.Value<decimal>() ?? 0,
                    TMIva = data["tmIva"]?.Value<decimal>() ?? 0,
                    Total = data["tiliquido"]?.Value<decimal>() ?? data["total"]?.Value<decimal>() ?? 0,
                    TotalMoeda = data["totalMoeda"]?.Value<decimal>() ?? 0,
                    Observacoes = data["observacoes"]?.Value<string>(),
                    // addFieldsByTable raw do PHC (traduzido para AddFields no handler)
                    AddFieldsByTableRaw = ParseRawAddFieldsByTable(data["addFieldsByTable"] as JObject)
                };

                // Mapear linhas
                var linhasArray = data["linhas"] as JArray;
                if (linhasArray != null)
                {
                    response.Linhas = linhasArray.Select(linha => new InvoiceLineOutputDTO
                    {
                        Ref = linha["ref"]?.Value<string>() ?? "",
                        Design = linha["design"]?.Value<string>() ?? "",
                        Qtt = linha["qtt"]?.Value<decimal>() ?? 0,
                        Armazem = linha["armazem"]?.Value<int>() ?? 0,
                        Pv = linha["pv"]?.Value<decimal>() ?? 0,
                        Pvmoeda = linha["pvmoeda"]?.Value<decimal>() ?? 0,
                        Tabiva = linha["tabiva"]?.Value<int>() ?? 0,
                        Iva = linha["iva"]?.Value<decimal>() ?? 0,
                        IvaIncl = linha["ivaincl"]?.Value<bool>() ?? linha["ivaIncl"]?.Value<bool>() ?? false,
                        Ttdeb = linha["tiliquido"]?.Value<decimal>() ?? linha["ttdeb"]?.Value<decimal>() ?? 0,
                        Tmoeda = linha["tmoeda"]?.Value<decimal>() ?? 0,
                        Lote = linha["lote"]?.Value<string>(),
                        // addFieldsByTable raw do PHC (traduzido para AddFields no handler)
                        AddFieldsByTableRaw = ParseRawAddFieldsByTable(linha["addFieldsByTable"] as JObject)
                    }).ToList();
                }

                return response;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Erro ao parsear resposta do PHC Web: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Converte um JObject {tableName: {columnName: value}} para Dictionary.
        /// </summary>
        private static Dictionary<string, Dictionary<string, object?>>? ParseRawAddFieldsByTable(JObject? jobj)
        {
            if (jobj == null) return null;
            var result = new Dictionary<string, Dictionary<string, object?>>(StringComparer.OrdinalIgnoreCase);
            foreach (var tableProp in jobj.Properties())
            {
                if (tableProp.Value is not JObject colsObj) continue;
                var cols = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                foreach (var colProp in colsObj.Properties())
                    cols[colProp.Name] = colProp.Value.Type == JTokenType.Null ? null : colProp.Value.ToObject<object>();
                result[tableProp.Name] = cols;
            }
            return result.Count > 0 ? result : null;
        }
    }
}
