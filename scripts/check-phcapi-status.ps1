# PHCAPI - Script de Status e Diagnóstico

Write-Host ""
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
Write-Host "      PHCAPI - Status e Diagnóstico" -ForegroundColor Cyan
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
Write-Host ""

# 1. Verificar processos
Write-Host "📊 PROCESSOS ATIVOS" -ForegroundColor Yellow
Write-Host "─────────────────────────────────────────────────" -ForegroundColor Gray

$processes = Get-Process -Name "PHCAPI.Host","dotnet" -ErrorAction SilentlyContinue
$processCount = ($processes | Measure-Object).Count

if ($processCount -eq 0) {
    Write-Host "   ❌ Nenhum processo rodando" -ForegroundColor Red
    Write-Host "   💡 Execute: .\scripts\start-phcapi.bat" -ForegroundColor DarkGray
} elseif ($processCount -eq 1) {
    Write-Host "   ✅ 1 processo rodando (IDEAL)" -ForegroundColor Green
    $processes | Select-Object Id, ProcessName, StartTime, @{Name="Memory(MB)";Expression={[math]::Round($_.WorkingSet/1MB,2)}} | Format-Table -AutoSize
} else {
    Write-Host "   ⚠️  $processCount processos rodando (PROBLEMA!)" -ForegroundColor Yellow
    Write-Host "   Múltiplas instâncias podem causar IOException" -ForegroundColor DarkYellow
    $processes | Select-Object Id, ProcessName, StartTime, @{Name="Memory(MB)";Expression={[math]::Round($_.WorkingSet/1MB,2)}} | Format-Table -AutoSize
    
    Write-Host ""
    Write-Host "   💡 SOLUÇÃO:" -ForegroundColor Cyan
    Write-Host "   1. Feche o VS Code completamente" -ForegroundColor White
    Write-Host "   2. Execute: Stop-Process -Name 'PHCAPI.Host','dotnet' -Force" -ForegroundColor White
    Write-Host "   3. Execute: .\scripts\start-phcapi.bat em PowerShell EXTERNO" -ForegroundColor White
}

Write-Host ""

# 2. Verificar porta 7298
Write-Host "🌐 PORTA 7298" -ForegroundColor Yellow
Write-Host "─────────────────────────────────────────────────" -ForegroundColor Gray

$portCheck = Get-NetTCPConnection -LocalPort 7298 -State Listen -ErrorAction SilentlyContinue
if ($portCheck) {
    $process = Get-Process -Id $portCheck.OwningProcess -ErrorAction SilentlyContinue
    Write-Host "   ✅ Porta 7298 está em uso" -ForegroundColor Green
    Write-Host "   Processo: $($process.ProcessName) (PID: $($process.Id))" -ForegroundColor White
} else {
    Write-Host "   ❌ Porta 7298 não está em uso" -ForegroundColor Red
    Write-Host "   Aplicação não está rodando" -ForegroundColor DarkGray
}

Write-Host ""

# 3. Testar conectividade HTTP
Write-Host "🔗 CONECTIVIDADE HTTP" -ForegroundColor Yellow
Write-Host "─────────────────────────────────────────────────" -ForegroundColor Gray

$urls = @(
    @{Name="Login"; Url="http://localhost:7298/Admin/Account/Login"},
    @{Name="API Health"; Url="http://localhost:7298/api/providers"},
    @{Name="Swagger"; Url="http://localhost:7298/swagger"}
)

foreach ($endpoint in $urls) {
    try {
        $response = Invoke-WebRequest -Uri $endpoint.Url -Method GET -TimeoutSec 3 -UseBasicParsing -ErrorAction Stop
        Write-Host "   ✅ $($endpoint.Name): $($response.StatusCode)" -ForegroundColor Green
    }
    catch {
        $statusCode = $_.Exception.Response.StatusCode.value__
        if ($statusCode -eq 401 -or $statusCode -eq 302) {
            Write-Host "   ✅ $($endpoint.Name): $statusCode (Requer autenticação - OK)" -ForegroundColor Green
        } else {
            Write-Host "   ❌ $($endpoint.Name): Falha ($statusCode)" -ForegroundColor Red
        }
    }
}

