namespace Stocks.Domain.DTOs;

public class BatchDetailDTO
{
    public string Lote { get; set; } = string.Empty;
    public string Referencia { get; set; } = string.Empty;
    public string Design { get; set; } = string.Empty;
    public string Forlote { get; set; } = string.Empty;
    public decimal Stock { get; set; }
    public decimal Qttacout { get; set; }
    public decimal Qttacin { get; set; }
    public DateTime? Uintr { get; set; }
    public DateTime? Validade { get; set; }
    public DateTime? Datafact { get; set; }
    public decimal Pcult { get; set; }
}
