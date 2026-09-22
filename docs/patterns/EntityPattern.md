# 🏛️ Entity Pattern (Domain Layer)

> **Nível**: Senior/Enterprise  
> **Camada**: Domain Layer  
> **Responsabilidade**: Representar conceitos de negócio com **comportamento e regras de domínio**

---

## 🎯 Objetivo

Entidades são o **coração do Domain Layer**: objetos ricos com identidade única, comportamentos e regras de negócio encapsuladas. Seguimos **Domain-Driven Design (DDD)** para criar um modelo de domínio expressivo e robusto.

---

## 📋 Características de Entidades

### Propriedades Fundamentais

✅ **Identidade Única**: Cada entidade tem um identificador único (Stamp, ID)  
✅ **Comportamento Rico**: Métodos que executam lógica de negócio  
✅ **Encapsulamento**: Setters privados, mudanças via métodos  
✅ **Validações de Domínio**: Regras de negócio na própria entidade  
✅ **Imutabilidade Parcial**: Mudanças controladas por métodos  
✅ **Auditoria**: Herda de `AuditableEntity` para tracking automático

---

## 🔧 Implementação Passo a Passo

### 1. Estrutura Básica

**Localização**: `ModuleName.Domain/Entities/EntityName.cs`

```csharp
using Shared.Abstractions.Entities;

namespace ModuleName.Domain.Entities;

/// <summary>
/// Representa uma entidade de domínio
/// </summary>
public class Entity : AuditableEntity
{
    // 1. Propriedades com setters privados
    public string Stamp { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public decimal? Value { get; private set; }

    // 2. Construtor privado (para EF Core)
    private Entity() { }

    // 3. Construtor público (factory)
    public Entity(
        string stamp,
        string code,
        string description,
        string? createdBy = null,
        decimal? value = null)
    {
        // Validações de domínio
        if (string.IsNullOrWhiteSpace(stamp))
            throw new ArgumentException("Stamp cannot be empty", nameof(stamp));

        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code cannot be empty", nameof(code));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty", nameof(description));

        Stamp = stamp;
        Code = code;
        Description = description;
        Value = value;

        // Auditoria de criação
        SetCreatedAudit(createdBy);
    }

    // 4. Métodos de negócio (comportamento)
    public void Update(string description, decimal? value, string? updatedBy = null)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty", nameof(description));

        Description = description;
        Value = value;

        // Auditoria de atualização
        SetUpdatedAudit(updatedBy);
    }

    public void Activate(string? updatedBy = null)
    {
        IsActive = true;
        SetUpdatedAudit(updatedBy);
    }

    public void Deactivate(string? updatedBy = null)
    {
        IsActive = false;
        SetUpdatedAudit(updatedBy);
    }
}
```

---

## ✅ **Boas Práticas**

### 1. Setters Privados

❌ **Ruim**:
```csharp
public class Entity
{
    public string Code { get; set; } = string.Empty; // ❌ Setter público
}

// Qualquer um pode mudar diretamente
entity.Code = "INVALID";  // ❌ Sem validação
```

✅ **Bom**:
```csharp
public class Entity
{
    public string Code { get; private set; } = string.Empty; // ✅ Setter privado

    public void UpdateCode(string code, string? updatedBy)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code cannot be empty");

        Code = code;
        SetUpdatedAudit(updatedBy);
    }
}
```

---

### 2. Validações de Domínio

Validações complexas na própria entidade:

```csharp
public class Order : AuditableEntity
{
    public string Stamp { get; private set; } = string.Empty;
    public OrderStatus Status { get; private set; }
    public decimal TotalAmount { get; private set; }
    public DateTime OrderDate { get; private set; }
    private List<OrderItem> _items = new();
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    public void AddItem(OrderItem item)
    {
        if (Status == OrderStatus.Completed)
            throw new InvalidOperationException("Cannot add items to completed order");

        if (item.Quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero");

        _items.Add(item);
        RecalculateTotal();
    }

    public void Complete(string? updatedBy)
    {
        if (_items.Count == 0)
            throw new InvalidOperationException("Cannot complete order without items");

        if (Status == OrderStatus.Completed)
            throw new InvalidOperationException("Order already completed");

        Status = OrderStatus.Completed;
        SetUpdatedAudit(updatedBy);
    }

    private void RecalculateTotal()
    {
        TotalAmount = _items.Sum(i => i.UnitPrice * i.Quantity);
    }
}
```

---

### 3. Herança de AuditableEntity

Todas as entidades devem herdar de `AuditableEntity`:

