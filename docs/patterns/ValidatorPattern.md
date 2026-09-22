# ✅ Validator Pattern (FluentValidation)

> **Nível**: Senior/Enterprise  
> **Camada**: Application Layer  
> **Responsabilidade**: Validação de dados de entrada com regras claras e reutilizáveis

---

## 🎯 Objetivo

Validators usando **FluentValidation** fornecem validação declarativa, testável e expressiva para Commands e Queries, garantindo que dados inválidos nunca cheguem ao domínio.

---

## 📋 Estrutura

```
ModuleName.Application/
└── Features/
    └── CreateEntity/
        ├── CreateEntityCommand.cs
        ├── CreateEntityCommandHandler.cs
        └── CreateEntityCommandValidator.cs  ← Validator
```

---

## 🔧 Implementação Passo a Passo

### 1. Criar o Validator

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
        // Campo obrigatório
        RuleFor(x => x.Dto.Code)
            .NotEmpty().WithMessage("Código é obrigatório")
            .MaximumLength(50).WithMessage("Código não pode exceder 50 caracteres");

        // String com regex
        RuleFor(x => x.Dto.Code)
            .Matches("^[A-Z0-9_]+$")
            .WithMessage("Código deve conter apenas letras maiúsculas, números e underscore");

        // String com tamanho
        RuleFor(x => x.Dto.Description)
            .NotEmpty().WithMessage("Descrição é obrigatória")
            .MinimumLength(3).WithMessage("Descrição deve ter pelo menos 3 caracteres")
            .MaximumLength(255).WithMessage("Descrição não pode exceder 255 caracteres");

        // Número com range
        RuleFor(x => x.Dto.Value)
            .GreaterThanOrEqualTo(0).WithMessage("Valor deve ser maior ou igual a zero")
            .LessThanOrEqualTo(1000000).WithMessage("Valor não pode exceder 1.000.000")
            .When(x => x.Dto.Value.HasValue);

        // Email
        RuleFor(x => x.Dto.Email)
            .NotEmpty().WithMessage("Email é obrigatório")
            .EmailAddress().WithMessage("Email inválido")
            .When(x => !string.IsNullOrWhiteSpace(x.Dto.Email));
    }
}
```

---

## 🎨 Regras de Validação Comuns

### Strings

```csharp
// Obrigatório
RuleFor(x => x.Name)
    .NotEmpty().WithMessage("Nome é obrigatório");

// Tamanho
RuleFor(x => x.Name)
    .MinimumLength(3).WithMessage("Nome deve ter pelo menos 3 caracteres")
    .MaximumLength(100).WithMessage("Nome não pode exceder 100 caracteres");

// Regex
RuleFor(x => x.Code)
    .Matches("^[A-Z0-9_]+$").WithMessage("Código inválido");

// Email
RuleFor(x => x.Email)
    .EmailAddress().WithMessage("Email inválido");

// URL
RuleFor(x => x.Website)
    .Must(BeAValidUrl).WithMessage("URL inválida");

private bool BeAValidUrl(string? url)
{
    return Uri.TryCreate(url, UriKind.Absolute, out _);
}
```

### Números

```csharp
// Range
RuleFor(x => x.Age)
    .InclusiveBetween(18, 120).WithMessage("Idade deve estar entre 18 e 120");

// Maior que
RuleFor(x => x.Quantity)
    .GreaterThan(0).WithMessage("Quantidade deve ser maior que zero");

// Maior ou igual
RuleFor(x => x.Price)
    .GreaterThanOrEqualTo(0).WithMessage("Preço não pode ser negativo");

// Precisão decimal
RuleFor(x => x.Amount)
    .PrecisionScale(10, 2, true)
    .WithMessage("Valor deve ter no máximo 10 dígitos com 2 casas decimais");
```

### Datas

```csharp
// Data futura
RuleFor(x => x.StartDate)
    .GreaterThan(DateTime.Now).WithMessage("Data de início deve ser futura");

// Data passada
RuleFor(x => x.BirthDate)
    .LessThan(DateTime.Now).WithMessage("Data de nascimento deve ser passada");

// Comparação entre datas
RuleFor(x => x.EndDate)
    .GreaterThan(x => x.StartDate)
    .WithMessage("Data de fim deve ser posterior à data de início");
