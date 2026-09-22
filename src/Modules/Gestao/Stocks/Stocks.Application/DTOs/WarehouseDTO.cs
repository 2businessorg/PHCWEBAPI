namespace Stocks.Application.DTOs;

using System.Text.Json.Serialization;

/// <summary>
/// Application DTO for warehouse data with JSON serialization attributes
/// </summary>
public class WarehouseDTO
{
    [JsonPropertyName("number")]
    public decimal No { get; set; }

    [JsonPropertyName("name")]
    public string? NomeArmazem { get; set; }
}
