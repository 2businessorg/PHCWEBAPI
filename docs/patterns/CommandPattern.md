# 📝 Command Pattern (CQRS)

> **Nível**: Senior/Enterprise  
> **Camada**: Application Layer  
> **Responsabilidade**: Executar operações que **alteram estado** (Create, Update, Delete)

---

## 🎯 Objetivo

Commands representam **intenções de alterar estado** do sistema. Seguindo CQRS, commands são separados de queries e encapsulam toda a lógica necessária para executar uma operação de escrita.

---

## 📋 Estrutura de um Command

### Organização por Funcionalidade (Feature)

```
ModuleName.Application/
└── Features/
    └── CreateEntity/                    ← Uma pasta por feature
        ├── CreateEntityCommand.cs       ← Record (imutável)
        ├── CreateEntityCommandHandler.cs ← Handler (lógica)
        └── CreateEntityCommandValidator.cs ← Validações
```

---

## 🔧 Implementação Passo a Passo

### 1. Criar o Command (Record Imutável)

**Localização**: `Features/CreateEntity/CreateEntityCommand.cs`

```csharp
using MediatR;
using ModuleName.Application.DTOs.Entities;

namespace ModuleName.Application.Features.CreateEntity;

/// <summary>
/// Command para criar uma nova entidade
/// </summary>
/// <param name="Dto">Dados de entrada validados</param>
/// <param name="CreatedBy">Usuário que está criando (auditoria)</param>
public record CreateEntityCommand(
    CreateEntityInputDTO Dto,
    string? CreatedBy
) : IRequest<EntityOutputDTO>;
```

#### ✅ **Boas Práticas**:
- ✅ Use `record` (imutável por padrão)
- ✅ Propriedades em PascalCase
- ✅ Documentação XML (`<summary>`)
- ✅ `IRequest<TResponse>` do MediatR
- ✅ Incluir dados de auditoria (`CreatedBy`, `UpdatedBy`)
- ✅ Retornar DTO, não entidade de domínio

#### ❌ **Evitar**:
- ❌ Classes mutáveis (`class` com setters)
- ❌ Lógica de negócio no Command
- ❌ Acesso direto ao banco
- ❌ Retornar entidades de domínio

---

### 2. Criar o Command Handler

**Localização**: `Features/CreateEntity/CreateEntityCommandHandler.cs`

```csharp
using MediatR;
using ModuleName.Application.DTOs.Entities;
using ModuleName.Application.Mappings;
using ModuleName.Domain.Repositories;

namespace ModuleName.Application.Features.CreateEntity;

/// <summary>
/// Handler responsável por criar uma nova entidade
/// </summary>
public class CreateEntityCommandHandler : IRequestHandler<CreateEntityCommand, EntityOutputDTO>
{
    private readonly IEntityRepository _repository;

    public CreateEntityCommandHandler(IEntityRepository repository)
    {
        _repository = repository;
    }

    public async Task<EntityOutputDTO> Handle(
        CreateEntityCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Converter DTO → Entity (com validações de domínio)
        var entity = request.Dto.ToEntity(request.CreatedBy);

        // 2. Regras de negócio adicionais (se necessário)
        // Exemplo: Verificar duplicatas
        var exists = await _repository.ExistsByCodeAsync(entity.Code, cancellationToken);
        if (exists)
        {
            throw new BusinessException("Entidade com este código já existe");
        }

        // 3. Persistir no banco
        var savedEntity = await _repository.AddAsync(entity, cancellationToken);

        // 4. Converter Entity → DTO de saída
        return savedEntity.ToDto<EntityOutputDTO>();
    }
}
```

#### ✅ **Boas Práticas**:
- ✅ **Single Responsibility**: Handler faz apenas UMA coisa
- ✅ **Async/Await**: Sempre operações assíncronas
- ✅ **CancellationToken**: Passado para todas operações I/O
- ✅ **Mapeamentos claros**: DTO → Entity → DTO
- ✅ **Validações de negócio**: No handler ou na entidade
- ✅ **Exceções tipadas**: `BusinessException`, `NotFoundException`
- ✅ **Dependency Injection**: Repositórios via construtor

#### ❌ **Evitar**:
- ❌ Múltiplas responsabilidades no handler
- ❌ Lógica duplicada (criar métodos reutilizáveis)
- ❌ Acesso direto ao DbContext
- ❌ Try-catch genéricos (deixe exceções propagarem)
- ❌ Retornar `null` (lance exceção ou retorne `Result<T>`)

