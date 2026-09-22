namespace Currencies.Presentation.REST.Controllers;

/// <summary>
/// DTO de requisição para criar uma moeda.
/// </summary>
public sealed class CreateCurrencyRequest
{
    /// <summary>
    /// País da moeda (máx 12 caracteres).
    /// </summary>
    public string Pais { get; set; } = string.Empty;

    /// <summary>
    /// Código da moeda ISO 4217 (2-3 caracteres).
    /// </summary>
    public string Moeda { get; set; } = string.Empty;
}
