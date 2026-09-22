# 🔄 Behavior Pattern (Pipeline Behaviors)

> **Nível**: Senior/Enterprise  
> **Camada**: Application Layer  
> **Responsabilidade**: Aspectos transversais aplicados automaticamente em todos Commands/Queries

---

## 🎯 Objetivo

Pipeline Behaviors são middlewares do MediatR que interceptam Commands e Queries, permitindo aplicar lógica transversal (logging, validação, transações, cache) de forma centralizada e reutilizável.

---

## 📋 Estrutura

```
ModuleName.Application/
└── Behaviors/
    ├── ValidationBehavior.cs      ← Validação automática
    ├── LoggingBehavior.cs         ← Logging automático
    ├── TransactionBehavior.cs     ← Transações automáticas
    └── CachingBehavior.cs         ← Cache automático
```

---

## 🔧 Behaviors Essenciais

### 1. Validation Behavior (Obrigatório)

**Localização**: `Behaviors/ValidationBehavior.cs`

```csharp
using FluentValidation;
using MediatR;

namespace ModuleName.Application.Behaviors;

/// <summary>
/// Pipeline behavior para validação automática usando FluentValidation
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Se não há validators, prossegue
        if (!_validators.Any())
        {
            return await next();
        }

        // Valida request
        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        // Coleta erros
        var failures = validationResults
            .Where(r => !r.IsValid)
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        // Lança exceção se houver erros
        if (failures.Count != 0)
        {
            throw new ValidationException(failures);
        }

        // Request é válido, prossegue
        return await next();
    }
}
```

#### ✅ **Características**:
- ✅ Executa **TODOS** os validators registrados
- ✅ Coleta **TODOS** os erros antes de lançar exceção
- ✅ Lança `ValidationException` do FluentValidation
- ✅ Funciona com qualquer `IRequest<TResponse>`

---

### 2. Logging Behavior

**Localização**: `Behaviors/LoggingBehavior.cs`

```csharp
using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ModuleName.Application.Behaviors;

/// <summary>
/// Pipeline behavior para logging automático de requisições
/// </summary>
public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var requestId = Guid.NewGuid().ToString();

        _logger.LogInformation(
            "Handling {RequestName} with RequestId {RequestId}",
            requestName,
            requestId);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next();

            stopwatch.Stop();

            _logger.LogInformation(
                "Handled {RequestName} with RequestId {RequestId} in {ElapsedMilliseconds}ms",
                requestName,
                requestId,
                stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(ex,
                "Error handling {RequestName} with RequestId {RequestId} after {ElapsedMilliseconds}ms - {ErrorMessage}",
                requestName,
                requestId,
                stopwatch.ElapsedMilliseconds,
                ex.Message);

            throw;
        }
    }
}
```

#### ✅ **Características**:
- ✅ Loga início da requisição
- ✅ Mede tempo de execução
- ✅ Loga sucesso com duração
- ✅ Loga erros com detalhes
- ✅ Gera RequestId único para rastreamento

---

### 3. Transaction Behavior (Para Commands)

**Localização**: `Behaviors/TransactionBehavior.cs`

```csharp
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModuleName.Infrastructure.Persistence;

namespace ModuleName.Application.Behaviors;

/// <summary>
/// Pipeline behavior para garantir transações automáticas em Commands
/// </summary>
public sealed class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ModuleDbContextEFCore _context;
    private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;

    public TransactionBehavior(
        ModuleDbContextEFCore context,
        ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        // Só aplica transação para Commands (não Queries)
        if (!requestName.EndsWith("Command"))
        {
            return await next();
        }

        _logger.LogInformation("Starting transaction for {RequestName}", requestName);

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var response = await next();

            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation("Transaction committed for {RequestName}", requestName);

            return response;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);

            _logger.LogError(ex, "Transaction rolled back for {RequestName}", requestName);

            throw;
        }
    }
}
```

#### ✅ **Características**:
- ✅ Aplica transação apenas em Commands
- ✅ Commit automático em sucesso
- ✅ Rollback automático em falha
- ✅ Logs de transações

---

### 4. Caching Behavior (Para Queries)

**Localização**: `Behaviors/CachingBehavior.cs`

```csharp
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace ModuleName.Application.Behaviors;

/// <summary>
/// Pipeline behavior para cache automático de Queries
/// </summary>
public sealed class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;

    public CachingBehavior(
        IMemoryCache cache,
        ILogger<CachingBehavior<TRequest, TResponse>> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        // Só aplica cache para Queries
        if (!requestName.EndsWith("Query"))
        {
            return await next();
        }

        // Gera chave de cache
        var cacheKey = $"{requestName}_{SerializeRequest(request)}";

        // Tenta obter do cache
        if (_cache.TryGetValue(cacheKey, out TResponse? cachedResponse))
        {
            _logger.LogInformation("Cache hit for {RequestName}", requestName);
            return cachedResponse!;
        }

        _logger.LogInformation("Cache miss for {RequestName}", requestName);

        // Executa query
        var response = await next();

        // Salva no cache (5 minutos)
        _cache.Set(cacheKey, response, TimeSpan.FromMinutes(5));

        return response;
    }

    private string SerializeRequest(TRequest request)
    {
        // Serialização simples para chave de cache
        return System.Text.Json.JsonSerializer.Serialize(request);
    }
}
```

#### ✅ **Características**:
- ✅ Aplica cache apenas em Queries
- ✅ Cache hit/miss logging
- ✅ TTL configurável (5 minutos)
- ✅ Chave de cache baseada no request

