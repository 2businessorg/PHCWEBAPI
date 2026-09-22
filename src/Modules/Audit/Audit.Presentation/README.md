# Audit.Presentation

Camada de apresentação do módulo **Audit**, expondo APIs REST para consulta de logs de auditoria.

## 📋 Endpoints REST

### 🔍 Buscar todos os logs com filtros
```http
GET /api/audit?startDate=2024-01-01&endDate=2024-12-31&correlationId=abc123&operation=CreateParameter&pageNumber=1&pageSize=50
```

**Parâmetros Query (todos opcionais):**
- `startDate` - Data inicial do filtro
- `endDate` - Data final do filtro
- `correlationId` - Filtrar por Correlation ID
- `operation` - Filtrar por nome da operação
- `pageNumber` - Número da página (padrão: 1)
- `pageSize` - Tamanho da página (padrão: 50)

**Response:**
```json
{
  "code": "0000",
  "data": [
    {
      "uLogsstamp": "20240210140530123456789AB",
      "requestId": "550e8400-e29b-41d4-a716-446655440000",
      "data": "2024-02-10T14:05:30.123Z",
      "code": "0000",
      "content": "{\"path\":\"/api/parameters\"}",
      "ip": "192.168.1.100",
      "responseDesc": "Success",
      "responseText": "{\"message\":\"Operation completed successfully\"}",
      "operation": "GET /api/parameters"
    }
  ],
  "content": null,
  "correlationId": "550e8400-e29b-41d4-a716-446655440000"
}
```

### 🎯 Buscar log específico por stamp
```http
GET /api/audit/{uLogsstamp}
```

**Exemplo:**
```http
GET /api/audit/20240210140530123456789AB
```

**Response (200 OK):**
```json
{
  "code": "0000",
  "data": {
    "uLogsstamp": "20240210140530123456789AB",
    "requestId": "550e8400-e29b-41d4-a716-446655440000",
    "data": "2024-02-10T14:05:30.123Z",
    "code": "0000",
    "content": "{\"path\":\"/api/parameters\"}",
    "ip": "192.168.1.100",
    "responseDesc": "Success",
    "responseText": "{\"message\":\"Operation completed successfully\"}",
    "operation": "GET /api/parameters"
  },
  "content": null,
  "correlationId": "550e8400-e29b-41d4-a716-446655440000"
}
```

**Response (404 Not Found):**
```json
{
  "code": "0015",
  "data": null,
  "content": "Resource not found",
  "correlationId": "550e8400-e29b-41d4-a716-446655440000"
}
```

### 🔗 Buscar logs por Correlation ID
```http
GET /api/audit/correlation/{correlationId}
```

**Exemplo:**
```http
GET /api/audit/correlation/550e8400-e29b-41d4-a716-446655440000
```

Útil para rastrear toda a jornada de uma requisição através da API.

**Response:**
```json
{
  "code": "0000",
  "data": [
    {
      "uLogsstamp": "20240210140530123456789AB",
      "requestId": "550e8400-e29b-41d4-a716-446655440000",
      "data": "2024-02-10T14:05:30.123Z",
      "code": "0000",
      "content": "{\"path\":\"/api/parameters\"}",
      "ip": "192.168.1.100",
      "responseDesc": "Success",
      "responseText": "{\"message\":\"Parameter created successfully\"}",
      "operation": "POST /api/parameters"
    }
  ],
  "content": "Found 1 logs for correlation ID: 550e8400-e29b-41d4-a716-446655440000",
  "correlationId": "current-request-correlation-id"
}
```

## 🏗️ Arquitetura

```
Audit.Presentation/
├── REST/
│   ├── Controllers/
│   │   └── AuditController.cs         ← Endpoints REST
│   └── RestDependencyInjection.cs     ← Configuração REST
└── DependencyInjection.cs             ← Entry point do módulo
```

## 🔧 Configuração

### Program.cs
```csharp
using Audit.Presentation;

var builder = WebApplication.CreateBuilder(args);

// Registrar módulo Audit (REST habilitado por padrão)
builder.Services.AddAuditPresentation(builder.Configuration, enableRest: true);

// ... resto da configuração
```

## 📦 Dependências

- **MediatR** - CQRS pattern
- **Audit.Application** - Queries e handlers
- **Audit.Infrastructure** - Repositórios e persistência
- **Shared.Kernel** - ResponseDTO e extensões

## 🎯 Casos de uso

### 1. Debugging de erro em produção
```bash
# Buscar todos os logs de um request específico
curl -X GET "https://api.example.com/api/audit/correlation/550e8400-e29b-41d4-a716-446655440000"
```

### 2. Auditoria de operações
```bash
# Buscar todas as criações de parâmetros no último mês
curl -X GET "https://api.example.com/api/audit?operation=CreateParameter&startDate=2024-01-01&endDate=2024-01-31"
```

### 3. Análise de falhas
```bash
# Buscar todos os erros (código != 0000) em um período
curl -X GET "https://api.example.com/api/audit?startDate=2024-02-01&endDate=2024-02-10"
```

## ✅ Status
- ✅ Queries implementadas
- ✅ Controller REST implementado
- ✅ DTOs definidos
- ✅ Documentação Swagger
- ⏳ GraphQL (futuro)
- ⏳ Paginação avançada (futuro)
