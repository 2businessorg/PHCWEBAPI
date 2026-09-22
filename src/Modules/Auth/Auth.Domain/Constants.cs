namespace Auth.Domain;

/// <summary>
/// Application roles constants
/// </summary>
public static class AppRoles
{
    public const string Administrator = "Administrator";
    public const string ApiUser = "ApiUser";
    public const string InternalUser = "InternalUser";
    public const string ExternalStakeholder = "ExternalStakeholder";
    public const string AuditViewer = "AuditViewer";
    
    public static readonly string[] AllRoles = 
    {
        Administrator,
        ApiUser,
        InternalUser,
        ExternalStakeholder,
        AuditViewer
    };
}

/// <summary>
/// Authorization policies constants
/// </summary>
public static class AppPolicies
{
    // Policy that requires internal user role (for sensitive modules like Audit)
    public const string InternalOnly = "InternalOnly";
    
    // Policy that requires any authenticated user
    public const string Authenticated = "Authenticated";
    
    // Policy that requires administrator role
    public const string AdminOnly = "AdminOnly";
    
    // Policy for API consumers
    public const string ApiAccess = "ApiAccess";
}

/// <summary>
/// Authentication response messages
/// </summary>
public static class AuthMessages
{
    public const string Authenticated = "AUTHENTICATED";
    public const string BadCredentials = "BAD_CREDENTIALS";
    public const string InternalError = "INTERNAL_ERROR";
    public const string UserCreated = "USER_CREATED";
    public const string UserAlreadyExists = "USER_ALREADY_EXISTS";
    public const string InvalidToken = "INVALID_TOKEN";
    public const string Unauthorized = "UNAUTHORIZED";
}
