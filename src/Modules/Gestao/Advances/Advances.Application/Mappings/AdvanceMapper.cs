using Advances.Application.DTOs;
using Advances.Application.Errors;
using Advances.Domain.Entities;
using System.Text.Json;

namespace Advances.Application.Mappings;

/// <summary>
/// Mapeamento entre entidades de domínio e DTOs do módulo Advances
/// </summary>
public static class AdvanceMapper
{
    /// <summary>
    /// Mapeia um Rd para AdvanceOutputDTO
    /// </summary>
    public static AdvanceOutputDTO FromRd(Rd rd, string bankAccountName = "")
    {
        return new AdvanceOutputDTO
        {
            Ndoc = rd.Ndoc,
            Nmdoc = NormalizeText(rd.Nmdoc),
            Rno = rd.Rno,
            Rdano = rd.Rdano,
            Rdata = rd.Rdata.ToString("yyyy-MM-dd"),
            No = rd.No,
            Nome = NormalizeText(rd.Nome),
            Total = RoundMoney(rd.Total),
            Moeda = NormalizeText(rd.Moeda),
            Contado = rd.Contado,
            BankAccountName = NormalizeText(bankAccountName),
            Descricao = NormalizeText(rd.Descricao)
        };
    }

    /// <summary>
    /// Interpreta a resposta JSON do script insertRdAPI do PHC WEB e constrói o CreateAdvanceOutputDTO.
    /// Estrutura esperada (flat):
    /// { "success": true, "clientId": N, "bankAccountId": N, "ndoc": N, "stamp": "...", "rno": N, "rdano": N, "total": N, "addFields": null }
    /// </summary>
    public static CreateAdvanceOutputDTO FromPhcCreateResponse(string rawResponse, CreateAdvanceInputDTO input)
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
                        ? codeProp.GetString() ?? "AD006"
                        : "AD006";
                    throw new AdvancesModuleException(errorCode, errorMsg);
                }

                return new CreateAdvanceOutputDTO
                {
                    ClientId = root.TryGetProperty("clientId", out var cid) ? cid.GetDecimal() : input.No,
                    BankAccountId = root.TryGetProperty("bankAccountId", out var ba) ? ba.GetDecimal() : input.Contado,
                    Ndoc = root.TryGetProperty("ndoc", out var ndoc) ? ndoc.GetDecimal() : input.Ndoc,
                    Stamp = root.TryGetProperty("stamp", out var stamp) ? stamp.GetString() ?? string.Empty : string.Empty,
                    Rno = root.TryGetProperty("rno", out var rno) ? rno.GetDecimal() : 0,
                    Rdano = root.TryGetProperty("rdano", out var rdano) ? rdano.GetDecimal() : DateTime.UtcNow.Year,
                    Total = root.TryGetProperty("total", out var total)
                        ? (total.TryGetDecimal(out var td) ? RoundMoney(td)
                            : total.TryGetDouble(out var tf) ? RoundMoney((decimal)tf) : 0m)
                        : 0m,
                    AddFields = ReadAddFields(root)
                };
            }
            catch (AdvancesModuleException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new AdvancesModuleException(
                    "AD006",
                    $"Falha ao interpretar resposta do PHC WEB: {ex.GetType().Name}: {ex.Message}");
            }
        }

        throw new AdvancesModuleException(
            "AD006",
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
                    dict[prop.Name] = ConvertJsonElement(prop.Value);
                return dict;
            }
            case JsonValueKind.Array:
            {
                var list = new List<object?>();
                foreach (var item in element.EnumerateArray())
                    list.Add(ConvertJsonElement(item));
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
            default:
                return null;
        }
    }
}
