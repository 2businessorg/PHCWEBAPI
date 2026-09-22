@echo off
SETLOCAL EnableDelayedExpansion

echo.
echo ========================================
echo     PHCAPI - Inicializacao Segura
echo ========================================
echo.

REM Definir caminho do projeto
set PROJECT_ROOT=%~dp0..
set HOST_PATH=%PROJECT_ROOT%\src\PHCAPI.Host

echo [1/4] Parando processos anteriores...
taskkill /F /IM "PHCAPI.Host.exe" >nul 2>&1
taskkill /F /IM "dotnet.exe" >nul 2>&1
echo      OK - Processos anteriores parados
timeout /t 2 /nobreak >nul

echo.
echo [2/4] Limpando logs antigos...
if exist "%HOST_PATH%\logs\*.txt" (
    del /F /Q "%HOST_PATH%\logs\*.txt" >nul 2>&1
    echo      OK - Logs removidos
) else (
    echo      OK - Nenhum log para limpar
)

echo.
echo [3/4] Navegando para o diretorio...
cd /d "%HOST_PATH%"
echo      OK - Diretorio: %CD%

echo.
echo [4/4] Iniciando PHCAPI.Host...
echo ========================================
echo.
echo  URLs disponiveis:
echo    Login:     http://localhost:7298/Admin/Account/Login
echo    Providers: http://localhost:7298/Admin/Providers
echo    Swagger:   http://localhost:7298/swagger
echo.
echo  Pressione Ctrl+C para parar
echo.
echo ========================================
echo.

dotnet run

REM Se chegou aqui, a aplicacao foi encerrada
echo.
echo ========================================
echo  PHCAPI.Host encerrado
echo ========================================
pause
