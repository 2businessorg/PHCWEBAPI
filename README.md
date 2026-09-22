# 🏗️ PHCAPI - Template de Clean Architecture

Template profissional para criação de APIs modulares usando ASP.NET Core com Clean Architecture.

[![.NET](https://img.shields.io/badge/.NET-10.0-blue)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-green)](LICENSE)

## 🚀 Instalação Rápida

```powershell
# 1. Clonar o repositório
git clone https://github.com/dnelse2b/PHCWEBAPI.git
cd PHCWEBAPI

# 2. Instalar o template
dotnet new install .

# 3. Criar seu projeto (em qualquer lugar)
cd ..
dotnet new phcapi -n MeuProjeto
```

**Pronto!** Você tem uma API completa com Clean Architecture, módulos, autenticação e muito mais! 🎉

---

## ✨ Características

- ✅ **Clean Architecture** - Separação clara de responsabilidades
- ✅ **Arquitetura Modular** - Módulos independentes e reutilizáveis
- ✅ **CQRS + MediatR** - Command Query Responsibility Segregation
- ✅ **Entity Framework Core** - ORM com Repository Pattern
- ✅ **ASP.NET Identity** - Autenticação e autorização completa
- ✅ **Swagger/OpenAPI** - Documentação automática da API
- ✅ **Serilog** - Logging estruturado
- ✅ **Hangfire** - Job scheduling e background tasks
- ✅ **Audit Logging** - Rastreamento automático de mudanças
- ✅ **FluentValidation** - Validação de dados
- ✅ **Admin UI** - Interface administrativa (Razor Pages)
- ✅ **Testes Incluídos** - Estrutura de testes configurada

---

## 📁 Estrutura do Projeto

```
YourAPI/
├── src/
│   ├── YourAPI.Host/              # 🚀 API Host principal
│   ├── Admin.UI/                  # 🖥️ Interface administrativa
│   ├── Modules/                   # 📦 Módulos da aplicação
│   │   ├── Audit/                # 📝 Auditoria e logs
│   │   ├── Auth/                 # 🔐 Autenticação/Autorização
│   │   ├── Parameters/           # ⚙️ Parâmetros do sistema
│   │   └── Providers/            # 🔌 Provedores externos
│   └── Shared/                    # 🔧 Código compartilhado
│       ├── Shared.Abstractions/
│       ├── Shared.Application/
│       ├── Shared.Infrastructure/
│       └── Shared.Kernel/
├── tests/                         # 🧪 Testes automatizados
├── docs/                          # 📚 Documentação
└── scripts/                       # 🛠️ Scripts utilitários
```

Cada módulo segue Clean Architecture:
```
Module/
├── Module.Domain/          # Entidades e regras de negócio
├── Module.Application/     # Casos de uso (Commands/Queries)
├── Module.Infrastructure/  # Implementações (Repositórios, DbContext)
└── Module.Presentation/    # Controllers e endpoints
```

---

## 🎯 Exemplos de Uso

### Criar API Completa
```powershell
dotnet new phcapi -n GestaoAPI
```

### API com Módulos Personalizados
```powershell
# Sem módulo de Providers
dotnet new phcapi -n MinhaAPI --providers false

# Apenas Auth e Audit
dotnet new phcapi -n AuthAPI --parameters false --providers false

# Sem interface admin
dotnet new phcapi -n ApiPura --admin-ui false
```

### Escolher Banco de Dados
```powershell
# SQL Server (padrão)
dotnet new phcapi -n SqlServerAPI

# PostgreSQL
dotnet new phcapi -n PostgresAPI --database PostgreSQL
```

---

## ⚙️ Parâmetros Disponíveis

| Parâmetro | Atalho | Padrão | Descrição |
|-----------|--------|--------|-----------|
| `--audit` | `-a` | true | Incluir módulo de Auditoria |
| `--auth` | `-au` | true | Incluir módulo de Autenticação |
| `--parameters` | `-p` | true | Incluir módulo de Parâmetros |
| `--providers` | `-pr` | true | Incluir módulo de Provedores |
| `--admin-ui` | `-ui` | true | Incluir interface administrativa |
| `--tests` | `-t` | true | Incluir projetos de teste |
| `--database` | `-db` | SqlServer | Banco de dados (SqlServer/PostgreSQL) |

```powershell
# Ver todas as opções
dotnet new phcapi --help
```

---

## 🚀 Começando

### 1. Instalar Template
```powershell
git clone https://github.com/dnelse2b/PHCWEBAPI.git
cd PHCWEBAPI
dotnet new install .
```

### 2. Criar Projeto
```powershell
dotnet new phcapi -n MeuProjeto
cd MeuProjeto
```

### 3. Configurar Banco de Dados
Edite `appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=MeuProjeto;Trusted_Connection=True;"
  }
}
```

### 4. Executar Migrations
```powershell
dotnet ef database update --project src/MeuProjeto.Host
```

### 5. Executar
```powershell
dotnet run --project src/MeuProjeto.Host
```

### 6. Acessar
- **API (Swagger):** https://localhost:5001/swagger
- **Admin UI:** https://localhost:5001/admin

---

## 📚 Documentação

Após criar seu projeto, veja a documentação completa em:

- [Como Executar](docs/COMO-EXECUTAR-PHCAPI.md)
- [Arquitetura](docs/Architecture.md)
- [Padrões e Práticas](docs/patterns/)
- [Gerenciamento de Módulos](docs/Standard-Modules-Management-GUIDE.md)

---

## 🔄 Atualizar Template
1. Atualizar o repositório
cd PHCWEBAPI
git pull

# 2. Reinstalar
dotnet new uninstall PHCAPI.Template
dotnet new install .
# Instalar versão mais recente
dotnet new install https://github.com/dnelse2b/PHCWEBAPI.git
```

---

## 🤝 Contribuindo

1. Fork o projeto
2. Crie uma branch para sua feature (`git checkout -b feature/MinhaFeature`)
3. Commit suas mudanças (`git commit -m 'Adiciona nova feature'`)
4. Push para a branch (`git push origin feature/MinhaFeature`)
5. Abra um Pull Request

---

## 📋 Requisitos

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download)
- SQL Server 2019+ ou PostgreSQL 13+
- Visual Studio 2022 / VS Code / Rider

---

## 📞 Suporte

- **Issues:** [GitHub Issues](https://github.com/dnelse2b/PHCWEBAPI/issues)
- **Documentação completa:** Veja pasta `/docs`

---

## 📄 Licença

Este projeto está sob a licença MIT. Veja o arquivo [LICENSE](LICENSE) para mais detalhes.

---

## 🌟 Stack Tecnológica

- ASP.NET Core 10.0
- Entity Framework Core 10.0
- MediatR
- FluentValidation
- AutoMapper / Mapster
- Serilog
- Swagger / Swashbuckle
- Hangfire
- ASP.NET Identity
- xUnit / NUnit

---

**Desenvolvido com ❤️ para acelerar o desenvolvimento de APIs de qualidade**
