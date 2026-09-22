# 🔌 Dependency Injection Pattern

> **Nível**: Senior/Enterprise  
> **Todas as Camadas**  
> **Responsabilidade**: Inversão de controle e registro de dependências

---

## 🎯 Objetivo

Dependency Injection (DI) permite criar código desacoplado, testável e manutenível, registrando serviços e suas dependências de forma centralizada.

---

## 📋 Estrutura

Cada camada tem seu próprio arquivo `DependencyInjection.cs`:

```
ModuleName.Domain/              (geralmente sem DI)
ModuleName.Application/
└── DependencyInjection.cs      ← MediatR, Validators, Mappers
ModuleName.Infrastructure/
└── DependencyInjection.cs      ← DbContext, Repositories
ModuleName.Presentation/
└── DependencyInjection.cs      ← Controllers, REST, GraphQL
```

---

## 🔧 Implementação por Camada

### 1. Application Layer DI

**Localização**: `ModuleName.Application/DependencyInjection.cs`

```csharp
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using MediatR;
using ModuleName.Application.Mappings;
using ModuleName.Application.DTOs.Entities;
using ModuleName.Domain.Entities;
using Shared.Kernel.Interfaces;

namespace ModuleName.Application;

/// <summary>
/// Registro de dependências da camada Application
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddModuleApplication(this IServiceCollection services)
    {
        // 1. Mappers
        services.AddSingleton<IDomainMapper<Entity, EntityOutputDTO>, EntityMapper>();

        // 2. MediatR (Commands & Queries)
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);

            // Pipeline Behaviors (ordem importa!)
            cfg.AddOpen Behavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(PerformanceMonitoringBehavior<,>));
        });

        // 3. FluentValidation (Validators)
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        // 4. Memory Cache (para CachingBehavior se usado)
        services.AddMemoryCache();

        return services;
    }
}
```

#### ✅ **Características**:
- ✅ **Extension method** para `IServiceCollection`
- ✅ MediatR com assembly scanning
- ✅ Behaviors registrados na ordem correta
- ✅ Validators auto-registrados

---

### 2. Infrastructure Layer DI

**Localização**: `ModuleName.Infrastructure/DependencyInjection.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModuleName.Domain.Repositories;
using ModuleName.Infrastructure.Persistence;
using ModuleName.Infrastructure.Repositories;
using Shared.Infrastructure.Persistence.Interceptors;

namespace ModuleName.Infrastructure;

/// <summary>
/// Registro de dependências da camada Infrastructure
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddModuleInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // 1. Interceptors (Shared - Auditoria)
        services.AddSingleton<AuditableEntityInterceptorEFCore>();

        // 2. DbContext (EF Core - Database First)
        services.AddDbContext<ModuleDbContextEFCore>((sp, options) =>
        {
            var auditInterceptor = sp.GetRequiredService<AuditableEntityInterceptorEFCore>();

            options.UseSqlServer(configuration.GetConnectionString("DBconnect"))
                   .AddInterceptors(auditInterceptor)
                   .EnableSensitiveDataLogging(false)  // ← Apenas em desenvolvimento
                   .EnableDetailedErrors(false);       // ← Apenas em desenvolvimento
        });

        // 3. Repositories (EF Core implementation)
        services.AddScoped<IEntityRepository, EntityRepositoryEFCore>();

        // 4. External Services (se houver)
        // services.AddHttpClient<IExternalApiService, ExternalApiService>();

        return services;
    }
}
```

#### ✅ **Características**:
- ✅ DbContext com interceptors
- ✅ Connection string do configuration
- ✅ Repositories com lifetime Scoped
- ✅ Interceptor Singleton (reutilizado)

---

### 3. Presentation Layer DI

**Localização**: `ModuleName.Presentation/DependencyInjection.cs`

```csharp
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModuleName.Application;
using ModuleName.Infrastructure;
using ModuleName.Presentation.REST;
using ModuleName.Presentation.GraphQL;

namespace ModuleName.Presentation;

/// <summary>
/// Registro de dependências da camada Presentation
/// Entry point para registro do módulo completo
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddModulePresentation(
        this IServiceCollection services,
        IConfiguration configuration,
        bool enableRest = true,
        bool enableGraphQL = false)
    {
        // 1. Application Layer (MediatR, Validators, Mappers)
        services.AddModuleApplication();

        // 2. Infrastructure Layer (DbContext, Repositories)
        services.AddModuleInfrastructure(configuration);

        // 3. Presentation Protocols
        if (enableRest)
        {
            services.AddModuleRestApi();
        }

        if (enableGraphQL)
        {
            services.AddModuleGraphQL();
        }

        return services;
    }
}
```

---

### 4. REST API DI

**Localização**: `ModuleName.Presentation/REST/RestDependencyInjection.cs`

```csharp
using Microsoft.Extensions.DependencyInjection;

namespace ModuleName.Presentation.REST;

/// <summary>
/// Registro de dependências REST API
/// </summary>
public static class RestDependencyInjection
{
    public static IServiceCollection AddModuleRestApi(this IServiceCollection services)
    {
        // Controllers são auto-registrados pelo ASP.NET Core
        // Aqui apenas configurações específicas REST se necessário

        return services;
    }
}
```

---

## 🎨 Lifetimes de Serviços

### Scoped (Recomendado para a maioria)

```csharp
services.AddScoped<IEntityRepository, EntityRepositoryEFCore>();
services.AddScoped<IOrderService, OrderService>();
```

**Características**:
- ✅ Uma instância por requisição HTTP
- ✅ Ideal para Repositories, Handlers, Services
- ✅ Compartilhado dentro da mesma requisição

---

### Singleton (Para serviços stateless)

