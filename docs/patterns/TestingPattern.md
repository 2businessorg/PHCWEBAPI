# 🧪 Testing Pattern (Unit & Integration Tests)

> **Nível**: Senior/Enterprise  
> **Camada**: Tests  
> **Responsabilidade**: Garantir qualidade, confiabilidade e regressões zero

---

## 🎯 Objetivo

Testes automatizados garantem que mudanças não quebram funcionalidades existentes, permitem refactoring seguro e documentam comportamento esperado.

---

## 📋 Estrutura de Testes

```
tests/
├── ModuleName.Domain.Tests/           ← Testes de entidades e regras
├── ModuleName.Application.Tests/      ← Testes de handlers e validators
├── ModuleName.Infrastructure.Tests/   ← Testes de repositories
└── ModuleName.Integration.Tests/      ← Testes end-to-end
```

---

## 🔧 Ferramentas e Bibliotecas

### Pacotes Necessários

```xml
<PackageReference Include="xunit" Version="2.6.2" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.5.4" />
<PackageReference Include="FluentAssertions" Version="6.12.0" />
<PackageReference Include="Moq" Version="4.20.70" />
<PackageReference Include="AutoFixture" Version="4.18.0" />
<PackageReference Include="AutoFixture.Xunit2" Version="4.18.0" />
<PackageReference Include="AutoFixture.AutoMoq" Version="4.18.0" />
<PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="8.0.0" />
```

---

## 🎨 Padrão AAA (Arrange-Act-Assert)

Todos os testes seguem o padrão **AAA**:

```csharp
[Fact]
public async Task Handle_ValidCommand_ShouldCreateEntity()
{
    // Arrange (Preparar)
    var repository = new Mock<IEntityRepository>();
    var command = new CreateEntityCommand(dto, "user");
    var handler = new CreateEntityCommandHandler(repository.Object);

    // Act (Executar)
    var result = await handler.Handle(command, CancellationToken.None);

    // Assert (Verificar)
    result.Should().NotBeNull();
    result.Stamp.Should().NotBeEmpty();
}
```

---

## 🏗️ Testes por Camada

### 1. Domain Layer Tests (Entidades)

**Projeto**: `ModuleName.Domain.Tests`

```csharp
using FluentAssertions;
using ModuleName.Domain.Entities;
using Xunit;

namespace ModuleName.Domain.Tests.Entities;

/// <summary>
/// Testes para Entity
/// </summary>
public class EntityTests
{
    [Fact]
    public void Constructor_ValidData_ShouldCreateEntity()
    {
        // Arrange & Act
        var entity = new Entity(
            stamp: "STAMP123",
            code: "CODE001",
            description: "Test Description",
            createdBy: "testuser"
        );

        // Assert
        entity.Stamp.Should().Be("STAMP123");
        entity.Code.Should().Be("CODE001");
        entity.Description.Should().Be("Test Description");
        entity.IsActive.Should().BeTrue();
        entity.OUsrInis.Should().Be("testuser");
        entity.OUsrData.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Constructor_EmptyStamp_ShouldThrowException(string stamp)
    {
        // Arrange & Act
        Action act = () => new Entity(stamp, "CODE", "Desc", "user");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Stamp cannot be empty*");
    }

    [Fact]
    public void Update_ValidData_ShouldUpdateEntity()
    {
        // Arrange
        var entity = new Entity("STAMP", "CODE", "Old Desc", "user");

        // Act
        entity.Update("New Description", 100m, "updater");

        // Assert
        entity.Description.Should().Be("New Description");
        entity.Value.Should().Be(100m);
        entity.UsrInis.Should().Be("updater");
        entity.UsrData.Should().NotBeNull();
    }

    [Fact]
    public void Deactivate_ActiveEntity_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var entity = new Entity("STAMP", "CODE", "Desc", "user");

        // Act
        entity.Deactivate("admin");

        // Assert
        entity.IsActive.Should().BeFalse();
        entity.UsrInis.Should().Be("admin");
    }
}
```

---

### 2. Application Layer Tests (Handlers)

**Projeto**: `ModuleName.Application.Tests`

#### AutoFixture + Moq Setup

```csharp
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;

namespace ModuleName.Application.Tests.Fixtures;

/// <summary>
/// Fixture customizado para testes com AutoFixture + Moq
/// </summary>
public class AutoMoqDataAttribute : AutoDataAttribute
{
    public AutoMoqDataAttribute()
        : base(() => new Fixture().Customize(new AutoMoqCustomization()))
    {
    }
}
```

#### Teste de Command Handler

