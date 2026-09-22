# Auth.UI - Authentication User Interface

Interface web baseada na biblioteca oficial **Microsoft.AspNetCore.Identity.UI** para gestão completa de usuários e autenticação.

## 📋 Visão Geral

Este módulo usa as **páginas oficiais da Microsoft** embeddadas na biblioteca `Microsoft.AspNetCore.Identity.UI`, fornecendo uma solução completa, testada e mantida pela Microsoft.

### ✅ Páginas de Conta (Account)

Todas disponíveis em `/Identity/Account/`:

| Página | Rota | Descrição |
|--------|------|-----------|
| **Login** | `/Identity/Account/Login` | Autenticação de usuários |
| **Logout** | `/Identity/Account/Logout` | Terminar sessão |
| **Register** | `/Identity/Account/Register` | Registo de novos usuários |
| **ForgotPassword** | `/Identity/Account/ForgotPassword` | Recuperação de password |
| **ResetPassword** | `/Identity/Account/ResetPassword` | Redefinir password |
| **ConfirmEmail** | `/Identity/Account/ConfirmEmail` | Confirmação de email |
| **Lockout** | `/Identity/Account/Lockout` | Conta bloqueada |
| **AccessDenied** | `/Identity/Account/AccessDenied` | Acesso negado |

### ✅ Páginas de Gestão (Manage)

Todas disponíveis em `/Identity/Account/Manage/`:

| Página | Rota | Descrição |
|--------|------|-----------|
| **Profile** | `/Identity/Account/Manage/Index` | Editar perfil |
| **Change Password** | `/Identity/Account/Manage/ChangePassword` | Alterar password |
| **Email** | `/Identity/Account/Manage/Email` | Gerir email |
| **Personal Data** | `/Identity/Account/Manage/PersonalData` | Dados pessoais |
| **Two-Factor Auth** | `/Identity/Account/Manage/TwoFactorAuthentication` | Autenticação 2FA |
| **Delete Account** | `/Identity/Account/Manage/DeletePersonalData` | Eliminar conta |

## 🏗️ Arquitetura

```
Auth.UI/
├── DependencyInjection.cs             # Configuração modular
├── Auth.UI.csproj
└── README.md

Páginas embeddadas (não visíveis no projeto):
Microsoft.AspNetCore.Identity.UI/Areas/Identity/Pages/
├── Account/
│   ├── Login.cshtml
│   ├── Register.cshtml
│   ├── ForgotPassword.cshtml
│   ├── ResetPassword.cshtml
│   └── Manage/
│       ├── Index.cshtml
│       ├── ChangePassword.cshtml
│       ├── Email.cshtml
│       └── TwoFactorAuthentication.cshtml
└── _Layout.cshtml (via Bootstrap)
```

## 🚀 Utilização

### 1. Configuração

O módulo já está configurado no `Program.cs`:

```csharp
// Adicionar serviços (após AddAuth...)
builder.Services.AddAuthUI();

// Mapear endpoints (no final, com MapControllers)
app.MapAuthUI();
```

### 2. Acesso às Páginas

Todas as páginas estão disponíveis automaticamente:

```
http://localhost:7298/Identity/Account/Login
http://localhost:7298/Identity/Account/Register
http://localhost:7298/Identity/Account/Manage/Index
```

### 3. Credenciais Padrão

O sistema seed cria um utilizador admin:

```
Username: admin
Password: YOUR_PASSWORD
```

⚠️ **IMPORTANTE:** Altere em produção!

## 🎨 Personalização

### Sobrescrever Páginas

Para customizar uma página específica, use scaffold no projeto host:

```bash
cd src/PHCAPI.Host
dotnet aspnet-codegenerator identity -dc Auth.Infrastructure.Persistence.AuthDbContext --files "Account.Login;Account.Register"
```

Isso cria cópias locais das páginas que você pode editar.

### Customizar Layout

Para aplicar tema próprio, crie `Areas/Identity/Pages/_ViewStart.cshtml` no projeto host:

```csharp
@{
    Layout = "/Views/Shared/_Layout.cshtml"; // Seu layout personalizado
}
```

### Desabilitar Páginas

Para desabilitar Register (ex: apenas admin cria users), não forneça link público. A página ainda existe mas usuários não encontram.

## 🔧 Dependências

```xml
<PackageReference Include="Microsoft.AspNetCore.Identity.UI" Version="8.0.11" />
<PackageReference Include="Microsoft.VisualStudio.Web.CodeGeneration.Design" Version="8.0.8" />
```

**Referências de Projeto:**
- `Auth.Infrastructure` → Para `AuthDbContext`
- `Auth.Domain` → Para entidades

## 📝 Vantagens desta Abordagem

✅ **Páginas oficiais Microsoft** - Testadas e mantidas  
✅ **Funcionalidades completas** - Login, Register, 2FA, Recovery, etc.  
✅ **Atualizações automáticas** - Ao atualizar pacote  
✅ **Sem código duplicado** - Páginas embedadas  
✅ **Scaffold sob demanda** - Customize apenas o necessário  
✅ **Arquitetura modular** - DependencyInjection pattern  

## 🔗 Integração com API

O Auth.UI funciona paralelamente com Auth.Presentation (API):

- **Auth.UI** → Interface web para browsers (Cookie Authentication)
- **Auth.Presentation** → API REST para aplicações (JWT Bearer)

Ambos usam o mesmo `AuthDbContext` e `UserManager<IdentityUser>`.

## 📚 Documentação Microsoft

Para detalhes sobre as páginas embeddadas:
- [ASP.NET Core Identity UI](https://docs.microsoft.com/aspnet/core/security/authentication/identity)
- [Scaffold Identity](https://docs.microsoft.com/aspnet/core/security/authentication/scaffold-identity)
- [Customize Identity UI](https://docs.microsoft.com/aspnet/core/security/authentication/customize-identity-model)
