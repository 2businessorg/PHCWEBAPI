# 🏗️ Infrastructure Pattern (EF Core)

> **Nível**: Senior/Enterprise  
> **Camada**: Infrastructure Layer  
> **Responsabilidade**: Implementações concretas de persistência e serviços externos

---

## 🎯 Objetivo

A Infrastructure Layer implementa interfaces definidas no Domain, usando tecnologias específicas (EF Core, Dapper, etc.) mantendo o desacoplamento.

---

## 📋 Estrutura

```
ModuleName.Infrastructure/
├── Persistence/
│   ├── ModuleDbContextEFCore.cs          ← DbContext
│   └── Configurations/
│       ├── EntityConfigurationEFCore.cs  ← Entity configuration
│       └── Entity2ConfigurationEFCore.cs
├── Repositories/
│   ├── EntityRepositoryEFCore.cs         ← Repository implementation
│   └── Entity2RepositoryEFCore.cs
└── DependencyInjection.cs                ← DI registration
```

---

## 🔧 Implementação Passo a Passo

### 1. DbContext (Database First)

**Localização**: `Persistence/ModuleDbContextEFCore.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using ModuleName.Domain.Entities;

namespace ModuleName.Infrastructure.Persistence;

/// <summary>
/// DbContext EF Core para o módulo (Database First)
/// </summary>
public class ModuleDbContextEFCore : DbContext
{
    public ModuleDbContextEFCore(DbContextOptions<ModuleDbContextEFCore> options) 
        : base(options)
    {
    }

    // DbSets
    public DbSet<Entity> Entities => Set<Entity>();
    public DbSet<Entity2> Entities2 => Set<Entity2>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplicar configurações
        modelBuilder.ApplyConfiguration(new EntityConfigurationEFCore());
        modelBuilder.ApplyConfiguration(new Entity2ConfigurationEFCore());

        // Ou aplicar todas automaticamente
        // modelBuilder.ApplyConfigurationsFromAssembly(typeof(ModuleDbContextEFCore).Assembly);
    }
}
```

#### ✅ **Características**:
- ✅ **Database First**: Tabelas já existem no banco
- ✅ DbSets com `Set<T>()` para lazy loading
- ✅ Configurações aplicadas no `OnModelCreating`
- ❌ **SEM Migrations** (Database First)

---

### 2. Entity Configuration

**Localização**: `Persistence/Configurations/EntityConfigurationEFCore.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModuleName.Domain.Entities;

namespace ModuleName.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuração EF Core para Entity (Database First)
/// </summary>
public class EntityConfigurationEFCore : IEntityTypeConfiguration<Entity>
{
    public void Configure(EntityTypeBuilder<Entity> builder)
    {
        // Tabela (já existe no banco)
        builder.ToTable("tb_entity");

        // Primary Key
        builder.HasKey(e => e.Stamp);

        // Propriedades
        builder.Property(e => e.Stamp)
            .HasColumnName("entity_stamp")
            .HasMaxLength(25)
            .IsRequired();

        builder.Property(e => e.Code)
            .HasColumnName("entity_code")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.Description)
            .HasColumnName("entity_description")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(e => e.Value)
            .HasColumnName("entity_value")
            .HasColumnType("decimal(18,2)");

        builder.Property(e => e.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        // Auditoria (mapeamento de AuditableEntity)
        builder.Property(e => e.OUsrData)
            .HasColumnName("ousrdata")
            .IsRequired();

        builder.Property(e => e.OUsrHora)
            .HasColumnName("ousrhora")
            .HasMaxLength(8)
            .IsRequired();

        builder.Property(e => e.OUsrInis)
            .HasColumnName("ousrinis")
            .HasMaxLength(50);

        builder.Property(e => e.UsrData)
            .HasColumnName("usrdata");

        builder.Property(e => e.UsrHora)
            .HasColumnName("usrhora")
            .HasMaxLength(8);

        builder.Property(e => e.UsrInis)
            .HasColumnName("usrinis")
            .HasMaxLength(50);

        // Índices (se aplicável)
        builder.HasIndex(e => e.Code).IsUnique();
        builder.HasIndex(e => e.Description);
        builder.HasIndex(e => e.OUsrData);
    }
}
```