```csharp
using AutoFixture.Xunit2;
using FluentAssertions;
using Moq;
using ModuleName.Application.Features.CreateEntity;
using ModuleName.Domain.Entities;
using ModuleName.Domain.Repositories;
using Xunit;

namespace ModuleName.Application.Tests.Features.CreateEntity;

/// <summary>
/// Testes para CreateEntityCommandHandler
/// </summary>
public class CreateEntityCommandHandlerTests
{
    [Theory, AutoMoqData]
    public async Task Handle_ValidCommand_ShouldCreateEntity(
        [Frozen] Mock<IEntityRepository> repositoryMock,
        CreateEntityCommandHandler sut,
        CreateEntityCommand command,
        Entity entity)
    {
        // Arrange
        repositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Entity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        // Act
        var result = await sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Stamp.Should().NotBeEmpty();
        repositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Entity>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory, AutoMoqData]
    public async Task Handle_DuplicateCode_ShouldThrowException(
        [Frozen] Mock<IEntityRepository> repositoryMock,
        CreateEntityCommandHandler sut,
        CreateEntityCommand command)
    {
        // Arrange
        repositoryMock
            .Setup(x => x.ExistsByCodeAsync(command.Dto.Code, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        Func<Task> act = async () => await sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*já existe*");
    }
}
```

#### Teste de Query Handler

```csharp
[Theory, AutoMoqData]
public async Task Handle_ExistingStamp_ShouldReturnDto(
    [Frozen] Mock<IEntityRepository> repositoryMock,
    GetEntityByStampQueryHandler sut,
    GetEntityByStampQuery query,
    Entity entity)
{
    // Arrange
    repositoryMock
        .Setup(x => x.GetByStampAsync(query.Stamp, It.IsAny<CancellationToken>()))
        .ReturnsAsync(entity);

    // Act
    var result = await sut.Handle(query, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result!.Stamp.Should().Be(entity.Stamp);
}

[Theory, AutoMoqData]
public async Task Handle_NonExistingStamp_ShouldReturnNull(
    [Frozen] Mock<IEntityRepository> repositoryMock,
    GetEntityByStampQueryHandler sut,
    GetEntityByStampQuery query)
{
    // Arrange
    repositoryMock
        .Setup(x => x.GetByStampAsync(query.Stamp, It.IsAny<CancellationToken>()))
        .ReturnsAsync((Entity?)null);

    // Act
    var result = await sut.Handle(query, CancellationToken.None);

    // Assert
    result.Should().BeNull();
}
```

#### Teste de Validator

```csharp
[Theory]
[InlineData("")]
[InlineData(null)]
public async Task Validate_EmptyCode_ShouldFail(string code)
{
    // Arrange
    var validator = new CreateEntityCommandValidator();
    var command = new CreateEntityCommand(
        new CreateEntityInputDTO { Code = code },
        "testuser"
    );

    // Act
    var result = await validator.ValidateAsync(command);

    // Assert
    result.IsValid.Should().BeFalse();
    result.Errors.Should().Contain(e => e.PropertyName == "Dto.Code");
}

[Fact]
public async Task Validate_ValidCommand_ShouldPass()
{
    // Arrange
    var validator = new CreateEntityCommandValidator();
    var command = new CreateEntityCommand(
        new CreateEntityInputDTO
        {
            Code = "VALID_CODE",
            Description = "Valid Description"
        },
        "testuser"
    );

    // Act
    var result = await validator.ValidateAsync(command);

    // Assert
    result.IsValid.Should().BeTrue();
}
```

---

### 3. Infrastructure Tests (Repositories)

**Projeto**: `ModuleName.Infrastructure.Tests`

```csharp
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ModuleName.Domain.Entities;
using ModuleName.Infrastructure.Persistence;
using ModuleName.Infrastructure.Repositories;
using Xunit;

namespace ModuleName.Infrastructure.Tests.Repositories;

/// <summary>
/// Testes de integração para EntityRepositoryEFCore
/// </summary>
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
        retrieved.Code.Should().Be("CODE001");
    }

    [Fact]
    public async Task GetByCodeAsync_ExistingCode_ShouldReturnEntity()
    {
        // Arrange
        var entity = new Entity("STAMP", "CODE001", "Test", "user");
        await _repository.AddAsync(entity);

        // Act
        var result = await _repository.GetByCodeAsync("CODE001");

        // Assert
        result.Should().NotBeNull();
        result!.Code.Should().Be("CODE001");
    }

    [Fact]
    public async Task UpdateAsync_ValidEntity_ShouldUpdateDatabase()
    {
        // Arrange
        var entity = new Entity("STAMP", "CODE", "Old Desc", "user");
        await _repository.AddAsync(entity);
        entity.Update("New Desc", 100m, "updater");

        // Act
        await _repository.UpdateAsync(entity);
        var retrieved = await _repository.GetByStampAsync(entity.Stamp);

        // Assert
        retrieved!.Description.Should().Be("New Desc");
        retrieved.Value.Should().Be(100m);
    }

    [Fact]
    public async Task DeleteAsync_ExistingEntity_ShouldRemoveFromDatabase()
    {
        // Arrange
        var entity = new Entity("STAMP", "CODE", "Test", "user");
        await _repository.AddAsync(entity);

        // Act
        await _repository.DeleteAsync(entity);
        var retrieved = await _repository.GetByStampAsync(entity.Stamp);

        // Assert
        retrieved.Should().BeNull();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
```

