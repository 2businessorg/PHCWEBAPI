namespace Shared.Kernel.Authorization;

/// <summary>
/// Application roles constants - shared across all modules
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
/// Authorization policies constants - shared across all modules
/// </summary>
public static class AppPolicies
{
    /// <summary>
    /// Policy that requires internal user role (for sensitive modules like Audit)
    /// </summary>
    public const string InternalOnly = "InternalOnly";
    
    /// <summary>
    /// Policy that requires any authenticated user
    /// </summary>
    public const string Authenticated = "Authenticated";
    
    /// <summary>
    /// Policy that requires administrator role
    /// </summary>
    public const string AdminOnly = "AdminOnly";
    
    /// <summary>
    /// Policy for API consumers
    /// </summary>
    public const string ApiAccess = "ApiAccess";
}
