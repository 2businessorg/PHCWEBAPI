using System.Text.Json.Serialization;

namespace Advances.Application.DTOs;

/// <summary>
/// DTO de saída após criação de um Adiantamento (RD) via PHC WEB
/// </summary>
public class CreateAdvanceOutputDTO
{
    [JsonPropertyName("clientId")]
    public decimal ClientId { get; set; }

    [JsonPropertyName("bankAccountId")]
    public decimal BankAccountId { get; set; }

    [JsonPropertyName("ndoc")]
    public decimal Ndoc { get; set; }

    [JsonPropertyName("stamp")]
    public string Stamp { get; set; } = string.Empty;

    [JsonPropertyName("rno")]
    public decimal Rno { get; set; }

    [JsonPropertyName("rdano")]
    public decimal Rdano { get; set; }

    [JsonPropertyName("total")]
    public decimal Total { get; set; }

    [JsonPropertyName("addFields")]
    public Dictionary<string, object?>? AddFields { get; set; }
}
