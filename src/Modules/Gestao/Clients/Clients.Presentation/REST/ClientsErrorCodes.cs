namespace Clients.Presentation.REST;

/// <summary>
/// Catálogo de códigos de resposta do módulo Clients.
/// </summary>
public static class ClientsErrorCodes
{
    public const string Success = "0000";

    // Erros CLxxx
    public const string ValidationError = "CL001";
    public const string BusinessRuleViolation = "CL002";
    public const string NotFound = "CL003";
}
