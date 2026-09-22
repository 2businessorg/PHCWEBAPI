namespace Providers.Application.DTOs.Providers;

/// <summary>
/// DTO de entrada para atualizar Provider com seus valores
/// </summary>
public record UpdateProviderInputDTO
{
    public int Codigo { get; init; }
    public string Provedor { get; init; } = string.Empty;
    public string Environment { get; init; } = string.Empty;
    public string Descricao { get; init; } = string.Empty;
    public bool Ativo { get; init; }
    
    /// <summary>
    /// Lista de valores (linhas) associados a este Provider.
    /// No update, as linhas antigas são eliminadas e estas novas são criadas.
    /// </summary>
    public List<ProviderValueItemDTO> Values { get; init; } = new();
}
