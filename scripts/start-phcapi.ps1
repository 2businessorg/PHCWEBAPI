# PHCAPI - Script de Inicialização Segura
# Garante que apenas uma instância da aplicação esteja rodando

Write-Host "🚀 PHCAPI - Inicializando..." -ForegroundColor Cyan
Write-Host ""

# 1. Parar processos existentes
Write-Host "🔍 Verificando processos existentes..." -ForegroundColor Yellow
$existingProcesses = Get-Process -Name "PHCAPI.Host","dotnet" -ErrorAction SilentlyContinue
if ($existingProcesses) {
    $count = ($existingProcesses | Measure-Object).Count
    Write-Host "⚠️  Encontrados $count processos rodando. Terminando..." -ForegroundColor Yellow
    Stop-Process -Name "PHCAPI.Host","dotnet" -Force -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 2
    Write-Host "✅ Processos anteriores terminados" -ForegroundColor Green
} else {
    Write-Host "✅ Nenhum processo anterior encontrado" -ForegroundColor Green
}

# 2. Limpar logs antigos (opcional)
$logsPath = "src\PHCAPI.Host\logs"
if (Test-Path $logsPath) {
    $oldLogs = Get-ChildItem "$logsPath\*.txt" -ErrorAction SilentlyContinue
    if ($oldLogs) {
        Write-Host "🗑️  Limpando logs antigos..." -ForegroundColor Yellow
        Remove-Item "$logsPath\*.txt" -Force -ErrorAction SilentlyContinue
    }
}

# 3. Navegar para o diretório correto
Set-Location "src\PHCAPI.Host"

Write-Host ""
Write-Host "▶️  Iniciando PHCAPI.Host..." -ForegroundColor Green
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Gray
Write-Host ""

# 4. Executar a aplicação
dotnet run

# Se chegou aqui, significa que a aplicação foi encerrada
Write-Host ""
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Gray
Write-Host "⏹️  PHCAPI.Host encerrado" -ForegroundColor Red
