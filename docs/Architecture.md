# 🏛️ Enterprise Architecture Guide

> **Target Level**: Senior/Enterprise  
> **Quality Standards**: High Integrity | High Scalability | High Security

Este documento define a arquitetura e padrões para desenvolvimento de módulos no PHCAPI seguindo **Clean Architecture** e princípios **SOLID** com qualidade enterprise.

---

## 📋 Índice

1. [Visão Geral da Arquitetura](#visão-geral-da-arquitetura)
2. [Camadas da Aplicação](#camadas-da-aplicação)
3. [Padrões Arquiteturais](#padrões-arquiteturais)
4. [Guias de Implementação](#guias-de-implementação)
5. [Princípios e Melhores Práticas](#princípios-e-melhores-práticas)

---

## 🎯 Visão Geral da Arquitetura

O PHCAPI utiliza **Clean Architecture** (Onion Architecture) com separação clara de responsabilidades em 4 camadas concêntricas:

```
┌─────────────────────────────────────────────────────┐
│              Presentation Layer                     │
│         (Controllers, GraphQL, APIs)                │
│  ┌──────────────────────────────────────────────┐  │
│  │         Application Layer                    │  │
│  │    (Use Cases, Commands, Queries, DTOs)     │  │
│  │  ┌────────────────────────────────────────┐ │  │
│  │  │        Domain Layer                    │ │  │
│  │  │  (Entities, Value Objects, Rules)     │ │  │
│  │  └────────────────────────────────────────┘ │  │
│  └──────────────────────────────────────────────┘  │
│                                                     │
│              Infrastructure Layer                   │
│      (Databases, External Services, I/O)           │
└─────────────────────────────────────────────────────┘
```

### Direção das Dependências

**Regra de Ouro**: As dependências sempre apontam para DENTRO (Domain é o centro).

- ✅ **Presentation** → Application → Domain
- ✅ **Infrastructure** → Domain (através de interfaces)
- ❌ **Domain** → Nunca depende de camadas externas

---

## 🏗️ Camadas da Aplicação

### 1. **Domain Layer** (Núcleo da Aplicação)

**Responsabilidade**: Lógica de negócio pura, independente de tecnologia

```
ModuleName.Domain/
├── Entities/           ← Entidades de domínio ricas
├── ValueObjects/       ← Objetos de valor imutáveis
├── Repositories/       ← Interfaces de repositórios
├── Exceptions/         ← Exceções específicas do domínio
└── Events/             ← Eventos de domínio
```

**Características**:
- ✅ Zero dependências externas
- ✅ Lógica de negócio pura
- ✅ Entities com comportamentos
- ✅ Validações de domínio
- ❌ Sem acesso a banco de dados
- ❌ Sem DTOs ou mapeamentos

📚 **Guia**: [EntityPattern.md](./patterns/EntityPattern.md)

---

### 2. **Application Layer** (Casos de Uso)

**Responsabilidade**: Orquestração de casos de uso usando CQRS

```
ModuleName.Application/
├── Features/           ← Organizado por funcionalidade
│   ├── CreateEntity/
│   │   ├── CreateEntityCommand.cs
│   │   ├── CreateEntityCommandHandler.cs
│   │   └── CreateEntityCommandValidator.cs
│   └── GetEntity/
│       ├── GetEntityQuery.cs
│       └── GetEntityQueryHandler.cs
├── DTOs/              ← Data Transfer Objects
├── Mappings/          ← Entity ↔ DTO conversões
├── Behaviors/         ← Pipeline behaviors (Logging, Validation)
└── DependencyInjection.cs
```

**Características**:
- ✅ CQRS (Commands & Queries separados)
- ✅ Validação com FluentValidation
- ✅ MediatR para desacoplamento
- ✅ Pipeline behaviors
- ❌ Sem dependência de EF Core diretamente

📚 **Guias**:
- [CommandPattern.md](./patterns/CommandPattern.md)
- [QueryPattern.md](./patterns/QueryPattern.md)
- [DTOPattern.md](./patterns/DTOPattern.md)
- [ValidatorPattern.md](./patterns/ValidatorPattern.md)
- [BehaviorPattern.md](./patterns/BehaviorPattern.md)
- [MappingPattern.md](./patterns/MappingPattern.md)

---

### 3. **Infrastructure Layer** (Implementações Técnicas)

**Responsabilidade**: Implementações concretas de persistência e serviços externos

```
ModuleName.Infrastructure/
├── Persistence/
│   ├── ModuleDbContextEFCore.cs      ← DbContext EF Core
│   └── Configurations/
│       └── EntityConfigurationEFCore.cs
├── Repositories/
│   └── EntityRepositoryEFCore.cs     ← Implementação do repositório
├── ExternalServices/                  ← APIs externas, Message Queues
└── DependencyInjection.cs
```

**Características**:
- ✅ Sufixo `EFCore` nas classes relacionadas a EF Core
- ✅ Implementa interfaces do Domain
- ✅ Auditoria automática via Interceptors
- ✅ Fácil substituir tecnologias (Dapper, MongoDB, etc.)
- ⚠️ Database First approach (tabelas já existem)

📚 **Guias**:
- [InfrastructurePattern.md](./patterns/InfrastructurePattern.md)
- [RepositoryPattern.md](./patterns/RepositoryPattern.md)

---

### 4. **Presentation Layer** (APIs e Endpoints)

**Responsabilidade**: Expor funcionalidades via protocolos (REST, GraphQL, gRPC)

```
ModuleName.Presentation/
├── REST/
│   ├── Controllers/
│   │   └── EntitiesController.cs
│   └── RestDependencyInjection.cs
├── GraphQL/                          ← Preparado para o futuro
│   ├── Queries/
│   ├── Mutations/
│   └── GraphQLDependencyInjection.cs
└── DependencyInjection.cs            ← Entry point
```

**Características**:
- ✅ Controllers stateless e finos
- ✅ Apenas chama MediatR
- ✅ Autorização e Rate Limiting
- ✅ Documentação XML para Swagger
- ✅ Múltiplos protocolos suportados

📚 **Guias**:
- [ControllerPattern.md](./patterns/ControllerPattern.md)
- [DependencyInjectionPattern.md](./patterns/DependencyInjectionPattern.md)

---

## 🎨 Padrões Arquiteturais

### CQRS (Command Query Responsibility Segregation)

Separação clara entre operações de leitura e escrita:

- **Commands**: Alteram estado (Create, Update, Delete)
- **Queries**: Apenas leitura (Get, GetAll)

**Benefícios**:
- ✅ Otimização independente de leitura/escrita
- ✅ Escalabilidade separada
- ✅ Código mais claro e testável

---

### Repository Pattern

Abstração do acesso a dados:

```csharp
// Domain/Repositories/IEntityRepository.cs
public interface IEntityRepository
{
    Task<Entity?> GetByIdAsync(int id, CancellationToken ct);
    Task<Entity> AddAsync(Entity entity, CancellationToken ct);
}

// Infrastructure/Repositories/EntityRepositoryEFCore.cs
public class EntityRepositoryEFCore : IEntityRepository
{
    // Implementação EF Core
}
```

**Benefícios**:
- ✅ Testável (mocking fácil)
- ✅ Mudança de tecnologia sem impacto
- ✅ Abstração de queries complexas

---

### Dependency Injection

Inversão de controle e injeção de dependências em todas as camadas:

```csharp
// Application
services.AddScoped<IEntityRepository, EntityRepositoryEFCore>();

// MediatR
services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(assembly);
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
    cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
});
```

---

### Pipeline Behaviors

Aspectos transversais aplicados automaticamente:

- **ValidationBehavior**: Validação automática via FluentValidation
- **LoggingBehavior**: Log de requisições e performance
- **TransactionBehavior**: Transações automáticas
- **CachingBehavior**: Cache de queries

---

## 📚 Guias de Implementação

### Por Tipo de Componente

| Componente | Guia | Camada |
|------------|------|--------|
| **Entidades de Domínio** | [EntityPattern.md](./patterns/EntityPattern.md) | Domain |
| **Comandos (Create, Update, Delete)** | [CommandPattern.md](./patterns/CommandPattern.md) | Application |
| **Queries (Get, GetAll)** | [QueryPattern.md](./patterns/QueryPattern.md) | Application |
| **Validadores** | [ValidatorPattern.md](./patterns/ValidatorPattern.md) | Application |
| **DTOs** | [DTOPattern.md](./patterns/DTOPattern.md) | Application |
| **Mapeamentos** | [MappingPattern.md](./patterns/MappingPattern.md) | Application |
| **Behaviors** | [BehaviorPattern.md](./patterns/BehaviorPattern.md) | Application |
| **Repositórios** | [RepositoryPattern.md](./patterns/RepositoryPattern.md) | Domain + Infrastructure |
| **Controllers REST** | [ControllerPattern.md](./patterns/ControllerPattern.md) | Presentation |
| **Infraestrutura (EF Core)** | [InfrastructurePattern.md](./patterns/InfrastructurePattern.md) | Infrastructure |
| **Dependency Injection** | [DependencyInjectionPattern.md](./patterns/DependencyInjectionPattern.md) | Todas |
| **Testes** | [TestingPattern.md](./patterns/TestingPattern.md) | Tests |

---

## 🎯 Princípios e Melhores Práticas

### SOLID Principles

#### **S - Single Responsibility Principle**
- Cada classe tem uma única responsabilidade
- Handlers fazem apenas uma coisa
- Controllers apenas roteiam para MediatR

#### **O - Open/Closed Principle**
- Aberto para extensão (Behaviors, Validators)
- Fechado para modificação (Interfaces estáveis)

#### **L - Liskov Substitution Principle**
- Implementações de repositórios são intercambiáveis
- EFCore ↔ Dapper ↔ MongoDB

#### **I - Interface Segregation Principle**
- Interfaces pequenas e específicas
- `IEntityRepository` não força métodos desnecessários

#### **D - Dependency Inversion Principle**
- Domain define interfaces
- Infrastructure implementa
- Injeção via DI

---

### Evitar Acoplamento

❌ **Ruim**:
```csharp
public class OrderService
{
    private SqlConnection _connection; // Acoplado ao SQL Server
}
```

✅ **Bom**:
```csharp
public class CreateOrderCommandHandler
{
    private readonly IOrderRepository _repository; // Interface
}
```

---

### Reutilização de Código

#### Shared Kernel

```
Shared/
├── Shared.Abstractions/     ← Interfaces comuns
│   └── Entities/
│       └── AuditableEntity.cs
├── Shared.Infrastructure/   ← Implementações comuns
│   └── Interceptors/
│       └── AuditableEntityInterceptorEFCore.cs
└── Shared.Kernel/           ← Utilities
    └── Extensions/
        └── StampGenerator.cs
```

**Exemplo**: Auditoria automática reutilizada por TODOS os módulos.

---

### Segurança Enterprise

#### Autenticação e Autorização

```csharp
[Authorize(Roles = AppRoles.Administrator)]
[EnableRateLimiting("entity-create")]
public async Task<IActionResult> Create(...)
```

#### Validação de Entrada

```csharp
RuleFor(x => x.Email)
    .NotEmpty()
    .EmailAddress()
    .MaximumLength(255);
```

#### Auditoria

```csharp
public class Entity : AuditableEntity
{
    // CreatedAt, UpdatedAt, CreatedBy, UpdatedBy automático
}
```

---

### Escalabilidade

#### Async/Await em Tudo

```csharp
public async Task<Entity> Handle(GetEntityQuery request, CancellationToken ct)
{
    return await _repository.GetByIdAsync(request.Id, ct);
}
```

#### Paginação Sempre

```csharp
Task<IEnumerable<Entity>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken ct);
```

#### Caching Estratégico

```csharp
[OutputCache(Duration = 300)] // 5 minutos
public async Task<IActionResult> GetAll(...)
```

---

### Performance

#### Mapeamentos Eficientes

- **Mapster** para mapeamentos rápidos (até 10x mais rápido que AutoMapper)
- Extension methods para clareza

#### Queries Otimizadas

```csharp
// Projeção direta para DTO
return await _context.Entities
    .Select(e => new EntityDto { ... })
    .ToListAsync(ct);
```

#### Índices no Banco

- Stamp columns (PK)
- Foreign Keys
- Campos de busca frequente

---

## 🔄 Fluxo de Requisição Completo

### Exemplo: Criar uma Entidade

```
1. HTTP POST /api/entities
   ↓
2. EntitiesController.Create()
   ↓
3. _mediator.Send(new CreateEntityCommand(...))
   ↓
4. ValidationBehavior valida o comando
   ↓
5. LoggingBehavior loga início da requisição
   ↓
6. CreateEntityCommandHandler.Handle()
   ├─ Mapeia DTO → Entity
   ├─ Chama _repository.AddAsync()
   │  ↓
   │  Infrastructure: EntityRepositoryEFCore
   │  ├─ AuditableEntityInterceptor (adiciona CreatedAt, CreatedBy)
   │  └─ SaveChangesAsync()
   ↓
7. Retorna Entity para Handler
   ↓
8. Handler mapeia Entity → DTO
   ↓
9. Controller retorna 201 Created com DTO
```

---

## 📐 Módulo de Referência

O **Parameters Module** é o módulo de referência (golden standard) que implementa TODOS os padrões desta arquitetura.

**Localização**: `src/Modules/Parameters/`

**Usar como base para**:
- Criar novos módulos
- Revisar implementações existentes
- Aprender padrões corretos

---

## 🔗 Links Rápidos

- [Criar um Comando](./patterns/CommandPattern.md)
- [Criar uma Query](./patterns/QueryPattern.md)
- [Criar uma Entidade](./patterns/EntityPattern.md)
- [Criar um Validador](./patterns/ValidatorPattern.md)
- [Criar um Controller](./patterns/ControllerPattern.md)
- [Configurar Repositório](./patterns/RepositoryPattern.md)
- [Escrever Testes](./patterns/TestingPattern.md)

---

## 📖 Referências

- [Clean Architecture - Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [CQRS Pattern - Martin Fowler](https://martinfowler.com/bliki/CQRS.html)
- [Domain-Driven Design - Eric Evans](https://domainlanguage.com/ddd/)
- [SOLID Principles](https://en.wikipedia.org/wiki/SOLID)

---

**Última atualização**: 2026-02-18  
**Versão**: 1.0  
**Autor**: PHC Development Team
