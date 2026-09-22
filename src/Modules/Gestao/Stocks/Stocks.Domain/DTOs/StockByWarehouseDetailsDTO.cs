namespace Stocks.Domain.DTOs;

/// <summary>
/// Domain DTO for stock data grouped by warehouse (warehouse-only view, not batch-specific)
/// </summary>
public class StockByWarehouseDetailsDTO
{
    public int Armazem { get; set; }
    public string? NomeArmazem { get; set; }
    public decimal Stock { get; set; }
    public decimal CustoStock { get; set; }
    public string? Localizacao { get; set; }
    public decimal EncomendadoPorClientes { get; set; }
    public decimal EncomendadoAFornecedores { get; set; }
    public decimal StockMinimo { get; set; }
    public decimal QuantidadeEmRecepcao { get; set; }
    public decimal QuantidadeCativada { get; set; }
}
