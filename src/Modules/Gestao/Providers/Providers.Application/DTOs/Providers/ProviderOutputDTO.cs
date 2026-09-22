namespace Providers.Application.DTOs.Providers;

/// <summary>
/// DTO de saída para Provider
/// </summary>
public record ProviderOutputDTO
{
    public string UProviderStamp { get; init; } = string.Empty;
    public int Codigo { get; init; }
    public string Provedor { get; init; } = string.Empty;
    public string Environment { get; init; } = string.Empty;
    public string Descricao { get; init; } = string.Empty;
    public bool Ativo { get; init; }
    public DateTime OUsrData { get; init; }
    public string OUsrHora { get; init; } = string.Empty;
    public string? OUsrInis { get; init; }
    public DateTime? UsrData { get; init; }
    public string? UsrHora { get; init; }
    public string? UsrInis { get; init; }
    
    /// <summary>
    /// Lista de valores (linhas) associados a este Provider
    /// </summary>
    public List<ProviderValueItemDTO> Values { get; init; } = new();
}
