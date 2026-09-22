using System.Text.Json.Serialization;

namespace Shared.Kernel.Responses;

/// <summary>
/// Código de resposta da API
/// </summary>
public sealed record ResponseCodeDTO
{
    [JsonPropertyName("cod")]
    public string Code { get; init; }

    [JsonPropertyName("codDesc")]
    public string Description { get; init; }

    [JsonPropertyName("id")]
    public decimal? Id { get; init; }

    public ResponseCodeDTO(string code, string description, decimal? id = null)
    {
        Code = code;
        Description = description;
        Id = id;
    }

    // Deconstruction para facilitar uso
    public void Deconstruct(out string code, out string description)
    {
        code = Code;
        description = Description;
    }

    public void Deconstruct(out string code, out string description, out decimal? id)
    {
        code = Code;
        description = Description;
        id = Id;
    }
}

