# 🔍 Query Pattern (CQRS)

> **Nível**: Senior/Enterprise  
> **Camada**: Application Layer  
> **Responsabilidade**: Executar operações de **leitura** que **NÃO alteram estado** (Get, GetAll, Search)

---

## 🎯 Objetivo

Queries representam **perguntas ao sistema** sem causar efeitos colaterais. Seguindo CQRS, queries são otimizadas para leitura e separadas de commands.

---

## 📋 Estrutura de uma Query

### Organização por Funcionalidade (Feature)

```
ModuleName.Application/
└── Features/
    └── GetEntity/                       ← Uma pasta por query
        ├── GetEntityQuery.cs            ← Record (imutável)
        └── GetEntityQueryHandler.cs     ← Handler (lógica)
```

**Nota**: Queries geralmente **NÃO precisam de Validators** (parâmetros simples).

---

## 🔧 Implementação Passo a Passo

### 1. Criar a Query (Record Imutável)

**Localização**: `Features/GetEntityByStamp/GetEntityByStampQuery.cs`

```csharp
using MediatR;
using ModuleName.Application.DTOs.Entities;

namespace ModuleName.Application.Features.GetEntityByStamp;

/// <summary>
/// Query para buscar uma entidade pelo stamp
/// </summary>
/// <param name="Stamp">Identificador único da entidade</param>
public record GetEntityByStampQuery(
    string Stamp
) : IRequest<EntityOutputDTO?>;  // ← Retorna DTO ou null
```

#### ✅ **Boas Práticas**:
- ✅ Use `record` (imutável por padrão)
- ✅ Parâmetros simples e diretos
- ✅ `IRequest<TResponse>` do MediatR
- ✅ Retornar DTO, não entidade de domínio
- ✅ `TResponse?` se pode retornar null

#### ❌ **Evitar**:
- ❌ Classes mutáveis (`class` com setters)
- ❌ Lógica de negócio na Query
- ❌ Retornar entidades de domínio

---

### 2. Criar o Query Handler

**Localização**: `Features/GetEntityByStamp/GetEntityByStampQueryHandler.cs`

```csharp
using MediatR;
using ModuleName.Application.DTOs.Entities;
using ModuleName.Application.Mappings;
using ModuleName.Domain.Repositories;

namespace ModuleName.Application.Features.GetEntityByStamp;

/// <summary>
/// Handler responsável por buscar uma entidade pelo stamp
/// </summary>
public class GetEntityByStampQueryHandler : IRequestHandler<GetEntityByStampQuery, EntityOutputDTO?>
{
    private readonly IEntityRepository _repository;

    public GetEntityByStampQueryHandler(IEntityRepository repository)
    {
        _repository = repository;
    }

    public async Task<EntityOutputDTO?> Handle(
        GetEntityByStampQuery request,
        CancellationToken cancellationToken)
    {
        // 1. Buscar entidade
        var entity = await _repository.GetByStampAsync(request.Stamp, cancellationToken);

        // 2. Retornar null ou DTO mapeado
        return entity?.ToDto<EntityOutputDTO>();
    }
}
```

#### ✅ **Boas Práticas**:
- ✅ **Simplicidade**: Handlers de query são simples
- ✅ **Async/Await**: Sempre operações assíncronas
- ✅ **CancellationToken**: Passado para operações I/O
- ✅ **Mapeamento direto**: Entity → DTO
- ✅ **Retornar null é válido**: Para queries opcionais
- ✅ **Sem efeitos colaterais**: Nunca altera estado

#### ❌ **Evitar**:
- ❌ Alterar dados no banco
- ❌ Lógica complexa (mova para repositório)
- ❌ Lançar exceções para "não encontrado" em queries opcionais
- ❌ Retornar entidades de domínio

---

## 🏗️ Exemplos Completos

### Exemplo 1: Get By ID/Stamp (Optional)

