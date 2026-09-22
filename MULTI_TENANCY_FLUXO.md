# Como Funciona o Sistema de Multi-Tenancy

## 📋 Visão Geral

O sistema PHCAPI funciona com **multi-tenancy dinâmica**: cada cliente (tenant) tem a sua própria base de dados, e o API carrega automaticamente as credenciais corretas para cada requisição.

---

## 🔄 FLUXO COMPLETO (Passo a Passo)

### **FASE 0: REGISTRO DO UTILIZADOR (Application Registration)**

```
┌─────────────────────────────────────────────────────────────┐
│              CLIENTE FAZ REGISTRO (Registration)             │
│                                                               │
│  POST /api/auth/register-application-user                   │
│  Body: {                                                    │
│    username: "isac.munguambe",                              │
│    email: "isac@empresa.pt",                                │
│    password: "YOUR_PASSWORD",                           │
│    appLicenseStamp: "ABC123XYZ"  ← License identificador   │
│  }                                                          │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│  AUTH CONTROLLER                                             │
│  📄 Auth/Presentation/Controllers/AuthenticateController.cs  │
│                                                               │
│  ✓ Recebe dados do registro                                 │
│  ✓ Chama RegisterApplicationUserAsync()                     │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│  IDENTITY AUTHENTICATION SERVICE (PASSO 1: VALIDAR STAMP)   │
│  📄 Auth/Services/IdentityAuthenticationService.cs           │
│                                                               │
│  ✓ Procura AppLicense na BD primária com o stamp           │
│  ✓ Se não existe → retorna erro 404                         │
│  ✓ Se existe e AspNetUsersId != NULL → erro (já tem user)   │
│  ✓ Se existe e AspNetUsersId IS NULL → continua             │
│  ✓ Valida se a licença é válida para API access            │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│  IDENTITY AUTHENTICATION SERVICE (PASSO 2: CRIAR USER)      │
│  📄 Auth/Services/IdentityAuthenticationService.cs           │
│                                                               │
│  ✓ Verifica se username já existe em AspNetUsers           │
│  ✓ Cria novo IdentityUser:                                 │
│    - UserName = "isac.munguambe"                           │
│    - Email = "isac@empresa.pt"                             │
│    - Password = YOUR_PASSWORD"SecurePassword123")                  │
│                                                               │
│  ✓ Retorna o ID gerado (ex: "user-123-guid-here")          │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│  IDENTITY AUTHENTICATION SERVICE (PASSO 3: LIGAR AO TENANT) │
│  📄 Auth/Services/IdentityAuthenticationService.cs           │
│                                                               │
│  ✓ Chama AppLicenseRepository.UpdateAspNetUsersIdAsync()    │
│  ✓ UPDATE u_applicense SET AspNetUsersId = "user-123"      │
│    WHERE Stamp = "ABC123XYZ"                                │
│                                                               │
│  ✓ Agora u_applicense aponta ao utilizador criado!         │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│  RESPOSTA AO CLIENTE (Registration Success)                 │
│                                                               │
│  ✅ Status: 200 OK                                           │
│  Body: {                                                    │
│    "success": true,                                         │
│    "message": "Application user registered successfully",    │
│    "userId": "user-123-guid-here",                          │
│    "username": "isac.munguambe",                            │
│    "tenantName": "Empresa ABC"                              │
│  }                                                          │
│                                                               │
│  ⚠️ NOTE: Nenhuma credencial é devolvida                    │
│  Cliente agora pode fazer LOGIN                             │
└─────────────────────────────────────────────────────────────┘
```

---

### **FASE 1: LOGIN DO UTILIZADOR**

