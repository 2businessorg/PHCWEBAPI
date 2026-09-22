namespace Audit.Application.DTOs;

/// <summary>
/// DTO de saída para logs de auditoria
/// </summary>
public sealed class AuditLogOutputDTO
{
    public string ULogsstamp { get; init; } = string.Empty;
    public string? RequestId { get; init; }
    public DateTime? Data { get; init; }
    public string? Code { get; init; }
    public string? Content { get; init; }
    public string? Ip { get; init; }
    public string? ResponseDesc { get; init; }
    public string? ResponseText { get; init; }
    public string? Operation { get; init; }
}
