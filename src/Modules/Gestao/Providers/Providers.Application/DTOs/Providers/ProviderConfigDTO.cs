namespace Providers.Application.DTOs.Providers;

/// <summary>
/// DTO para retornar a configuração de um endpoint como Dictionary
/// ⭐ DTO PRINCIPAL - usado para obter config de APIs externas
/// </summary>
public record ProviderConfigDTO
{
    public string Provedor { get; init; } = string.Empty;
    public string Environment { get; init; } = string.Empty;
    public string OperationCode { get; init; } = string.Empty;
    public Dictionary<string, string> Properties { get; init; } = new();
}
