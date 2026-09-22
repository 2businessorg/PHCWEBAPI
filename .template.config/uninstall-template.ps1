# Script para desinstalar o template PHCAPI
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Desinstalando Template PHCAPI" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$templatePath = $PSScriptRoot

Write-Host "Desinstalando template..." -ForegroundColor Yellow
dotnet new uninstall $templatePath

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Template desinstalado!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
