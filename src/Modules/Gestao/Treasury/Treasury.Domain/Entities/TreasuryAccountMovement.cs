namespace Treasury.Domain.Entities;

/// <summary>
/// Movement posted in a PHC treasury account.
/// PHC table <c>ba</c> — dictionary name: "Movimentos em Contas de Tesouraria".
/// </summary>
public class TreasuryAccountMovement
{
    /// <summary>Primary key (<c>bastamp</c>).</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Document date (<c>data</c>).</summary>
    public DateTime Date { get; set; }

    /// <summary>Value date (<c>dvalor</c>).</summary>
    public DateTime ValueDate { get; set; }

    /// <summary>Document reference (<c>documento</c>).</summary>
    public string Document { get; set; } = string.Empty;

    /// <summary>Movement description (<c>descricao</c>).</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Inflow amount (<c>entrada</c>). Kept separate from outflow.</summary>
    public decimal Inflow { get; set; }

    /// <summary>Outflow amount (<c>saida</c>). Kept separate from inflow.</summary>
    public decimal Outflow { get; set; }

    /// <summary>Cheque / reference (<c>cheque</c>).</summary>
    public string Cheque { get; set; } = string.Empty;

    /// <summary>Treasury account code (<c>contado</c> → <c>bl.noconta</c>).</summary>
    public decimal AccountCode { get; set; }

    /// <summary>Whether the movement is already reconciled (<c>reco</c>).</summary>
    public bool Reconciled { get; set; }
}
