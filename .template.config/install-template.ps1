# Script para instalar o template PHCAPI
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Instalando Template PHCAPI" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$templatePath = Split-Path $PSScriptRoot

# Verificar se estamos no diretório correto
if (-not (Test-Path "$templatePath\PHCAPI.slnx")) {
    Write-Host "ERRO: Execute este script da pasta PHCWEBAPI!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Caminho esperado: .template.config\install-template.ps1" -ForegroundColor Yellow
    exit 1
}

Write-Host "Instalando de: $templatePath" -ForegroundColor Yellow

# Verificar se já está instalado
Write-Host "Verificando templates instalados..." -ForegroundColor Yellow
$installedTemplates = dotnet new list phcapi 2>&1

if ($installedTemplates -match "phcapi") {
    Write-Host "Template já está instalado. Desinstalando versão anterior..." -ForegroundColor Yellow
    dotnet new uninstall PHCAPI.Template
    Write-Host ""
}

# Instalar template
Write-Host "Instalando template..." -ForegroundColor Green
dotnet new install $templatePath

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Template instalado com sucesso!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Para criar um novo projeto, use:" -ForegroundColor White
Write-Host "  dotnet new phcapi -n MeuProjeto" -ForegroundColor Yellow
Write-Host ""
Write-Host "Para ver todas as opções:" -ForegroundColor White
Write-Host "  dotnet new phcapi --help" -ForegroundColor Yellow
Write-Host ""
Write-Host "Exemplos:" -ForegroundColor White
Write-Host "  # Projeto completo" -ForegroundColor Gray
Write-Host "  dotnet new phcapi -n MinhAPI -cn MinhaEmpresa" -ForegroundColor Yellow
Write-Host ""
Write-Host "  # Sem módulo de Providers" -ForegroundColor Gray
Write-Host "  dotnet new phcapi -n MinhAPI --providers false" -ForegroundColor Yellow
Write-Host ""
Write-Host "  # Com PostgreSQL" -ForegroundColor Gray
Write-Host "  dotnet new phcapi -n MinhAPI --database PostgreSQL" -ForegroundColor Yellow
Write-Host ""
