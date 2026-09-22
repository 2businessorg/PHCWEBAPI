# 🔄 Mapping Pattern (Entity ↔ DTO)

> **Nível**: Senior/Enterprise  
> **Camada**: Application Layer  
> **Responsabilidade**: Conversão entre Entidades de Domínio e DTOs

---

## 🎯 Objetivo

Mapeamentos convertem dados entre camadas mantendo o isolamento. Usamos **Mapster** (high-performance) com **extension methods** para clareza e facilidade de uso.

---

## 📋 Estrutura

```
ModuleName.Application/
└── Mappings/
    ├── EntityMapper.cs            ← Mapper principal
    └── EntityMappingExtensions.cs ← Extension methods
```

---

## 🔧 Implementação com Mapster

### 1. Instalação do Mapster

```bash
dotnet add package Mapster
```

---

### 2. Mapper Principal

**Localização**: `Mappings/EntityMapper.cs`

```csharp
using Mapster;
using ModuleName.Domain.Entities;
using ModuleName.Application.DTOs.Entities;
using Shared.Kernel.Interfaces;

namespace ModuleName.Application.Mappings;

/// <summary>
/// Mapper principal para Entity
/// </summary>
public sealed class EntityMapper : IDomainMapper<Entity, EntityOutputDTO>
{
    /// <summary>
    /// Converte Entity para DTO genérico
    /// </summary>
    public TDto ToDto<TDto>(Entity entity) where TDto : class
    {
        return entity.Adapt<TDto>();
    }

    /// <summary>
    /// Converte coleção de Entities para DTOs genéricos
    /// </summary>
    public IEnumerable<TDto> ToDtos<TDto>(IEnumerable<Entity> entities) where TDto : class
    {
        return entities.Select(e => e.Adapt<TDto>());
    }

    /// <summary>
    /// Converte Entity para EntityOutputDTO específico
    /// </summary>
    public EntityOutputDTO ToOutputDto(Entity entity)
    {
        return entity.Adapt<EntityOutputDTO>();
    }

    /// <summary>
    /// Converte coleção de Entities para EntityOutputDTOs
    /// </summary>
    public IEnumerable<EntityOutputDTO> ToOutputDtos(IEnumerable<Entity> entities)
    {
        return entities.Select(e => e.Adapt<EntityOutputDTO>());
    }
}
```

---

### 3. Extension Methods

**Localização**: `Mappings/EntityMappingExtensions.cs`

```csharp
using Mapster;
using ModuleName.Domain.Entities;
using ModuleName.Application.DTOs.Entities;
using Shared.Kernel.Extensions;

namespace ModuleName.Application.Mappings;

/// <summary>
/// Extension methods para mapeamento de Entity
/// </summary>
public static class EntityMappingExtensions
{
    private static readonly EntityMapper _mapper = new();

    // ===== DTO → Entity (Input) =====

    /// <summary>
    /// Converte CreateEntityInputDTO para Entity
    /// </summary>
    public static Entity ToEntity(this CreateEntityInputDTO dto, string? createdBy)
    {
        return new Entity(
            stamp: 25.GenerateStamp(),  // ← Gera stamp de 25 caracteres
            code: dto.Code,
            description: dto.Description,
            createdBy: createdBy,
            value: dto.Value
        );
    }

    /// <summary>
    /// Atualiza Entity existente com UpdateEntityInputDTO
    /// </summary>
    public static void UpdateEntity(this Entity entity, UpdateEntityInputDTO dto, string? updatedBy)
    {
        entity.Update(
            description: dto.Description,
            value: dto.Value,
            updatedBy: updatedBy
        );
    }

    // ===== Entity → DTO (Output) =====

    /// <summary>
    /// Converte Entity para DTO genérico
    /// </summary>
    public static TDto ToDto<TDto>(this Entity entity) where TDto : class
    {
        return _mapper.ToDto<TDto>(entity);
    }

    /// <summary>
    /// Converte coleção de Entities para DTOs genéricos
    /// </summary>
    public static IEnumerable<TDto> ToDtos<TDto>(this IEnumerable<Entity> entities) where TDto : class
    {
        return _mapper.ToDtos<TDto>(entities);
    }

    /// <summary>
    /// Converte Entity para EntityOutputDTO
    /// </summary>
    public static EntityOutputDTO ToOutputDto(this Entity entity)
    {
        return _mapper.ToOutputDto(entity);
    }

    /// <summary>
    /// Converte coleção de Entities para EntityOutputDTOs
    /// </summary>
    public static IEnumerable<EntityOutputDTO> ToOutputDtos(this IEnumerable<Entity> entities)
    {
        return _mapper.ToOutputDtos(entities);
    }
}
```

---

## 🎨 Mapeamentos Personalizados

