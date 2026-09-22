# 🧪 Guia de Teste: Auditoria Admin.UI

## 🎯 Objetivo
Verificar se todas as operações do Admin.UI estão sendo registradas na tabela `AuditLogs` com os dados corretos.

---

## 📝 Pré-requisitos
1. Aplicação PHCAPI rodando
2. Acesso ao banco de dados (SQL Server)
3. Usuário admin criado

---

## ✅ Teste 1: Login Admin.UI

### Passos:
1. Acesse: `https://localhost:5001/Admin/Account/Login`
2. Faça login com credenciais válidas
3. Execute a query:

```sql
SELECT TOP 1 
    Action,
    IpAddress,
    UserAgent,
    StatusCode,
    RequestBody,
    ResponseBody,
    Timestamp
FROM AuditLogs
WHERE Action LIKE '%Login%'
ORDER BY Timestamp DESC;
```

### ✅ Resultado Esperado:
```json
{
  "Action": "Admin UI - User Login Attempt",
  "IpAddress": "::1" ou "127.0.0.1",
  "UserAgent": "Mozilla/5.0...",
  "StatusCode": 302,
  "RequestBody": "...",
  "ResponseBody": {
    "response": {
      "code": "0000",
      "description": "Admin UI - User Login Attempt"
    },
    "data": {
      "action": "User Login Attempt",
      "user": {
        "username": "admin@phcapi.com",
        "isAuthenticated": true
      },
      "network": {
        "ipAddress": "::1",
        "userAgent": "Mozilla/5.0..."
      }
    }
  }
}
```

---

## ✅ Teste 2: Criar Novo Usuário

### Passos:
1. Vá para: `https://localhost:5001/Admin/Users`
2. Clique em "Create New User"
3. Preencha o formulário:
   - Username: `teste@phcapi.com`
   - Email: `teste@phcapi.com`
   - Password: YOUR_PASSWORD
4. Clique em "Create"
5. Execute a query:

```sql
SELECT TOP 1 
    Action,
    IpAddress,
    RequestBody,
    ResponseBody,
    Timestamp
FROM AuditLogs
WHERE Action LIKE '%Create User%'
ORDER BY Timestamp DESC;
```

### ✅ Resultado Esperado:
- `Action`: "Admin UI - Create User"
- `IpAddress`: Deve estar preenchido
- `RequestBody`: Deve conter form data
- `ResponseBody.data.request.formData`: Password deve estar como `***REDACTED***`

```json
{
  "data": {
    "action": "Create User",
    "user": {
      "username": "admin@phcapi.com",
      "roles": ["Admin"]
    },
    "request": {
      "formData": {
        "username": "teste@phcapi.com",
        "email": "teste@phcapi.com",
        "password": ""
      }
    },
    "network": {
      "ipAddress": "::1"
    }
  }
}
```

---

## ✅ Teste 3: Listar Usuários (GET)

### Passos:
1. Acesse: `https://localhost:5001/Admin/Users`
2. Execute a query:

```sql
SELECT TOP 1 
    Action,
    IpAddress,
    ResponseBody,
    Timestamp
FROM AuditLogs
WHERE Action LIKE '%List Users%'
ORDER BY Timestamp DESC;
```

### ✅ Resultado Esperado:
- `Action`: "Admin UI - List Users"
- `IpAddress`: Deve estar preenchido
- `ResponseBody.data.action`: "List Users"
- `ResponseBody.data.user.username`: Nome do usuário logado

---

## ✅ Teste 4: Criar Role

### Passos:
1. Vá para: `https://localhost:5001/Admin/Roles`
2. Clique em "Create New Role"
3. Preencha:
   - Name: `TestRole`
   - Description: `Role de teste`
4. Clique em "Create"
5. Execute a query:

```sql
SELECT TOP 1 
    Action,
    IpAddress,
    RequestBody,
    ResponseBody,
    Timestamp
FROM AuditLogs
WHERE Action LIKE '%Create Role%'
ORDER BY Timestamp DESC;
```

### ✅ Resultado Esperado:
- `Action`: "Admin UI - Create Role"
- `IpAddress`: Deve estar preenchido
- `ResponseBody.data.action`: "Create Role"
- `ResponseBody.content.category`: "Role Management"

---

## ✅ Teste 5: Logout

### Passos:
1. Clique em "Logout" no Admin.UI
2. Execute a query:

```sql
SELECT TOP 1 
    Action,
    IpAddress,
    StatusCode,
    Timestamp
FROM AuditLogs
WHERE Action LIKE '%Logout%'
ORDER BY Timestamp DESC;
```

### ✅ Resultado Esperado:
- `Action`: "Admin UI - User Logout"
- `IpAddress`: Deve estar preenchido
- `StatusCode`: 302 (redirect)

---

## 📊 Teste Completo: Ver Todas as Operações

```sql
-- Ver todas as operações do Admin.UI nos últimos 30 minutos
SELECT 
    Action,
    IpAddress,
    CAST(ResponseBody AS NVARCHAR(MAX)) AS ResponseBody,
    StatusCode,
    Timestamp
FROM AuditLogs
WHERE Action LIKE '%Admin UI%'
  AND Timestamp >= DATEADD(MINUTE, -30, GETUTCDATE())
ORDER BY Timestamp DESC;
```

---

## 🔍 Verificações Importantes

### ✅ Checklist de Dados:
- [ ] **IP Address** está capturado em todas as operações
- [ ] **User Agent** está presente
- [ ] **Username** do usuário autenticado está correto
- [ ] **Passwords** estão como `***REDACTED***`
- [ ] **Action** descreve claramente a operação
- [ ] **StatusCode** está correto (200, 302, etc.)
- [ ] **Timestamp** está em UTC
- [ ] **Correlation ID** está presente

### ✅ Operações que DEVEM ser auditadas:
- [ ] Login (POST)
- [ ] Logout (POST/GET)
- [ ] Create User (POST)
- [ ] Edit User (PUT/POST)
- [ ] Delete User (DELETE/POST)
- [ ] List Users (GET)
- [ ] View User Details (GET)
- [ ] Create Role (POST)
- [ ] Edit Role (PUT/POST)
- [ ] Delete Role (DELETE/POST)
- [ ] List Roles (GET)
- [ ] Assign Permissions (POST)

### ❌ Operações que NÃO devem ser auditadas:
- [ ] Requisições de assets estáticos (CSS, JS, imagens)
- [ ] Health checks
- [ ] Swagger UI

---

## 🐛 Troubleshooting

### Problema: Nenhum registro em AuditLogs
**Solução:**
1. Verifique se o serviço `IAuditLogService` está registrado
2. Verifique os logs da aplicação: `logs/PHCAPI-{Date}.txt`
3. Procure por `[MIDDLEWARE START]` e `[MIDDLEWARE END]`

### Problema: IP Address está NULL
**Solução:**
- Se rodando localmente, é normal aparecer `::1` (IPv6 localhost)
- Em produção, deve aparecer o IP real do cliente

### Problema: Password não está redacted
**Solução:**
- Verifique o método `SanitizeFormData()` no middleware
- Adicione o campo específico do formulário se necessário

---

## ✅ Teste Passou!

Se todos os testes acima passaram, a auditoria do Admin.UI está funcionando corretamente e você tem:

✅ Rastreabilidade completa de todas as operações administrativas  
✅ Dados de fiscalização (IP, User, Action)  
✅ Segurança (passwords redacted)  
✅ Compliance e auditoria prontos para inspeção
