# 🌐 Controller Pattern (REST API)

> **Nível**: Senior/Enterprise  
> **Camada**: Presentation Layer  
> **Responsabilidade**: Expor funcionalidades via HTTP, delegar para MediatR

---

## 🎯 Objetivo

Controllers são **finos e stateless**, apenas roteando requisições HTTP para Commands/Queries via MediatR. Toda lógica de negócio está na Application Layer.

---

## 📋 Estrutura

```
ModuleName.Presentation/
└── REST/
    ├── Controllers/
    │   └── EntitiesController.cs
    └── RestDependencyInjection.cs
```

---

## 🔧 Implementação Passo a Passo

### 1. Controller Básico

**Localização**: `REST/Controllers/EntitiesController.cs`

```csharp
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ModuleName.Application.Features.CreateEntity;
using ModuleName.Application.Features.GetEntityByStamp;
using ModuleName.Application.Features.GetAllEntities;
using ModuleName.Application.Features.UpdateEntity;
using ModuleName.Application.Features.DeleteEntity;
using ModuleName.Application.DTOs.Entities;
using Shared.DTOs;
using Shared.Extensions;

namespace ModuleName.Presentation.REST.Controllers;

/// <summary>
/// Controller para gerenciamento de entidades
/// </summary>
[ApiController]
[Route("api/entities")]
[Produces("application/json")]
public sealed class EntitiesController : ControllerBase
{
    private readonly IMediator _mediator;

    public EntitiesController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Buscar todas as entidades
    /// </summary>
    /// <param name="includeInactive">Incluir entidades inativas</param>
    /// <param name="ct">Token de cancelamento</param>
    [HttpGet]
    [Authorize(Roles = AppRoles.Administrator)]
    [EnableRateLimiting("entity-query")]
    [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ResponseDTO>> GetAll(
        [FromQuery] bool includeInactive = false,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetAllEntitiesQuery(includeInactive), ct);
        var correlationId = HttpContext.GetCorrelationId();

        return Ok(ResponseDTO.Success(data: result, correlationId: correlationId));
    }

    /// <summary>
    /// Buscar entidade por stamp
    /// </summary>
    /// <param name="stamp">Identificador único da entidade</param>
    /// <param name="ct">Token de cancelamento</param>
    [HttpGet("{stamp}")]
    [Authorize(Roles = AppRoles.Administrator)]
    [EnableRateLimiting("entity-query")]
    [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status500InternalServerError)]
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

    /// <summary>
    /// Criar nova entidade
    /// </summary>
    /// <param name="dto">Dados da entidade</param>
    /// <param name="ct">Token de cancelamento</param>
    [HttpPost]
    [Authorize(Policy = AppPolicies.ApiAccess)]
    [EnableRateLimiting("entity-create")]
    [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ResponseDTO>> Create(
        [FromBody] CreateEntityInputDTO dto,
        CancellationToken ct = default)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var command = new CreateEntityCommand(dto, userId);
        var result = await _mediator.Send(command, ct);
        var correlationId = HttpContext.GetCorrelationId();

        return CreatedAtAction(
            nameof(GetByStamp),
            new { stamp = result.Stamp },
            ResponseDTO.Success(data: result, correlationId: correlationId)
        );
    }

    /// <summary>
    /// Atualizar entidade existente
    /// </summary>
    /// <param name="stamp">Identificador único da entidade</param>
    /// <param name="dto">Novos dados da entidade</param>
    /// <param name="ct">Token de cancelamento</param>
    [HttpPut("{stamp}")]
    [Authorize(Policy = AppPolicies.ApiAccess)]
    [EnableRateLimiting("entity-update")]
    [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ResponseDTO>> Update(
        string stamp,
        [FromBody] UpdateEntityInputDTO dto,
        CancellationToken ct = default)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var command = new UpdateEntityCommand(stamp, dto, userId);
        var result = await _mediator.Send(command, ct);
        var correlationId = HttpContext.GetCorrelationId();

        return result is null
            ? NotFound(ResponseDTO.Error(ResponseCodes.Entity.NotFound, correlationId: correlationId))
            : Ok(ResponseDTO.Success(data: result, correlationId: correlationId));
    }

    /// <summary>
    /// Eliminar entidade
    /// </summary>
    /// <param name="stamp">Identificador único da entidade</param>
    /// <param name="ct">Token de cancelamento</param>
    [HttpDelete("{stamp}")]
    [Authorize(Roles = AppRoles.Administrator)]
    [EnableRateLimiting("entity-delete")]
    [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ResponseDTO>> Delete(
        string stamp,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new DeleteEntityCommand(stamp), ct);
        var correlationId = HttpContext.GetCorrelationId();

        return result
            ? Ok(ResponseDTO.Success(content: ResponseCodes.Entity.DeletedSuccessfully, correlationId: correlationId))
            : NotFound(ResponseDTO.Error(ResponseCodes.Entity.NotFound, correlationId: correlationId));
    }
}
```