### Mapeamento com Transformação

```csharp
public static EntityOutputDTO ToOutputDto(this Entity entity)
{
    return new EntityOutputDTO
    {
        Stamp = entity.Stamp,
        Code = entity.Code,
        Description = entity.Description,
        Value = entity.Value,
        IsActive = entity.IsActive,
        
        // Transformações
        CreatedAt = entity.OUsrData,
        CreatedTime = entity.OUsrHora,
        CreatedBy = entity.OUsrInis,
        UpdatedAt = entity.UsrData,
        UpdatedTime = entity.UsrHora,
        UpdatedBy = entity.UsrInis,
        
        // Formatação customizada
        DisplayName = $"{entity.Code} - {entity.Description}",
        
        // Cálculos
        AgeInDays = (DateTime.Now - entity.OUsrData).Days
    };
}
```

---

### Mapeamento com Relacionamentos

```csharp
/// <summary>
/// Converte Order com Items para DTO
/// </summary>
public static OrderOutputDTO ToDto(this Order order)
{
    return new OrderOutputDTO
    {
        OrderStamp = order.OrderStamp,
        CustomerCode = order.CustomerCode,
        Status = order.Status.ToString(),
        TotalAmount = order.TotalAmount,
        OrderDate = order.OUsrData,
        
        // Mapear relacionamentos
        Items = order.Items.Select(item => item.ToDto()).ToList()
    };
}

public static OrderItemOutputDTO ToDto(this OrderItem item)
{
    return new OrderItemOutputDTO
    {
        ProductCode = item.ProductCode,
        Quantity = item.Quantity,
        UnitPrice = item.UnitPrice,
        TotalPrice = item.TotalPrice
    };
}
```

---

## 🏗️ Configuração Mapster (Opcional)

Para mapeamentos complexos, configure Mapster:

```csharp
using Mapster;

namespace ModuleName.Application.Mappings;

public static class MapsterConfig
{
    public static void Configure()
    {
        // Configuração global
        TypeAdapterConfig.GlobalSettings.Default
            .IgnoreNullValues(true)
            .PreserveReference(true);

        // Mapeamento específico
        TypeAdapterConfig<Entity, EntityOutputDTO>
            .NewConfig()
            .Map(dest => dest.CreatedAt, src => src.OUsrData)
            .Map(dest => dest.CreatedBy, src => src.OUsrInis)
            .Map(dest => dest.UpdatedAt, src => src.UsrData)
            .Map(dest => dest.UpdatedBy, src => src.UsrInis);

        // Mapeamento com condição
        TypeAdapterConfig<Entity, EntityOutputDTO>
            .NewConfig()
            .Map(dest => dest.IsNew, src => src.OUsrData > DateTime.Now.AddDays(-7));

        // Ignorar propriedades
        TypeAdapterConfig<Entity, EntityOutputDTO>
            .NewConfig()
            .Ignore(dest => dest.InternalField);
    }
}
```

Registrar no DI:

```csharp
// DependencyInjection.cs
public static IServiceCollection AddModuleApplication(this IServiceCollection services)
{
    // Configurar Mapster
    MapsterConfig.Configure();
    
    // ...
    return services;
}
```

---

## 🧪 Testes de Mapeamentos

### Teste de Mapeamento Simples

```csharp
[Fact]
public void ToDto_ValidEntity_ShouldMapCorrectly()
{
    // Arrange
    var entity = new Entity(
        stamp: "STAMP123",
        code: "CODE001",
        description: "Test Description",
        createdBy: "testuser",
        value: 100m
    );

    // Act
    var dto = entity.ToDto<EntityOutputDTO>();

    // Assert
    dto.Should().NotBeNull();
    dto.Stamp.Should().Be("STAMP123");
    dto.Code.Should().Be("CODE001");
    dto.Description.Should().Be("Test Description");
    dto.Value.Should().Be(100m);
    dto.CreatedBy.Should().Be("testuser");
}
```

### Teste de Mapeamento de Coleção

```csharp
[Fact]
public void ToDtos_ValidEntities_ShouldMapAll()
{
    // Arrange
    var entities = new List<Entity>
    {
        new Entity("STAMP1", "CODE1", "Desc1", "user1"),
        new Entity("STAMP2", "CODE2", "Desc2", "user2"),
        new Entity("STAMP3", "CODE3", "Desc3", "user3")
    };

    // Act
    var dtos = entities.ToDtos<EntityOutputDTO>().ToList();

    // Assert
    dtos.Should().HaveCount(3);
    dtos[0].Stamp.Should().Be("STAMP1");
    dtos[1].Stamp.Should().Be("STAMP2");
    dtos[2].Stamp.Should().Be("STAMP3");
}
```

### Teste de Mapeamento Input → Entity