```
┌─────────────────────────────────────────────────────────────┐
│                    CLIENTE FAZ LOGIN                         │
│                                                               │
│  POST /api/auth/login                                        │
│  Body: { username: "user@empresa.pt", password: "YOUR_PASSWORD" }    │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│  AUTH CONTROLLER                                             │
│  📄 Auth/Controllers/AuthController.cs                       │
│                                                               │
│  ✓ Recebe username e password                               │
│  ✓ Chama IdentityAuthenticationService.LoginAsync()         │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│  IDENTITY AUTHENTICATION SERVICE (PASSO 1)                  │
│  📄 Auth/Services/IdentityAuthenticationService.cs           │
│                                                               │
│  ✓ Valida username/password contra a BD primária            │
│    (tabela AspNetUsers)                                      │
│  ✓ Se inválido → retorna erro                               │
│  ✓ Se válido → continua para próximo passo                  │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│  IDENTITY AUTHENTICATION SERVICE (PASSO 2)                  │
│  📄 Auth/Services/IdentityAuthenticationService.cs           │
│                                                               │
│  ✓ Carrega AppLicense da tabela u_applicense                │
│    usando o AspNetUsersId do utilizador autenticado         │
│    (que tem: servidor, base de dados, user, password)      │
│                                                               │
│  SELECT * FROM u_applicense                                 │
│  WHERE AspNetUsersId = user.Id                              │
│           ↑                                                  │
│       Relacionamento principal!                             │
│                                                               │
│  ✓ Valida se a licença está ativa                          │
│  ✓ Se inválida → retorna erro                              │
│  ✓ Se válida → continua                                    │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│  CREDENTIAL ENCRYPTION SERVICE                              │
│  📄 Auth/Services/CredentialEncryptionService.cs             │
│                                                               │
│  ✓ Encripta as credenciais da BD do tenant:                │
│    - db_server                                              │
│    - db_database                                            │
│    - db_userid                                              │
│    - db_password                                            │
│    - applicense_stamp (não encriptado)                      │
│                                                               │
│  🔐 Usa: Security:EncryptionKey (AES-256)                   │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│  CRIAR JWT TOKEN                                            │
│  📄 Auth/Services/IdentityAuthenticationService.cs           │
│                                                               │
│  ✓ Gera um token JWT que contém:                           │
│    - Username                                               │
│    - Roles                                                  │
│    - Credenciais ENCRIPTADAS como custom claims             │
│    - Data de expiração (3 horas)                            │
│                                                               │
│  🔑 Assina com: JWT:Secret (chave privada)                  │
│                                                               │
│  Token parece assim:                                        │
│  eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ...}.xyz          │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│  RESPOSTA DO LOGIN                                          │
│                                                               │
│  ✅ Status: 200 OK                                           │
│  Body: {                                                    │
│    "token": "eyJhbGciOiJIUzI1NiIs...",                      │
│    "expiration": "2025-04-15T15:30:00Z",                    │
│    "allowed": true,                                         │
│    "outputResponse": { ... }                                │
│  }                                                          │
│                                                               │
│  ⚠️ NOTE: Credenciais NÃO são devolvidas ao cliente!       │
│  (Estão guardadas encriptadas no token)                     │
└─────────────────────────────────────────────────────────────┘
```

---

### **FASE 2: CLIENTE FAZ UMA REQUISIÇÃO NORMAL**

```
┌─────────────────────────────────────────────────────────────┐
│                  CLIENTE FAZ REQUEST                         │
│                                                               │
│  GET /api/dossiers/all                                      │
│  Header: Authorization: Bearer eyJhbGciOi...               │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│  MIDDLEWARE / AUTHENTICATION                                │
│  📄 PHCAPI.Host/Middleware/... ou ASP.NET Core              │
│                                                               │
│  ✓ Extrai o token do header Authorization                  │
│  ✓ Valida assinatura com JWT:Secret                        │
│  ✓ Se inválido → retorna 401 Unauthorized                  │
│  ✓ Se válido → continua                                    │
│  ✓ Coloca claims do token no contexto HTTP (User)          │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│  TENANT CONTEXT (SCOPED - criado por cada request)          │
│  📄 Auth/Services/TenantContext.cs                          │
│                                                               │
│  ✓ Lê o HttpContext (tem access ao User com claims)        │
│  ✓ Extrai claims encriptados:                              │
│    - db_server_encrypted                                   │
│    - db_database_encrypted                                 │
│    - db_userid_encrypted                                   │
│    - db_password_encrypted                                 │
│    - applicense_stamp                                      │
│                                                               │
│  ✓ Desencripta usando Security:EncryptionKey               │
│  ✓ Constrói connection string do tenant                    │
│  ✓ Armazena em memória para esta requisição                │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│  DOSSIERS CONTROLLER                                        │
│  📄 Modules/Dossiers/Controllers/DossiersController.cs      │
│                                                               │
│  ✓ Recebe a requisição                                     │
│  ✓ Faz injeção de DossiersDbContext                        │
│  ✓ Faz injeção de ITenantContext (automático via DI)       │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│  DOSSIERS DB CONTEXT (OnConfiguring)                        │
│  📄 Modules/Dossiers/Persistence/DossiersDbContextEFCore.cs │
│                                                               │
│  ✓ Constructor recebe ITenantContext                        │
│  ✓ OnConfiguring é chamado:                                │
│    - Pergunta ao TenantContext:                             │
│      "Tens credenciais para esta requisição?"              │
│    - TenantContext responde com connection string         │
│      do tenant específico                                  │
│    - DbContext usa .UseSqlServer(connectionString)        │
│                                                               │
│  🔗 Conecta à BD CORRETA do tenant!                         │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│  QUERY À BASE DE DADOS                                      │
│  📄 Modules/Dossiers/Application/Services/...              │
│                                                               │
│  ✓ Executa query SELECT * FROM dossiers                    │
│  ✓ Conecta à BD do tenant específico                       │
│  ✓ Retorna dados DO TENANT (não de outros tenants!)        │
│                                                               │
│  🛡️ ISOLAMENTO GARANTIDO: cada requisição tem sua própria   │
│     instância de DbContext com conexão do seu tenant       │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│  RESPOSTA AO CLIENTE                                        │
│                                                               │
│  ✅ Status: 200 OK                                           │
│  Body: [ { id: 1, nome: "Dossier 1", ... }, ... ]          │
│                                                               │
│  Dados apenas do tenant (banco de dados desse tenant)      │
└─────────────────────────────────────────────────────────────┘
```