---

### 5. Performance Monitoring Behavior

**Localização**: `Behaviors/PerformanceMonitoringBehavior.cs`

```csharp
using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ModuleName.Application.Behaviors;

/// <summary>
/// Pipeline behavior para monitorar performance
/// </summary>
public sealed class PerformanceMonitoringBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<PerformanceMonitoringBehavior<TRequest, TResponse>> _logger;
    private const int PerformanceThresholdMs = 500; // 500ms

    public PerformanceMonitoringBehavior(ILogger<PerformanceMonitoringBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var response = await next();
        stopwatch.Stop();

        var requestName = typeof(TRequest).Name;
        var elapsedMs = stopwatch.ElapsedMilliseconds;

        if (elapsedMs > PerformanceThresholdMs)
        {
            _logger.LogWarning(
                "⚠️ Slow request detected: {RequestName} took {ElapsedMilliseconds}ms (threshold: {ThresholdMs}ms)",
                requestName,
                elapsedMs,
                PerformanceThresholdMs);
        }

        return response;
    }
}
```

---

## 🔗 Registro dos Behaviors

### DependencyInjection.cs

```csharp
using Microsoft.Extensions.DependencyInjection;
using MediatR;
using FluentValidation;

namespace ModuleName.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddModuleApplication(this IServiceCollection services)
    {
        // MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);

            // Behaviors (ordem importa!)
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));              // 1º - Log início
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));           // 2º - Validar
            cfg.AddOpenBehavior(typeof(PerformanceMonitoringBehavior<,>));// 3º - Monitorar
            cfg.AddOpenBehavior(typeof(CachingBehavior<,>));              // 4º - Cache (queries)
            cfg.AddOpenBehavior(typeof(TransactionBehavior<,>));          // 5º - Transação (commands)
        });

        // FluentValidation
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        // Memory Cache (para CachingBehavior)
        services.AddMemoryCache();

        return services;
    }
}
```

#### ⚠️ **Ordem dos Behaviors**:
1. **Logging** - Loga TUDO
2. **Validation** - Valida cedo
3. **Performance Monitoring** - Mede tudo
4. **Caching** - Antes de executar handler
5. **Transaction** - Por último (mais próximo do handler)

---

## 🧪 Testes de Behaviors

### Teste do ValidationBehavior

```csharp
[Fact]
public async Task Handle_InvalidRequest_ShouldThrowValidationException()
{
    // Arrange
    var validators = new List<IValidator<CreateEntityCommand>>
    {
        new CreateEntityCommandValidator()
    };

    var behavior = new ValidationBehavior<CreateEntityCommand, EntityOutputDTO>(validators);

    var invalidCommand = new CreateEntityCommand(
        new CreateEntityInputDTO { Code = "" }, // ← Inválido
        "testuser"
    );

    // Act
    Func<Task> act = async () => await behavior.Handle(
        invalidCommand,
        () => Task.FromResult(new EntityOutputDTO()),
        CancellationToken.None);

    // Assert
    await act.Should().ThrowAsync<ValidationException>();
}
```

### Teste do LoggingBehavior

```csharp
[Fact]
public async Task Handle_ValidRequest_ShouldLogInformation()
{
    // Arrange
    var loggerMock = new Mock<ILogger<LoggingBehavior<CreateEntityCommand, EntityOutputDTO>>>();
    var behavior = new LoggingBehavior<CreateEntityCommand, EntityOutputDTO>(loggerMock.Object);

    var command = new CreateEntityCommand(
        new CreateEntityInputDTO { Code = "VALID" },
        "testuser"
    );

    // Act
    await behavior.Handle(
        command,
        () => Task.FromResult(new EntityOutputDTO()),
        CancellationToken.None);

    // Assert
    loggerMock.Verify(
        x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Handling CreateEntityCommand")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.AtLeastOnce);
}
```

---

## 🎯 Checklist de Qualidade

Antes de finalizar um Behavior, verifique:

- [ ] **Implementa `IPipelineBehavior<TRequest, TResponse>`**
- [ ] **É genérico** (`<TRequest, TResponse>`)
- [ ] **Chama `await next()`** (sempre)
- [ ] **Try-catch apropriado** (se necessário)
- [ ] **Logs informativos**
- [ ] **Performance considerada** (overhead mínimo)
- [ ] **Registrado no DI na ordem correta**
- [ ] **Sealed class** (não permite herança)
- [ ] **Testes unitários**
- [ ] **Documentação XML**

---

## 📚 Quando Criar um Behavior

✅ **Criar Behavior quando**:
- Lógica transversal aplicada a MÚLTIPLOS handlers
- Logging, auditoria, validação
- Transações, cache, retry policies
- Performance monitoring
- Error handling global

❌ **NÃO criar Behavior quando**:
- Lógica específica de um handler
- Lógica de negócio
- Transformação de dados
- Regras de domínio

---

## 📚 Referências

- [MediatR Pipeline Behaviors](https://github.com/jbogard/MediatR/wiki/Behaviors)
- [Aspect-Oriented Programming](https://en.wikipedia.org/wiki/Aspect-oriented_programming)
- [Módulo de Referência: Parameters](../../src/Modules/Parameters/)

---

**Ver também**:
- [CommandPattern.md](./CommandPattern.md) - Commands com behaviors
- [QueryPattern.md](./QueryPattern.md) - Queries com behaviors
- [ValidatorPattern.md](./ValidatorPattern.md) - Validações
- [DependencyInjectionPattern.md](./DependencyInjectionPattern.md) - Registro
