namespace VatTaxes.Domain;

/// <summary>
/// Entity de Taxa de IVA (Taxas de IVA - taxasiva)
/// </summary>
public class TaxasIva
{
    /// <summary>
    /// Stamp da taxa (identificador único)
    /// </summary>
    public string TaxasIvaStamp { get; set; } = string.Empty;

    /// <summary>
    /// Código da taxa
    /// </summary>
    public decimal Codigo { get; set; }

    /// <summary>
    /// Percentagem de IVA (ex: 16.00 para 16%)
    /// </summary>
    public decimal Taxa { get; set; }

    /// <summary>
    /// Referência da taxa (ex: "IVA_NORMAL")
    /// </summary>
    public string Ref { get; set; } = string.Empty;

    /// <summary>
    /// Descrição da taxa (ex: "IVA Normal")
    /// </summary>
    public string Design { get; set; } = string.Empty;

    /// <summary>
    /// Utilizador que criou
    /// </summary>
    public string UsrInis { get; set; } = string.Empty;

    /// <summary>
    /// Data de criação
    /// </summary>
    public DateTime UsrData { get; set; }

    /// <summary>
    /// Hora de criação
    /// </summary>
    public string UsrHora { get; set; } = string.Empty;

    /// <summary>
    /// Utilizador que atualizou
    /// </summary>
    public string OusrInis { get; set; } = string.Empty;

    /// <summary>
    /// Data de atualização
    /// </summary>
    public DateTime OusrData { get; set; }

    /// <summary>
    /// Hora de atualização
    /// </summary>
    public string OusrHora { get; set; } = string.Empty;

    /// <summary>
    /// Marcada para eliminação
    /// </summary>
    public bool Marcada { get; set; }
}
