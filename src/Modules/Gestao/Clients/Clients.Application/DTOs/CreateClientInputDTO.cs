using System.Text.Json.Serialization;

namespace Clients.Application.DTOs;

/// <summary>
/// DTO de entrada para criar um cliente
/// </summary>
public class CreateClientInputDTO
{
    /// <summary>
    /// Número canônico do cliente (opcional).
    /// Quando omitido ou 0, o sistema gera automaticamente o próximo número disponível.
    /// </summary>
    [JsonPropertyName("id")]
    public decimal? No { get; set; }

    /// <summary>
    /// Estabelecimento do cliente (opcional). Default = 0.
    /// Se informado com valor diferente de 0, o campo No também deve ser informado.
    /// </summary>
    [JsonPropertyName("branch")]
    public decimal? Estab { get; set; }
    
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
    /// Telefone do cliente (opcional)
    /// </summary>
    [JsonPropertyName("phone")]
    public string? Telefone { get; set; }

    /// <summary>
    /// Morada do cliente (opcional)
    /// </summary>
    [JsonPropertyName("address")]
    public string? Morada { get; set; }

    /// <summary>
    /// Email do cliente (opcional)
    /// </summary>
    [JsonPropertyName("email")]
    public string? Email { get; set; }

    /// <summary>
    /// Campos adicionais específicos do tenant (opcional)
    /// </summary>
    [JsonPropertyName("addFields")]
    public Dictionary<string, object?>? AddFields { get; set; }
}
