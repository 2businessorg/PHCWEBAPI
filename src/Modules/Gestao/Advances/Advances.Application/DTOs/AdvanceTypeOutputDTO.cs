using System.Text.Json.Serialization;

namespace Advances.Application.DTOs;

/// <summary>
/// DTO de saída para tipos/séries de adiantamento (tabela tsrd)
/// </summary>
public class AdvanceTypeOutputDTO
{
    [JsonPropertyName("ndoc")]
    public decimal Ndoc { get; set; }

    [JsonPropertyName("nmdoc")]
    public string Nmdoc { get; set; } = string.Empty;
}
