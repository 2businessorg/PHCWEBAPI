# 📦 DTO Pattern (Data Transfer Objects)

> **Nível**: Senior/Enterprise  
> **Camada**: Application Layer  
> **Responsabilidade**: Transferência de dados entre camadas sem expor entidades de domínio

---

## 🎯 Objetivo

DTOs são objetos simples que transportam dados entre camadas, isolando o domínio da apresentação. **NÃO** contêm lógica de negócio, apenas propriedades.

---

## 📋 Tipos de DTOs

### 1. Input DTOs (Request)
Dados que entram na aplicação (POST, PUT).

### 2. Output DTOs (Response)
Dados que saem da aplicação (GET responses).

### 3. Internal DTOs
Dados trocados entre camadas internas.

---

## 🔧 Implementação Passo a Passo

### Estrutura de Pastas

```
ModuleName.Application/
└── DTOs/
    └── Entities/
        ├── CreateEntityInputDTO.cs    ← Input para criação
        ├── UpdateEntityInputDTO.cs    ← Input para atualização
        └── EntityOutputDTO.cs         ← Output (resposta)
```

---

## 📝 Input DTOs

### CreateEntityInputDTO

**Localização**: `DTOs/Entities/CreateEntityInputDTO.cs`

```csharp
namespace ModuleName.Application.DTOs.Entities;

/// <summary>
/// DTO para criação de uma entidade
/// </summary>
public record CreateEntityInputDTO
{
    /// <summary>
    /// Código único da entidade
    /// </summary>
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// Descrição da entidade
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Valor opcional
    /// </summary>
    public decimal? Value { get; init; }

    /// <summary>
    /// Categoria
    /// </summary>
    public string Category { get; init; } = string.Empty;

    /// <summary>
    /// Status ativo/inativo
    /// </summary>
    public bool IsActive { get; init; } = true;
}
```

### UpdateEntityInputDTO

**Localização**: `DTOs/Entities/UpdateEntityInputDTO.cs`

```csharp
namespace ModuleName.Application.DTOs.Entities;

/// <summary>
/// DTO para atualização de uma entidade
/// </summary>
public record UpdateEntityInputDTO
{
    /// <summary>
    /// Descrição da entidade
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Valor opcional
    /// </summary>
    public decimal? Value { get; init; }

    /// <summary>
    /// Categoria
    /// </summary>
    public string Category { get; init; } = string.Empty;

    /// <summary>
    /// Status ativo/inativo
    /// </summary>
    public bool IsActive { get; init; } = true;
}
```

#### ✅ **Boas Práticas - Input DTOs**:
- ✅ Use `record` (imutável)
- ✅ Propriedades com `init` apenas
- ✅ Valores padrão quando apropriado
- ✅ **NÃO** incluir `Stamp` ou `Id` (gerado pelo sistema)
- ✅ **NÃO** incluir dados de auditoria (`CreatedBy`, `UpdatedBy`)
- ✅ Documentação XML em todas propriedades
- ✅ Nullable (`?`) para campos opcionais

#### ❌ **Evitar**:
- ❌ Setters públicos (`{ get; set; }`)
- ❌ Lógica de negócio
- ❌ Métodos
- ❌ Incluir entidades relacionadas (usar IDs)

---

## 📤 Output DTOs

### EntityOutputDTO

**Localização**: `DTOs/Entities/EntityOutputDTO.cs`

```csharp
namespace ModuleName.Application.DTOs.Entities;

/// <summary>
/// DTO de resposta para Entity
/// </summary>
public record EntityOutputDTO
{
    /// <summary>
    /// Identificador único (Stamp)
    /// </summary>
    public string Stamp { get; init; } = string.Empty;

    /// <summary>
    /// Código único da entidade
    /// </summary>
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// Descrição da entidade
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Valor opcional
    /// </summary>
    public decimal? Value { get; init; }

    /// <summary>
    /// Categoria
    /// </summary>
    public string Category { get; init; } = string.Empty;

    /// <summary>
    /// Status ativo/inativo
    /// </summary>
    public bool IsActive { get; init; }

    // Auditoria
    /// <summary>
    /// Data de criação
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Hora de criação
    /// </summary>
    public string CreatedTime { get; init; } = string.Empty;

    /// <summary>
    /// Usuário que criou
    /// </summary>
    public string? CreatedBy { get; init; }

    /// <summary>
    /// Data de atualização
    /// </summary>
    public DateTime? UpdatedAt { get; init; }

    /// <summary>
    /// Hora de atualização
    /// </summary>
    public string? UpdatedTime { get; init; }

    /// <summary>
    /// Usuário que atualizou
    /// </summary>
    public string? UpdatedBy { get; init; }
}
```

#### ✅ **Boas Práticas - Output DTOs**:
- ✅ Use `record` (imutável)
- ✅ Inclua `Stamp` ou `Id`
- ✅ Inclua dados de auditoria
- ✅ Propriedades com `init` apenas
- ✅ Documentação XML completa
- ✅ Nomes amigáveis (não colunas de banco)

#### ❌ **Evitar**:
- ❌ Expor entidades de domínio diretamente
- ❌ Incluir senhas ou dados sensíveis
- ❌ Campos internos do banco

---

## 🏗️ Exemplos Práticos

### Exemplo 1: DTO com Relacionamentos

```csharp
/// <summary>
/// DTO de resposta para Order com Items
/// </summary>
public record OrderOutputDTO
{
    public string OrderStamp { get; init; } = string.Empty;
    public string CustomerCode { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public DateTime OrderDate { get; init; }

    /// <summary>
    /// Lista de itens do pedido
    /// </summary>
    public IEnumerable<OrderItemOutputDTO> Items { get; init; } = Array.Empty<OrderItemOutputDTO>();
}

public record OrderItemOutputDTO
{
    public string ProductCode { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal TotalPrice { get; init; }
}
```