---

## ✅ Boas Práticas

### 1. Controller Fino

❌ **Ruim** (lógica no controller):
```csharp
[HttpPost]
public async Task<IActionResult> Create([FromBody] CreateEntityInputDTO dto)
{
    // ❌ Validação no controller
    if (string.IsNullOrEmpty(dto.Code))
        return BadRequest("Code is required");

    // ❌ Lógica de negócio no controller
    var entity = new Entity();
    entity.Code = dto.Code;
    entity.CreatedAt = DateTime.Now;
    
    // ❌ Acesso direto ao repositório
    await _repository.AddAsync(entity);
    
    return Ok(entity);
}
```

✅ **Bom** (delega para MediatR):
```csharp
[HttpPost]
[Authorize(Policy = AppPolicies.ApiAccess)]
[EnableRateLimiting("entity-create")]
public async Task<ActionResult<ResponseDTO>> Create(
    [FromBody] CreateEntityInputDTO dto,
    CancellationToken ct = default)
{
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var command = new CreateEntityCommand(dto, userId);
    var result = await _mediator.Send(command, ct);
    var correlationId = HttpContext.GetCorrelationId();

    return CreatedAtAction(
        nameof(GetByStamp),
        new { stamp = result.Stamp },
        ResponseDTO.Success(data: result, correlationId: correlationId)
    );
}
```

---

### 2. Documentação XML Completa

```csharp
/// <summary>
/// Buscar entidade por stamp
/// </summary>
/// <param name="stamp">Identificador único da entidade (25 caracteres)</param>
/// <param name="ct">Token de cancelamento</param>
/// <returns>Entidade encontrada ou 404 Not Found</returns>
/// <response code="200">Entidade encontrada com sucesso</response>
/// <response code="404">Entidade não encontrada</response>
/// <response code="500">Erro interno do servidor</response>
[HttpGet("{stamp}")]
public async Task<ActionResult<ResponseDTO>> GetByStamp(
    string stamp,
    CancellationToken ct = default)
{
    // ...
}
```

---

### 3. Autorização e Segurança

```csharp
// Apenas Administrator
[Authorize(Roles = AppRoles.Administrator)]

// Administrator OU ApiUser
[Authorize(Roles = $"{AppRoles.Administrator},{AppRoles.ApiUser}")]

// Policy customizada
[Authorize(Policy = AppPolicies.ApiAccess)]

// Rate Limiting
[EnableRateLimiting("entity-create")]  // Limite de criações
[EnableRateLimiting("entity-query")]   // Limite de queries
```

---

### 4. ProducesResponseType

```csharp
[ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ResponseDTO), StatusCodes.Status500InternalServerError)]
```

**Benefícios**:
- ✅ Swagger documentation precisa
- ✅ OpenAPI schema correto
- ✅ IntelliSense no cliente

---

### 5. CancellationToken

```csharp
public async Task<ActionResult<ResponseDTO>> GetAll(
    [FromQuery] bool includeInactive = false,
    CancellationToken ct = default)  // ← Sempre incluir
{
    var result = await _mediator.Send(new GetAllEntitiesQuery(includeInactive), ct);
    // ...
}
```

---

### 6. CorrelationId

```csharp
var correlationId = HttpContext.GetCorrelationId();

return Ok(ResponseDTO.Success(data: result, correlationId: correlationId));
```