#### ✅ **Boas Práticas**:
- ✅ **ToTable**: Nome exato da tabela no banco
- ✅ **HasColumnName**: Nomes exatos das colunas
- ✅ **HasMaxLength**: Tamanhos exatos
- ✅ **IsRequired**: Não nulos
- ✅ **HasIndex**: Índices de performance
- ✅ **HasColumnType**: Tipos específicos (decimal, etc.)

---

### 3. Relacionamentos 1:N

```csharp
public class OrderConfigurationEFCore : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("tb_orders");

        builder.HasKey(o => o.OrderStamp);

        // Propriedades...

        // Relacionamento 1:N (Order → OrderItems)
        builder.HasMany(o => o.Items)
            .WithOne()
            .HasForeignKey("OrderStamp")  // ← FK na tabela de items
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class OrderItemConfigurationEFCore : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("tb_order_items");

        // Chave composta
        builder.HasKey(i => new { i.OrderStamp, i.ProductCode });

        // Propriedades...
    }
}
```

---

### 4. Auditoria Automática (Interceptor)

**Interceptor Compartilhado**: `Shared.Infrastructure/Persistence/Interceptors/AuditableEntityInterceptorEFCore.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Shared.Abstractions.Entities;

namespace Shared.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Interceptor para auditoria automática de entidades
/// Reutilizado por TODOS os módulos
/// </summary>
public class AuditableEntityInterceptorEFCore : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;

        if (context is null)
            return base.SavingChangesAsync(eventData, result, cancellationToken);

        // Detectar entidades adicionadas/modificadas
        var entries = context.ChangeTracker.Entries<AuditableEntity>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                var now = DateTime.Now;
                entry.Entity.OUsrData = now;
                entry.Entity.OUsrHora = now.ToString("HH:mm:ss");
                // OUsrInis já setado na entidade
            }
            else if (entry.State == EntityState.Modified)
            {
                var now = DateTime.Now;
                entry.Entity.UsrData = now;
                entry.Entity.UsrHora = now.ToString("HH:mm:ss");
                // UsrInis já setado na entidade
            }
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
```

#### ✅ **Características**:
- ✅ **Centralizado**: Um interceptor para TODOS os módulos
- ✅ **Auditoria automática**: `OUsrData`, `UsrData`, etc.
- ✅ **Sem código repetido**: Reutilização total

---

### 5. Repository Implementation

Ver [RepositoryPattern.md](./RepositoryPattern.md) para detalhes completos.

```csharp
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

    // ... outros métodos
}
```

---

## 🔗 Dependency Injection

Ver [DependencyInjectionPattern.md](./DependencyInjectionPattern.md) para detalhes completos.

