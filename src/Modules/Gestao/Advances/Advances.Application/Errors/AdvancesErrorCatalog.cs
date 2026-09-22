namespace Advances.Application.Errors;

/// <summary>
/// Catálogo de erros do módulo Advances
/// </summary>
public static class AdvancesErrorCatalog
{
    public static readonly (string Code, string Description) Success              = ("0000", "Operação concluída com sucesso");
    public static readonly (string Code, string Description) AdvanceNotFound      = ("AD001", "Adiantamento não encontrado");
    public static readonly (string Code, string Description) InvalidAdvanceType   = ("AD002", "Série de adiantamento inválida: {0}");
    public static readonly (string Code, string Description) ClientNotFound       = ("AD003", "Cliente {0} não encontrado");
    public static readonly (string Code, string Description) InvalidAmount        = ("AD004", "O valor do adiantamento deve ser maior que zero");
    public static readonly (string Code, string Description) ValidationError      = ("AD005", "Erro de validação");
    public static readonly (string Code, string Description) PhcWebIntegrationError = ("AD006", "Erro na integração com PHC WEB: {0}");
    public static readonly (string Code, string Description) InternalError        = ("AD999", "Erro interno");
}