---

### 4. Integration Tests (API End-to-End)

**Projeto**: `ModuleName.Integration.Tests`

```csharp
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using ModuleName.Application.DTOs.Entities;
using Xunit;

namespace ModuleName.Integration.Tests.Controllers;

/// <summary>
/// Testes de integração end-to-end
/// </summary>
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
        // Act
        var response = await _client.GetAsync("/api/entities");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("\"success\":true");
    }

    [Fact]
    public async Task GetByStamp_NonExistingStamp_ShouldReturn404()
    {
        // Act
        var response = await _client.GetAsync("/api/entities/INVALID_STAMP");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_ValidDto_ShouldReturn201()
    {
        // Arrange
        var dto = new CreateEntityInputDTO
        {
            Code = $"TEST{Guid.NewGuid().ToString()[..8]}",
            Description = "Integration Test Entity"
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

    [Fact]
    public async Task Create_InvalidDto_ShouldReturn400()
    {
        // Arrange
        var dto = new CreateEntityInputDTO
        {
            Code = "",  // ← Inválido
            Description = "Test"
        };

        var content = new StringContent(
            JsonSerializer.Serialize(dto),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/entities", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_ValidDto_ShouldReturn200()
    {
        // Arrange - Criar entidade primeiro
        var createDto = new CreateEntityInputDTO
        {
            Code = $"TEST{Guid.NewGuid().ToString()[..8]}",
            Description = "Original"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/entities", createDto);
        var location = createResponse.Headers.Location!.ToString();
        var stamp = location.Split('/').Last();

        var updateDto = new UpdateEntityInputDTO
        {
            Description = "Updated"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/entities/{stamp}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
```

---

## 🎯 Cobertura de Testes

### Meta: > 80% de Cobertura

```bash
# Executar testes com cobertura
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura

# Ver relatório
reportgenerator -reports:coverage.cobertura.xml -targetdir:coverage-report
```

---

## ✅ Boas Práticas

### 1. Nomenclatura de Testes

✅ **Bom** (descritivo):
```csharp
public async Task Handle_ValidCommand_ShouldCreateEntity()
public async Task Handle_DuplicateCode_ShouldThrowException()
public async Task GetByStamp_NonExistingStamp_ShouldReturnNull()
```

Padrão: `MethodName_Scenario_ExpectedResult`

---

### 2. Um Assert por Teste (quando possível)

✅ **Bom**:
```csharp
[Fact]
public void Constructor_ValidData_ShouldSetStamp()
{
    var entity = new Entity("STAMP", "CODE", "Desc", "user");
    entity.Stamp.Should().Be("STAMP");
}

[Fact]
public void Constructor_ValidData_ShouldSetCode()
{
    var entity = new Entity("STAMP", "CODE", "Desc", "user");
    entity.Code.Should().Be("CODE");
}
```

---

### 3. FluentAssertions para Clareza

✅ **Bom** (legível):
```csharp
result.Should().NotBeNull();
result.Stamp.Should().Be("STAMP123");
result.Errors.Should().ContainSingle(e => e.PropertyName == "Code");
```

❌ **Ruim** (menos claro):
```csharp
Assert.NotNull(result);
Assert.Equal("STAMP123", result.Stamp);
```

---

### 4. Theory para Múltiplos Cenários

```csharp
[Theory]
[InlineData("")]
[InlineData(null)]
[InlineData("   ")]
public void Validate_InvalidCode_ShouldFail(string code)
{
    // ...
}
```

---

## 🎯 Checklist de Qualidade

Antes de finalizar Testes, verifique:

- [ ] **Cobertura > 80%**
- [ ] **Testes de entidades** (Domain)
- [ ] **Testes de handlers** (Application)
- [ ] **Testes de validators** (Application)
- [ ] **Testes de repositories** (Infrastructure)
- [ ] **Testes de integração** (API)
- [ ] **Padrão AAA seguido**
- [ ] **Nomenclatura descritiva**
- [ ] **FluentAssertions usado**
- [ ] **AutoFixture + Moq**
- [ ] **Testes isolados** (sem dependências entre si)
- [ ] **Testes rápidos** (< 100ms cada)

---

## 📚 Referências

- [XUnit Documentation](https://xunit.net/)
- [FluentAssertions Documentation](https://fluentassertions.com/)
- [Moq Documentation](https://github.com/moq/moq4)
- [AutoFixture Documentation](https://github.com/AutoFixture/AutoFixture)
- [Módulo de Referência: Parameters](../../src/Modules/Parameters/tests/)

---

**Ver também**:
- [EntityPattern.md](./EntityPattern.md) - Testes de entidades
- [CommandPattern.md](./CommandPattern.md) - Testes de commands
- [QueryPattern.md](./QueryPattern.md) - Testes de queries
- [ValidatorPattern.md](./ValidatorPattern.md) - Testes de validators
- [RepositoryPattern.md](./RepositoryPattern.md) - Testes de repositories
