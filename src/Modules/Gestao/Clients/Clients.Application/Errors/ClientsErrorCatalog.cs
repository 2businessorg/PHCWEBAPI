namespace Clients.Application.Errors;

/// <summary>
/// Catálogo padronizado de códigos e descrições do módulo Clients.
/// </summary>
public static class ClientsErrorCatalog
{
    public sealed record ErrorInfo(string Code, string Description);

    public static readonly ErrorInfo Success =
        new("0000", "Operação concluída com sucesso");

    public static readonly ErrorInfo DatabaseUpdateError =
        new("CL001", "Erro ao persistir cliente na base de dados");

    public static readonly ErrorInfo ClientNoEstabAlreadyExists =
        new("CL002", "Já existe um cliente com No {0} e Estab {1}");

    public static readonly ErrorInfo ClientNotFound =
        new("CL004", "Cliente não encontrado");

    public static readonly ErrorInfo ClientNcontAlreadyExists =
        new("CL005", "Cliente com Ncont {0} já existe");

    public static readonly ErrorInfo ClientNcontLinkedToAnotherNo =
        new("CL006", "Cliente com Ncont {0} já existe associado ao No {1}");

    public static readonly ErrorInfo InvalidNoEstabCombination =
        new("CL007", "Combinação inválida entre No e Estab");

    public static readonly ErrorInfo BulkMaxItemsExceeded =
        new("CL010", "Lote não pode conter mais de {0} itens");

    public static readonly ErrorInfo BulkDuplicateNcont =
        new("CL011", "Lote contém Ncont duplicado: {0}");

    public static readonly ErrorInfo BulkItemValidationFailed =
        new("CL012", "Item no índice {0} falhou validação: {1}");

    public static readonly ErrorInfo BulkItemDatabaseError =
        new("CL013", "Item no índice {0} falhou ao persistir: {1}");

    public static readonly ErrorInfo BulkProcessed =
        new("CL015", "Lote processado: {0} sucessos, {1} falhas");

    public static readonly ErrorInfo BulkEmpty =
        new("CL014", "Lote vazio ou inválido");
}
