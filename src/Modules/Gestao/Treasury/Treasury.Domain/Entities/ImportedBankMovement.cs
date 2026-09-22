namespace Treasury.Domain.Entities;

/// <summary>
/// Movement imported from a bank statement.
/// PHC table <c>br</c> — dictionary name: "Movimentos Bancários Importados".
/// </summary>
public class ImportedBankMovement
{
    /// <summary>Primary key (<c>brstamp</c>).</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Document date (<c>data</c>).</summary>
    public DateTime Date { get; set; }

    /// <summary>Value date (<c>dvalor</c>).</summary>
    public DateTime ValueDate { get; set; }

    /// <summary>Document reference (<c>documento</c>).</summary>
    public string Document { get; set; } = string.Empty;

    /// <summary>Bank description (<c>descricao</c>).</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Signed amount from the statement (<c>valor</c>).</summary>
    public decimal Amount { get; set; }

    /// <summary>Cheque / reference (<c>cheque</c>).</summary>
    public string Cheque { get; set; } = string.Empty;

    /// <summary>Treasury account code (<c>contado</c> → <c>bl.noconta</c>).</summary>
    public decimal AccountCode { get; set; }

    /// <summary>Whether the movement is already reconciled (<c>reco</c>).</summary>
    public bool Reconciled { get; set; }

    /// <summary>Whether the line was ignored during import (<c>ignorado</c>).</summary>
    public bool Ignored { get; set; }
}
