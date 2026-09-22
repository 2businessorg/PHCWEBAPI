using System.Text.Json.Serialization;

namespace Advances.Application.DTOs;

/// <summary>
/// DTO de entrada para criação de um Adiantamento (RD)
/// </summary>
public class CreateAdvanceInputDTO
{
    /// <summary>Série do adiantamento (ndoc da tabela tsrd)</summary>
    [JsonPropertyName("ndoc")]
    public decimal Ndoc { get; set; }

    /// <summary>Número do cliente (no da tabela cl)</summary>
    [JsonPropertyName("no")]
    public decimal No { get; set; }

    /// <summary>Número da conta bancária (noconta da tabela bl)</summary>
    [JsonPropertyName("contado")]
    public decimal Contado { get; set; }

    /// <summary>Valor do adiantamento (deve ser maior que zero)</summary>
    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    /// <summary>Data do adiantamento (opcional; usa hoje se null)</summary>
    [JsonPropertyName("date")]
    public DateTime? Date { get; set; }

    /// <summary>Descrição/observação do adiantamento (opcional)</summary>
    [JsonPropertyName("descricao")]
    public string? Descricao { get; set; }

    /// <summary>Campos de utilizador dinâmicos para a tabela rd (opcional)</summary>
    [JsonPropertyName("addFields")]
    public Dictionary<string, object?>? AddFields { get; set; }
}
