# 🗄️ Repository Pattern

> **Nível**: Senior/Enterprise  
> **Camadas**: Domain (Interface) + Infrastructure (Implementação)  
> **Responsabilidade**: Abstração de acesso a dados

---

## 🎯 Objetivo

O Repository Pattern abstrai a lógica de acesso a dados, permitindo que o Domain Layer defina **WHAT** precisa sem saber **HOW** é persistido. Facilita testes, mudança de tecnologia e mantém o código limpo.

---

## 📋 Estrutura

### Interface (Domain Layer)

```
ModuleName.Domain/
└── Repositories/
    └── IEntityRepository.cs  ← Interface (contrato)
```

### Implementação (Infrastructure Layer)

```
ModuleName.Infrastructure/
└── Repositories/
    └── EntityRepositoryEFCore.cs  ← Implementação EF Core
    └── EntityRepositoryDapper.cs  ← (Opcional) Implementação Dapper
```

---

## 🔧 Implementação Passo a Passo

### 1. Definir Interface no Domain

**Localização**: `ModuleName.Domain/Repositories/IEntityRepository.cs`

```csharp
using ModuleName.Domain.Entities;

namespace ModuleName.Domain.Repositories;

/// <summary>
/// Contrato para persistência de Entity
/// </summary>
public interface IEntityRepository
{
    // Queries (Read)
    Task<Entity?> GetByStampAsync(string stamp, CancellationToken ct = default);
    Task<Entity?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<IEnumerable<Entity>> GetAllAsync(bool includeInactive = false, CancellationToken ct = default);
    Task<IEnumerable<Entity>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken ct = default);
    Task<bool> ExistsAsync(string stamp, CancellationToken ct = default);
    Task<int> CountAsync(CancellationToken ct = default);

    // Commands (Write)
    Task<Entity> AddAsync(Entity entity, CancellationToken ct = default);
    Task UpdateAsync(Entity entity, CancellationToken ct = default);
    Task DeleteAsync(Entity entity, CancellationToken ct = default);
}
```

#### ✅ **Boas Práticas**:
- ✅ **Async/Await**: Todos os métodos assíncronos
- ✅ **CancellationToken**: Sempre com valor padrão
- ✅ **Retornar entidades**, não DTOs
- ✅ **Métodos específicos**: `GetByStampAsync`, `GetByCodeAsync`
- ✅ **Nullable**: `Entity?` quando pode não existir
- ✅ **Sem lógica de negócio**: Apenas CRUD

#### ❌ **Evitar**:
- ❌ Métodos síncronos
- ❌ Retornar DTOs (responsabilidade da Application)
- ❌ Lógica de negócio no repositório
- ❌ Métodos genéricos demais (`Get(Expression<Func<...>>)`)

---

### 2. Implementar Repositório (EF Core)

**Localização**: `ModuleName.Infrastructure/Repositories/EntityRepositoryEFCore.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using ModuleName.Domain.Entities;
using ModuleName.Domain.Repositories;
using ModuleName.Infrastructure.Persistence;

namespace ModuleName.Infrastructure.Repositories;

/// <summary>
/// Implementação EF Core do repositório Entity
/// </summary>
public class EntityRepositoryEFCore : IEntityRepository
{
    private readonly ModuleDbContextEFCore _context;

    public EntityRepositoryEFCore(ModuleDbContextEFCore context)
    {
        _context = context;
    }

    public async Task<Entity?> GetByStampAsync(string stamp, CancellationToken ct = default)
    {
        return await _context.Entities
            .FirstOrDefaultAsync(e => e.Stamp == stamp, ct);
    }

    public async Task<Entity?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        return await _context.Entities
            .FirstOrDefaultAsync(e => e.Code == code, ct);
    }

    public async Task<IEnumerable<Entity>> GetAllAsync(bool includeInactive = false, CancellationToken ct = default)
    {
        var query = _context.Entities.AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(e => e.IsActive);
        }

        return await query
            .OrderBy(e => e.Description)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<Entity>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken ct = default)
    {
        return await _context.Entities
            .OrderBy(e => e.Description)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task<bool> ExistsAsync(string stamp, CancellationToken ct = default)
    {
        return await _context.Entities
            .AnyAsync(e => e.Stamp == stamp, ct);
    }

    public async Task<int> CountAsync(CancellationToken ct = default)
    {
        return await _context.Entities.CountAsync(ct);
    }

    public async Task<Entity> AddAsync(Entity entity, CancellationToken ct = default)
    {
        await _context.Entities.AddAsync(entity, ct);
        await _context.SaveChangesAsync(ct);
        return entity;
    }

    public async Task UpdateAsync(Entity entity, CancellationToken ct = default)
    {
        _context.Entities.Update(entity);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Entity entity, CancellationToken ct = default)
    {
        _context.Entities.Remove(entity);
        await _context.SaveChangesAsync(ct);
    }
}
```

