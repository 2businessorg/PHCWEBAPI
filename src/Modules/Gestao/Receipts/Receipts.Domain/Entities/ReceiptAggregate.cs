namespace Receipts.Domain.Entities;

/// <summary>
/// Aggregate root para um Recibo (Re + List&lt;Rl&gt;)
/// </summary>
public class ReceiptAggregate
{
    public Re Header { get; set; } = new();
    public List<Rl> Lines { get; set; } = new();
}
