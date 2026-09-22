namespace Stocks.Domain.DTOs;

public class StockByWarehouseDTO
{
    public string Lote { get; set; } = string.Empty;
    public string Referencia { get; set; } = string.Empty;
    public decimal Armazem { get; set; }
    public string NomeArmazem { get; set; } = string.Empty;
    public decimal Stock { get; set; }
    public string Localizacao { get; set; } = string.Empty;
}
