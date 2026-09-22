# 🔍 Auditoria de Usuários - Audit Logs

## 📋 Objetivo
Adicionar rastreamento de usuário aos logs de auditoria, permitindo identificar **quem** fez cada request na API.

---

## 🗄️ 1. Script SQL

### Arquivo: `AddUserInfoToAuditLogs.sql`

Execute este script no banco de dados para adicionar as colunas:

```sql
-- Colunas adicionadas:
- userId NVARCHAR(450) NULL     -- ID do usuário (AspNetUsers.Id)
- username NVARCHAR(256) NULL   -- Nome do usuário (AspNetUsers.UserName)

-- Índices criados:
- idx_ulogs_user     (username, userId) INCLUDE (data, code, operation)
- idx_ulogs_userid   (userId) INCLUDE (data, username, operation)
```

### Como Executar:

```powershell
# SQL Server Management Studio (SSMS)
sqlcmd -S localhost -d BILENE_DESENV -i AddUserInfoToAuditLogs.sql

# Ou abrir no SSMS e executar
```

---

## 🔧 2. Mudanças no Código

### 2.1 **Entidade AuditLog** 
📁 `Audit.Domain/Entities/AuditLog.cs`

```csharp
public class AuditLog
{
    // ... propriedades existentes ...
    
    // ✅ NOVAS PROPRIEDADES
    public string? UserId { get; private set; }     // ID do AspNetUsers
    public string? Username { get; private set; }   // UserName do AspNetUsers
    
    public AuditLog(
        // ... parâmetros existentes ...
        string? userId = null,      // ✅ NOVO
        string? username = null)    // ✅ NOVO
    {
        // ... código existente ...
        UserId = userId;
        Username = username;
    }
}
```

---

### 2.2 **Configuração EF Core**
📁 `Audit.Infrastructure/Persistence/Configurations/AuditLogConfigurationEFCore.cs`

```csharp
// ✅ Mapeamento das novas colunas
builder.Property(a => a.UserId)
    .HasColumnName("userId")
    .HasMaxLength(450)
    .IsUnicode(false);

builder.Property(a => a.Username)
    .HasColumnName("username")
    .HasMaxLength(256)
    .IsUnicode(false);

// ✅ Índices para consultas por usuário
builder.HasIndex(a => new { a.Username, a.UserId })
    .HasDatabaseName("idx_ulogs_user");
```

---

### 2.3 **Middleware - Captura do Usuário**
📁 `PHCAPI.Host/Middleware/ResponseLoggingMiddleware.cs`

```csharp
private async Task LogAuditAsync(...)
{
    // ✅ Capturar usuário do HttpContext.User (do JWT Token)
    var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var username = context.User?.Identity?.Name;

    auditLogService.LogResponseAsync(
        responseDto,
        context.TraceIdentifier,
        $"{context.Request.Method} {context.Request.Path}",
        context.Connection.RemoteIpAddress?.ToString(),
        context.Request.Headers.UserAgent.ToString(),
        context.Response.StatusCode,
        requestBody,
        responseJson,
        userId,      // ✅ NOVO
        username     // ✅ NOVO
    );
}
```

---

### 2.4 **Service & Job**
📁 `Audit.Application/Services/AuditLogService.cs`

```csharp
public void LogResponseAsync(
    ResponseDTO response,
    string? requestId,
    string operation,
    string? ipAddress,
    string? userAgent,
    int statusCode,
    string? requestBody = null,
    string? responseJson = null,
    string? userId = null,      // ✅ NOVO
    string? username = null)    // ✅ NOVO
{
    // Enfileirar job com userId e username
    _backgroundJobClient.Enqueue<SaveAuditLogJob>(job =>
        job.ExecuteAsync(code, requestId, responseDesc, operation,
            requestBody, responseJson, ipAddress, userId, username));
}
```

📁 `Audit.Application/Jobs/SaveAuditLogJob.cs`

```csharp
public async Task ExecuteAsync(
    string code,
    string? requestId,
    string responseDesc,
    string operation,
    string? requestBody = null,
    string? responseJson = null,
    string? ipAddress = null,
    string? userId = null,      // ✅ NOVO
    string? username = null)    // ✅ NOVO
{
    var auditLog = new AuditLog(
        code, requestId, responseDesc, operation,
        requestBody, responseJson, ipAddress,
        userId, username);  // ✅ Passa para entidade
    
    await _repository.AddAsync(auditLog);
}
```

---

## 🔄 3. Fluxo de Dados

