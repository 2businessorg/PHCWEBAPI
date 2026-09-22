# 🚀 Como Executar PHCAPI de Forma Estável

## ⚠️ PROBLEMA IDENTIFICADO

O **Visual Studio Code** inicia automaticamente múltiplos processos `dotnet` (build servers, OmniSharp, debugger, etc.) que causam conflitos ao escrever nos arquivos de log, resultando em `System.IO.IOException`.

---

## ✅ SOLUÇÃO RECOMENDADA: Executar FORA do VS Code

### Opção 1: PowerShell Externo (RECOMENDADO)

1. **Abra um PowerShell EXTERNO** (não o terminal do VS Code):
   - Pressione `Win + X` → **Windows PowerShell** ou **Windows Terminal**

2. **Navegue para o diretório do projeto:**
   ```powershell
   cd C:\Users\dbarreto\source\repos\PHCWEBAPI\src\PHCAPI.Host
   ```

3. **Execute a aplicação:**
   ```powershell
   dotnet run
   ```

4. **Acesse a aplicação:**
   - Login: http://localhost:7298/Admin/Account/Login
   - Providers: http://localhost:7298/Admin/Providers
   - Swagger: http://localhost:7298/swagger

5. **Para parar:** Pressione `Ctrl + C` no terminal

---

### Opção 2: Usar o Script Automatizado

1. **PowerShell Externo:**
   ```powershell
   cd C:\Users\dbarreto\source\repos\PHCWEBAPI
   .\scripts\start-phcapi.ps1
   ```

   Este script:
   - ✅ Para processos anteriores
   - ✅ Limpa logs antigos
   - ✅ Inicia a aplicação limpa

---

### Opção 3: Publicar e Executar (Produção-like)

Para ambiente de produção ou testes mais estáveis:

```powershell
cd C:\Users\dbarreto\source\repos\PHCWEBAPI\src\PHCAPI.Host

# Build Release
dotnet publish -c Release -o publish

# Execute o binário diretamente
cd publish
.\PHCAPI.Host.exe
```

---

## ❌ O QUE EVITAR

### NÃO usar o terminal integrado do VS Code
- O VS Code inicia automaticamente:
  - Build servers (8+ processos dotnet)
  - OmniSharp (IntelliSense)
  - Hot Reload watchers
  - Debugger background processes

### NÃO executar múltiplas vezes sem verificar
Antes de executar, sempre verifique processos anteriores:

```powershell
# Ver processos ativos
Get-Process -Name "PHCAPI.Host","dotnet" -ErrorAction SilentlyContinue

# Limpar tudo antes de executar
Stop-Process -Name "PHCAPI.Host","dotnet" -Force -ErrorAction SilentlyContinue
```

---

## 🔧 CORREÇÕES APLICADAS

### 1. Serilog com Acesso Compartilhado
Configurado em `Program.cs`:

```csharp
.WriteTo.File(
    path: "logs/PHCAPI-.txt",
    rollingInterval: RollingInterval.Day,
    shared: true,  // ✅ Permite múltiplos processos
    flushToDiskInterval: TimeSpan.FromSeconds(1))
```

### 2. Script de Inicialização Segura
Criado em `scripts/start-phcapi.ps1` que:
- Para processos anteriores
- Limpa logs
- Inicia única instância

---

## 🐛 TROUBLESHOOTING

### Erro: "System.IO.IOException"
**Causa:** Múltiplos processos tentando escrever no mesmo log

**Solução:**
```powershell
# 1. Parar TUDO
Stop-Process -Name "PHCAPI.Host","dotnet" -Force -ErrorAction SilentlyContinue

# 2. Limpar logs
Remove-Item "C:\Users\dbarreto\source\repos\PHCWEBAPI\src\PHCAPI.Host\logs\*.txt" -Force

# 3. Executar em PowerShell EXTERNO
cd C:\Users\dbarreto\source\repos\PHCWEBAPI\src\PHCAPI.Host
dotnet run
```

### Aplicação fecha sozinha após alguns minutos
**Causa:** VS Code/Visual Studio interferindo

**Solução:** Execute em PowerShell externo conforme Opção 1

### Porta 7298 já em uso
**Causa:** Instância anterior ainda rodando

**Solução:**
```powershell
# Verificar o que está usando a porta
Get-NetTCPConnection -LocalPort 7298 -State Listen

# Parar processos
Stop-Process -Name "PHCAPI.Host","dotnet" -Force -ErrorAction SilentlyContinue
```

---

## 📊 VERIFICAR STATUS

### Processos rodando
```powershell
Get-Process -Name "PHCAPI.Host","dotnet" | Select-Object Id, ProcessName, StartTime
```

**Ideal:** Apenas 1 processo `PHCAPI.Host`

**Problema:** Múltiplos processos `dotnet` (VS Code interferindo)

### Testar se está online
```powershell
Invoke-WebRequest -Uri "http://localhost:7298/Admin/Account/Login" -UseBasicParsing
```

Deve retornar `StatusCode: 200`

---

## ✅ CONFIGURAÇÃO CORRETA

Quando executado corretamente (PowerShell externo):

- ✅ **1 processo** PHCAPI.Host
- ✅ **0 ou poucos** processos dotnet (apenas runtime necessário)
- ✅ Aplicação permanece aberta indefinidamente
- ✅ Navegador pode ser fechado sem afetar a aplicação
- ✅ Sem IOException nos logs

---

## 🎯 RESUMO RÁPIDO

**CERTO:**
```powershell
# PowerShell EXTERNO (fora do VS Code)
cd C:\Users\dbarreto\source\repos\PHCWEBAPI\src\PHCAPI.Host
dotnet run
```

**ERRADO:**
```powershell
# Terminal do VS Code (integrado)
dotnet run  # ❌ VS Code inicia 9+ processos dotnet
```

---

## 📝 CREDENCIAIS PADRÃO

- **Username:** `admin`
- **Password:YOUR_PASSWORD (configurada no seed da base de dados)

---

## 🔗 URLS PRINCIPAIS

| Página | URL |
|--------|-----|
| Login | http://localhost:7298/Admin/Account/Login |
| Users | http://localhost:7298/Admin/Users |
| Roles | http://localhost:7298/Admin/Roles |
| **Providers** | http://localhost:7298/Admin/Providers |
| Logs | http://localhost:7298/Admin/Logs |
| API Swagger | http://localhost:7298/swagger |
| Hangfire | http://localhost:7298/hangfire |

---

**Última atualização:** 2026-02-17  
**Versão:** 1.0.0