Write-Host ""

# 4. Verificar logs recentes
Write-Host "📝 LOGS RECENTES" -ForegroundColor Yellow
Write-Host "─────────────────────────────────────────────────" -ForegroundColor Gray

$logsPath = "src\PHCAPI.Host\logs"
if (Test-Path $logsPath) {
    $recentLogs = Get-ChildItem "$logsPath\*.txt" -ErrorAction SilentlyContinue | 
                  Sort-Object LastWriteTime -Descending | 
                  Select-Object -First 3
    
    if ($recentLogs) {
        foreach ($log in $recentLogs) {
            $size = [math]::Round($log.Length / 1KB, 2)
            Write-Host "   📄 $($log.Name) - $size KB - $($log.LastWriteTime)" -ForegroundColor White
        }
        
        # Verificar erros no último log
        $lastLog = $recentLogs | Select-Object -First 1
        if ($lastLog) {
            $errors = Select-String -Path $lastLog.FullName -Pattern "ERR|Exception|IOException" -ErrorAction SilentlyContinue | Select-Object -Last 5
            if ($errors) {
                Write-Host ""
                Write-Host "   ⚠️  ÚLTIMOS ERROS ENCONTRADOS:" -ForegroundColor Yellow
                foreach ($error in $errors) {
                    Write-Host "      $($error.Line.Substring(0, [Math]::Min(80, $error.Line.Length)))..." -ForegroundColor DarkYellow
                }
            } else {
                Write-Host ""
                Write-Host "   ✅ Nenhum erro nos últimos logs" -ForegroundColor Green
            }
        }
    } else {
        Write-Host "   ℹ️  Nenhum log encontrado" -ForegroundColor DarkGray
    }
} else {
    Write-Host "   ℹ️  Diretório de logs não existe" -ForegroundColor DarkGray
}

Write-Host ""

# 5. Resumo e Recomendações
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
Write-Host "      RESUMO" -ForegroundColor Cyan
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan

if ($processCount -eq 1 -and $portCheck) {
    Write-Host ""
    Write-Host "✅ STATUS: APLICAÇÃO RODANDO CORRETAMENTE" -ForegroundColor Green
    Write-Host ""
    Write-Host "🌐 Acesse:" -ForegroundColor Cyan
    Write-Host "   Login:     http://localhost:7298/Admin/Account/Login" -ForegroundColor White
    Write-Host "   Providers: http://localhost:7298/Admin/Providers" -ForegroundColor White
    Write-Host "   Swagger:   http://localhost:7298/swagger" -ForegroundColor White
} elseif ($processCount -gt 1) {
    Write-Host ""
    Write-Host "⚠️  STATUS: MÚLTIPLOS PROCESSOS DETECTADOS" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "💡 AÇÕES RECOMENDADAS:" -ForegroundColor Cyan
    Write-Host "   1. Feche o Visual Studio Code" -ForegroundColor White
    Write-Host "   2. Execute: Stop-Process -Name 'PHCAPI.Host','dotnet' -Force" -ForegroundColor White
    Write-Host "   3. Execute: .\scripts\start-phcapi.bat" -ForegroundColor White
} else {
    Write-Host ""
    Write-Host "❌ STATUS: APLICAÇÃO NÃO ESTÁ RODANDO" -ForegroundColor Red
    Write-Host ""
    Write-Host "💡 PARA INICIAR:" -ForegroundColor Cyan
    Write-Host "   .\scripts\start-phcapi.bat" -ForegroundColor White
    Write-Host ""
    Write-Host "   OU em PowerShell:" -ForegroundColor DarkGray
    Write-Host "   cd src\PHCAPI.Host; dotnet run" -ForegroundColor DarkGray
}

Write-Host ""
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
Write-Host ""
