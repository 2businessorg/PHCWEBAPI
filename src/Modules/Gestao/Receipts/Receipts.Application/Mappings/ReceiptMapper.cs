using Receipts.Application.DTOs;
using Receipts.Domain.Entities;
using System.Text.Json;

namespace Receipts.Application.Mappings;

/// <summary>
/// Mapeamento entre entidades de domínio e DTOs do módulo Receipts
/// </summary>
public static class ReceiptMapper
{
    /// <summary>
    /// Mapeia um header Re para ReceiptOutputDTO (sem linhas)
    /// </summary>
    public static ReceiptOutputDTO FromRe(Re re, string bankAccountName = "")
    {
        return new ReceiptOutputDTO
        {
            Ndoc = re.Ndoc,
            Nmdoc = NormalizeText(re.Nmdoc),
            Rno = re.Rno,
            Reano = re.Reano,
            Rdata = re.Rdata.ToString("yyyy-MM-dd"),
            No = re.No,
            Nome = NormalizeText(re.Nome),
            Total = RoundMoney(re.Total),
            Totalmoeda = RoundMoney(re.Totalmoeda),
            Moeda = NormalizeText(re.Moeda),
            Contado = re.Contado,
            BankAccountName = NormalizeText(bankAccountName)
        };
    }

    /// <summary>
    /// Mapeia uma linha Rl para ReceiptLineOutputDTO
    /// </summary>
    public static ReceiptLineOutputDTO FromRl(Rl rl, string invoiceTypeId = "")
    {
        return new ReceiptLineOutputDTO
        {
            Nrdoc = rl.Nrdoc,
            Cdesc = NormalizeText(rl.Cdesc),
            Rec = RoundMoney(rl.Rec),
            Eval = RoundMoney(rl.Eval),
            Datalc = rl.Datalc.ToString("yyyy-MM-dd"),
            InvoiceTypeId = NormalizeText(invoiceTypeId)
        };
    }

    /// <summary>
    /// Interpreta a resposta JSON/XML do PHC WEB e constrói o ReceiptOutputDTO
    /// </summary>
    public static ReceiptOutputDTO FromPhcResponse(string rawResponse, CreateReceiptInputDTO input, string nmdoc)
    {
        // O PHC WEB retorna XML com resultado JSON encapsulado
        // Extrai o JSON da resposta SOAP
        var jsonStart = rawResponse.IndexOf('{');
        var jsonEnd = rawResponse.LastIndexOf('}');

        if (jsonStart >= 0 && jsonEnd > jsonStart)
        {
            var json = rawResponse[jsonStart..(jsonEnd + 1)];
            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                return new ReceiptOutputDTO
                {
                    Ndoc = input.Ndoc,
                    Nmdoc = NormalizeText(nmdoc),
                    Rno = root.TryGetProperty("rno", out var rno) ? rno.GetDecimal() : 0,
                    Reano = root.TryGetProperty("reano", out var reano) ? reano.GetDecimal() : DateTime.UtcNow.Year,
                    Rdata = root.TryGetProperty("rdata", out var rdata) ? rdata.GetString() ?? DateTime.UtcNow.ToString("yyyy-MM-dd") : DateTime.UtcNow.ToString("yyyy-MM-dd"),
                    No = input.No,
                    Nome = NormalizeText(root.TryGetProperty("nome", out var nome) ? nome.GetString() ?? string.Empty : string.Empty),
                    Total = root.TryGetProperty("total", out var total)
                        ? (total.TryGetDecimal(out var tDec)
                            ? RoundMoney(tDec)
                            : (total.TryGetDouble(out var tDbl) ? RoundMoney((decimal)tDbl) : 0m))
                        : 0,
                    Totalmoeda = root.TryGetProperty("totalmoeda", out var totalmoeda)
                        ? (totalmoeda.TryGetDecimal(out var tmDec)
                            ? RoundMoney(tmDec)
                            : (totalmoeda.TryGetDouble(out var tmDbl) ? RoundMoney((decimal)tmDbl) : 0m))
                        : 0,
                    Moeda = NormalizeText(root.TryGetProperty("moeda", out var moeda) ? moeda.GetString() ?? string.Empty : string.Empty),
                    Contado = input.Contado,
                    BankAccountName = string.Empty,
                    AddFields = ReadAddFields(root)
                };
            }
            catch
            {
                // fallback mínimo com dados do input
            }
        }

