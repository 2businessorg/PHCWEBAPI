using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Abstractions.Privacy.Pseudonymization;
using Shared.Infrastructure.Privacy.Pseudonymization.Options;

namespace Shared.Infrastructure.Privacy.Pseudonymization.Presidio;

public sealed class PresidioHttpAnalyzerClient : IPresidioAnalyzerClient
{
    private readonly HttpClient _http;
    private readonly ILogger<PresidioHttpAnalyzerClient> _logger;

    public PresidioHttpAnalyzerClient(
        HttpClient http,
        IOptions<PseudonymizationOptions> options,
        ILogger<PresidioHttpAnalyzerClient> logger)
    {
        _http = http;
        _logger = logger;
        var baseUrl = options.Value.PresidioBaseUrl?.TrimEnd('/') ?? "http://127.0.0.1:5001";
        _http.BaseAddress = new Uri(baseUrl + "/");
        _http.Timeout = TimeSpan.FromSeconds(Math.Max(1, options.Value.PresidioTimeoutSeconds));
    }

    public async Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _http.GetAsync("health", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Presidio sidecar health check failed");
            return false;
        }
    }

    public async Task<IReadOnlyList<RawEntitySpan>> AnalyzeAsync(
        string text,
        string language,
        CancellationToken cancellationToken = default)
    {
        using var response = await _http.PostAsJsonAsync(
            "analyze",
            new AnalyzeRequest(text, language),
            cancellationToken);

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<AnalyzeResponse>(cancellationToken: cancellationToken);
        if (payload?.Entities is null)
            return Array.Empty<RawEntitySpan>();

        return payload.Entities
            .Select(e => new RawEntitySpan(
                e.EntityType ?? "ORGANIZATION",
                text.Substring(e.Start, Math.Max(0, e.End - e.Start)),
                e.Start,
                e.End,
                e.Score,
                e.RecognizerName ?? "presidio"))
            .ToList();
    }

    private sealed record AnalyzeRequest(string Text, string Language);

    private sealed class AnalyzeResponse
    {
        [JsonPropertyName("entities")]
        public List<AnalyzeEntity>? Entities { get; set; }
    }

    private sealed class AnalyzeEntity
    {
        [JsonPropertyName("entity_type")]
        public string? EntityType { get; set; }

        [JsonPropertyName("start")]
        public int Start { get; set; }

        [JsonPropertyName("end")]
        public int End { get; set; }

        [JsonPropertyName("score")]
        public float Score { get; set; }

        [JsonPropertyName("recognizer_name")]
        public string? RecognizerName { get; set; }
    }
}
