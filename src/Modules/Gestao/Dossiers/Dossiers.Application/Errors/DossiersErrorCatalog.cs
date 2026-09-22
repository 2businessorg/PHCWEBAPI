namespace Dossiers.Application.Errors;

/// <summary>
/// Catálogo padronizado de códigos e descrições do módulo Dossiers.
/// </summary>
public static class DossiersErrorCatalog
{
    public sealed record ErrorInfo(string Code, string Description);

    public static readonly ErrorInfo Success =
        new("0000", "Operação concluída com sucesso");

    public static readonly ErrorInfo DatabaseUpdateError =
        new("BO001", "Erro ao persistir dossier na base de dados");

    public static readonly ErrorInfo DossierAlreadyExistsByKey =
        new("BO002", "Já existe dossier com chave {0}/{1}/{2}");

    public static readonly ErrorInfo DossierAlreadyExistsByNdos =
        new("BO003", "Já existe dossier com NDos {0}");

    public static readonly ErrorInfo DossierNotFound =
        new("BO004", "Dossier não encontrado");

    public static readonly ErrorInfo ClientNotFound =
        new("BO005", "{0} {1}/{2} não encontrado");

    public static readonly ErrorInfo InvalidTipoDossier =
        new("BO006", "Número do Tipo de Dossier inválido: {0}");

    public static readonly ErrorInfo ReferenceNotFound =
        new("BO007", "Referência {0} não existe");

    public static readonly ErrorInfo InvalidReference =
        new("BO016", "Produto/Referência não encontrada: {0}");

    public static readonly ErrorInfo InvalidQuantity =
        new("BO008", "Quantidade deve ser maior que 0");

    public static readonly ErrorInfo ValidationError =
        new("BO009", "Erro de validação");

    public static readonly ErrorInfo InvalidCurrency =
        new("BO012", "Moeda {0} inválida");

    public static readonly ErrorInfo InvalidVatTable =
        new("BO013", "Código de IVA {0} inválido");

    public static readonly ErrorInfo BulkEmpty =
        new("BO010", "Lote vazio ou inválido");

    public static readonly ErrorInfo BulkMaxItemsExceeded =
        new("BO011", "Lote não pode conter mais de {0} itens");

    /// <summary>
    /// Mapeia código de tabela para nome legível
    /// </summary>
    public static string GetTableName(string? tableCode)
    {
        return (tableCode ?? string.Empty).Trim().ToUpperInvariant() switch
        {
            "CL" => "Cliente",
            "FL" => "Fornecedor",
            "AG" => "Entidade",
            "EM" => "Contacto",
            _ => "Tabela"
        };
    }

    public static readonly ErrorInfo BulkPartialSuccess =
        new("BO015", "Lote processado: {0} sucessos, {1} falhas");

    public static readonly ErrorInfo PhcWebIntegrationError =
        new("BO014", "Erro na integração com PHC WEB: {0}");
}
