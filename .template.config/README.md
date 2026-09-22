# PHC Web API Template

Template para criar APIs modulares baseadas em Clean Architecture com ASP.NET Core.

## 🚀 Instalação Rápida

```powershell
# Clonar e instalar
git clone https://github.com/dnelse2b/PHCWEBAPI.git
cd PHCWEBAPI
dotnet new install .
```

## Estrutura

```
├── src/
│   ├── PHCAPI.Host/          # Host principal da API
│   ├── Admin.UI/              # Interface administrativa
│   ├── Modules/               # Módulos da aplicação
│   │   ├── Audit/            # Auditoria
│   │   ├── Auth/             # Autenticação/Autorização
│   │   ├── Parameters/       # Parâmetros do sistema
│   │   └── Providers/        # Provedores externos
│   └── Shared/               # Código compartilhado
│       ├── Shared.Abstractions/
│       ├── Shared.Application/
│       ├── Shared.Infrastructure/
│       └── Shared.Kernel/
├── tests/
├── docs/
└── scripts/
```

## Como Usar Este Template

### 1. Instalar o Template

**Via GitHub (Recomendado):**
```powershell
git clone https://github.com/dnelse2b/PHCWEBAPI.git
cd PHCWEBAPI
dotnet new install .
```

**Se já clonou:**
```powershell
cd PHCWEBAPI
dotnet new install .
```

### 2. Criar Novo Projeto

```powershell
# Criar projeto completo com todos os módulos
dotnet new phcapi -n MeuProjeto

# Criar projeto com módulos específicos
dotnet new phcapi -n MeuProjeto --audit false --providers false

# Criar sem Admin UI
dotnet new phcapi -n MeuProjeto --admin-ui false

# Escolher banco de dados
dotnet new phcapi -n MeuProjeto --database PostgreSQL
```

### 3. Parâmetros Disponíveis

| Parâmetro | Short | Padrão | Descrição |
|-----------|-------|--------|-----------|
| `--company-name` | `-cn` | YourCompany | Nome da empresa/projeto |
| `--audit` | `-a` | true | Incluir módulo Audit |
| `--auth` | `-au` | true | Incluir módulo Auth |
| `--parameters` | `-p` | true | Incluir módulo Parameters |
| `--providers` | `-pr` | true | Incluir módulo Providers |
| `--admin-ui` | `-ui` | true | Incluir Admin UI |
| `--tests` | `-t` | true | Incluir projetos de teste |
| `--database` | `-db` | SqlServer | Provedor BD (SqlServer/PostgreSQL) |

### 4. Após Criar o Projeto

```powershell
cd MeuProjeto
dotnet restore
dotnet build

# Configurar connection string em appsettings.json
# Executar migrations
dotnet ef database update --project src/PHCAPI.Host

# Executar
dotnet run --project PHCAPI.Template
```

## Atualizar Template

```powershell
cd PHCWEBAPI
git pull
dotnet new uninstall PHCAPI.Template
dotnet new install .
```

## Desinstalar Template

```powershell
dotnet new uninstall .
```

## Características

✅ **Clean Architecture** - Separação clara de responsabilidades  
✅ **Modular** - Módulos independentes e reutilizáveis  
✅ **CQRS** - Command Query Responsibility Segregation  
✅ **MediatR** - Mediator pattern  
✅ **Entity Framework Core** - ORM  
✅ **Identity** - Autenticação e autorização  
✅ **Swagger** - Documentação da API  
✅ **Serilog** - Logging estruturado  
✅ **Hangfire** - Job scheduling  
✅ **Audit Logging** - Rastreamento de alterações  

## Padrões Implementados

- Repository Pattern
- Unit of Work
- Dependency Injection
- DTO Pattern
- Validator Pattern (FluentValidation)
- Mapping Pattern (AutoMapper/Mapster)
