# 🎭 Parameters.Presentation

Camada de apresentação do módulo Parameters, suportando múltiplos protocolos de API.

---

## 📁 Estrutura

```
Parameters.Presentation/
├── REST/                          # REST API (implementado)
│   ├── Controllers/
│   │   └── ParametersController.cs
│   └── RestDependencyInjection.cs
│
├── GraphQL/                       # GraphQL API (preparado)
│   ├── Queries/
│   │   └── ParametersQueries.cs
│   ├── Mutations/
│   │   └── ParametersMutations.cs
│   ├── Types/
│   │   └── ParameterType.cs
│   └── GraphQLDependencyInjection.cs
│
└── DependencyInjection.cs         # Configuração geral
```

---

## 🚀 Uso

### REST API (Atual)

```csharp
// Program.cs ou Startup.cs
services.AddParametersPresentation(
    enableRest: true,
    enableGraphQL: false
);
```

**Endpoints REST:**
- `GET /api/parameters` - Listar todos
- `GET /api/parameters/{para1Stamp}` - Buscar por stamp
- `POST /api/parameters` - Criar novo
- `PUT /api/parameters/{para1Stamp}` - Atualizar
- `DELETE /api/parameters/{para1Stamp}` - Eliminar

---

### GraphQL API (Futuro)

```csharp
// Program.cs ou Startup.cs
services.AddParametersPresentation(
    enableRest: true,
    enableGraphQL: true  // Quando implementar
);
```

**Queries GraphQL (futuro):**
```graphql
query {
  parameters(includeInactive: false) {
    para1Stamp
    descricao
    valor
    tipo
  }
}
```

**Mutations GraphQL (futuro):**
```graphql
mutation {
  createParameter(input: {
    descricao: "Novo Parâmetro"
    valor: "123"
    tipo: "N"
  }) {
    para1Stamp
    descricao
  }
}
```

---

## 🏗️ Arquitetura

### Separação por Protocolo

```
REST/           ← Implementação REST (Controllers)
GraphQL/        ← Implementação GraphQL (Queries, Mutations, Types)
```

**Vantagens:**
- ✅ Múltiplos protocolos no mesmo módulo
- ✅ Separação clara de responsabilidades
- ✅ Fácil adicionar novos protocolos (gRPC, SignalR)
- ✅ Mesma Application Layer para todos

---

## 📊 Fluxo de Dados

### REST
```
HTTP Request → Controller → MediatR → Application → Domain → Infrastructure
                                                           ↓
HTTP Response ← Controller ← Result ← Handler ← Repository
```

### GraphQL (futuro)
```
GraphQL Query → Resolver → MediatR → Application → Domain → Infrastructure
                                                         ↓
GraphQL Response ← Resolver ← Result ← Handler ← Repository
```

---

## 🔧 Implementar GraphQL

Quando quiser adicionar GraphQL:

### 1. Adicionar HotChocolate
```bash
dotnet add package HotChocolate.AspNetCore
dotnet add package HotChocolate.Data
```

### 2. Implementar Queries
```csharp
// GraphQL/Queries/ParametersQueries.cs
public class ParametersQueries
{
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public async Task<IEnumerable<ParameterDto>> GetParameters(
        [Service] IMediator mediator,
        bool includeInactive = false)
    {
        return await mediator.Send(new GetAllParametersQuery(includeInactive));
    }

    public async Task<ParameterDto?> GetParameter(
        [Service] IMediator mediator,
        string para1Stamp)
    {
        return await mediator.Send(new GetParameterByStampQuery(para1Stamp));
    }
}
```

### 3. Implementar Mutations
```csharp
// GraphQL/Mutations/ParametersMutations.cs
public class ParametersMutations
{
    public async Task<ParameterDto> CreateParameter(
        [Service] IMediator mediator,
        CreateParameterDto input)
    {
        var command = new CreateParameterCommand(
            input.Descricao,
            input.Valor,
            input.Tipo,
            input.Dec,
            input.Tam,
            null
        );
        
        return await mediator.Send(command);
    }

    public async Task<ParameterDto> UpdateParameter(
        [Service] IMediator mediator,
        string para1Stamp,
        UpdateParameterDto input)
    {
        var command = new UpdateParameterCommand(
            para1Stamp,
            input.Descricao,
            input.Valor,
            input.Tipo,
            input.Dec,
            input.Tam,
            null
        );
        
        return await mediator.Send(command);
    }

    public async Task<bool> DeleteParameter(
        [Service] IMediator mediator,
        string para1Stamp)
    {
        return await mediator.Send(new DeleteParameterCommand(para1Stamp));
    }
}
```

