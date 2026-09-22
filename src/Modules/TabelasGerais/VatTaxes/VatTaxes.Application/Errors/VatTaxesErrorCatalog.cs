namespace VatTaxes.Application.Errors;

/// <summary>
/// Catálogo central de erros do módulo VatTaxes
/// </summary>
public static class VatTaxesErrorCatalog
{
    public sealed record ErrorInfo(string Code, string Description);

    public static readonly ErrorInfo Success = new("0000", "Operação concluída com sucesso");
    public static readonly ErrorInfo VatTaxNotFound = new("VT001", "Taxa de IVA não encontrada");
    public static readonly ErrorInfo VatTaxAlreadyExists = new("VT002", "Já existe taxa com esta combinação de código e taxa");
    public static readonly ErrorInfo InvalidCode = new("VT003", "Código de taxa inválido");
    public static readonly ErrorInfo InvalidRate = new("VT004", "Valor de taxa IVA inválido");
    public static readonly ErrorInfo UpdatedSuccessfully = new("VT005", "Taxa atualizada com sucesso");
    public static readonly ErrorInfo PersistenceError = new("VT006", "Erro ao persistir taxa na base de dados");
}