### Exemplo 2: DTO com Enums

```csharp
public record CreateOrderInputDTO
{
    public string CustomerCode { get; init; } = string.Empty;
    
    /// <summary>
    /// Tipo de pagamento: CreditCard, DebitCard, Cash
    /// </summary>
    public PaymentType PaymentType { get; init; }
    
    public IEnumerable<CreateOrderItemInputDTO> Items { get; init; } = Array.Empty<CreateOrderItemInputDTO>();
}

public enum PaymentType
{
    CreditCard = 1,
    DebitCard = 2,
    Cash = 3
}
```

### Exemplo 3: DTO Paginado

```csharp
/// <summary>
/// Resultado paginado
/// </summary>
public class PagedResult<T>
{
    /// <summary>
    /// Lista de itens da página atual
    /// </summary>
    public IEnumerable<T> Items { get; init; } = Array.Empty<T>();

    /// <summary>
    /// Total de registros
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// Número da página atual
    /// </summary>
    public int PageNumber { get; init; }

    /// <summary>
    /// Tamanho da página
    /// </summary>
    public int PageSize { get; init; }

    /// <summary>
    /// Total de páginas
    /// </summary>
    public int TotalPages { get; init; }

    /// <summary>
    /// Indica se há página anterior
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    /// Indica se há próxima página
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;
}
```

### Exemplo 4: DTO Response Padrão

```csharp
/// <summary>
/// Response padrão da API
/// </summary>
public record ResponseDTO
{
    /// <summary>
    /// Indica se a operação foi bem-sucedida
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Mensagem ou código de resposta
    /// </summary>
    public string? Message { get; init; }

    /// <summary>
    /// Dados da resposta
    /// </summary>
    public object? Data { get; init; }

    /// <summary>
    /// Erros (se houver)
    /// </summary>
    public IEnumerable<string>? Errors { get; init; }

    /// <summary>
    /// ID de correlação para rastreamento
    /// </summary>
    public string? CorrelationId { get; init; }

    /// <summary>
    /// Timestamp da resposta
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    // Factory methods
    public static ResponseDTO Success(object? data = null, string? content = null, string? correlationId = null)
    {
        return new ResponseDTO
        {
            Success = true,
            Data = data,
            Message = content,
            CorrelationId = correlationId
        };
    }

    public static ResponseDTO Error(string message, IEnumerable<string>? errors = null, string? correlationId = null)
    {
        return new ResponseDTO
        {
            Success = false,
            Message = message,
            Errors = errors,
            CorrelationId = correlationId
        };
    }
}
```

---

## 🔗 Integração com Mapeamentos

### Mapeamento DTO ↔ Entity

Ver [MappingPattern.md](./MappingPattern.md) para detalhes.

```csharp
// DTO → Entity (Input)
public static Entity ToEntity(this CreateEntityInputDTO dto, string? createdBy)
{
    return new Entity(
        stamp: GenerateStamp(),
        code: dto.Code,
        description: dto.Description,
        createdBy: createdBy,
        value: dto.Value
    );
}

// Entity → DTO (Output)
public static EntityOutputDTO ToDto(this Entity entity)
{
    return new EntityOutputDTO
    {
        Stamp = entity.Stamp,
        Code = entity.Code,
        Description = entity.Description,
        Value = entity.Value,
        IsActive = entity.IsActive,
        CreatedAt = entity.OUsrData,
        CreatedBy = entity.OUsrInis
    };
}
```

---

## 🎯 Checklist de Qualidade

Antes de finalizar um DTO, verifique:

- [ ] **É um `record` imutável**
- [ ] **Propriedades com `init` apenas**
- [ ] **Documentação XML em todas as propriedades**
- [ ] **Input DTOs NÃO têm Stamp/Id**
- [ ] **Output DTOs incluem Stamp/Id**
- [ ] **Output DTOs incluem auditoria**
- [ ] **Nullable (`?`) para campos opcionais**
- [ ] **Valores padrão apropriados**
- [ ] **Sem lógica de negócio**
- [ ] **Sem métodos (exceto factory methods em Response DTOs)**
- [ ] **Nomes claros e descritivos**
- [ ] **Sem dados sensíveis expostos**

---

## 📚 Comparação: Input vs Output DTOs

| Aspecto | Input DTO | Output DTO |
|---------|-----------|------------|
| **Uso** | Receber dados (POST/PUT) | Retornar dados (GET) |
| **Stamp/Id** | ❌ Não inclui | ✅ Inclui |
| **Auditoria** | ❌ Não inclui | ✅ Inclui (`CreatedAt`, `CreatedBy`, etc.) |
| **Validação** | ✅ FluentValidation | ❌ Não necessária |
| **Mutabilidade** | `init` apenas | `init` apenas |
| **Campos Obrigatórios** | Via validação | Sempre preenchidos |

---

## 📚 Referências

- [DTO Pattern - Martin Fowler](https://martinfowler.com/eaaCatalog/dataTransferObject.html)
- [Records in C#](https://docs.microsoft.com/dotnet/csharp/language-reference/builtin-types/record)
- [Módulo de Referência: Parameters](../../src/Modules/Parameters/)

---

**Ver também**:
- [MappingPattern.md](./MappingPattern.md) - Conversão DTO ↔ Entity
- [CommandPattern.md](./CommandPattern.md) - Uso de Input DTOs
- [QueryPattern.md](./QueryPattern.md) - Uso de Output DTOs
- [ValidatorPattern.md](./ValidatorPattern.md) - Validação de Input DTOs
