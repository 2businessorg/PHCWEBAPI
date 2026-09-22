namespace Receipts.Application.Errors;

/// <summary>
/// Catálogo padronizado de códigos e descrições do módulo Receipts.
/// </summary>
public static class ReceiptsErrorCatalog
{
    public sealed record ErrorInfo(string Code, string Description);

    public static readonly ErrorInfo Success =
        new("0000", "Operação concluída com sucesso");

    public static readonly ErrorInfo ReceiptNotFound =
        new("RE001", "Recibo não encontrado");

    public static readonly ErrorInfo InvalidReceiptType =
        new("RE002", "Série de recibo inválida: {0}");

    public static readonly ErrorInfo ClientNotFound =
        new("RE003", "Cliente {0} não encontrado");

    public static readonly ErrorInfo InvoiceNotFoundInCc =
        new("RE004", "Factura nº {0} / ano {1} / série '{2}' não encontrada na conta corrente do cliente {3}");

    public static readonly ErrorInfo InvalidAmountSettled =
        new("RE006", "O valor a regularizar deve ser maior que zero");

    public static readonly ErrorInfo LinesRequired =
        new("RE007", "O recibo deve ter pelo menos uma linha de regularização");

    public static readonly ErrorInfo ValidationError =
        new("RE008", "Erro de validação");

    public static readonly ErrorInfo PhcWebIntegrationError =
        new("RE009", "Erro na integração com PHC WEB: {0}");

    public static readonly ErrorInfo TreasuryCodeRequired =
        new("RE010", "O código de tesouraria (treasuryCode) é obrigatório");
}