### 4. Configurar DI
```csharp
// GraphQL/GraphQLDependencyInjection.cs
public static IServiceCollection AddParametersGraphQL(this IServiceCollection services)
{
    services
        .AddGraphQLServer()
        .AddQueryType<ParametersQueries>()
        .AddMutationType<ParametersMutations>()
        .AddType<ParameterType>()
        .AddProjections()
        .AddFiltering()
        .AddSorting();
    
    return services;
}
```

### 5. Ativar no Host
```csharp
// PHCAPI.Host/Program.cs
services.AddParametersPresentation(
    enableRest: true,
    enableGraphQL: true
);

app.MapGraphQL("/graphql");
```

---

## 🎯 Clean Architecture

```
┌─────────────────────────────────────┐
│     Presentation Layer              │
│  ┌───────────┐     ┌──────────┐    │
│  │   REST    │     │ GraphQL  │    │
│  └─────┬─────┘     └────┬─────┘    │
│        │                │           │
│        └────────┬───────┘           │
└─────────────────┼───────────────────┘
                  │
┌─────────────────▼───────────────────┐
│     Application Layer (MediatR)     │
│  ┌────────────────────────────┐    │
│  │  Commands & Queries        │    │
│  └────────────┬───────────────┘    │
└─────────────────┼───────────────────┘
                  │
┌─────────────────▼───────────────────┐
│       Domain Layer                  │
│  ┌────────────────────────────┐    │
│  │  Entities & Business Logic │    │
│  └────────────┬───────────────┘    │
└─────────────────┼───────────────────┘
                  │
┌─────────────────▼───────────────────┐
│     Infrastructure Layer            │
│  ┌────────────────────────────┐    │
│  │  Repositories & Database   │    │
│  └────────────────────────────┘    │
└─────────────────────────────────────┘
```

**Benefícios:**
- ✅ Application Layer é **reutilizada** por REST e GraphQL
- ✅ Domain Layer **independente** do protocolo
- ✅ Infrastructure Layer **não conhece** REST ou GraphQL
- ✅ Fácil adicionar **novos protocolos** (gRPC, WebSockets)

---

## 📝 Notas

### Por que Presentation separada?

1. **REST e GraphQL compartilham Application Layer**
   - Mesmo MediatR, mesmos Commands/Queries
   - Sem duplicação de lógica

2. **Separação de Responsabilidades**
   - REST: Controllers (HTTP)
   - GraphQL: Resolvers (GraphQL Schema)
   - Ambos usam MediatR para Application

3. **Facilita Migração**
   - Pode ter REST e GraphQL simultaneamente
   - Migração gradual
   - Desativar REST quando GraphQL estiver pronto

4. **Clean Architecture**
   - Presentation não é Infrastructure
   - Infrastructure = Database, File System
   - Presentation = HTTP, GraphQL, gRPC

---

## 🔍 Exemplo: Mesma Query, Protocolos Diferentes

### REST Controller
```csharp
[HttpGet]
public async Task<ActionResult<ResponseDTO>> GetAll(
    [FromQuery] bool includeInactive = false)
{
    var result = await _mediator.Send(new GetAllParametersQuery(includeInactive));
    return Ok(ResponseDTO.Success(data: result));
}
```

### GraphQL Resolver (futuro)
```csharp
public async Task<IEnumerable<ParameterDto>> GetParameters(
    [Service] IMediator mediator,
    bool includeInactive = false)
{
    return await mediator.Send(new GetAllParametersQuery(includeInactive));
}
```

**Ambos usam:**
- ✅ Mesma Query: `GetAllParametersQuery`
- ✅ Mesmo Handler: `GetAllParametersQueryHandler`
- ✅ Mesmo Repository: `IPara1Repository`
- ✅ Mesma Entidade: `Para1`

---

## 📚 Referências

- [Clean Architecture - Uncle Bob](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [HotChocolate GraphQL](https://chillicream.com/docs/hotchocolate)
- [MediatR Pattern](https://github.com/jbogard/MediatR)

---

**Desenvolvido seguindo Clean Architecture e preparado para REST + GraphQL! 🚀**