```csharp
// GetParameterByStampQuery.cs
public record GetParameterByStampQuery(
    string Para1Stamp
) : IRequest<ParameterOutputDTO?>;

// GetParameterByStampQueryHandler.cs
public class GetParameterByStampQueryHandler : IRequestHandler<GetParameterByStampQuery, ParameterOutputDTO?>
{
    private readonly IPara1Repository _para1Repository;

    public GetParameterByStampQueryHandler(IPara1Repository para1Repository)
    {
        _para1Repository = para1Repository;
    }

    public async Task<ParameterOutputDTO?> Handle(
        GetParameterByStampQuery request,
        CancellationToken cancellationToken)
    {
        var para1 = await _para1Repository.GetByStampAsync(request.Para1Stamp, cancellationToken);
        return para1?.ToDto<ParameterOutputDTO>();
    }
}
```

---

### Exemplo 2: Get All (Collection)

```csharp
// GetAllParametersQuery.cs
public record GetAllParametersQuery(
    bool IncludeInactive = false
) : IRequest<IEnumerable<ParameterOutputDTO>>;

// GetAllParametersQueryHandler.cs
public class GetAllParametersQueryHandler : IRequestHandler<GetAllParametersQuery, IEnumerable<ParameterOutputDTO>>
{
    private readonly IPara1Repository _para1Repository;

    public GetAllParametersQueryHandler(IPara1Repository para1Repository)
    {
        _para1Repository = para1Repository;
    }

    public async Task<IEnumerable<ParameterOutputDTO>> Handle(
        GetAllParametersQuery request,
        CancellationToken cancellationToken)
    {
        var para1List = await _para1Repository.GetAllAsync(request.IncludeInactive, cancellationToken);
        return para1List.ToDtos<ParameterOutputDTO>();
    }
}
```

---

### Exemplo 3: Get Paged (Com Paginação)

```csharp
// GetPagedEntitiesQuery.cs
public record GetPagedEntitiesQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    string? SortBy = "CreatedAt",
    bool SortDescending = true
) : IRequest<PagedResult<EntityOutputDTO>>;

// GetPagedEntitiesQueryHandler.cs
public class GetPagedEntitiesQueryHandler : IRequestHandler<GetPagedEntitiesQuery, PagedResult<EntityOutputDTO>>
{
    private readonly IEntityRepository _repository;

    public GetPagedEntitiesQueryHandler(IEntityRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<EntityOutputDTO>> Handle(
        GetPagedEntitiesQuery request,
        CancellationToken cancellationToken)
    {
        // 1. Buscar dados paginados
        var entities = await _repository.GetPagedAsync(
            request.PageNumber,
            request.PageSize,
            request.SearchTerm,
            request.SortBy,
            request.SortDescending,
            cancellationToken
        );

        // 2. Contar total de registros
        var totalCount = await _repository.CountAsync(request.SearchTerm, cancellationToken);

        // 3. Mapear para DTOs
        var dtos = entities.ToDtos<EntityOutputDTO>();

        // 4. Retornar resultado paginado
        return new PagedResult<EntityOutputDTO>
        {
            Items = dtos,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };
    }
}

// PagedResult.cs (DTO para resultados paginados)
public class PagedResult<T>
{
    public IEnumerable<T> Items { get; init; } = Array.Empty<T>();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
```

---

### Exemplo 4: Search/Filter (Busca Complexa)

```csharp
// SearchEntitiesQuery.cs
public record SearchEntitiesQuery(
    string? Code = null,
    string? Description = null,
    DateTime? CreatedFrom = null,
    DateTime? CreatedTo = null,
    bool? IsActive = null,
    int PageNumber = 1,
    int PageSize = 20
) : IRequest<PagedResult<EntityOutputDTO>>;

// SearchEntitiesQueryHandler.cs
public class SearchEntitiesQueryHandler : IRequestHandler<SearchEntitiesQuery, PagedResult<EntityOutputDTO>>
{
    private readonly IEntityRepository _repository;

    public SearchEntitiesQueryHandler(IEntityRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<EntityOutputDTO>> Handle(
        SearchEntitiesQuery request,
        CancellationToken cancellationToken)
    {
        // Construir filtro
        var filter = new EntitySearchFilter
        {
            Code = request.Code,
            Description = request.Description,
            CreatedFrom = request.CreatedFrom,
            CreatedTo = request.CreatedTo,
            IsActive = request.IsActive
        };

        // Buscar com filtro
        var entities = await _repository.SearchAsync(
            filter,
            request.PageNumber,
            request.PageSize,
            cancellationToken
        );

        var totalCount = await _repository.CountAsync(filter, cancellationToken);

        return new PagedResult<EntityOutputDTO>
        {
            Items = entities.ToDtos<EntityOutputDTO>(),
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };
    }
}
```

