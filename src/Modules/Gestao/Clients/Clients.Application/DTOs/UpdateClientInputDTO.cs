using System.Text.Json.Serialization;

namespace Clients.Application.DTOs;

/// <summary>
/// DTO de entrada para atualizar um cliente
/// </summary>
public class UpdateClientInputDTO
{
    /// <summary>
    /// Nome do cliente
    /// </summary>
    [JsonPropertyName("name")]
    public string? Nome { get; set; }

    /// <summary>
    /// Telefone do cliente
    /// </summary>
    [JsonPropertyName("phone")]
    public string? Telefone { get; set; }

    /// <summary>
    /// Morada do cliente
    /// </summary>
    [JsonPropertyName("address")]
    public string? Morada { get; set; }

    /// <summary>
    /// Email do cliente
    /// </summary>
    [JsonPropertyName("email")]
    public string? Email { get; set; }

    /// <summary>
    /// Estado de inatividade do cliente
    /// </summary>
    [JsonPropertyName("inactive")]
    public bool? Inactivo { get; set; }
}