```csharp
[Fact]
public void ToEntity_ValidInputDTO_ShouldCreateEntity()
{
    // Arrange
    var dto = new CreateEntityInputDTO
    {
        Code = "CODE001",
        Description = "Test Description",
        Value = 100m
    };

    // Act
    var entity = dto.ToEntity("testuser");

    // Assert
    entity.Should().NotBeNull();
    entity.Code.Should().Be("CODE001");
    entity.Description.Should().Be("Test Description");
    entity.Value.Should().Be(100m);
    entity.OUsrInis.Should().Be("testuser");
    entity.Stamp.Should().HaveLength(25); // Stamp gerado
}
```

### Teste de Performance (Grande Volume)

```csharp
[Fact]
public void ToDtos_LargeCollection_ShouldBePerformant()
{
    // Arrange
    var entities = Enumerable.Range(1, 10000)
        .Select(i => new Entity($"STAMP{i}", $"CODE{i}", $"Desc{i}", "user"))
        .ToList();

    // Act
    var stopwatch = Stopwatch.StartNew();
    var dtos = entities.ToDtos<EntityOutputDTO>().ToList();
    stopwatch.Stop();

    // Assert
    dtos.Should().HaveCount(10000);
    stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000, 
        "Mapster deve mapear 10k itens em < 1s");
}
```

---

## ✅ Boas Práticas

### 1. Extension Methods para Clareza

✅ **Bom**:
```csharp
var dto = entity.ToDto<EntityOutputDTO>();
var entities = repository.GetAll().ToDtos<EntityOutputDTO>();
```

❌ **Ruim**:
```csharp
var dto = _mapper.Map<EntityOutputDTO>(entity);  // Menos intuitivo
```

---

### 2. Mapeamentos Explícitos para Input

✅ **Bom** (explícito, com validações):
```csharp
public static Entity ToEntity(this CreateEntityInputDTO dto, string? createdBy)
{
    return new Entity(
        stamp: 25.GenerateStamp(),
        code: dto.Code,
        description: dto.Description,
        createdBy: createdBy,
        value: dto.Value
    );
}
```

❌ **Ruim** (automático sem validações):
```csharp
var entity = dto.Adapt<Entity>(); // ❌ Pula construtor e validações
```

---

### 3. Mapster para Output

✅ **Bom** (automático, sem lógica):
```csharp
public static EntityOutputDTO ToDto(this Entity entity)
{
    return entity.Adapt<EntityOutputDTO>();  // ✅ Rápido e seguro
}
```

---

### 4. Update Entities

✅ **Bom** (chama método de domínio):
```csharp
public static void UpdateEntity(this Entity entity, UpdateEntityInputDTO dto, string? updatedBy)
{
    entity.Update(        // ← Método de domínio com validações
        dto.Description,
        dto.Value,
        updatedBy
    );
}
```

❌ **Ruim** (modifica diretamente):
```csharp
entity.Description = dto.Description;  // ❌ Pula validações
entity.Value = dto.Value;
```

---

## 🎯 Checklist de Qualidade

Antes de finalizar Mapeamentos, verifique:

- [ ] **Mapper registrado no DI** (se usar interface)
- [ ] **Extension methods para facilitar uso**
- [ ] **Input → Entity usa construtor** (validações de domínio)
- [ ] **Output usa Mapster** (performance)
- [ ] **Update chama métodos de domínio**
- [ ] **Relacionamentos mapeados corretamente**
- [ ] **Testes de mapeamento**
- [ ] **Teste de performance** (grandes volumes)
- [ ] **Documentação XML**

---

## 📊 Mapster vs AutoMapper

| Aspecto | Mapster | AutoMapper |
|---------|---------|------------|
| **Performance** | ⚡ Até 10x mais rápido | ❌ Mais lento |
| **Configuração** | ✅ Mínima | ❌ Verbosa |
| **Curva de Aprendizado** | ✅ Simples | ❌ Complexa |
| **Uso** | `entity.Adapt<DTO>()` | `_mapper.Map<DTO>(entity)` |
| **Recomendação** | ✅ Preferir | ⚠️ Evitar |

---

## 📚 Referências

- [Mapster Documentation](https://github.com/MapsterMapper/Mapster)
- [DTO Pattern](../DTOPattern.md)
- [Entity Pattern](../EntityPattern.md)
- [Módulo de Referência: Parameters](../../src/Modules/Parameters/)

---

**Ver também**:
- [DTOPattern.md](./DTOPattern.md) - DTOs
- [EntityPattern.md](./EntityPattern.md) - Entidades de domínio
- [CommandPattern.md](./CommandPattern.md) - Uso de mapeamentos
- [QueryPattern.md](./QueryPattern.md) - Queries com mapeamentos
