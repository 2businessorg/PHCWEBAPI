# 🔌 Arquitetura de Comunicação: Admin UI ↔️ Backend

## 📐 Visão Geral da Arquitetura

### 🎯 Tipo de Aplicação: **Server-Side Rendering (SSR)**

O Admin UI **NÃO É** uma Single Page Application (SPA). É uma aplicação **Razor Pages** tradicional onde:

```
┌─────────────────────────────────────────────────────────────┐
│  Browser (Cliente)                                          │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  HTML renderizado pelo servidor                       │  │
│  │  (Não há JavaScript fazendo API calls)                │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                           ↕️ HTTP Request/Response
┌─────────────────────────────────────────────────────────────┐
│  Servidor ASP.NET Core                                      │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  Razor Pages (Login.cshtml.cs, Index.cshtml.cs)      │  │
│  │           ↕️ Dependency Injection                     │  │
│  │  Services (UserManager, SignInManager, etc)          │  │
│  │           ↕️ Entity Framework Core                    │  │
│  │  Database (SQL Server - Identity tables)             │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
```

**Importante:** Não há chamadas AJAX/Fetch/API entre cliente e servidor. Tudo é processado no servidor!

---

## 🔐 Exemplo 1: Login (Autenticação)

### 📁 Arquivos Envolvidos

```
src/Admin.UI/Pages/Account/
├── Login.cshtml         ← View (HTML + Razor)
└── Login.cshtml.cs      ← PageModel (Lógica no servidor)
```

### 🔄 Fluxo Completo do Login

#### 1️⃣ **GET Request (Carregar página)**

```http
GET /Admin/Account/Login
```

**O que acontece:**

```csharp
// Login.cshtml.cs
public async Task OnGetAsync(string? returnUrl = null)
{
    // 1. Limpa cookies externos
    await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
    
    // 2. Define para onde redirecionar após login
    ReturnUrl = string.IsNullOrEmpty(returnUrl) ? "/Admin/Users" : returnUrl;
    
    // 3. Razor renderiza Login.cshtml e envia HTML para o browser
}
```

**Resultado:** Browser recebe HTML completo do formulário de login.

---

#### 2️⃣ **POST Request (Submeter formulário)**

**Browser:**
```html
<form method="post">
    <input name="Input.Username" value="admin@phcapi.com" />
    <input name="Input.Password" value="CHANGE_ME_PASSWORD" />
    <input name="Input.RememberMe" type="checkbox" />
    <button type="submit">Iniciar sessão</button>
</form>
```

**Request HTTP:**
```http
POST /Admin/Account/Login
Content-Type: application/x-www-form-urlencoded

Input.Username=admin@phcapi.com&Input.Password=YOUR_PASSWORD
```

---

#### 3️⃣ **Processamento no Servidor**

```csharp
// Login.cshtml.cs
public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
{
    // ✅ STEP 1: Validar dados do formulário
    if (!ModelState.IsValid)
        return Page(); // Retorna formulário com erros
    
    // ✅ STEP 2: Tentar autenticar com Identity
    var result = await _signInManager.PasswordSignInAsync(
        Input.Username,      // "admin@phcapi.com"
        Input.Password,      // "CHANGE_ME_PASSWORD"
        Input.RememberMe,    // true
        lockoutOnFailure: false
    );
    
    // ✅ STEP 3: Verificar resultado
    if (result.Succeeded)
    {
        // 🎉 Login bem-sucedido!
        // 1. SignInManager criou cookie de autenticação
        // 2. Cookie é enviado automaticamente no response
        // 3. Redirecionar para /Admin/Users
        return LocalRedirect("/Admin/Users");
    }
    else
    {
        // ❌ Login falhou
        ErrorMessage = "Nome de utilizador ou password incorretos";
        return Page(); // Re-renderiza formulário com erro
    }
}
```

---

#### 4️⃣ **Dependency Injection (Como os serviços são injetados)**

```csharp
public class LoginModel : PageModel
{
    // ⬇️ Estes serviços são INJETADOS automaticamente pelo ASP.NET Core
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly ILogger<LoginModel> _logger;

    // 🏗️ Construtor: ASP.NET Core resolve as dependências
    public LoginModel(
        SignInManager<IdentityUser> signInManager,  // ← Vem do DI Container
        ILogger<LoginModel> logger)                 // ← Vem do DI Container
    {
        _signInManager = signInManager;
        _logger = logger;
    }
}
```

**De onde vem o `SignInManager`?**

