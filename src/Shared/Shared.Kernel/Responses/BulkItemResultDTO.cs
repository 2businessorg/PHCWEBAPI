namespace Shared.Kernel.Responses;

using System.Text.Json.Serialization;

/// <summary>
/// DTO de erro padrão para respostas bulk
/// </summary>
public class BulkErrorDTO
{
    /// <summary>
    /// Código de erro
    /// </summary>
    [JsonPropertyName("code")]
    public string Code { get; set; } = "";

    /// <summary>
    /// Mensagem de erro
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = "";
}

/// <summary>
/// DTO de resultado padrão para cada item de um lote
/// </summary>
public class BulkItemResultDTO
{
    /// <summary>
    /// Índice do item no lote original (0-based)
    /// </summary>
    [JsonPropertyName("index")]
    public int Index { get; set; }

    /// <summary>
    /// Sucesso da operação: true para sucesso, false para erro
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    /// <summary>
    /// Dados do item criado - não-null se sucesso (Success=true), null se erro
    /// </summary>
    [JsonPropertyName("data")]
    public object? Data { get; set; }

    /// <summary>
    /// Detalhes do erro - não-null se erro (Success=false), null se sucesso
    /// </summary>
    [JsonPropertyName("error")]
    public BulkErrorDTO? Error { get; set; }
}
