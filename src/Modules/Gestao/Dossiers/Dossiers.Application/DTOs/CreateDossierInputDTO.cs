using System.Text.Json.Serialization;
using Shared.Kernel.Converters;

namespace Dossiers.Application.DTOs;

/// <summary>
/// DTO de entrada para criar dossier
/// </summary>
public class CreateDossierInputDTO
{
    [JsonPropertyName("docTypeId")]
    public decimal Ndos { get; set; }

    [JsonPropertyName("year")]
    public decimal? Boano { get; set; }

    [JsonPropertyName("entityId")]
    public decimal No { get; set; }

    [JsonPropertyName("entityBranch")]
    public decimal? Estab { get; set; }

    [JsonPropertyName("entityName")]
    public string? Nome { get; set; } = string.Empty;

    [JsonPropertyName("date")]
    [JsonConverter(typeof(NullableDateOnlyFormatConverter))]
    public DateTime? Data { get; set; }

    [JsonPropertyName("currency")]
    public string? Moeda { get; set; }

    [JsonPropertyName("addFields")]
    public Dictionary<string, object?>? AddFields { get; set; }

    [JsonPropertyName("lines")]
    public List<CreateDossierLineInputDTO> Linhas { get; set; } = new();
}
