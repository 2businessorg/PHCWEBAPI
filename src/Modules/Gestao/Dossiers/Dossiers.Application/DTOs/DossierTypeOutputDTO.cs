using System.Text.Json.Serialization;
using Shared.Kernel.Converters;

namespace Dossiers.Application.DTOs;

/// <summary>
/// DTO de saída para tipo de dossier
/// </summary>
public class DossierTypeOutputDTO
{
    /// <summary>Número do tipo de dossier</summary>
    [JsonPropertyName("type")]
    [JsonConverter(typeof(DecimalFormatConverter))]
    public decimal Ndos { get; set; }

    /// <summary>Nome do tipo de dossier</summary>
    [JsonPropertyName("docTypeName")]
    public string Nmdos { get; set; } = string.Empty;

    /// <summary>Nome do tipo de dossier em Inglês</summary>
    [JsonPropertyName("tableName")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Chave de entidade associada (ex: CL, FL, AG, EM)</summary>
    [JsonPropertyName("table")]
    public string Table { get; set; } = string.Empty;
}
