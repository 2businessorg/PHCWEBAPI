# Audit Module

Módulo de **Auditoria** implementado com **Clean Architecture** para rastreamento de todas as operações da API.

## 🏗️ Arquitetura

```
Audit/
├── Audit.Domain/          ← Entidades (AuditLog), Interfaces (IAuditLogRepository)
├── Audit.Application/     ← Services (IAuditLogService)
└── Audit.Infrastructure/  ← EF Core (Repositories, DbContext), Hangfire (BackgroundJobs)
```

## 🎯 Objetivo

Registrar **automaticamente** todas as responses da API no banco de dados para:
- ✅ **Rastreabilidade** - Saber quem fez o quê e quando
- ✅ **Auditoria** - Compliance e regulamentações
- ✅ **Debugging** - Investigar problemas usando Correlation ID
- ✅ **Analytics** - Analisar padrões de uso da API

## 📊 Modelo de Dados

### Tabela `u_logs` (Database First)

| Campo | Tipo | Descrição |
|-------|------|-----------|
| `u_logsstamp` | VARCHAR(50) PK | Identificador único do log |
| `requestId` | VARCHAR | Correlation ID do request |
| `data` | DATETIME | Timestamp do log |
| `code` | VARCHAR | Código da resposta (0000, 0015, etc.) |
| `content` | TEXT | JSON do campo `content` da ResponseDTO |
| `ip` | VARCHAR | IP do cliente |
| `responseDesc` | VARCHAR | Descrição da resposta |
| `responsetext` | TEXT | JSON do campo `data` da ResponseDTO |
| `operation` | VARCHAR | Operação executada (ex: "GET /api/parameters") |

## 🔄 Fluxo de Auditoria

```
1. Request → API
   ↓
2. CorrelationIdMiddleware
   └── Gera/captura Correlation ID
   ↓
3. Controller/ExceptionHandler
   └── Processa e cria ResponseDTO
   ↓
4. ResponseAuditMiddleware (futuro)
   └── Intercepta ResponseDTO
   └── Chama: AuditLogService.LogResponseAsync()
   ↓
5. AuditLogService
   └── Enfileira job no Hangfire
   └── Não bloqueia o request ✅
   ↓
6. Hangfire Background Job
   └── AuditLogBackgroundJob.SaveAuditLogAsync()
   └── Salva no banco: u_logs
```

## 🚀 Uso

### Automático (via Middleware - Recomendado)

Será implementado no próximo passo - um middleware único que audita automaticamente todas as responses.

### Manual (em Controllers - Não Recomendado)

```csharp
public class ParametersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IAuditLogService _auditLogService;
    
    [HttpGet]
    public async Task<ActionResult<ResponseDTO>> GetAll(...)
    {
        var result = await _mediator.Send(...);
        var response = ResponseDTO.Success(data: result, ...);
        
        // ✅ Log assíncrono (Hangfire)
        _auditLogService.LogResponseAsync(
            response,
            HttpContext.GetCorrelationId()?.ToString(),
            "GetAllParameters",
            HttpContext);
        
        return Ok(response);
    }
}
```

## 📦 Dependências

### Pacotes NuGet
- `Hangfire.Core` 1.8.9 - Background jobs
- `Microsoft.EntityFrameworkCore` 8.0.0
- `Microsoft.EntityFrameworkCore.SqlServer` 8.0.0

### Módulos Internos
- `Shared.Kernel` - ResponseDTO, Correlation ID

## ⚙️ Configuração

### 1. Adicionar ao Program.cs

```csharp
// src/PHCAPI.Host/Program.cs

// ✅ Registrar Audit Module
builder.Services.AddAuditApplication();
builder.Services.AddAuditInfrastructure(builder.Configuration);

// ✅ Registrar Hangfire
builder.Services.AddHangfire(config =>
{
    config.UseSqlServerStorage(
        builder.Configuration.GetConnectionString("DBconnect"));
});
builder.Services.AddHangfireServer();

var app = builder.Build();

// ✅ Hangfire Dashboard (opcional - apenas dev)
if (app.Environment.IsDevelopment())
{
    app.UseHangfireDashboard("/hangfire");
}
```

