# Auth Module

## Overview

The Auth module provides authentication and authorization for the PHCAPI system using **Clean Architecture** with **CQRS pattern** (MediatR). It follows **exactly the same structure** as other modules (Audit, Parameters).

## Architecture

```
Auth/
├── Auth.Domain/              # Interfaces, domain contracts
├── Auth.Application/         # Features (CQRS), DTOs
│   └── Features/            # Commands & Queries with Handlers
│       ├── Login/
│       ├── Register/
│       ├── CreateRole/
│       ├── AddUserToRole/
│       ├── GetAllRoles/
│       └── GetUserRoles/
├── Auth.Infrastructure/      # Implementations (Identity, JWT, Repositories)
├── Auth.Presentation/        # Controllers (uses IMediator only)
└── Auth.UI/                  # 🆕 Web UI for user management (Razor Pages)
```

## 🆕 Auth.UI Module

The Auth module now includes a **Web UI** for visual user management!

**Features:**
- ✅ Login/Logout pages
- ✅ User profile management
- ✅ Modern responsive design
- ✅ Bootstrap 5 based
- ✅ Cookie-based authentication

**Quick Access:**
```
http://localhost:7298/Identity/Account/Login
```

**Default Credentials:**
```
Username: admin
Password: YOUR_PASSWORD
```

📖 **Full Documentation:** See `Auth.UI/README.md` and `Auth.UI/QUICK-START.md`

---

### CQRS Pattern with MediatR

Following the **same pattern as Audit module**:

✅ **Controller** → Injects only `IMediator`
✅ **Features** → Commands (write) and Queries (read)
✅ **Handlers** → Business logic separated by feature
✅ **Repositories** → Data access in Infrastructure

Example flow:
```
Controller → IMediator.Send(Command) → CommandHandler → Repository → Database
```

## Features

- ✅ JWT Token Authentication
- ✅ User Registration and Login
- ✅ Role-Based Authorization
- ✅ Policy-Based Authorization
- ✅ ASP.NET Identity Integration
- ✅ Password Hashing and Security
- ✅ Token Validation
- ✅ Extensible Provider Architecture

## Roles

The system defines the following roles:

- **Administrator**: Full system access, can manage users and roles
- **InternalUser**: Internal staff with access to most features
- **ApiUser**: External API consumers with limited access
- **ExternalStakeholder**: External partners with restricted access
- **AuditViewer**: Read-only access to audit logs

## Authorization Policies

The system implements the following authorization policies:

- **InternalOnly**: Requires Administrator, InternalUser, or AuditViewer roles (e.g., Audit module)
- **AdminOnly**: Requires Administrator role
- **ApiAccess**: Requires Administrator, ApiUser, or InternalUser roles
- **Authenticated**: Requires any authenticated user

## API Endpoints

### Authentication

#### POST /api/authenticate/login
Authenticate user and receive JWT token.

**Request:**
```json
{
  "username": "string",
  "password": ""
}
```

**Response (Success):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiration": "2026-02-13T17:00:00Z",
  "allowed": true,
  "outputResponse": "AUTHENTICATED",
  "roles": ["Administrator", "InternalUser"]
}
```

**Response (Failure):**
```json
{
  "token": "",
  "expiration": null,
  "allowed": false,
  "outputResponse": "BAD_CREDENTIALS"
}
```

### User Management (Admin Only)

#### POST /api/authenticate/register
Register a new user (requires Administrator role).

**Request:**
```json
{
  "username": "string",
  "email": "user@example.com",
  "password": ""
}
```

#### GET /api/authenticate/roles
Get all available roles (requires Administrator role).

#### POST /api/authenticate/roles
Create a new role (requires Administrator role).

#### POST /api/authenticate/users/add-role
Add a user to a role (requires Administrator role).

#### GET /api/authenticate/users/{username}/roles
Get roles for a specific user (requires Administrator role).

## Configuration

Add the following to `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=...;User Id=...;Password=YOUR_PASSWORD;"
  },
  "JWT": {
    "ValidAudience": "http://localhost:7298",
    "ValidIssuer": "http://localhost:7298",
    "Secret": "YourSecretKeyHereAtLeast32CharactersLong",
    "ExpirationHours": 3
  }
}
```

## Database Migration

The module creates the following ASP.NET Identity tables:

- `AspNetUsers` - User accounts
- `AspNetRoles` - Role definitions
- `AspNetUserRoles` - User-role relationships
- `AspNetUserClaims` - User claims
- `AspNetUserLogins` - External login providers
- `AspNetUserTokens` - User tokens
- `AspNetRoleClaims` - Role claims

To apply migrations, run:

```powershell
cd src/Modules/Auth/Auth.Infrastructure
dotnet ef migrations add InitialCreate --context AuthDbContext
dotnet ef database update --context AuthDbContext
```

Or use the manual migration provided in `Migrations/20260213_InitialAuthMigration.cs`.

## Usage Examples

### Protecting Endpoints

```csharp
using Microsoft.AspNetCore.Authorization;
using Shared.Kernel.Authorization;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = AppPolicies.InternalOnly)]
public class SensitiveController : ControllerBase
{
    [HttpGet]
    public IActionResult GetSensitiveData()
    {
        return Ok("This data is only for internal users");
    }
}
```

### Using JWT Token

1. Call `/api/authenticate/login` with credentials
2. Extract the `token` from the response
3. Add the token to subsequent requests:

```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

## Future Enhancements

The architecture supports easy integration of:

- Auth0 authentication
- Firebase authentication
- OAuth2/OpenID Connect
- Multi-factor authentication (MFA)
- Token refresh mechanism
- Token blacklist/revocation
- Social login providers

To add a new provider, simply implement the `IAuthenticationService` and `ITokenService` interfaces without modifying existing code.

## Security Considerations

- ✅ Passwords are hashed using ASP.NET Identity's default hashing algorithm
- ✅ JWT tokens are signed using HMAC-SHA256
- ✅ Token expiration is enforced
- ✅ HTTPS is required for production
- ⚠️ Change the JWT secret in production to a secure, randomly generated value
- ⚠️ Consider implementing token refresh for better UX
- ⚠️ Implement rate limiting on authentication endpoints to prevent brute-force attacks

## Dependencies

- Microsoft.AspNetCore.Identity.EntityFrameworkCore 8.0.0
- Microsoft.AspNetCore.Authentication.JwtBearer 8.0.0
- Microsoft.IdentityModel.Tokens 7.3.1
- System.IdentityModel.Tokens.Jwt 7.3.1
- Microsoft.EntityFrameworkCore.SqlServer 8.0.0

## Contact

For questions or issues, contact the development team.
