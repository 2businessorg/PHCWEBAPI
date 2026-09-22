namespace Providers.Application.DTOs.Providers;

/// <summary>
/// DTO de entrada para criar Provider com seus valores
/// </summary>
public record CreateProviderInputDTO
{
    public int Codigo { get; init; }
    public string Provedor { get; init; } = string.Empty;
    public string Environment { get; init; } = string.Empty;
    public string Descricao { get; init; } = string.Empty;
    public bool Ativo { get; init; } = true;
    
    /// <summary>
    /// Lista de valores (linhas) associados a este Provider
    /// </summary>
    public List<ProviderValueItemDTO> Values { get; init; } = new();
}