### 2. Connection String (já existe)

```json
{
  "ConnectionStrings": {
    "DBconnect": "Server=SRV05\\SQLDEV2022;Database=BILENE_DESENV;..."
  }
}
```

### 3. Tabela no Banco (Database First)

A tabela `u_logs` **já existe** no banco PHC. Não precisa criar migrations.

## 🔍 Consultar Logs

### Por Correlation ID (Rastrear Request)

```csharp
var logs = await _auditLogRepository.GetByRequestIdAsync("38475928412345");

// Resultado: Todos os logs desse request específico
// Útil para debugging
```

### Por Código de Resposta (Encontrar Erros)

```csharp
var errors = await _auditLogRepository.GetByCodeAsync("0015");

// Resultado: Todos os erros de validação
```

### Por Período (Analytics)

```csharp
var logs = await _auditLogRepository.GetByDateRangeAsync(
    startDate: DateTime.Today,
    endDate: DateTime.Now);

// Resultado: Todos os logs de hoje
```

### Por Operação (Uso de Endpoints)

```csharp
var logs = await _auditLogRepository.GetByOperationAsync("GET /api/parameters");

// Resultado: Quantas vezes GetAll foi chamado
```

## 🎓 Padrões Arquiteturais

### ✅ Clean Architecture
- **Domain**: Entidades puras (sem dependências)
- **Application**: Lógica de negócio (services)
- **Infrastructure**: Implementações técnicas (EF Core, Hangfire)

### ✅ Database First
- Tabela `u_logs` já existe no banco PHC
- EF Core apenas **mapeia**, não cria

### ✅ Async/Background Processing
- Usa Hangfire para processamento assíncrono
- Não impacta performance dos requests

### ✅ Retry Logic
- Hangfire retenta automaticamente (3 tentativas)
- Delays: 10s, 30s, 60s

### ✅ Modular
- Módulo independente
- Pode ser usado por qualquer outro módulo
- Fácil desabilitar (remover middleware)

## 🔐 Segurança

### Dados Sensíveis
- ⚠️ **ResponseText** pode conter dados sensíveis
- Considere:
  - Mascarar senhas, tokens, etc.
  - Não logar campos sensíveis
  - Criptografar logs (se necessário)

### Exemplo: Mascarar Dados

```csharp
// Em AuditLogService (futuro)
private string MaskSensitiveData(string json)
{
    // Mascarar password, token, etc.
    return json
        .Replace("\"password\":\"...\", "\"password\":\"***\"")
        .Replace("\"token\":\"...\", "\"token\":\"***\"");
}
```

## 📈 Performance

### ✅ Não Bloqueia Requests
- Hangfire processa em background
- Request retorna imediatamente

### ✅ Índices no Banco
- `idx_ulogs_requestid` - Buscar por Correlation ID
- `idx_ulogs_data` - Buscar por período
- `idx_ulogs_code` - Buscar por código de resposta
- `idx_ulogs_operation` - Buscar por operação

### ⚠️ Volume de Dados
- Tabela `u_logs` pode crescer rapidamente
- Considere:
  - Particionamento por data
  - Arquivamento de logs antigos
  - Retenção de 90 dias (configurável)

## 🧪 Testes

### Unit Tests (Futuro)
```
tests/Unit/Audit.Domain.Tests/
tests/Unit/Audit.Application.Tests/
```

### Integration Tests (Futuro)
```
tests/Integration/Audit.Infrastructure.Tests/
```

## 🔄 Próximos Passos

1. ✅ **Módulo Audit criado** (concluído)
2. 🔜 **Criar Response Audit Middleware** (único)
3. 🔜 **Registrar no Program.cs**
4. 🔜 **Testar fluxo completo**
5. 🔜 **Adicionar dashboard de logs** (opcional)

## 📚 Referências

- [Hangfire Documentation](https://docs.hangfire.io/)
- [EF Core Documentation](https://docs.microsoft.com/ef/core/)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)

---

**Status:** ✅ Módulo criado e pronto para uso!  
**Próximo:** Implementar middleware único para auditoria automática.