---

## 🔐 Ciclo de Vida - Isolamento por Tenant (após login bem-sucedido)

```
REQUEST 1: Tenant A (empresa.pt)
├─ TenantContext lê claims → desencripta → Server=192.168.0.25, DB=TenantA_BD
├─ DossiersDbContext conecta a TenantA_BD
├─ Query retorna dados de TenantA apenas
└─ TenantContext é destruído (fim da requisição)

REQUEST 2: Tenant B (empresa2.pt) - na mesma altura
├─ TenantContext NOVO lê claims → desencripta → Server=192.168.0.25, DB=TenantB_BD
├─ DossiersDbContext NOVO conecta a TenantB_BD
├─ Query retorna dados de TenantB apenas
└─ TenantContext DIFERENTE é destruído

⚠️ CRÍTICO: Cada requisição tem seu próprio TenantContext (SCOPED)
           Impossível "vazar" dados entre tenants
```

---

## 🔄 Fluxo Simplificado em 3 Passos

```
1️⃣ LOGIN
   User faz login → Carrega AppLicense → Encripta credenciais → Retorna JWT

2️⃣ REQUEST NORMAL
   Cliente envia JWT → Server valida JWT → TenantContext desencripta

3️⃣ DATABASE
   DbContext conecta à BD CORRETA do tenant → Query → Dados isolados
```

---

## 📊 Relação entre AspNetUsers e u_applicense

### **A Ligação Crítica**

```
ANTES DO REGISTRO:
┌─────────────────────────┐     ┌──────────────────────────┐
│  AspNetUsers            │     │  u_applicense            │
├─────────────────────────┤     ├──────────────────────────┤
│ (BD primária)           │     │ (BD primária)            │
│                         │     │                          │
│ (vazio)                 │     │ id: 1                    │
│                         │     │ Stamp: "ABC123XYZ"       │
│                         │     │ servidor: "192.168.0.25" │
│                         │     │ AspNetUsersId: NULL      │
│                         │     │                          │
│                         │     │ ❌ Sem utilizador ainda  │
└─────────────────────────┘     └──────────────────────────┘


DEPOIS DO REGISTRO:
┌─────────────────────────────────┐     ┌──────────────────────────┐
│  AspNetUsers                    │     │  u_applicense            │
├─────────────────────────────────┤     ├──────────────────────────┤
│ (BD primária)                   │     │ (BD primária)            │
│                                 │     │                          │
│ Id: "3fa85f64-5717-4562-b3fc"   │     │ id: 1                    │
│ UserName: "isac.munguambe"      │────>│ Stamp: "ABC123XYZ"       │
│ Email: "isac@empresa.pt"        │ 1:1 │ servidor: "192.168.0.25" │
│ PasswordHash: "...xxxxx..."     │     │ AspNetUsersId: "3fa85..." │
│                                 │     │                          │
│                                 │     │ ✅ Utilizador ligado!    │
└─────────────────────────────────┘     └──────────────────────────┘
```