```csharp
// Shared.Abstractions/Entities/AuditableEntity.cs
public abstract class AuditableEntity
{
    public DateTime OUsrData { get; protected set; }    // Created at
    public string OUsrHora { get; protected set; } = string.Empty;  // Created time
    public string? OUsrInis { get; protected set; }     // Created by
    public DateTime? UsrData { get; protected set; }    // Updated at
    public string? UsrHora { get; protected set; }      // Updated time
    public string? UsrInis { get; protected set; }      // Updated by

    protected void SetCreatedAudit(string? createdBy)
    {
        var now = DateTime.Now;
        OUsrData = now;
        OUsrHora = now.ToString("HH:mm:ss");
        OUsrInis = createdBy;
    }

    protected void SetUpdatedAudit(string? updatedBy)
    {
        var now = DateTime.Now;
        UsrData = now;
        UsrHora = now.ToString("HH:mm:ss");
        UsrInis = updatedBy;
    }
}
```

**Benefícios**:
- ✅ Auditoria automática em todas as entidades
- ✅ Tracking de criação e modificação
- ✅ Reutilização de código

---

### 4. Construtor Privado para EF Core

EF Core precisa de um construtor sem parâmetros:

```csharp
public class Entity : AuditableEntity
{
    // Construtor privado (EF Core usa reflection)
    private Entity() { }

    // Construtor público com validações
    public Entity(string stamp, string code, string description, string? createdBy)
    {
        // Validações aqui
    }
}
```

---

### 5. Coleções Encapsuladas

```csharp
public class Aggregate : AuditableEntity
{
    private List<ChildEntity> _children = new();  // ← Lista privada

    // Propriedade pública read-only
    public IReadOnlyCollection<ChildEntity> Children => _children.AsReadOnly();

    public void AddChild(ChildEntity child)
    {
        // Validações
        if (child == null)
            throw new ArgumentNullException(nameof(child));

        _children.Add(child);
    }

    public void RemoveChild(string childStamp)
    {
        var child = _children.FirstOrDefault(c => c.Stamp == childStamp);
        if (child != null)
        {
            _children.Remove(child);
        }
    }
}
```

---

## 🏗️ Exemplos Completos

### Exemplo 1: Entidade Simples (Parameters)

```csharp
using Shared.Abstractions.Entities;

namespace Parameters.Domain.Entities;

public class Para1 : AuditableEntity
{
    public string Para1Stamp { get; private set; } = string.Empty;
    public string Descricao { get; private set; } = string.Empty;
    public string Valor { get; private set; } = string.Empty;
    public string Tipo { get; private set; } = string.Empty;
    public decimal? Dec { get; private set; }
    public decimal? Tam { get; private set; }

    private Para1() { }

    public Para1(
        string para1Stamp,
        string descricao,
        string valor,
        string tipo,
        decimal? dec = null,
        decimal? tam = null,
        string? criadoPor = null)
    {
        if (string.IsNullOrWhiteSpace(para1Stamp))
            throw new ArgumentException("Para1Stamp cannot be empty", nameof(para1Stamp));

        if (string.IsNullOrWhiteSpace(descricao))
            throw new ArgumentException("Descricao cannot be empty", nameof(descricao));

        Para1Stamp = para1Stamp;
        Descricao = descricao;
        Valor = valor ?? string.Empty;
        Tipo = tipo ?? string.Empty;
        Dec = dec;
        Tam = tam;

        SetCreatedAudit(criadoPor);
    }

    public void Update(
        string descricao,
        string valor,
        string tipo,
        int? dec,
        int? tam,
        string? atualizadoPor = null)
    {
        if (string.IsNullOrWhiteSpace(descricao))
            throw new ArgumentException("Descricao cannot be empty", nameof(descricao));

        Descricao = descricao;
        Valor = valor ?? string.Empty;
        Tipo = tipo ?? string.Empty;
        Dec = dec;
        Tam = tam;

        SetUpdatedAudit(atualizadoPor);
    }
}
```

---

### Exemplo 2: Entidade com Relacionamento 1:N