```csharp
// src/Modules/Auth/Auth.Infrastructure/DependencyInjection.cs
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    // Configurações do Identity
})
.AddEntityFrameworkStores<AuthDbContext>()  // ← Configura EF Core
.AddSignInManager()                          // ← Registra SignInManager
.AddDefaultTokenProviders();
```

---

#### 5️⃣ **O que o SignInManager faz internamente?**

```csharp
// Simplificado - o que acontece dentro do SignInManager
public async Task<SignInResult> PasswordSignInAsync(
    string userName, 
    string password, 
    bool isPersistent, 
    bool lockoutOnFailure)
{
    // 1. Buscar utilizador na base de dados
    var user = await _userManager.FindByNameAsync(userName);
    if (user == null)
        return SignInResult.Failed;
    
    // 2. Verificar password (hash comparison)
    var isPasswordCorrect = await _userManager.CheckPasswordAsync(user, password);
    if (!isPasswordCorrect)
        return SignInResult.Failed;
    
    // 3. Criar cookie de autenticação
    await _signInManager.SignInAsync(user, isPersistent);
    
    // 4. Retornar sucesso
    return SignInResult.Success;
}
```

**Query SQL executada (Entity Framework):**
```sql
SELECT TOP(1) [u].[Id], [u].[UserName], [u].[PasswordHash], ...
FROM [AspNetUsers] AS [u]
WHERE [u].[NormalizedUserName] = N'ADMIN@PHCAPI.COM'
```

---

## 👥 Exemplo 2: Listar Utilizadores

### 📁 Arquivos Envolvidos

```
src/Admin.UI/Pages/Users/
├── Index.cshtml         ← View (Tabela HTML)
└── Index.cshtml.cs      ← PageModel (Buscar dados)
```

### 🔄 Fluxo Completo

#### 1️⃣ **GET Request**

```http
GET /Admin/Users
Cookie: .AspNetCore.Identity.Application=CfDJ8K... (cookie de autenticação)
```

---

#### 2️⃣ **Processamento no Servidor**

```csharp
// Users/Index.cshtml.cs
[Authorize(Roles = "Administrator")]  // ← Verifica permissões ANTES de executar
public class IndexModel : PageModel
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    // 🏗️ Dependency Injection
    public IndexModel(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    // 📊 Dados que serão enviados para a View
    public List<UserViewModel> Users { get; set; } = new();
    public List<string> AllRoles { get; set; } = new();

    // 🚀 Método executado ao receber GET request
    public async Task OnGetAsync()
    {
        // ✅ STEP 1: Buscar todos os utilizadores da base de dados
        var users = _userManager.Users.ToList();
        
        // ✅ STEP 2: Buscar todas as roles
        AllRoles = _roleManager.Roles.Select(r => r.Name!).ToList();

        // ✅ STEP 3: Para cada utilizador, buscar suas roles
        Users = new List<UserViewModel>();
        foreach (var user in users)
        {
            // Query para buscar roles do utilizador
            var roles = await _userManager.GetRolesAsync(user);
            
            Users.Add(new UserViewModel
            {
                Id = user.Id,
                UserName = user.UserName!,
                Email = user.Email!,
                EmailConfirmed = user.EmailConfirmed,
                LockoutEnd = user.LockoutEnd,
                Roles = roles.ToList()  // ["Administrator", "InternalUser"]
            });
        }
        
        // ✅ STEP 4: Razor renderiza Index.cshtml com estes dados
        // e envia HTML completo para o browser
    }
}
```

---

#### 3️⃣ **Queries SQL Executadas (automaticamente pelo EF Core)**

```sql
-- 1. Buscar todos os utilizadores
SELECT [u].[Id], [u].[UserName], [u].[Email], [u].[EmailConfirmed], 
       [u].[LockoutEnd], [u].[AccessFailedCount], ...
FROM [AspNetUsers] AS [u]

-- 2. Buscar todas as roles
SELECT [r].[Id], [r].[Name], [r].[NormalizedName]
FROM [AspNetRoles] AS [r]

-- 3. Para cada utilizador, buscar suas roles (loop de N queries)
SELECT [r].[Name]
FROM [AspNetUserRoles] AS [ur]
INNER JOIN [AspNetRoles] AS [r] ON [ur].[RoleId] = [r].[Id]
WHERE [ur].[UserId] = @UserId  -- Repete para cada user

-- Resultado:
-- User 1: ["Administrator", "InternalUser"]
-- User 2: ["ExternalUser"]
-- User 3: ["Administrator"]
```

---

#### 4️⃣ **Renderização da View**

