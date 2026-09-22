namespace Stocks.Presentation.REST;

/// <summary>
/// Catálogo de códigos de resposta do módulo Stocks.
/// </summary>
public static class StocksErrorCodes
{
    public const string Success = "0000";
    public const string ValidationError = "ST001";
    public const string BusinessRuleViolation = "ST002";
    public const string NotFound = "ST004";
}
