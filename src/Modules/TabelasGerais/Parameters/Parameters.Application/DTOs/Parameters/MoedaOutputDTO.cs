namespace Parameters.Application.DTOs.Parameters;

/// <summary>
/// DTO de saída para moeda disponível.
/// </summary>
public sealed record MoedaOutputDTO
{
    public string Moeda { get; init; } = string.Empty;
}