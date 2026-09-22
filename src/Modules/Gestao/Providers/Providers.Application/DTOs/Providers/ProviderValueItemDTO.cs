namespace Providers.Application.DTOs.Providers;

/// <summary>
/// DTO para linhas de valores do Provider (usado em Create/Update)
/// </summary>
public record ProviderValueItemDTO
{
    public string OperationCode { get; init; } = string.Empty;
    public string Chave { get; init; } = string.Empty;
    public string Valor { get; init; } = string.Empty;
    public bool Encriptado { get; init; }
    public int Ordem { get; init; }
    public bool Ativo { get; init; } = true;
}
