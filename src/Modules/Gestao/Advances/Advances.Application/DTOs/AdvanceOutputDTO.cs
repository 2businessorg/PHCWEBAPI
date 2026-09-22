using System.Text.Json.Serialization;

namespace Advances.Application.DTOs;

/// <summary>
/// DTO de saída para consulta de um Adiantamento (GetAll / GetById)
/// </summary>
public class AdvanceOutputDTO
{
    [JsonPropertyName("ndoc")]
    public decimal Ndoc { get; set; }

    [JsonPropertyName("nmdoc")]
    public string Nmdoc { get; set; } = string.Empty;

    [JsonPropertyName("rno")]
    public decimal Rno { get; set; }

    [JsonPropertyName("rdano")]
    public decimal Rdano { get; set; }

    [JsonPropertyName("rdata")]
    public string Rdata { get; set; } = string.Empty;

    [JsonPropertyName("no")]
    public decimal No { get; set; }

    [JsonPropertyName("nome")]
    public string Nome { get; set; } = string.Empty;

    [JsonPropertyName("total")]
    public decimal Total { get; set; }

    [JsonPropertyName("moeda")]
    public string Moeda { get; set; } = string.Empty;

    [JsonPropertyName("contado")]
    public decimal Contado { get; set; }

    [JsonPropertyName("bankAccountName")]
    public string BankAccountName { get; set; } = string.Empty;

    [JsonPropertyName("descricao")]
    public string Descricao { get; set; } = string.Empty;

    [JsonPropertyName("addFields")]
    public Dictionary<string, object?>? AddFields { get; set; }
}
