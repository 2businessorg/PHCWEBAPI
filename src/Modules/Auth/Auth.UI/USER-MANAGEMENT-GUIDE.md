# 👥 Gestão de Utilizadores - PHCAPI

## 🔒 Abordagem de Segurança

✅ **Registro público DESABILITADO**  
❌ Qualquer pessoa NÃO pode se registar  
✅ Apenas **Administradores** podem criar usuários via API

---

## 🚀 Como Criar o Primeiro Administrador

### 1. Usar o Seeding existente

O sistema já cria automaticamente na inicialização:

```csharp
// Em Auth.Infrastructure/Data/AuthDbContextSeed.cs
Username: "admin"
Password: "YOUR_PASSWORD"
Role: Administrator
```

### 2. Fazer login e obter token

```bash
POST http://localhost:7298/api/authenticate/login
Content-Type: application/json

{
  "username": "",
  "password": ""
}
```

**Resposta:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "expiration": "2026-02-14T10:00:00Z",
  "allowed": true,
  "outputResponse": "AUTHENTICATED",
  "roles": ["Administrator"]
}
```

---

## 👤 Criar Novos Utilizadores

### ✅ Apenas via API (Requer token de Admin)

```bash
POST http://localhost:7298/api/authenticate/register
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
Content-Type: application/json

{
  "username": "joao.silva",
  "email": "joao.silva@empresa.com",
  "password": ""
}
```

**Resposta:**
```json
{
  "success": true,
  "message": "User created successfully",
  "userId": "a1b2c3d4-...",
  "errors": []
}
```

---

## 🎭 Gestão de Roles

### 1. Listar Roles Disponíveis

```bash
GET http://localhost:7298/api/authenticate/roles
Authorization: Bearer {admin-token}
```

**Resposta:**
```json
[
  "Administrator",
  "ApiUser",
  "InternalUser",
  "AuditViewer"
]
```

### 2. Criar Nova Role

```bash
POST http://localhost:7298/api/authenticate/roles
Authorization: Bearer {admin-token}
Content-Type: application/json

{
  "roleName": "ReportsViewer"
}
```

### 3. Adicionar Role a Utilizador

```bash
POST http://localhost:7298/api/authenticate/users/add-role
Authorization: Bearer {admin-token}
Content-Type: application/json

{
  "username": "joao.silva",
  "role": "ApiUser"
}
```

### 4. Ver Roles de um Utilizador

```bash
GET http://localhost:7298/api/authenticate/users/joao.silva/roles
Authorization: Bearer {admin-token}
```

**Resposta:**
```json
["ApiUser"]
```

---

## 🔐 Políticas de Autorização

Definidas em `Shared.Kernel.Authorization`:

| Política | Roles Permitidas |
|----------|------------------|
| **AdminOnly** | Administrator |
| **InternalOnly** | Administrator, InternalUser, AuditViewer |
| **ApiAccess** | Administrator, ApiUser, InternalUser |
| **Authenticated** | Qualquer utilizador autenticado |

---

## 📋 Roles Standard

| Role | Descrição | Acesso |
|------|-----------|--------|
| **Administrator** | Administrador do sistema | Tudo |
| **ApiUser** | Utilizador externo da API | Endpoints públicos da API |
| **InternalUser** | Utilizador interno | API + módulos internos |
| **AuditViewer** | Visualizador de auditorias | Dashboard de auditoria |

---

## 🧪 Exemplos de Uso - cURL

### Criar Utilizador

```bash
curl -X POST http://localhost:7298/api/authenticate/register \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIs..." \
  -H "Content-Type: application/json" \
  -d '{
    "username": "maria.costa",
    "email": "maria.costa@empresa.com", 
    "password": ""
  }'
```

### Adicionar Role

```bash
curl -X POST http://localhost:7298/api/authenticate/users/add-role \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIs..." \
  -H "Content-Type: application/json" \
  -d '{
    "username": "maria.costa",
    "role": "ApiUser"
  }'
```

---

## 🎯 Fluxo Recomendado

```mermaid
graph TD
    A[Admin faz login] --> B[Obtém JWT Token]
    B --> C[Cria novo utilizador via API]
    C --> D[Atribui roles ao utilizador]
    D --> E[Utilizador faz login]
    E --> F[Utilizador acede à API com token]
```

### Passo a Passo

1. **Admin autentica-se:**
   - Username: `admin`
   - Password: YOUR_PASSWORD
   - Recebe JWT token

2. **Admin cria novo utilizador:**
   - POST `/api/authenticate/register`
   - Com token de admin no header

3. **Admin atribui roles:**
   - POST `/api/authenticate/users/add-role`
   - Define permissões do utilizador

4. **Novo utilizador acede:**
   - POST `/api/authenticate/login`
   - Recebe seu próprio token
   - Usa token para chamar API

---

## 🛡️ Segurança

✅ **O que está protegido:**
- Criação de utilizadores (AdminOnly)
- Gestão de roles (AdminOnly)
- Páginas web de Register (Removido da UI)
- Atribuição de permissões (AdminOnly)

✅ **Apenas públicos:**
- `/api/authenticate/login` - Login
- `/Identity/Account/Login` - Página de login web
- `/swagger` - Documentação API

---

## 💡 Dicas

1. **Altere a senha do admin** após primeiro login
2. **Crie roles específicas** para diferentes níveis de acesso
3. **Use ApiUser** para integrações externas
4. **Use InternalUser** para utilizadores da empresa
5. **Use AuditViewer** apenas para visualização de logs

---

## 📚 Documentação Adicional

- [Auth Module README](../README.md)
- [Authorization Policies](../../../../Shared/Shared.Kernel/Shared.Kernel/Authorization/AppPolicies.cs)
- [Swagger UI](http://localhost:7298/swagger) - Testar endpoints