---

## 🔗 Integração com Controller

### Get Single

```csharp
[HttpGet("{stamp}")]
[Authorize(Roles = AppRoles.Administrator)]
[EnableRateLimiting("entity-query")]
public async Task<ActionResult<ResponseDTO>> GetByStamp(
    string stamp,
    CancellationToken ct = default)
{
    var result = await _mediator.Send(new GetEntityByStampQuery(stamp), ct);
    var correlationId = HttpContext.GetCorrelationId();

    return result is null
        ? NotFound(ResponseDTO.Error(ResponseCodes.Entity.NotFound, correlationId: correlationId))
        : Ok(ResponseDTO.Success(data: result, correlationId: correlationId));
}
```

### Get All

```csharp
[HttpGet]
[Authorize(Roles = AppRoles.Administrator)]
[EnableRateLimiting("entity-query")]
public async Task<ActionResult<ResponseDTO>> GetAll(
    [FromQuery] bool includeInactive = false,
    CancellationToken ct = default)
{
    var result = await _mediator.Send(new GetAllEntitiesQuery(includeInactive), ct);
    var correlationId = HttpContext.GetCorrelationId();

    return Ok(ResponseDTO.Success(data: result, correlationId: correlationId));
}
```

### Get Paged

```csharp
[HttpGet("paged")]
[Authorize(Roles = AppRoles.Administrator)]
[EnableRateLimiting("entity-query")]
public async Task<ActionResult<ResponseDTO>> GetPaged(
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] string? searchTerm = null,
    CancellationToken ct = default)
{
    var query = new GetPagedEntitiesQuery(pageNumber, pageSize, searchTerm);
    var result = await _mediator.Send(query, ct);
    var correlationId = HttpContext.GetCorrelationId();

    return Ok(ResponseDTO.Success(data: result, correlationId: correlationId));
}
```

---

## ⚡ Performance e Otimização

### 1. Projeção Direta para DTO (EF Core)

**Melhor performance**: Evita carregar entidades completas

```csharp
public async Task<IEnumerable<EntityOutputDTO>> GetAllOptimizedAsync(CancellationToken ct)
{
    return await _context.Entities
        .Where(e => e.IsActive)
        .Select(e => new EntityOutputDTO
        {
            Stamp = e.Stamp,
            Code = e.Code,
            Description = e.Description,
            CreatedAt = e.CreatedAt
        })
        .ToListAsync(ct);
}
```

### 2. AsNoTracking para Queries

**Sem rastreamento**: Queries não precisam de tracking

```csharp
return await _context.Entities
    .AsNoTracking()  // ← Importante para queries
    .Where(e => e.Stamp == stamp)
    .FirstOrDefaultAsync(ct);
```

### 3. Paginação no Banco

```csharp
return await _context.Entities
    .OrderBy(e => e.CreatedAt)
    .Skip((pageNumber - 1) * pageSize)
    .Take(pageSize)
    .ToListAsync(ct);
```

### 4. Caching de Queries Freq uentes

```csharp
public class GetAllEntitiesQueryHandler : IRequestHandler<GetAllEntitiesQuery, IEnumerable<EntityOutputDTO>>
{
    private readonly IEntityRepository _repository;
    private readonly IMemoryCache _cache;

    public async Task<IEnumerable<EntityOutputDTO>> Handle(
        GetAllEntitiesQuery request,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"entities_all_{request.IncludeInactive}";
        
        if (_cache.TryGetValue(cacheKey, out IEnumerable<EntityOutputDTO>? cached))
        {
            return cached!;
        }

        var entities = await _repository.GetAllAsync(request.IncludeInactive, cancellationToken);
        var dtos = entities.ToDtos<EntityOutputDTO>();

        _cache.Set(cacheKey, dtos, TimeSpan.FromMinutes(5));
        
        return dtos;
    }
}
```

---

## 🧪 Testes

### Unit Test do Handler

