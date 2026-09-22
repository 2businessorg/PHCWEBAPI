namespace Stocks.Application.Errors;

/// <summary>
/// Catálogo padronizado de códigos e descrições do módulo Stocks.
/// </summary>
public static class StocksErrorCatalog
{
    public sealed record ErrorInfo(string Code, string Description);

    public static readonly ErrorInfo Success =
        new("0000", "Operação concluída com sucesso");

    public static readonly ErrorInfo DatabaseUpdateError =
        new("ST001", "Erro ao persistir stock na base de dados");

    public static readonly ErrorInfo StockRefAlreadyExists =
        new("ST002", "Já existe um artigo com referência {0}");

    public static readonly ErrorInfo InvalidReference =
        new("ST003", "Referência inválida");

    public static readonly ErrorInfo StockNotFound =
        new("ST004", "Stock não encontrado");

    public static readonly ErrorInfo BulkMaxItemsExceeded =
        new("ST010", "Lote não pode conter mais de {0} itens");

    public static readonly ErrorInfo BulkDuplicateReference =
        new("ST011", "Lote contém referência duplicada: {0}");

    public static readonly ErrorInfo BulkItemValidationFailed =
        new("ST012", "Item no índice {0} falhou validação: {1}");

    public static readonly ErrorInfo BulkItemDatabaseError =
        new("ST013", "Item no índice {0} falhou ao persistir: {1}");

    public static readonly ErrorInfo BulkEmpty =
        new("ST014", "Lote vazio ou inválido");

    public static readonly ErrorInfo BulkProcessed =
        new("ST015", "Lote processado: {0} sucessos, {1} falhas");

    public static readonly ErrorInfo ValidationError =
        new("ST016", "Erro de validação");

    public static readonly ErrorInfo InvalidPriceTable =
        new("ST017", "Tabela de preço inválida (deve estar entre 1 e 5)");

}