### **Como Funciona o Relacionamento**

| Fase | Operação | Query |
|------|----------|-------|
| **Registration** | 1. Verifica stamp | `SELECT * FROM u_applicense WHERE Stamp = 'ABC123XYZ'` |
|  | 2. Cria user | `INSERT INTO AspNetUsers (UserName, Email, ...)` |
|  | 3. Linca ao tenant | `UPDATE u_applicense SET AspNetUsersId = 'user-123' WHERE Stamp = 'ABC123XYZ'` |
| **Login** | 1. Autentica | `SELECT * FROM AspNetUsers WHERE UserName = 'isac.munguambe'` |
|  | 2. Carrega tenant | `SELECT * FROM u_applicense WHERE AspNetUsersId = user.Id` |
|  | 3. Encripta creds | Encripta servidor, BD, user, password |
|  | 4. Cria JWT | Token com claims encriptados + ExpiraçãoRelação |

### **Dados Cruciais no u_applicense**

Campo | Tipo | Descrição | Exemplo
------|------|-----------|----------
`Stamp` | nvarchar(50) | Identificador único da licença | `ABC123XYZ`
`AspNetUsersId` | nvarchar(50) | **FK para AspNetUsers.Id** | `3fa85f64-5717-4562-b3fc`
`Name` | nvarchar(100) | Nome da empresa/tenant | `Empresa ABC`
`DbServer` | nvarchar(100) | Servidor SQL do tenant | `192.168.0.25\SQLDEV2022`
`DbDatabase` | nvarchar(100) | Nome da BD do tenant | `OnDEV_2Business`
`DbUserId` | nvarchar(100) | User SQL de acesso | `websa`
`DbPassword` | nvarchar(100) | Password SQL (encriptado em JWT) | `YOUR_PASSWORD`
`ApiAccessEnabled` | bit | Permite acesso API? | `1` (true)
`ApiAccessStartDate` | datetime | Quando começa acesso | `2024-01-01`
`ApiAccessEndDate` | datetime | Quando termina acesso | `2025-12-31`

---

## ⚙️ Como Tudo Funciona Junto (Agora com AspNetUsersId)

```
APLICAÇÃO INICIADA
    ↓
Program.cs Registra DependencyInjection:
    ├─ PrimaryDbContext (BD primária)
    ├─ CredentialEncryptionService (chave do appsettings)
    ├─ TenantContext (SCOPED - importante!)
    ├─ AppLicenseRepository
    └─ AuthenticationService
    ↓
CENÁRIO 1: NOVO UTILIZADOR (REGISTRATION)
    ├─ POST /api/auth/register-application-user
    ├─ AuthController valida dados
    ├─ IdentityAuthenticationService.RegisterApplicationUserAsync():
    │  ├─ Valida stamp em u_applicense
    │  ├─ Cria novo IdentityUser em AspNetUsers
    │  ├─ UPDATE u_applicense SET AspNetUsersId = novo user.Id
    │  └─ Retorna sucesso
    └─ Utilizador agora pode fazer LOGIN
    ↓
CENÁRIO 2: LOGIN EXISTENTE
    ├─ POST /api/auth/login
    ├─ AuthController recebe username + password
    ├─ IdentityAuthenticationService.LoginAsync():
    │  ├─ Valida username/password em AspNetUsers
    │  ├─ Obtém user.Id
    │  ├─ SELECT u_applicense WHERE AspNetUsersId = user.Id
    │  ├─ Encripta credenciais (DbServer, DbDatabase, DbUserId, DbPassword)
    │  ├─ Cria JWT com claims encriptados
    │  └─ Retorna token
    └─ Utilizador recebe JWT
    ↓
CENÁRIO 3: REQUEST COM JWT VÁLIDO
    ├─ Cliente envia GET /api/dossiers/all + Authorization: Bearer JWT
    ├─ Middleware valida JWT (JWT:Secret)
    ├─ TenantContext é criado NOVO (SCOPED)
    ├─ TenantContext lê User.Claims (do JWT)
    ├─ TenantContext desencripta credenciais (Security:EncryptionKey)
    ├─ Controller/DbContext usa TenantContext para conexão
    ├─ Query é executada na BD do tenant específico
    ├─ Resposta é enviada
    └─ TenantContext é destruído (FIM DO SCOPED)
```

---
