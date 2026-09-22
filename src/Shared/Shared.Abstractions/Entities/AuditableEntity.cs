namespace Shared.Abstractions.Entities;


public abstract class AuditableEntity
{
    // Campos de criação original (O = Original)
    public DateTime OUsrData { get; set; }

    public string OUsrHora { get; set; } = string.Empty;
 
    public string? OUsrInis { get; set; }

    // ✅ Campos de última atualização (sempre preenchidos, inclusive na criação)
    public DateTime UsrData { get; set; }

    public string UsrHora { get; set; } = string.Empty;
  
    public string? UsrInis { get; set; }

    protected void SetCreatedAudit(string? createdBy)
    {
        var now = DateTime.UtcNow;
        var timeString = now.ToString("HH:mm:ss");
        
        // Campos de criação original (O = Original)
        OUsrData = now;
        OUsrHora = timeString;
        OUsrInis = createdBy;
        
        // ✅ Inicializar também os campos de atualização na criação (padrão PHC)
        // Isso evita NULL em campos que não permitem nulo no banco
        UsrData = now;
        UsrHora = timeString;
        UsrInis = createdBy;
    }

   
    protected void SetUpdatedAudit(string? updatedBy)
    {
        var now = DateTime.UtcNow;
        UsrData = now;
        UsrHora = now.ToString("HH:mm:ss");
        UsrInis = updatedBy;
    }
}