```csharp
public static class DependencyInjection
{
    public static IServiceCollection AddModuleInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Interceptor (Shared)
        services.AddSingleton<AuditableEntityInterceptorEFCore>();

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

## ⚡ Performance e Otimizações

### 1. AsNoTracking para Queries

```csharp
public async Task<IEnumerable<Entity>> GetAllAsync(CancellationToken ct = default)
{
    return await _context.Entities
        .AsNoTracking()  // ← Sem tracking de mudanças
        .ToListAsync(ct);
}
```

---

### 2. Projeção Direta

```csharp
public async Task<IEnumerable<EntityDTO>> GetAllDtosAsync(CancellationToken ct = default)
{
    return await _context.Entities
        .AsNoTracking()
        .Select(e => new EntityDTO  // ← Projeção direta (mais rápido)
        {
            Stamp = e.Stamp,
            Code = e.Code,
            Description = e.Description
        })
        .ToListAsync(ct);
}
```

---

### 3. Include para Relacionamentos

```csharp
public async Task<Order?> GetWithItemsAsync(string stamp, CancellationToken ct = default)
{
    return await _context.Orders
        .Include(o => o.Items)  // ← Eager loading
        .FirstOrDefaultAsync(o => o.OrderStamp == stamp, ct);
}
```

---

### 4. Split Query (N+1 Prevention)

```csharp
public async Task<IEnumerable<Order>> GetAllWithItemsAsync(CancellationToken ct = default)
{
    return await _context.Orders
        .Include(o => o.Items)
        .AsSplitQuery()  // ← Evita cartesian explosion
        .ToListAsync(ct);
}
```

---

## 🧪 Testes

### Integration Test com InMemory Database

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

## ✅ Boas Práticas

### 1. Naming Convention

✅ **Bom**:
```csharp
ModuleDbContextEFCore.cs           // ← Sufixo EFCore
EntityRepositoryEFCore.cs          // ← Sufixo EFCore
EntityConfigurationEFCore.cs       // ← Sufixo EFCore
```

❌ **Ruim**:
```csharp
ModuleDbContext.cs                 // ❌ Pode confundir com abstração
EntityRepository.cs                // ❌ Não indica tecnologia
```

---

### 2. Database First

✅ **Bom**:
```csharp
// Mapeamento manual via Entity Configuration
builder.ToTable("tb_entity");  // ← Nome exato no banco
builder.Property(e => e.Code).HasColumnName("entity_code");
```

❌ **Ruim**:
```csharp
// ❌ Migrations (Code First) não devem ser usadas
dotnet ef migrations add InitialCreate
dotnet ef database update
```

---

### 3. Interceptors Compartilhados

✅ **Bom** (reutilizável):
```csharp
// Shared.Infrastructure/Interceptors/AuditableEntityInterceptorEFCore.cs
public class AuditableEntityInterceptorEFCore : SaveChangesInterceptor
{
    // Usado por TODOS os módulos
}
```

❌ **Ruim** (duplicado):
```csharp
// ❌ Cada módulo com seu próprio interceptor = duplicação
```

---

## 🎯 Checklist de Qualidade

Antes de finalizar Infrastructure, verifique:

- [ ] **DbContext com sufixo `EFCore`**
- [ ] **Entity Configurations para todas as entidades**
- [ ] **ToTable com nome exato da tabela**
- [ ] **HasColumnName com nome exato da coluna**
- [ ] **Índices de performance configurados**
- [ ] **Relacionamentos mapeados**
- [ ] **Interceptor de auditoria registrado**
- [ ] **Connection string do Configuration**
- [ ] **Repositories implementados**
- [ ] **AsNoTracking em queries**
- [ ] **Include para relacionamentos**
- [ ] **Testes de integração**

---

## 📊 EF Core vs Dapper

| Aspecto | EF Core | Dapper |
|---------|---------|--------|
| **Facilidade** | ✅ Simples | ⚠️ Mais código |
| **Performance Leitura** | ⚠️ Mais lento | ✅ Muito rápido |
| **Performance Escrita** | ✅ Bom | ✅ Bom |
| **Tracking** | ✅ Automático | ❌ Manual |
| **Migrations** | ✅ Integrado | ❌ N/A |
| **Recomendação** | ✅ Usar | ⚠️ Queries complexas |

---

## 📚 Referências

- [EF Core Documentation](https://docs.microsoft.com/ef/core/)
- [EF Core Performance](https://docs.microsoft.com/ef/core/performance/)
- [EF Core Interceptors](https://docs.microsoft.com/ef/core/logging-events-diagnostics/interceptors)
- [Módulo de Referência: Parameters](../../src/Modules/Parameters/)

---

**Ver também**:
- [RepositoryPattern.md](./RepositoryPattern.md) - Repositórios
- [EntityPattern.md](./EntityPattern.md) - Entidades de domínio
- [DependencyInjectionPattern.md](./DependencyInjectionPattern.md) - Registro de DI
