using Shared.Kernel.Extensions;

namespace Audit.Domain.Entities;

public class AuditLog
{
    public string ULogsstamp { get; private set; } = 25.GenerateStamp();
    public string? RequestId { get; private set; }
    public DateTime? Data { get; private set; }
    public string? Code { get; private set; }
    public string? Content { get; private set; }
    public string? Ip { get; private set; }
    public string? ResponseDesc { get; private set; }
    public string? ResponseText { get; private set; }
    public string? Operation { get; private set; }
    
    // ✅ Identificação do usuário que fez o request
    public string? UserId { get; private set; }
    public string? Username { get; private set; }

    private AuditLog() { }

    public AuditLog(
        string code,
        string? requestId,
        string responseDesc,
        string? operation,
        string? content = null,
        string? responseText = null,
        string? ip = null,
        string? userId = null,
        string? username = null)
    {
        ULogsstamp = 25.GenerateStamp();
        RequestId = requestId;
        Data = DateTime.UtcNow;
        Code = code;
        Content = content;
        Ip = ip;
        ResponseDesc = responseDesc;
        ResponseText = responseText;
        Operation = operation;
        UserId = userId;
        Username = username;
    }

  
    public void UpdateResponseText(string responseText)
    {
        ResponseText = responseText;
    }
}