```razor
@* Index.cshtml *@
@page "/Admin/Users"
@model Admin.UI.Pages.Users.IndexModel

<h1>Utilizadores</h1>

<table>
    <thead>
        <tr>
            <th>Username</th>
            <th>Email</th>
            <th>Roles</th>
        </tr>
    </thead>
    <tbody>
        @foreach (var user in Model.Users)  @* ← Dados vêm do PageModel *@
        {
            <tr>
                <td>@user.UserName</td>
                <td>@user.Email</td>
                <td>
                    @foreach (var role in user.Roles)
                    {
                        <span class="badge">@role</span>
                    }
                </td>
            </tr>
        }
    </tbody>
</table>
```

**HTML gerado (enviado para o browser):**

```html
<h1>Utilizadores</h1>
<table>
    <thead>
        <tr>
            <th>Username</th>
            <th>Email</th>
            <th>Roles</th>
        </tr>
    </thead>
    <tbody>
        <tr>
            <td>admin@phcapi.com</td>
            <td>admin@phcapi.com</td>
            <td>
                <span class="badge">Administrator</span>
            </td>
        </tr>
        <tr>
            <td>john.doe@example.com</td>
            <td>john.doe@example.com</td>
            <td>
                <span class="badge">InternalUser</span>
                <span class="badge">AuditViewer</span>
            </td>
        </tr>
    </tbody>
</table>
```

---

## 🔄 Diferença: SSR vs SPA (API)

### ❌ Como **NÃO** funciona (SPA - React/Angular/Vue):

```javascript
// Frontend (JavaScript no browser)
async function loadUsers() {
    const response = await fetch('/api/users', {
        headers: { 'Authorization': 'Bearer ' + token }
    });
    const users = await response.json();
    renderUsersTable(users);  // Atualiza DOM
}
```

### ✅ Como **FUNCIONA** no Admin UI (Razor Pages SSR):

```csharp
// Backend (C# no servidor)
public async Task OnGetAsync()
{
    var users = _userManager.Users.ToList();
    Users = MapToViewModel(users);
    // Razor renderiza HTML completo e envia para browser
}
```

**Vantagens do SSR:**
- ✅ Sem necessidade de criar APIs separadas
- ✅ Segurança: lógica de negócio nunca exposta ao cliente
- ✅ SEO-friendly
- ✅ Menos JavaScript = carregamento mais rápido
- ✅ Autenticação via cookies (mais seguro que tokens no localStorage)

**Desvantagens:**
- ❌ Cada ação recarrega a página (sem SPA smoothness)
- ❌ Não há interatividade rich do lado do cliente

---

## 🏗️ Dependency Injection: Como funciona?

### 1. **Registo de Serviços (Startup)**

```csharp
// Program.cs ou DependencyInjection.cs
public static IServiceCollection AddAuthInfrastructure(this IServiceCollection services)
{
    // ✅ Registar Identity (UserManager, SignInManager, RoleManager)
    services.AddIdentity<IdentityUser, IdentityRole>(options => { ... })
            .AddEntityFrameworkStores<AuthDbContext>();
    
    // ✅ Registar DbContext
    services.AddDbContext<AuthDbContext>(options =>
        options.UseSqlServer(connectionString));
    
    return services;
}
```

### 2. **Resolução Automática (Constructor Injection)**

```csharp
public class IndexModel : PageModel
{
    // 🎯 ASP.NET Core vê estas dependências no construtor
    public IndexModel(
        UserManager<IdentityUser> userManager,     // ← Preciso disto
        RoleManager<IdentityRole> roleManager)     // ← E disto
    {
        // 🏗️ Framework procura no DI Container:
        // - "Alguém registou UserManager?" → Sim! (AddIdentity)
        // - "Alguém registou RoleManager?" → Sim! (AddIdentity)
        // - Cria instâncias e injeta automaticamente
    }
}
```

### 3. **Lifetime dos Serviços**

```csharp
// Scoped: Uma instância por request HTTP
services.AddDbContext<AuthDbContext>();  // ← Scoped por padrão

// Singleton: Uma única instância para toda a aplicação
services.AddSingleton<ICacheService, RedisCacheService>();

// Transient: Nova instância sempre que solicitada
services.AddTransient<IEmailService, EmailService>();
```

**No Admin UI:**
- `UserManager` → **Scoped** (cada request tem sua instância)
- `RoleManager` → **Scoped**
- `SignInManager` → **Scoped**
- `AuthDbContext` → **Scoped** (evita problemas de concorrência)

---

## 📊 Diagrama de Fluxo Completo (Login)