**Benefícios**:
- ✅ Rastreamento de requisições
- ✅ Debugging facilitado
- ✅ Logs correlacionados

---

### 7. Capturar UserId

```csharp
var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
var command = new CreateEntityCommand(dto, userId);
```

---

## 🏗️ Exemplos Avançados

### Paginação

```csharp
/// <summary>
/// Buscar entidades paginadas
/// </summary>
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

### Busca com Filtros

```csharp
/// <summary>
/// Buscar entidades com filtros
/// </summary>
[HttpGet("search")]
[Authorize(Roles = AppRoles.Administrator)]
[EnableRateLimiting("entity-query")]
public async Task<ActionResult<ResponseDTO>> Search(
    [FromQuery] string? code = null,
    [FromQuery] string? description = null,
    [FromQuery] DateTime? createdFrom = null,
    [FromQuery] DateTime? createdTo = null,
    [FromQuery] bool? isActive = null,
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 20,
    CancellationToken ct = default)
{
    var query = new SearchEntitiesQuery(
        code, description, createdFrom, createdTo, isActive, pageNumber, pageSize);
        
    var result = await _mediator.Send(query, ct);
    var correlationId = HttpContext.GetCorrelationId();

    return Ok(ResponseDTO.Success(data: result, correlationId: correlationId));
}
```

### Bulk Operations

```csharp
/// <summary>
/// Criar múltiplas entidades
/// </summary>
[HttpPost("bulk")]
[Authorize(Roles = AppRoles.Administrator)]
[EnableRateLimiting("entity-bulk-create")]
public async Task<ActionResult<ResponseDTO>> CreateBulk(
    [FromBody] IEnumerable<CreateEntityInputDTO> dtos,
    CancellationToken ct = default)
{
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var command = new CreateEntitiesBulkCommand(dtos, userId);
    var result = await _mediator.Send(command, ct);
    var correlationId = HttpContext.GetCorrelationId();

    return Ok(ResponseDTO.Success(data: result, correlationId: correlationId));
}
```

---

## 🧪 Testes de Controller (Integration Tests)

```csharp
public class EntitiesControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public EntitiesControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ValidRequest_ShouldReturn200()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/entities");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("\"success\":true");
    }

    [Fact]
    public async Task GetByStamp_NonExistingStamp_ShouldReturn404()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/entities/INVALID_STAMP");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_ValidDto_ShouldReturn201()
    {
        // Arrange
        var dto = new CreateEntityInputDTO
        {
            Code = "TEST001",
            Description = "Test Entity"
        };

        var content = new StringContent(
            JsonSerializer.Serialize(dto),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/entities", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
    }
}
```

---

## 🎯 Checklist de Qualidade

Antes de finalizar um Controller, verifique:

- [ ] **Sealed class**
- [ ] **Herda de `ControllerBase`**
- [ ] **Atributos `[ApiController]`, `[Route]`, `[Produces]`**
- [ ] **Apenas injeção de `IMediator`**
- [ ] **Métodos com XML documentation**
- [ ] **ProducesResponseType em todos os métodos**
- [ ] **Autorização apropriada** (`[Authorize]`)
- [ ] **Rate Limiting** (`[EnableRateLimiting]`)
- [ ] **CancellationToken em todos os métodos async**
- [ ] **CorrelationId em todas as responses**
- [ ] **CreatedAtAction para POST** (com Location header)
- [ ] **StatusCodes corretos** (200, 201, 404, 400, 500)
- [ ] **Testes de integração**

---

## 📚 Referências

- [ASP.NET Core Controllers](https://docs.microsoft.com/aspnet/core/web-api/)
- [REST API Best Practices](https://restfulapi.net/)
- [Swagger/OpenAPI](https://swagger.io/specification/)
- [Módulo de Referência: Parameters](../../src/Modules/Parameters/)

---

**Ver também**:
- [CommandPattern.md](./CommandPattern.md) - Commands chamados
- [QueryPattern.md](./QueryPattern.md) - Queries chamadas
- [DTOPattern.md](./DTOPattern.md) - Input/Output DTOs
- [DependencyInjectionPattern.md](./DependencyInjectionPattern.md) - Registro