```csharp
public class Order : AuditableEntity
{
    public string OrderStamp { get; private set; } = string.Empty;
    public string CustomerCode { get; private set; } = string.Empty;
    public OrderStatus Status { get; private set; }
    public decimal TotalAmount { get; private set; }

    private List<OrderItem> _items = new();
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    private Order() { }

    public Order(string orderStamp, string customerCode, string? createdBy = null)
    {
        if (string.IsNullOrWhiteSpace(orderStamp))
            throw new ArgumentException("OrderStamp cannot be empty");

        if (string.IsNullOrWhiteSpace(customerCode))
            throw new ArgumentException("CustomerCode cannot be empty");

        OrderStamp = orderStamp;
        CustomerCode = customerCode;
        Status = OrderStatus.Draft;
        TotalAmount = 0;

        SetCreatedAudit(createdBy);
    }

    public void AddItem(string productCode, int quantity, decimal unitPrice)
    {
        if (Status == OrderStatus.Completed)
            throw new InvalidOperationException("Cannot modify completed order");

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero");

        var item = new OrderItem(OrderStamp, productCode, quantity, unitPrice);
        _items.Add(item);
        RecalculateTotal();
    }

    public void RemoveItem(string productCode)
    {
        if (Status == OrderStatus.Completed)
            throw new InvalidOperationException("Cannot modify completed order");

        var item = _items.FirstOrDefault(i => i.ProductCode == productCode);
        if (item != null)
        {
            _items.Remove(item);
            RecalculateTotal();
        }
    }

    public void Submit(string? updatedBy = null)
    {
        if (_items.Count == 0)
            throw new InvalidOperationException("Cannot submit order without items");

        if (Status != OrderStatus.Draft)
            throw new InvalidOperationException("Only draft orders can be submitted");

        Status = OrderStatus.Submitted;
        SetUpdatedAudit(updatedBy);
    }

    public void Complete(string? updatedBy = null)
    {
        if (Status != OrderStatus.Submitted)
            throw new InvalidOperationException("Only submitted orders can be completed");

        Status = OrderStatus.Completed;
        SetUpdatedAudit(updatedBy);
    }

    public void Cancel(string? updatedBy = null)
    {
        if (Status == OrderStatus.Completed)
            throw new InvalidOperationException("Cannot cancel completed order");

        Status = OrderStatus.Cancelled;
        SetUpdatedAudit(updatedBy);
    }

    private void RecalculateTotal()
    {
        TotalAmount = _items.Sum(i => i.TotalPrice);
    }
}

public class OrderItem
{
    public string OrderStamp { get; private set; } = string.Empty;
    public string ProductCode { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal TotalPrice => Quantity * UnitPrice;

    private OrderItem() { }

    public OrderItem(string orderStamp, string productCode, int quantity, decimal unitPrice)
    {
        if (string.IsNullOrWhiteSpace(orderStamp))
            throw new ArgumentException("OrderStamp cannot be empty");

        if (string.IsNullOrWhiteSpace(productCode))
            throw new ArgumentException("ProductCode cannot be empty");

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero");

        OrderStamp = orderStamp;
        ProductCode = productCode;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    public void UpdateQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero");

        Quantity = quantity;
    }
}

public enum OrderStatus
{
    Draft = 0,
    Submitted = 1,
    Completed = 2,
    Cancelled = 3
}
```

---

### Exemplo 3: Value Objects

Objetos sem identidade, definidos apenas por seus valores:

```csharp
namespace ModuleName.Domain.ValueObjects;

/// <summary>
/// Value Object para representar um endereço
/// </summary>
public record Address
{
    public string Street { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string PostalCode { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;

    public Address(string street, string city, string postalCode, string country)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new ArgumentException("Street cannot be empty");

        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City cannot be empty");

        Street = street;
        City = city;
        PostalCode = postalCode;
        Country = country;
    }
}

// Uso em uma entidade
public class Customer : AuditableEntity
{
    public string CustomerStamp { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public Address Address { get; private set; } = null!;

    public void UpdateAddress(Address address)
    {
        Address = address ?? throw new ArgumentNullException(nameof(address));
        SetUpdatedAudit(null);
    }
}
```

---

## 🧪 Testes de Entidades

```csharp
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
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Constructor_EmptyStamp_ShouldThrowException(string stamp)
    {
        // Act
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

## 🎯 Checklist de Qualidade

Antes de finalizar uma Entidade, verifique:

- [ ] **Herda de `AuditableEntity`**
- [ ] **Setters são privados**
- [ ] **Construtor privado sem parâmetros** (para EF Core)
- [ ] **Construtor público com validações**
- [ ] **Métodos de negócio encapsulam mudanças**
- [ ] **Validações de domínio no construtor e métodos**
- [ ] **Coleções são read-only externamente**
- [ ] **Exceções descritivas** (`ArgumentException`, `InvalidOperationException`)
- [ ] **Auditoria chamada** (`SetCreatedAudit`, `SetUpdatedAudit`)
- [ ] **Documentação XML nas propriedades e métodos**
- [ ] **Testes unitários** (construtor, métodos, validações)

---

## 📚 Referências

- [Domain-Driven Design (DDD) - Eric Evans](https://domainlanguage.com/ddd/)
- [Implementing Domain-Driven Design - Vaughn Vernon](https://vaughnvernon.com/)
- [Entity vs Value Object](https://enterprisecraftsmanship.com/posts/entity-vs-value-object-the-ultimate-list-of-differences/)
- [Módulo de Referência: Parameters](../../src/Modules/Parameters/)

---

**Ver também**:
- [RepositoryPattern.md](./RepositoryPattern.md) - Persistência de entidades
- [CommandPattern.md](./CommandPattern.md) - Operações com entidades
- [MappingPattern.md](./MappingPattern.md) - Converter entidades para DTOs