---

### 3. Criar o Validator (FluentValidation)

**Localização**: `Features/CreateEntity/CreateEntityCommandValidator.cs`

```csharp
using FluentValidation;

namespace ModuleName.Application.Features.CreateEntity;

/// <summary>
/// Validador para CreateEntityCommand
/// </summary>
public class CreateEntityCommandValidator : AbstractValidator<CreateEntityCommand>
{
    public CreateEntityCommandValidator()
    {
        // Validações do DTO
        RuleFor(x => x.Dto.Code)
            .NotEmpty().WithMessage("Código é obrigatório")
            .MaximumLength(50).WithMessage("Código não pode exceder 50 caracteres")
            .Matches("^[A-Z0-9_]+$").WithMessage("Código deve conter apenas letras maiúsculas, números e underscore");

        RuleFor(x => x.Dto.Description)
            .NotEmpty().WithMessage("Descrição é obrigatória")
            .MaximumLength(255).WithMessage("Descrição não pode exceder 255 caracteres");

        RuleFor(x => x.Dto.Value)
            .GreaterThanOrEqualTo(0).WithMessage("Valor deve ser maior ou igual a zero")
            .When(x => x.Dto.Value.HasValue);

        // Validação condicional
        RuleFor(x => x.Dto.Category)
            .NotEmpty().WithMessage("Categoria é obrigatória quando o tipo é 'PREMIUM'")
            .When(x => x.Dto.Type == "PREMIUM");

        // Validações customizadas
        RuleFor(x => x.Dto.StartDate)
            .LessThan(x => x.Dto.EndDate)
            .WithMessage("Data de início deve ser anterior à data de fim")
            .When(x => x.Dto.EndDate.HasValue);
    }
}
```

#### ✅ **Boas Práticas**:
- ✅ **Mensagens descritivas**: Clareza para o usuário
- ✅ **Validações condicionais**: `.When()`
- ✅ **Validações complexas**: Use `.Must()` com métodos helper
- ✅ **Validações cross-field**: Comparar campos entre si
- ✅ **Reutilização**: Criar `AbstractValidator` base se necessário

#### ❌ **Evitar**:
- ❌ Validações de negócio (pertencem ao Handler ou Entity)
- ❌ Acesso a banco de dados (validações síncronas)
- ❌ Mensagens genéricas ("Campo inválido")
- ❌ Validações duplicadas (uma vez é suficiente)

---

## 🏗️ Exemplos Completos

### Exemplo 1: Create Command

```csharp
// CreateParameterCommand.cs
public record CreateParameterCommand(
    CreateParameterInputDTO Dto,
    string? CreatedBy
) : IRequest<ParameterOutputDTO>;

// CreateParameterCommandHandler.cs
public class CreateParameterCommandHandler : IRequestHandler<CreateParameterCommand, ParameterOutputDTO>
{
    private readonly IPara1Repository _para1Repository;

    public CreateParameterCommandHandler(IPara1Repository para1Repository)
    {
        _para1Repository = para1Repository;
    }

    public async Task<ParameterOutputDTO> Handle(
        CreateParameterCommand request,
        CancellationToken cancellationToken)
    {
        // Converter DTO para Entity
        var para1 = request.Dto.ToEntity(request.CreatedBy);

        // Persistir
        var savedPara1 = await _para1Repository.AddAsync(para1, cancellationToken);

        // Retornar DTO
        return savedPara1.ToDto<ParameterOutputDTO>();
    }
}

// CreateParameterCommandValidator.cs
public class CreateParameterCommandValidator : AbstractValidator<CreateParameterCommand>
{
    public CreateParameterCommandValidator()
    {
        RuleFor(x => x.Dto.Descricao)
            .NotEmpty().WithMessage("A Descrição é obrigatória")
            .MaximumLength(100).WithMessage("A Descrição não pode exceder 100 caracteres");

        RuleFor(x => x.Dto.Valor)
            .NotEmpty().WithMessage("O Valor é obrigatório");

        RuleFor(x => x.Dto.Dec)
            .NotNull().WithMessage("Dec é obrigatório para parâmetros do tipo Numérico")
            .When(x => x.Dto.Tipo == "N");
    }
}
```

---

### Exemplo 2: Update Command