#### ✅ **Boas Práticas**:
- ✅ **Sufixo EFCore**: Indica tecnologia usada
- ✅ **SaveChangesAsync**: Sempre após Add, Update, Delete
- ✅ **AsQueryable**: Para queries complexas
- ✅ **FirstOrDefaultAsync**: Para buscar um item
- ✅ **ToListAsync**: Para coleções
- ✅ **OrderBy**: Sempre ordenar coleções

---

### 3. Registrar no DI

**Localização**: `ModuleName.Infrastructure/DependencyInjection.cs`

```csharp
using Microsoft.Extensions.DependencyInjection;
using ModuleName.Domain.Repositories;
using ModuleName.Infrastructure.Repositories;

namespace ModuleName.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddModuleInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // DbContext
        services.AddDbContext<ModuleDbContextEFCore>((sp, options) =>
        {
            var auditInterceptor = sp.GetRequiredService<AuditableEntityInterceptorEFCore>();
            options.UseSqlServer(configuration.GetConnectionString("DBconnect"))
                   .AddInterceptors(auditInterceptor);
        });

        // Repositories
        services.AddScoped<IEntityRepository, EntityRepositoryEFCore>();

        return services;
    }
}
```

---

## 🏗️ Exemplos Avançados

### Exemplo 1: Repositório com Relacionamentos

```csharp
public interface IOrderRepository
{
    Task<Order?> GetByStampWithItemsAsync(string stamp, CancellationToken ct = default);
    Task<IEnumerable<Order>> GetByCustomerAsync(string customerCode, CancellationToken ct = default);
}

public class OrderRepositoryEFCore : IOrderRepository
{
    private readonly ModuleDbContextEFCore _context;

    public OrderRepositoryEFCore(ModuleDbContextEFCore context)
    {
        _context = context;
    }

    public async Task<Order?> GetByStampWithItemsAsync(string stamp, CancellationToken ct = default)
    {
        return await _context.Orders
            .Include(o => o.Items)  // ← Eager loading
            .FirstOrDefaultAsync(o => o.OrderStamp == stamp, ct);
    }

    public async Task<IEnumerable<Order>> GetByCustomerAsync(string customerCode, CancellationToken ct = default)
    {
        return await _context.Orders
            .Where(o => o.CustomerCode == customerCode)
            .Include(o => o.Items)
            .OrderByDescending(o => o.OUsrData)
            .ToListAsync(ct);
    }
}
```

---

### Exemplo 2: Repositório com Filtros Complexos

```csharp
public interface IEntityRepository
{
    Task<IEnumerable<Entity>> SearchAsync(
        EntitySearchFilter filter,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default);

    Task<int> CountAsync(EntitySearchFilter filter, CancellationToken ct = default);
}

// Filter object
public class EntitySearchFilter
{
    public string? Code { get; set; }
    public string? Description { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
    public bool? IsActive { get; set; }
}

// Implementação
public async Task<IEnumerable<Entity>> SearchAsync(
    EntitySearchFilter filter,
    int pageNumber,
    int pageSize,
    CancellationToken ct = default)
{
    var query = _context.Entities.AsQueryable();

    if (!string.IsNullOrWhiteSpace(filter.Code))
    {
        query = query.Where(e => e.Code.Contains(filter.Code));
    }

    if (!string.IsNullOrWhiteSpace(filter.Description))
    {
        query = query.Where(e => e.Description.Contains(filter.Description));
    }

    if (filter.CreatedFrom.HasValue)
    {
        query = query.Where(e => e.OUsrData >= filter.CreatedFrom.Value);
    }

    if (filter.CreatedTo.HasValue)
    {
        query = query.Where(e => e.OUsrData <= filter.CreatedTo.Value);
    }

    if (filter.IsActive.HasValue)
    {
        query = query.Where(e => e.IsActive == filter.IsActive.Value);
    }

    return await query
        .OrderByDescending(e => e.OUsrData)
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync(ct);
}
```

---

### Exemplo 3: Repositório com Bulk Operations

```csharp
public interface IEntityRepository
{
    Task<IEnumerable<Entity>> AddRangeAsync(IEnumerable<Entity> entities, CancellationToken ct = default);
    Task UpdateRangeAsync(IEnumerable<Entity> entities, CancellationToken ct = default);
    Task DeleteRangeAsync(IEnumerable<Entity> entities, CancellationToken ct = default);
}

public class EntityRepositoryEFCore : IEntityRepository
{
    public async Task<IEnumerable<Entity>> AddRangeAsync(
        IEnumerable<Entity> entities,
        CancellationToken ct = default)
    {
        await _context.Entities.AddRangeAsync(entities, ct);
        await _context.SaveChangesAsync(ct);
        return entities;
    }

    public async Task UpdateRangeAsync(
        IEnumerable<Entity> entities,
        CancellationToken ct = default)
    {
        _context.Entities.UpdateRange(entities);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteRangeAsync(
        IEnumerable<Entity> entities,
        CancellationToken ct = default)
    {
        _context.Entities.RemoveRange(entities);
        await _context.SaveChangesAsync(ct);
    }
}
```