```
1. Usuário autentica (Login)
   ↓
   JWT Token gerado com Claims:
   - ClaimTypes.NameIdentifier = userId
   - ClaimTypes.Name = username

2. Request com JWT Token
   ↓
   Middleware JWT valida e popula HttpContext.User

3. ResponseLoggingMiddleware
   ↓
   Captura: context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
           context.User?.Identity?.Name

4. AuditLogService
   ↓
   Enfileira job Hangfire com userId e username

5. SaveAuditLogJob (background)
   ↓
   Cria AuditLog com userId e username

6. Banco de Dados
   ↓
   INSERT INTO u_logs 
   (u_logsstamp, requestId, code, ..., userId, username)
   VALUES (...)
```

---

## 📊 4. Consultas SQL Úteis

### Ver logs por usuário:
```sql
SELECT 
    data,
    username,
    userId,
    operation,
    code,
    responseDesc,
    ip
FROM u_logs
WHERE username = 'admin'
ORDER BY data DESC;
```

### Top 10 usuários mais ativos:
```sql
SELECT TOP 10
    username,
    COUNT(*) as TotalRequests,
    COUNT(DISTINCT CAST(data AS DATE)) as DaysActive,
    MAX(data) as LastActivity
FROM u_logs
WHERE username IS NOT NULL
GROUP BY username
ORDER BY TotalRequests DESC;
```

### Operações por usuário (hoje):
```sql
SELECT 
    username,
    operation,
    COUNT(*) as Total
FROM u_logs
WHERE CAST(data AS DATE) = CAST(GETDATE() AS DATE)
  AND username IS NOT NULL
GROUP BY username, operation
ORDER BY username, Total DESC;
```

### Usuários com mais erros:
```sql
SELECT TOP 10
    username,
    code,
    COUNT(*) as ErrorCount
FROM u_logs
WHERE code != '0000'  -- Não é sucesso
  AND username IS NOT NULL
GROUP BY username, code
ORDER BY ErrorCount DESC;
```

---

## ✅ 5. Checklist de Implementação

- [x] Script SQL criado: `AddUserInfoToAuditLogs.sql`
- [x] Colunas `userId` e `username` adicionadas à tabela `u_logs`
- [x] Índices criados para performance
- [x] Entidade `AuditLog` atualizada
- [x] Configuração EF Core atualizada
- [x] `ResponseLoggingMiddleware` captura usuário do contexto
- [x] `IAuditLogService` atualizado
- [x] `AuditLogService` passa userId/username para job
- [x] `SaveAuditLogJob` salva userId/username no banco
- [ ] **Executar script SQL no banco de dados**
- [ ] Testar com usuário autenticado
- [ ] Verificar logs no banco com dados do usuário

---

## 🔐 6. Segurança & Privacidade

### Dados Capturados:
- ✅ **userId**: ID interno (GUID) do AspNetUsers
- ✅ **username**: Nome de usuário (não é email)
- ❌ **NÃO captura**: Senha, email, dados pessoais sensíveis

### GDPR/LGPD Compliance:
- Logs de auditoria são **necessários** para compliance
- Retenção de logs deve seguir política da empresa (ex: 90 dias, 1 ano)
- Implementar rotina de limpeza de logs antigos:

```sql
-- Exemplo: Eliminar logs com mais de 1 ano
DELETE FROM u_logs 
WHERE data < DATEADD(YEAR, -1, GETDATE());
```

---

## 🐛 7. Troubleshooting

### UserId/Username sempre NULL?
✅ **Verifique se o middleware de autenticação está configurado ANTES do ResponseLoggingMiddleware:**

```csharp
// Program.cs
app.UseAuthentication();  // ✅ DEVE VIR ANTES
app.UseAuthorization();   // ✅ DEVE VIR ANTES
app.UseMiddleware<ResponseLoggingMiddleware>();  // Depois
```

### Logs de endpoints anônimos não têm usuário?
✅ **Normal!** Endpoints sem `[Authorize]` não exigem autenticação, então `userId` e `username` serão NULL.

---

## 📚 8. Referências

- [ClaimTypes (Microsoft Docs)](https://docs.microsoft.com/en-us/dotnet/api/system.security.claims.claimtypes)
- [JWT Authentication ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/)
- [Audit Logging Best Practices](https://owasp.org/www-community/Logging_Best_Practices)

---

**Data de Implementação**: 2026-02-17  
**Autor**: GitHub Copilot  
**Versão**: 1.0