```
┌─────────────┐
│   Browser   │
└──────┬──────┘
       │ 1. GET /Admin/Account/Login
       ↓
┌──────────────────────────────────────────────┐
│  ASP.NET Core Middleware Pipeline            │
│  ┌────────────────────────────────────────┐  │
│  │ 1. Authentication Middleware           │  │
│  │    ↓ (Verifica cookie, user anónimo)   │  │
│  │ 2. Authorization Middleware            │  │
│  │    ↓ (Login é AllowAnonymous, passa)   │  │
│  │ 3. Routing Middleware                  │  │
│  │    ↓ (Encontra LoginModel.OnGetAsync)  │  │
│  └────────────────────────────────────────┘  │
└──────────────┬───────────────────────────────┘
               ↓
       ┌───────────────┐
       │  LoginModel   │
       │  OnGetAsync() │
       └───────┬───────┘
               ↓
       ┌──────────────────────┐
       │  Razor Engine        │
       │  Renderiza HTML      │
       └──────────┬───────────┘
                  ↓
          HTML completo enviado
                  ↓
       ┌──────────────────┐
       │   Browser        │
       │  (mostra form)   │
       └──────────────────┘
                  │
       2. User preenche e submete
                  │ POST /Admin/Account/Login
                  ↓
       ┌─────────────────────────┐
       │  LoginModel             │
       │  OnPostAsync()          │
       └──────────┬──────────────┘
                  ↓
       ┌─────────────────────────┐
       │  SignInManager          │
       │  PasswordSignInAsync()  │
       └──────────┬──────────────┘
                  ↓
       ┌─────────────────────────┐
       │  UserManager            │
       │  FindByNameAsync()      │
       │  CheckPasswordAsync()   │
       └──────────┬──────────────┘
                  ↓
       ┌─────────────────────────┐
       │  AuthDbContext          │
       │  (Entity Framework)     │
       └──────────┬──────────────┘
                  ↓
       ┌─────────────────────────┐
       │  SQL Server             │
       │  SELECT FROM AspNetUsers│
       └──────────┬──────────────┘
                  │ Retorna dados
                  ↓
          Password está correto?
                  ↓ Sim
       ┌─────────────────────────┐
       │  SignInManager          │
       │  SignInAsync()          │
       │  (Cria cookie)          │
       └──────────┬──────────────┘
                  ↓
       HTTP 302 Redirect: /Admin/Users
       Set-Cookie: .AspNetCore.Identity.Application=...
                  ↓
       ┌──────────────────┐
       │   Browser        │
       │  (segue redirect)│
       └──────────────────┘
```

---

## 🔑 Conceitos-Chave

### 1. **PageModel = Controller + ViewModel**
No MVC tradicional:
- **Controller** → Lógica
- **ViewModel** → Dados para a View

No Razor Pages:
- **PageModel** → Tudo junto!

### 2. **OnGetAsync vs OnPostAsync**
```csharp
OnGetAsync()   // ← GET request (carregar página)
OnPostAsync()  // ← POST request (submeter formulário)
```

### 3. **[BindProperty]**
```csharp
[BindProperty]
public InputModel Input { get; set; }

// Automaticamente popula Input com dados do formulário
```

### 4. **[Authorize] Attribute**
```csharp
[Authorize(Roles = "Administrator")]  // ← Verifica ANTES de executar
public class IndexModel : PageModel { ... }
```

### 5. **TempData**
```csharp
// Page 1
TempData["Message"] = "Utilizador criado com sucesso!";
return RedirectToPage("Index");

// Page 2 (após redirect)
var message = TempData["Message"];  // "Utilizador criado..."
// TempData é automaticamente limpo após ser lido
```

---

## 📚 Resumo: Como Tudo se Conecta

```
Request HTTP
    ↓
ASP.NET Core Middleware Pipeline
    ↓
Routing → Encontra PageModel correto
    ↓
Authorization → Verifica [Authorize]
    ↓
PageModel.OnGetAsync() ou OnPostAsync()
    ↓
Dependency Injection → Injeta serviços (UserManager, etc)
    ↓
Serviços → Comunicam com EF Core
    ↓
Entity Framework Core → Gera SQL queries
    ↓
SQL Server → Retorna dados
    ↓
PageModel → Popula propriedades públicas
    ↓
Razor Engine → Renderiza .cshtml com dados do PageModel
    ↓
HTML completo enviado para o browser
```

**Sem APIs! Sem JavaScript! Tudo no servidor! 🎯**

---

## 💡 Próximos Passos

Se precisar criar uma **API REST** (para mobile apps, por exemplo):
```csharp
// Criar em src/API/Controllers/
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<UserDto>>> GetUsers()
    {
        // Mesma lógica, mas retorna JSON em vez de HTML
    }
}
```

Mas para o Admin UI atual: **SSR é suficiente e mais seguro!** ✅
