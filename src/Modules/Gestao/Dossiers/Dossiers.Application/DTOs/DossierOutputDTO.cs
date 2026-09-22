using System.Text.Json.Serialization;
using Shared.Kernel.Converters;

namespace Dossiers.Application.DTOs;

/// <summary>
/// DTO de saída de dossier
/// </summary>
public class DossierOutputDTO
{
    /// <summary>Tipo de dossier</summary>
    [JsonPropertyName("docTypeId")]
    [JsonConverter(typeof(DecimalFormatConverter))]
    public decimal Ndos { get; set; }

    /// <summary>Nome do tipo de dossier</summary>
    [JsonPropertyName("docTypeName")]
    public string Nmdos { get; set; } = string.Empty;

    /// <summary>Número do dossier</summary>
    [JsonPropertyName("docNumber")]
    [JsonConverter(typeof(DecimalFormatConverter))]
    public decimal Obrano { get; set; }

    /// <summary>Ano do dossier</summary>
    [JsonPropertyName("year")]
    [JsonConverter(typeof(DecimalFormatConverter))]
    public decimal Boano { get; set; }

    /// <summary>Nome da entidade</summary>
    [JsonPropertyName("entityName")]
    public string Nome { get; set; } = string.Empty;

    /// <summary>ID da entidade</summary>
    [JsonPropertyName("entityId")]
    [JsonConverter(typeof(DecimalFormatConverter))]
    public decimal No { get; set; }

    /// <summary>Estabelecimento da entidade</summary>
    [JsonPropertyName("entityBranch")]
    [JsonConverter(typeof(DecimalFormatConverter))]
    public decimal Estab { get; set; }

    /// <summary>Data do documento</summary>
    [JsonPropertyName("date")]
    [JsonConverter(typeof(DateOnlyFormatConverter))]
    public DateTime Data { get; set; }

    /// <summary>Moeda</summary>
    [JsonPropertyName("currency")]
    public string Moeda { get; set; } = string.Empty;

    /// <summary>Total do dossier</summary>
    [JsonPropertyName("total")]
    [JsonConverter(typeof(DecimalFormatConverter))]
    public decimal Total { get; set; }

    /// <summary>Campos adicionais configurados no tenant (bo/bo2/bo3)</summary>
    [JsonPropertyName("addFields")]
    public Dictionary<string, object?>? AddFields { get; set; }

    /// <summary>Linhas do dossier</summary>
    [JsonPropertyName("lines")]
    public List<DossierLineOutputDTO> Linhas { get; set; } = new();
}
