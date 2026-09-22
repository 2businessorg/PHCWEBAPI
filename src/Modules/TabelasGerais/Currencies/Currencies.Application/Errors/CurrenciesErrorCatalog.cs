namespace Currencies.Application.Errors;

/// <summary>
/// Catálogo padronizado de códigos e descrições do módulo Currencies.
/// </summary>
public static class CurrenciesErrorCatalog
{
    public sealed record ErrorInfo(string Code, string Description);

    public static readonly ErrorInfo Success =
        new("0000", "Operação concluída com sucesso");

    public static readonly ErrorInfo DatabaseUpdateError =
        new("CUR001", "Erro ao persistir moeda na base de dados");

    public static readonly ErrorInfo CurrencyAlreadyExists =
        new("CUR002", "Moeda já existe");

    public static readonly ErrorInfo InvalidExchangeRateCombination =
        new("CUR003", "Combinação de moeda e país inválida");

    public static readonly ErrorInfo CurrencyNotFound =
        new("CUR004", "Moeda não encontrada");

    public static readonly ErrorInfo ValidationError =
        new("CUR005", "Erro de validação");
}