```

### Coleções

```csharp
// Não vazia
RuleFor(x => x.Items)
    .NotEmpty().WithMessage("Lista de itens não pode estar vazia");

// Quantidade
RuleFor(x => x.Items)
    .Must(x => x.Count() <= 100)
    .WithMessage("Máximo de 100 itens permitidos");

// Validar cada item
RuleForEach(x => x.Items)
    .SetValidator(new OrderItemValidator());
```

---

## 🔄 Validações Condicionais

### When (Quando)

```csharp
// Validar somente quando condição é verdadeira
RuleFor(x => x.Dto.Category)
    .NotEmpty().WithMessage("Categoria é obrigatória para produtos premium")
    .When(x => x.Dto.Type == "PREMIUM");

// Validar somente quando valor não é nulo
RuleFor(x => x.Dto.Discount)
    .GreaterThan(0).WithMessage("Desconto deve ser maior que zero")
    .LessThanOrEqualTo(100).WithMessage("Desconto não pode exceder 100%")
    .When(x => x.Dto.Discount.HasValue);
```

### Unless (A menos que)

```csharp
RuleFor(x => x.Dto.TaxId)
    .NotEmpty().WithMessage("CPF/CNPJ é obrigatório")
    .Unless(x => x.Dto.IsForeign);
```

---

## 🏗️ Validações Complexas

### Validação Customizada com Must

```csharp
public class CreateEntityCommandValidator : AbstractValidator<CreateEntityCommand>
{
    public CreateEntityCommandValidator()
    {
        RuleFor(x => x.Dto.Code)
            .Must(BeUniqueCode).WithMessage("Código já existe");

        RuleFor(x => x.Dto.Document)
            .Must(BeValidCPF).WithMessage("CPF inválido")
            .When(x => x.Dto.DocumentType == "CPF");
    }

    private bool BeUniqueCode(string code)
    {
        // ⚠️ Evitar acesso ao banco aqui (operação síncrona)
        // Melhor: validar no Handler
        return !_existingCodes.Contains(code);
    }

    private bool BeValidCPF(string? cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf)) return false;
        
        // Lógica de validação de CPF
        cpf = cpf.Replace(".", "").Replace("-", "");
        if (cpf.Length != 11) return false;
        
        // ... restante da validação
        return true;
    }
}
```

### Validação Assíncrona (Evitar se possível)

```csharp
public class CreateEntityCommandValidator : AbstractValidator<CreateEntityCommand>
{
    private readonly IEntityRepository _repository;

    public CreateEntityCommandValidator(IEntityRepository repository)
    {
        _repository = repository;

        RuleFor(x => x.Dto.Code)
            .MustAsync(BeUniqueCodeAsync)
            .WithMessage("Código já existe");
    }

    private async Task<bool> BeUniqueCodeAsync(string code, CancellationToken ct)
    {
        return !await _repository.ExistsByCodeAsync(code, ct);
    }
}
```

**⚠️ Nota**: Validações assíncronas impactam performance. Prefira validar no Handler.

---

## 🔁 Validadores Reutilizáveis

### Validator Base Comum

```csharp
public abstract class EntityValidatorBase<T> : AbstractValidator<T>
{
    protected void RuleForStamp(Expression<Func<T, string>> expression)
    {
        RuleFor(expression)
            .NotEmpty().WithMessage("Stamp é obrigatório")
            .Length(25).WithMessage("Stamp deve ter 25 caracteres");
    }

    protected void RuleForCode(Expression<Func<T, string>> expression)
    {
        RuleFor(expression)
            .NotEmpty().WithMessage("Código é obrigatório")
            .MaximumLength(50).WithMessage("Código não pode exceder 50 caracteres")
            .Matches("^[A-Z0-9_]+$").WithMessage("Código deve conter apenas letras maiúsculas, números e underscore");
    }
}

// Uso
public class CreateEntityCommandValidator : EntityValidatorBase<CreateEntityCommand>
{
    public CreateEntityCommandValidator()
    {
        RuleForCode(x => x.Dto.Code);
        // ... outras validações
    }
}
```

### Validator de Child Objects

```csharp
public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.Dto.CustomerCode)
            .NotEmpty().WithMessage("Código do cliente é obrigatório");

        RuleFor(x => x.Dto.Items)
            .NotEmpty().WithMessage("Pedido deve ter pelo menos um item");

        // Validar cada item da coleção
        RuleForEach(x => x.Dto.Items)
            .SetValidator(new OrderItemValidator());
    }
}