---

## ⚡ Performance e Otimizações

### 1. AsNoTracking para Queries

```csharp
public async Task<IEnumerable<Entity>> GetAllAsync(bool includeInactive = false, CancellationToken ct = default)
{
    var query = _context.Entities
        .AsNoTracking()  // ← Não rastreia mudanças (+ performance)
        .AsQueryable();

    if (!includeInactive)
    {
        query = query.Where(e => e.IsActive);
    }

    return await query.ToListAsync(ct);
}
```

### 2. Projeção Direta (para DTOs)

```csharp
// Alternativa: Projetar direto para DTO (mais rápido)
public async Task<IEnumerable<EntityDTO>> GetAllDtosAsync(CancellationToken ct = default)
{
    return await _context.Entities
        .AsNoTracking()
        .Select(e => new EntityDTO
        {
            Stamp = e.Stamp,
            Code = e.Code,
            Description = e.Description
        })
        .ToListAsync(ct);
}
```

### 3. Compiled Queries (Alta Performance)

```csharp
private static readonly Func<ModuleDbContextEFCore, string, Task<Entity?>> GetByStampCompiled =
    EF.CompileAsyncQuery((ModuleDbContextEFCore context, string stamp) =>
        context.Entities.FirstOrDefault(e => e.Stamp == stamp));

public async Task<Entity?> GetByStampAsync(string stamp, CancellationToken ct = default)
{
    return await GetByStampCompiled(_context, stamp);
}
```

---

## 🧪 Testes

### Unit Test com Mock

```csharp
[Theory, AutoMoqData]
public async Task GetByStampAsync_ExistingStamp_ShouldReturnEntity(
    [Frozen] Mock<IEntityRepository> repositoryMock,
    Entity entity)
{
    // Arrange
    repositoryMock
        .Setup(x => x.GetByStampAsync(entity.Stamp, It.IsAny<CancellationToken>()))
        .ReturnsAsync(entity);

    // Act
    var result = await repositoryMock.Object.GetByStampAsync(entity.Stamp);

    // Assert
    result.Should().NotBeNull();
    result!.Stamp.Should().Be(entity.Stamp);
}
```

### Integration Test (Real Database)

```csharp
public class EntityRepositoryIntegrationTests : IDisposable
{
    private readonly ModuleDbContextEFCore _context;
    private readonly EntityRepositoryEFCore _repository;

    public EntityRepositoryIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<ModuleDbContextEFCore>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ModuleDbContextEFCore(options);
        _repository = new EntityRepositoryEFCore(_context);
    }

    [Fact]
    public async Task AddAsync_ValidEntity_ShouldPersist()
    {
        // Arrange
        var entity = new Entity("STAMP123", "CODE001", "Test", "user");

        // Act
        var result = await _repository.AddAsync(entity);
        var retrieved = await _repository.GetByStampAsync(entity.Stamp);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Stamp.Should().Be("STAMP123");
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
```

---

## 🎯 Checklist de Qualidade

Antes de finalizar um Repositório, verifique:

- [ ] **Interface no Domain Layer**
- [ ] **Implementação no Infrastructure Layer**
- [ ] **Sufixo `EFCore` na implementação**
- [ ] **Todos os métodos são assíncronos**
- [ ] **CancellationToken com valor padrão**
- [ ] **SaveChangesAsync após Add/Update/Delete**
- [ ] **AsNoTracking em queries read-only**
- [ ] **OrderBy em coleções**
- [ ] **Paginação implementada**
- [ ] **Include para relacionamentos**
- [ ] **Registrado no DI**
- [ ] **Testes unitários (mocks)**
- [ ] **Testes de integração (banco real/in-memory)**
- [ ] **Documentação XML**

---

## 📚 Referências

- [Repository Pattern - Martin Fowler](https://martinfowler.com/eaaCatalog/repository.html)
- [EF Core Best Practices](https://docs.microsoft.com/ef/core/performance/)
- [Módulo de Referência: Parameters](../../src/Modules/Parameters/)

---

**Ver também**:
- [EntityPattern.md](./EntityPattern.md) - Entidades de domínio
- [InfrastructurePattern.md](./InfrastructurePattern.md) - DbContext e configurações
- [CommandPattern.md](./CommandPattern.md) - Uso de repositories
- [QueryPattern.md](./QueryPattern.md) - Queries otimizadas
