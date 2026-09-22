# 🚀 Guia de Instalação - PHCAPI Template

## 📦 Instalação Via GitHub (RECOMENDADO)

### Para qualquer pessoa na equipe:

```powershell
# 1. Clonar o repositório
git clone https://github.com/dnelse2b/PHCWEBAPI.git

# 2. Entrar na pasta
cd PHCWEBAPI

# 3. Instalar o template
dotnet new install .
```

✅ **Vantagens:**
- Fácil de atualizar (git pull)
- Acesso ao código-fonte do template
- Pode ser customizado localmente
- Ideal para equipes

---

## 🔧 Instalação Local (Desenvolvedores do template)

Se você está desenvolvendo/modificando o template:

```powershell
# 1. Clonar o repositório
git clone https://github.com/dnelse2b/PHCWEBAPI.git
cd PHCWEBAPI

# 2. Instalar localmente
dotnet new install .
```

---

## 📝 Criar Novo Projeto

Após instalar o template (qualquer método acima):

```powershell
# Criar projeto completo
dotnet new phcapi -n MeuProjeto

# Ver todas as opções
dotnet new phcapi --help
```

### Exemplos práticos:

```powershell
# API para sistema de gestão
dotnet new phcapi -n SGOFWS

# API sem módulo de Providers
dotnet new phcapi -n MinhaAPI --providers false

# API com PostgreSQL
dotnet new phcapi -n PostgresAPI --database PostgreSQL

# API apenas com Auth e Audit
dotnet new phcapi -n SimplesAPI --parameters false --providers false
```

---

## 🔄 Atualizar Template

```powershell
# 1. Ir para a pasta do template
cd PHCWEBAPI

# 2. Puxar atualizações do GitHub
git pull

# 3. Reinstalar
dotnet new uninstall PHCAPI.Template
dotnet new install .
```

---

## 🗑️ Desinstalar

```powershell
dotnet new uninstall PHCAPI.Template
```

---

## 📋 Verificar Instalação

```powershell
# Listar todos templates instalados
dotnet new list

# Verificar se phcapi está disponível
dotnet new list phcapi
```

Você deve ver:
```
Template Name                             Short Name  Language  Tags
----------------------------------------  ----------  --------  -----------------------------------------------
PHC Web API - Modular Clean Architecture  phcapi      [C#]      Web/API/Clean Architecture/Modular/ASP.NET Core
```

---

## 🌐 Compartilhar com a Equipe

Envie para sua equipe:

```text
Para criar APIs com nossa arquitetura padrão:

1. Instalar o template:
   dotnet new install https://github.com/dnelse2b/PHCWEBAPI.git
Clonar e instalar o template:
   git clone https://github.com/dnelse2b/PHCWEBAPI.git
   cd PHCWEBAPI
   dotnet new install .

2. Criar seu projeto (em qualquer pasta):
   cd ..
3. Configurar e executar:
   cd NomeDaSuaAPI
   dotnet restore
   dotnet build
```

---

## ❓ Problemas Comuns

git clone https://github.com/dnelse2b/PHCWEBAPI.git
cd PHCWEBAPI
dotnet new install .
```

### "Template already installed"
**Solução:** Desinstale e reinstale
```powershell
dotnet new uninstall PHCAPI.Template
cd PHCWEBAPI
dotnet new install .
```

### Erro ao clonar (repositório privado)
**Solução:** Configure credenciais Git
```powershell
git config --global credential.helper store
git clone https://github.com/dnelse2b/PHCWEBAPI.git
**Solução:** Verifique sua conexão e credenciais Git
```powershell
git config --global credential.helper store
```

---

## 📞 Suporte

- **Repositório:** https://github.com/dnelse2b/PHCWEBAPI
- **Issues:** https://github.com/dnelse2b/PHCWEBAPI/issues
- **Documentação:** Veja pasta `/docs` no projeto criado