```csharp
// UpdateParameterCommand.cs
public record UpdateParameterCommand(
    string Para1Stamp,
    UpdateParameterInputDTO Dto,
    string? UpdatedBy
) : IRequest<ParameterOutputDTO>;

// UpdateParameterCommandHandler.cs
public class UpdateParameterCommandHandler : IRequestHandler<UpdateParameterCommand, ParameterOutputDTO>
{
    private readonly IPara1Repository _para1Repository;

    public UpdateParameterCommandHandler(IPara1Repository para1Repository)
    {
        _para1Repository = para1Repository;
    }

    public async Task<ParameterOutputDTO> Handle(
        UpdateParameterCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Buscar entidade existente
        var para1 = await _para1Repository.GetByStampAsync(request.Para1Stamp, cancellationToken);
        if (para1 is null)
        {
            throw new NotFoundException($"Parameter {request.Para1Stamp} not found");
        }

        // 2. Atualizar a entidade (método de domínio)
        para1.UpdateEntity(request.Dto, request.UpdatedBy);

        // 3. Persistir mudanças
        await _para1Repository.UpdateAsync(para1, cancellationToken);

        // 4. Retornar DTO atualizado
        return para1.ToDto<ParameterOutputDTO>();
    }
}
```

---

### Exemplo 3: Delete Command

```csharp
// DeleteParameterCommand.cs
public record DeleteParameterCommand(
    string Para1Stamp
) : IRequest<bool>;  // ← Retorna bool (sucesso/falha)

// DeleteParameterCommandHandler.cs
public class DeleteParameterCommandHandler : IRequestHandler<DeleteParameterCommand, bool>
{
    private readonly IPara1Repository _para1Repository;

    public DeleteParameterCommandHandler(IPara1Repository para1Repository)
    {
        _para1Repository = para1Repository;
    }

    public async Task<bool> Handle(
        DeleteParameterCommand request,
        CancellationToken cancellationToken)
    {
        var para1 = await _para1Repository.GetByStampAsync(request.Para1Stamp, cancellationToken);
        if (para1 is null)
            return false;

        await _para1Repository.DeleteAsync(para1, cancellationToken);
        return true;
    }
}
```

---

## 🔗 Integração com Controller

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

## 🧪 Testes

### Unit Test do Handler

```csharp
[Theory, AutoMoqData]
public async Task Handle_ValidCommand_ShouldCreateEntity(
    [Frozen] Mock<IEntityRepository> repositoryMock,
    CreateEntityCommand command,
    Entity entity)
{
    // Arrange
    repositoryMock
        .Setup(x => x.AddAsync(It.IsAny<Entity>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(entity);

    var sut = new CreateEntityCommandHandler(repositoryMock.Object);

    // Act
    var result = await sut.Handle(command, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.Stamp.Should().NotBeEmpty();
    repositoryMock.Verify(x => x.AddAsync(It.IsAny<Entity>(), It.IsAny<CancellationToken>()), Times.Once);
}
```

### Teste do Validator

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
```

---

## 🎯 Checklist de Qualidade

Antes de finalizar um Command, verifique:

- [ ] **Command é um `record` imutável**
- [ ] **Handler implementa `IRequestHandler<TCommand, TResponse>`**
- [ ] **Validator herda de `AbstractValidator<TCommand>`**
- [ ] **Todas operações são assíncronas** (`async/await`)
- [ ] **CancellationToken é passado para operações I/O**
- [ ] **Mapeamentos DTO ↔ Entity estão corretos**
- [ ] **Validações de negócio estão no lugar correto** (Handler ou Entity)
- [ ] **Exceções tipadas são lançadas** (não genéricas)
- [ ] **Auditoria está sendo capturada** (`CreatedBy`, `UpdatedBy`)
- [ ] **Handler tem testes unitários** (cobertura > 80%)
- [ ] **Validator tem testes** (casos válidos e inválidos)
- [ ] **Documentação XML está presente**

---

## 📚 Referências

- [CQRS Pattern - Martin Fowler](https://martinfowler.com/bliki/CQRS.html)
- [MediatR Documentation](https://github.com/jbogard/MediatR/wiki)
- [FluentValidation Documentation](https://docs.fluentvalidation.net/)
- [Módulo de Referência: Parameters](../../src/Modules/Parameters/)

---

**Ver também**:
- [QueryPattern.md](./QueryPattern.md) - Operações de leitura
- [ValidatorPattern.md](./ValidatorPattern.md) - Validações detalhadas
- [EntityPattern.md](./EntityPattern.md) - Entidades de domínio
- [ControllerPattern.md](./ControllerPattern.md) - Integração com API
