using System.Text.Json.Serialization;

namespace Shared.Kernel.Responses;

/// <summary>
/// Link HATEOAS para navegação entre recursos
/// Segue o padrão JSON:API / HAL
/// </summary>
public sealed record HATEOASLink
{
    [JsonPropertyName("rel")]
    public string Rel { get; init; }

    [JsonPropertyName("href")]
    public string Href { get; init; }

    [JsonPropertyName("method")]
    public string Method { get; init; } = "GET";

    public HATEOASLink(string rel, string href, string method = "GET")
    {
        Rel = rel;
        Href = href;
        Method = method;
    }
}