```csharp
[Theory, AutoMoqData]
public async Task Handle_ExistingStamp_ShouldReturnDto(
    [Frozen] Mock<IEntityRepository> repositoryMock,
    GetEntityByStampQuery query,
    Entity entity)
{
    // Arrange
    repositoryMock
        .Setup(x => x.GetByStampAsync(query.Stamp, It.IsAny<CancellationToken>()))
        .ReturnsAsync(entity);

    var sut = new GetEntityByStampQueryHandler(repositoryMock.Object);

    // Act
    var result = await sut.Handle(query, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result!.Stamp.Should().Be(entity.Stamp);
}

[Theory, AutoMoqData]
public async Task Handle_NonExistingStamp_ShouldReturnNull(
    [Frozen] Mock<IEntityRepository> repositoryMock,
    GetEntityByStampQuery query)
{
    // Arrange
    repositoryMock
        .Setup(x => x.GetByStampAsync(query.Stamp, It.IsAny<CancellationToken>()))
        .ReturnsAsync((Entity?)null);

    var sut = new GetEntityByStampQueryHandler(repositoryMock.Object);

    // Act
    var result = await sut.Handle(query, CancellationToken.None);

    // Assert
    result.Should().BeNull();
}
```

### Teste de Performance (Grande Volume)

```csharp
[Fact]
public async Task Handle_LargeDataset_ShouldHandleEfficiently()
{
    // Arrange
    var entities = Enumerable.Range(1, 1000)
        .Select(i => new Entity($"STAMP{i}", $"Code{i}", $"Desc{i}"))
        .ToList();

    repositoryMock
        .Setup(x => x.GetAllAsync(false, It.IsAny<CancellationToken>()))
        .ReturnsAsync(entities);

    var sut = new GetAllEntitiesQueryHandler(repositoryMock.Object);
    var query = new GetAllEntitiesQuery(false);

    // Act
    var stopwatch = Stopwatch.StartNew();
    var result = await sut.Handle(query, CancellationToken.None);
    stopwatch.Stop();

    // Assert
    result.Should().HaveCount(1000);
    stopwatch.ElapsedMilliseconds.Should().BeLessThan(500, 
        "mapeamento de 1000 itens deve ser rápido (<500ms)");
}
```

---

## 🎯 Checklist de Qualidade

Antes de finalizar uma Query, verifique:

- [ ] **Query é um `record` imutável**
- [ ] **Handler implementa `IRequestHandler<TQuery, TResponse>`**
- [ ] **Todas operações são assíncronas** (`async/await`)
- [ ] **CancellationToken é passado para operações I/O**
- [ ] **Query NÃO altera estado** (read-only)
- [ ] **Retorna DTOs, não entidades**
- [ ] **Paginação implementada** (se lista grande)
- [ ] **AsNoTracking usado** (EF Core queries)
- [ ] **Projeção direta para DTO** (quando possível)
- [ ] **Caching considerado** (queries frequentes)
- [ ] **Handler tem testes unitários** (cobertura > 80%)
- [ ] **Teste de performance** (grandes volumes)
- [ ] **Documentação XML está presente**

---

## 📚 Comparação: Query vs Command

| Aspecto | Query | Command |
|---------|-------|---------|
| **Objetivo** | Leitura | Escrita |
| **Altera Estado?** | ❌ Não | ✅ Sim |
| **Retorno** | DTO(s) ou null | DTO ou bool |
| **Validação** | Simples (geralmente não precisa) | FluentValidation obrigatória |
| **Performance** | Otimizada com projeções | Menos crítica |
| **Caching** | Recomendado | ❌ Não aplicável |
| **AsNoTracking** | ✅ Sempre | ❌ Nunca |
| **Transação** | ❌ Não necessária | ✅ Sim (SaveChanges) |

---

## 📚 Referências

- [CQRS Pattern - Martin Fowler](https://martinfowler.com/bliki/CQRS.html)
- [MediatR Documentation](https://github.com/jbogard/MediatR/wiki)
- [EF Core Performance](https://docs.microsoft.com/ef/core/performance/)
- [Módulo de Referência: Parameters](../../src/Modules/Parameters/)

---

**Ver também**:
- [CommandPattern.md](./CommandPattern.md) - Operações de escrita
- [DTOPattern.md](./DTOPattern.md) - Data Transfer Objects
- [RepositoryPattern.md](./RepositoryPattern.md) - Acesso a dados
- [ControllerPattern.md](./ControllerPattern.md) - Integração com API
