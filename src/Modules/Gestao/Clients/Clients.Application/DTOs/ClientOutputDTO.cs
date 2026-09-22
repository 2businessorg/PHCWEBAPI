using System.Text.Json.Serialization;

namespace Clients.Application.DTOs;

/// <summary>
/// DTO de saída para representar um cliente
/// </summary>
public class ClientOutputDTO
{
    /// <summary>
    /// Número do cliente (No)
    /// </summary>
    [JsonPropertyName("id")]
    public decimal No { get; set; }

    /// <summary>
    /// Estabelecimento (Estab)
    /// </summary>
    [JsonPropertyName("branch")]
    public decimal Estab { get; set; }

    /// <summary>
    /// Nome do cliente
    /// </summary>
    [JsonPropertyName("name")]
    public string Nome { get; set; } = null!;

    /// <summary>
    /// Número de contribuinte (NUIT)
    /// </summary>
    [JsonPropertyName("nuit")]
    public string Ncont { get; set; } = null!;

    /// <summary>
    /// Telefone do cliente
    /// </summary>
    [JsonPropertyName("phone")]
    public string Telefone { get; set; } = null!;

    /// <summary>
    /// Morada do cliente
    /// </summary>
    [JsonPropertyName("address")]
    public string Morada { get; set; } = null!;

    /// <summary>
    /// Email do cliente
    /// </summary>
    [JsonPropertyName("email")]
    public string Email { get; set; } = null!;

    /// <summary>
    /// Status ativo/inativo
    /// </summary>
    [JsonPropertyName("inactive")]
    public bool Inactivo { get; set; }

    /// <summary>
    /// Additional fields specific to this tenant's database configuration.
    /// Keys are the aliases defined in u_addfields (e.g. "reference").
    /// Null when no additional fields are configured for this tenant.
    /// </summary>
    [JsonPropertyName("addFields")]
    public Dictionary<string, object?>? AddFields { get; set; }
}
