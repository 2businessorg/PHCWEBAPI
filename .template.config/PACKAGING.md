# Criar Template PHCAPI como NuGet Package

Este guia explica como empacotar e distribuir o template como um pacote NuGet.

## 1. Criar arquivo .nuspec

Primeiro, crie um arquivo `PHCAPI.Templates.nuspec` na raiz do projeto:

```xml
<?xml version="1.0" encoding="utf-8"?>
<package xmlns="http://schemas.microsoft.com/packaging/2012/06/nuspec.xsd">
  <metadata>
    <id>PHCAPI.Templates</id>
    <version>1.0.0</version>
    <authors>Your Organization</authors>
    <description>Templates for creating modular Clean Architecture Web APIs with ASP.NET Core</description>
    <packageTypes>
      <packageType name="Template" />
    </packageTypes>
    <tags>dotnet-new;templates;webapi;cleanarchitecture;modular;csharp</tags>
    <license type="expression">MIT</license>
    <projectUrl>https://github.com/yourorg/phcapi-templates</projectUrl>
    <requireLicenseAcceptance>false</requireLicenseAcceptance>
  </metadata>
  <files>
    <file src="**\*" target="content" exclude="**\bin\**;**\obj\**;**\.vs\**;**\logs\**" />
  </files>
</package>
```

## 2. Empacotar o Template

```powershell
# No diretório raiz do projeto
nuget pack PHCAPI.Templates.nuspec

# Ou usando dotnet
dotnet pack -p:NuspecFile=PHCAPI.Templates.nuspec
```

## 3. Instalar do Pacote Local

```powershell
dotnet new install .\PHCAPI.Templates.1.0.0.nupkg
```

## 4. Publicar no NuGet.org (Opcional)

```powershell
# Obter API key em https://www.nuget.org/account/apikeys
nuget setApiKey YOUR-API-KEY

# Publicar
nuget push PHCAPI.Templates.1.0.0.nupkg -Source https://api.nuget.org/v3/index.json
```

## 5. Instalar de NuGet.org

Após publicado, qualquer pessoa pode instalar:

```powershell
dotnet new install PHCAPI.Templates
```

## 6. Publicar em Feed Interno (Azure DevOps/GitHub Packages)

### Azure Artifacts

```powershell
# Adicionar source
dotnet nuget add source "https://pkgs.dev.azure.com/yourorg/_packaging/yourfeed/nuget/v3/index.json" -n "YourFeed"

# Publicar
dotnet nuget push PHCAPI.Templates.1.0.0.nupkg --source "YourFeed" --api-key az
```

### GitHub Packages

```powershell
# Adicionar source
dotnet nuget add source --username YOUR_GITHUB_USERNAME --password YOUR_PAT --store-password-in-clear-text --name github "https://nuget.pkg.github.com/YOURORG/index.json"

# Publicar
dotnet nuget push PHCAPI.Templates.1.0.0.nupkg --source "github"
```

## 7. Versionar o Template

Para novas versões, atualize o número da versão no arquivo `.nuspec` e repita o processo de empacotamento.

```xml
<version>1.1.0</version>
```

## Comandos Úteis

```powershell
# Listar templates instalados
dotnet new list

# Ver detalhes do template
dotnet new phcapi --help

# Desinstalar template
dotnet new uninstall PHCAPI.Templates
```