```csharp
services.AddSingleton<IDomainMapper<Entity, EntityOutputDTO>, EntityMapper>();
services.AddSingleton<AuditableEntityInterceptorEFCore>();
services.AddSingleton<IMemoryCache, MemoryCache>();
```

**Características**:
- ✅ Uma única instância para toda a aplicação
- ✅ Ideal para Mappers, Interceptors, Caches
- ⚠️ Deve ser thread-safe
- ⚠️ Não pode depender de Scoped services

---

### Transient (Usar com cautela)

```csharp
services.AddTransient<IEmailService, EmailService>();
services.AddTransient<INotificationService, NotificationService>();
```

**Características**:
- Nova instância a cada injeção
- ⚠️ Overhead de performance
- ⚠️ Use apenas quando necessário

---

## 🏗️ Registro no Host

**Localização**: `PHCAPI.Host/Program.cs`

```csharp
var builder = WebApplication.CreateBuilder(args);

// ===== Shared Services =====
builder.Services.AddSharedInfrastructure(builder.Configuration);

// ===== Modules =====
// Parameters Module
builder.Services.AddParametersPresentation(
    builder.Configuration,
    enableRest: true,
    enableGraphQL: false
);

// Auth Module
builder.Services.AddAuthPresentation(
    builder.Configuration,
    enableRest: true
);

// Audit Module
builder.Services.AddAuditPresentation(
    builder.Configuration,
    enableRest: true
);

// Providers Module
builder.Services.AddProvidersPresentation(
    builder.Configuration,
    enableRest: true
);

var app = builder.Build();

// ... middleware configuration

app.Run();
```

---

## 🧪 Testes com DI

### Sobrescrever Dependências em Testes

```csharp
public class EntityHandlerTests
{
    private readonly ServiceProvider _serviceProvider;

    public EntityHandlerTests()
    {
        var services = new ServiceCollection();

        // Registros reais
        services.AddModuleApplication();

        // Mock de repositório para testes
        var repositoryMock = new Mock<IEntityRepository>();
        services.AddScoped(_ => repositoryMock.Object);

        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateEntity()
    {
        // Arrange
        var mediator = _serviceProvider.GetRequiredService<IMediator>();
        var command = new CreateEntityCommand(new CreateEntityInputDTO { ... }, "user");

        // Act
        var result = await mediator.Send(command);

        // Assert
        result.Should().NotBeNull();
    }
}
```

---

## ✅ Boas Práticas

### 1. Extension Methods

✅ **Bom** (extension method):
```csharp
public static IServiceCollection AddModuleApplication(this IServiceCollection services)
{
    // ...
    return services;
}
```

❌ **Ruim** (método estático normal):
```csharp
public static void RegisterServices(IServiceCollection services)  // ❌ Não é fluente
{
    // ...
}
```

---

### 2. Assembly Scanning

✅ **Bom** (auto-discovery):
```csharp
services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
```

❌ **Ruim** (registro manual):
```csharp
services.AddTransient<CreateEntityCommandHandler>();  // ❌ Trabalhoso
services.AddTransient<UpdateEntityCommandHandler>();
// ... todos os handlers
```

---

### 3. Configurações Tipadas

✅ **Bom** (strongly-typed):
```csharp
services.Configure<SmtpSettings>(configuration.GetSection("Smtp"));
services.Configure<ExternalApiSettings>(configuration.GetSection("ExternalApi"));
```

Uso:
```csharp
public class EmailService
{
    private readonly SmtpSettings _settings;

    public EmailService(IOptions<SmtpSettings> options)
    {
        _settings = options.Value;
    }
}
```

---

### 4. Named HttpClients

```csharp
services.AddHttpClient("ExternalApi", client =>
{
    client.BaseAddress = new Uri(configuration["ExternalApi:BaseUrl"]!);
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});
```

Uso:
```csharp
public class ExternalApiService
{
    private readonly HttpClient _httpClient;

    public ExternalApiService(IHttpClientFactory clientFactory)
    {
        _httpClient = clientFactory.CreateClient("ExternalApi");
    }
}
```

---

## 🎯 Checklist de Qualidade

Antes de finalizar DI, verifique:

- [ ] **Extension methods** para todas as camadas
- [ ] **Retorna `IServiceCollection`** (fluent)
- [ ] **Lifetime apropriado** (Scoped, Singleton, Transient)
- [ ] **Assembly scanning** (MediatR, Validators)
- [ ] **Configurações tipadas** (`IOptions<T>`)
- [ ] **Interceptors registrados** (Auditoria)
- [ ] **DbContext com connection string**
- [ ] **Repositories registrados**
- [ ] **Behaviors na ordem correta**
- [ ] **Documentação XML**

---

## 📚 Comparação de Lifetimes

| Lifetime | Use Para | Evitar Para |
|----------|----------|-------------|
| **Scoped** | Repositories, Handlers, Services | Singletons que dependem de Scoped |
| **Singleton** | Mappers, Caches, Interceptors | Services com estado mutável |
| **Transient** | Services leves, Notificações | Repositories, DbContext |

---

## 📚 Referências

- [Dependency Injection in ASP.NET Core](https://docs.microsoft.com/aspnet/core/fundamentals/dependency-injection)
- [Service Lifetimes](https://docs.microsoft.com/dotnet/core/extensions/dependency-injection#service-lifetimes)
- [Módulo de Referência: Parameters](../../src/Modules/Parameters/)

---

**Ver também**:
- [RepositoryPattern.md](./RepositoryPattern.md) - Registro de repositories
- [BehaviorPattern.md](./BehaviorPattern.md) - Registro de behaviors
- [InfrastructurePattern.md](./InfrastructurePattern.md) - DbContext configuration
