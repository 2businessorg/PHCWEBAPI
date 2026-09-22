namespace Currencies.Presentation.REST.Controllers;

/// <summary>
/// DTO de requisição para atualizar uma moeda.
/// </summary>
public sealed class UpdateCurrencyRequest
{
    /// <summary>
    /// País da moeda (máx 12 caracteres).
    /// </summary>
    public string Pais { get; set; } = string.Empty;
}