public class OrderItemValidator : AbstractValidator<CreateOrderItemInputDTO>
{
    public OrderItemValidator()
    {
        RuleFor(x => x.ProductCode)
            .NotEmpty().WithMessage("Código do produto é obrigatório");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantidade deve ser maior que zero");

        RuleFor(x => x.UnitPrice)
            .GreaterThan(0).WithMessage("Preço unitário deve ser maior que zero");
    }
}
```

---

## 🧪 Testes de Validators

### Teste de Validação Bem-Sucedida

```csharp
[Fact]
public async Task Validate_ValidCommand_ShouldPass()
{
    // Arrange
    var validator = new CreateEntityCommandValidator();
    var command = new CreateEntityCommand(
        new CreateEntityInputDTO
        {
            Code = "VALID_CODE",
            Description = "Valid Description",
            Value = 100
        },
        "testuser"
    );

    // Act
    var result = await validator.ValidateAsync(command);

    // Assert
    result.IsValid.Should().BeTrue();
    result.Errors.Should().BeEmpty();
}
```

### Teste de Validação com Falha

```csharp
[Theory]
[InlineData("")]
[InlineData(null)]
[InlineData("   ")]
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
    result.Errors.Should().ContainSingle(e => e.PropertyName == "Dto.Code");
    result.Errors.First().ErrorMessage.Should().Be("Código é obrigatório");
}
```

### Teste de Validação Condicional

```csharp
[Fact]
public async Task Validate_PremiumType_ShouldRequireCategory()
{
    // Arrange
    var validator = new CreateEntityCommandValidator();
    var command = new CreateEntityCommand(
        new CreateEntityInputDTO
        {
            Code = "CODE",
            Type = "PREMIUM",
            Category = "" // ← Vazio
        },
        "testuser"
    );

    // Act
    var result = await validator.ValidateAsync(command);

    // Assert
    result.IsValid.Should().BeFalse();
    result.Errors.Should().Contain(e => 
        e.PropertyName == "Dto.Category" && 
        e.ErrorMessage.Contains("obrigatória"));
}
```

---

## 🎯 Checklist de Qualidade

Antes de finalizar um Validator, verifique:

- [ ] **Herda de `AbstractValidator<TCommand>`**
- [ ] **Todas validações têm `.WithMessage()` descritiva**
- [ ] **Validações condicionais usam `.When()` ou `.Unless()`**
- [ ] **Sem acesso ao banco (validações síncronas)**
- [ ] **Validações complexas isoladas em métodos privados**
- [ ] **Child objects validados com `SetValidator()`**
- [ ] **Coleções validadas com `RuleForEach()`**
- [ ] **Testes para casos válidos**
- [ ] **Testes para casos inválidos**
- [ ] **Testes para validações condicionais**
- [ ] **Documentação XML na classe**

---

## 📚 Distinção: Validator vs Domain Validation

| Aspecto | FluentValidation | Domain Validation |
|---------|------------------|-------------------|
| **Localização** | Application Layer | Domain Layer (Entity) |
| **Objetivo** | Validar input | Manter invariantes |
| **Execução** | Antes do Handler | No construtor/métodos da Entity |
| **Exemplo** | "Email é obrigatório" | "Stamp não pode ser vazio" |
| **Acesso DB** | ❌ Evitar | ❌ Nunca |
| **Contexto** | Request do usuário | Regras de negócio |

---

## 📚 Referências

- [FluentValidation Documentation](https://docs.fluentvalidation.net/)
- [Validation in ASP.NET Core](https://docs.microsoft.com/aspnet/core/mvc/models/validation)
- [Módulo de Referência: Parameters](../../src/Modules/Parameters/)

---

**Ver também**:
- [CommandPattern.md](./CommandPattern.md) - Uso de validators
- [DTOPattern.md](./DTOPattern.md) - DTOs validados
- [EntityPattern.md](./EntityPattern.md) - Validações de domínio
- [BehaviorPattern.md](./BehaviorPattern.md) - ValidationBehavior