        // Fallback: construir DTO mínimo com dados do request
        return new ReceiptOutputDTO
        {
            Ndoc = input.Ndoc,
            Nmdoc = NormalizeText(nmdoc),
            No = input.No,
            Contado = input.Contado,
            BankAccountName = string.Empty,
            Rdata = DateTime.UtcNow.ToString("yyyy-MM-dd")
        };
    }

    /// <summary>
    /// Interpreta a resposta JSON do script insertReAPI do PHC WEB e constrói o CreateReceiptOutputDTO.
    /// Estrutura esperada da resposta:
    /// { "success": true, "clientId": N, "bankAccountId": N, "totalCount": N, "documents": { "Re": {...}, "Rd": [...] } }
    /// </summary>
    public static CreateReceiptOutputDTO FromPhcCreateResponse(string rawResponse, CreateReceiptInputDTO input)
    {
        var jsonStart = rawResponse.IndexOf('{');
        var jsonEnd = rawResponse.LastIndexOf('}');

        if (jsonStart >= 0 && jsonEnd > jsonStart)
        {
            var json = rawResponse[jsonStart..(jsonEnd + 1)];
            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                // Verificar se o PHC WEB reportou erro
                if (root.TryGetProperty("success", out var successProp) && !successProp.GetBoolean())
                {
                    var errorMsg = root.TryGetProperty("message", out var msgProp)
                        ? msgProp.GetString() ?? "Erro desconhecido"
                        : "Erro desconhecido";
                    var errorCode = root.TryGetProperty("code", out var codeProp)
                        ? codeProp.GetString() ?? "RE009"
                        : "RE009";
                    throw new Errors.ReceiptsModuleException(errorCode, errorMsg);
                }

                var clientId = root.TryGetProperty("clientId", out var cid)
                    ? cid.GetDecimal() : input.No;
                var bankAccountId = root.TryGetProperty("bankAccountId", out var ba)
                    ? ba.GetDecimal() : input.Contado;
                var totalCount = root.TryGetProperty("totalCount", out var tc)
                    ? tc.GetInt32() : 0;

                var documents = new CreateReceiptDocumentsDTO();

                if (root.TryGetProperty("documents", out var docsEl))
                {
                    // Mapear RE
                    if (docsEl.TryGetProperty("Re", out var reEl) && reEl.ValueKind == JsonValueKind.Object)
                    {
                        var reDoc = new ReceiptDocumentOutputDTO
                        {
                            DocTypeId = reEl.TryGetProperty("ndoc", out var rNdoc) ? rNdoc.GetDecimal() : input.Ndoc,
                            ReceiptNumber = reEl.TryGetProperty("rno", out var rRno) ? rRno.GetDecimal() : 0,
                            Year = DateTime.UtcNow.Year,
                            Total = reEl.TryGetProperty("total", out var rTotal)
                                ? (rTotal.TryGetDecimal(out var rtd) ? RoundMoney(rtd)
                                    : rTotal.TryGetDouble(out var rtf) ? RoundMoney((decimal)rtf) : 0m)
                                : 0m,
                            AddFields = ReadAddFields(reEl)
                        };

                        if (reEl.TryGetProperty("linhas", out var linhasEl) && linhasEl.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var l in linhasEl.EnumerateArray())
                            {
                                reDoc.Lines.Add(new ReceiptDocumentLineOutputDTO
                                {
                                    InvoiceNumber = l.TryGetProperty("nrdoc", out var lNrdoc) ? lNrdoc.GetDecimal() : 0,
                                    InvoiceYear = l.TryGetProperty("ftano", out var lAno) ? lAno.GetDecimal() : 0,
                                    DocDescription = NormalizeText(l.TryGetProperty("cdesc", out var lDesc) ? lDesc.GetString() : string.Empty),
                                    AmountSettled = l.TryGetProperty("erec", out var lErec)
                                        ? (lErec.TryGetDecimal(out var erd) ? RoundMoney(erd)
                                            : lErec.TryGetDouble(out var erf) ? RoundMoney((decimal)erf) : 0m)
                                        : 0m,
                                    AmountTotal = l.TryGetProperty("eval", out var lEval)
                                        ? (lEval.TryGetDecimal(out var evd) ? RoundMoney(evd)
                                            : lEval.TryGetDouble(out var evf) ? RoundMoney((decimal)evf) : 0m)
                                        : 0m,
                                    AddFields = ReadAddFields(l)
                                });
                            }
                        }

                        documents.Receipt = reDoc;
                    }

                    // Mapear RD (adiantamentos)
                    if (docsEl.TryGetProperty("Rd", out var rdEl) && rdEl.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var rd in rdEl.EnumerateArray())
                        {
                            documents.Advances.Add(new ReceiptAdvanceOutputDTO
                            {
                                DocTypeId = rd.TryGetProperty("ndoc", out var rdNdoc) ? rdNdoc.GetDecimal() : 0,
                                ReceiptNumber = rd.TryGetProperty("rno", out var rdRno) ? rdRno.GetDecimal() : 0,
                                Total = rd.TryGetProperty("total", out var rdTotal)
                                    ? (rdTotal.TryGetDecimal(out var rdt) ? RoundMoney(rdt)
                                        : rdTotal.TryGetDouble(out var rdf) ? RoundMoney((decimal)rdf) : 0m)
                                    : 0m,
                                InvoiceNumber = rd.TryGetProperty("nrdoc", out var rdNrdoc) ? rdNrdoc.GetDecimal() : 0,
                                InvoiceYear = rd.TryGetProperty("ftano", out var rdAno) ? rdAno.GetDecimal() : 0
                            });
                        }
                    }
                }

                return new CreateReceiptOutputDTO
                {
                    ClientId = clientId,
                    BankAccountId = bankAccountId,
                    TotalCount = totalCount,
                    Documents = documents
                };
            }
            catch (Errors.ReceiptsModuleException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Errors.ReceiptsModuleException(
                    "RE009",
                    $"Falha ao interpretar resposta do PHC WEB: {ex.GetType().Name}: {ex.Message}");
            }
        }

        throw new Errors.ReceiptsModuleException(
            "RE009",
            $"Resposta do PHC WEB não contém JSON válido. Resposta recebida: {rawResponse[..Math.Min(200, rawResponse.Length)]}");
    }

    private static string NormalizeText(string? value) => (value ?? string.Empty).Trim();

    private static decimal RoundMoney(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);

    private static Dictionary<string, object?>? ReadAddFields(JsonElement element)
    {
        if (!element.TryGetProperty("addFields", out var addFieldsEl) || addFieldsEl.ValueKind != JsonValueKind.Object)
            return null;

        return (Dictionary<string, object?>?)ConvertJsonElement(addFieldsEl);
    }

    private static object? ConvertJsonElement(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
            {
                var dict = new Dictionary<string, object?>();
                foreach (var prop in element.EnumerateObject())
                {
                    dict[prop.Name] = ConvertJsonElement(prop.Value);
                }

                return dict;
            }
            case JsonValueKind.Array:
            {
                var list = new List<object?>();
                foreach (var item in element.EnumerateArray())
                {
                    list.Add(ConvertJsonElement(item));
                }

                return list;
            }
            case JsonValueKind.String:
                return element.GetString();
            case JsonValueKind.Number:
                if (element.TryGetInt64(out var intValue)) return intValue;
                if (element.TryGetDecimal(out var decimalValue)) return decimalValue;
                return element.GetDouble();
            case JsonValueKind.True:
            case JsonValueKind.False:
                return element.GetBoolean();
            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
            default:
                return null;
        }
    }
}
