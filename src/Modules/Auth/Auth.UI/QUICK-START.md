# 🚀 Auth.UI - Quick Start

Guia rápido para usar o sistema de autenticação web (páginas oficiais Microsoft Identity UI).

## ✅ 1. Verificar Configuração

O módulo já está configurado no `Program.cs`:

```csharp
builder.Services.AddAuthUI();  // Linha ~78
app.MapAuthUI();               // Linha ~328
```

## ✅ 2. Iniciar Aplicação

```bash
cd src/PHCAPI.Host
dotnet run
```

Ou pressione **F5** no Visual Studio.

## ✅ 3. Testar Páginas

### Login
1. Abra: `http://localhost:7298/Identity/Account/Login`
2. Credenciais padrão:
   - **Username:** `admin`
   - **Password:YOUR_PASSWORD `CHANGE_ME_PASSWORD
3. Clique **Log in**

### Register (Criar Novo User)
1. Abra: `http://localhost:7298/Identity/Account/Register`
2. Preencha:
   - Email: `seuemail@example.com`
   - Password: YOUR_PASSWORD 6 caracteres, letra maiúscula, número
3. Clique **Register**

### Change Password
1. Faça login primeiro
2. Abra: `http://localhost:7298/Identity/Account/Manage/ChangePassword`
3. Preencha os campos
4. Clique **Update password**

### Outros Recursos
- **Profile:** `/Identity/Account/Manage/Index`
- **Email:** `/Identity/Account/Manage/Email`
- **Two-Factor:** `/Identity/Account/Manage/TwoFactorAuthentication`
- **Logout:** `/Identity/Account/Logout`

## 📸 Páginas Disponíveis

| Categoria | Página | URL |
|-----------|--------|-----|
| **Account** | Login | `/Identity/Account/Login` |
| | Register | `/Identity/Account/Register` |
| | Logout | `/Identity/Account/Logout` |
| | Forgot Password | `/Identity/Account/ForgotPassword` |
| | Reset Password | `/Identity/Account/ResetPassword` |
| **Manage** | Profile | `/Identity/Account/Manage/Index` |
| | Change Password | `/Identity/Account/Manage/ChangePassword` |
| | Email | `/Identity/Account/Manage/Email` |
| | Two-Factor Auth | `/Identity/Account/Manage/TwoFactorAuthentication` |
| | Personal Data | `/Identity/Account/Manage/PersonalData` |

## 🎨 Personalizar Páginas

Se precisar customizar alguma página (ex: Login):

```bash
cd src/PHCAPI.Host
dotnet aspnet-codegenerator identity -dc Auth.Infrastructure.Persistence.AuthDbContext --files "Account.Login"
```

Isso cria uma cópia local em `Areas/Identity/Pages/Account/Login.cshtml` que você pode editar.

## ⚠️ Troubleshooting

### Página não encontra (404)
- Confirme que `app.UseRouting()` está ANTES de `app.MapAuthUI()`
- Confirme que `app.MapRazorPages()` está sendo chamado

### Erro "No DbContext"
- Verifique se `AddAuthInfrastructure()` está antes de `AddAuthUI()`

### Cookie não persiste
- Verifique se `app.UseAuthentication()` está ANTES de `app.UseAuthorization()`

## 📚 Mais Informações

- [README.md](README.md) - Documentação completa
- [Microsoft Identity UI Docs](https://docs.microsoft.com/aspnet/core/security/authentication/identity)
