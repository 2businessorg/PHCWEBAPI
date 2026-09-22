namespace Invoices.Application.Errors;

/// <summary>
/// Catálogo padronizado de códigos e descrições do módulo Invoices
/// </summary>
public static class InvoicesErrorCatalog
{
    public sealed record ErrorInfo(string Code, string Description);

    public static readonly ErrorInfo Success =
        new("0000", "Operação concluída com sucesso");

    public static readonly ErrorInfo ValidationError =
        new("FT001", "Erro de validação");

    public static readonly ErrorInfo InvoiceNotFound =
        new("FT002", "Fatura não encontrada");

    public static readonly ErrorInfo InvalidDocType =
        new("FT003", "Não existe uma serie de facturação com ID: : {0}");

    public static readonly ErrorInfo ClientNotFound =
        new("FT004", "Cliente não encontrado: {0}/{1}");

    public static readonly ErrorInfo ProductNotFound =
        new("FT005", "Referência do stock não existe: {0}");

    public static readonly ErrorInfo InvalidQuantity =
        new("FT006", "Quantidade deve ser maior que 0");

    public static readonly ErrorInfo InvalidVatCode =
        new("FT007", "Código de IVA inválido: {0}");

    public static readonly ErrorInfo InvalidCurrency =
        new("FT008", "Moeda inválida: {0}");

    public static readonly ErrorInfo InvalidDate =
        new("FT009", "Data em formato inválido");

    public static readonly ErrorInfo InvalidHour =
        new("FT010", "Hora em formato inválido");

    public static readonly ErrorInfo DuplicateInvoice =
        new("FT011", "Já existe fatura com chave {0}/{1}/{2}");

    public static readonly ErrorInfo PhcWebError =
        new("FT012", "Erro ao processar no PHC Web: {0}");

    public static readonly ErrorInfo InvalidWarehouse =
        new("FT013", "Armazém inválido: {0}");

    public static readonly ErrorInfo InvalidPaymentCondition =
        new("FT014", "Condição de pagamento inválida: {0}");

    public static readonly ErrorInfo NoLines =
        new("FT015", "Nenhuma linha fornecida");

    public static readonly ErrorInfo DatabaseError =
        new("FT999", "Erro interno do servidor");
}
