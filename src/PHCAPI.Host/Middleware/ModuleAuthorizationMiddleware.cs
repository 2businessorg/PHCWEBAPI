using Auth.Domain.Interfaces;

namespace PHCAPI.Host.Middleware;

public sealed class ModuleAuthorizationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ModuleAuthorizationMiddleware> _logger;
    private readonly bool _enabled;
    private readonly string[] _excludedPrefixes;
    private readonly string? _defaultModule;
    private readonly IReadOnlyList<RouteModuleMap> _routeModules;

    public ModuleAuthorizationMiddleware(
        RequestDelegate next,
        IConfiguration configuration,
        ILogger<ModuleAuthorizationMiddleware> logger)
    {
        _next = next;
        _logger = logger;

        _enabled = configuration.GetValue("ModuleAuthorization:Enabled", true);
        _defaultModule = configuration["ModuleAuthorization:DefaultModule"];
        _excludedPrefixes = configuration
            .GetSection("ModuleAuthorization:ExcludedPrefixes")
            .Get<string[]>() ?? ["/api/auth"];

        _routeModules = configuration
            .GetSection("ModuleAuthorization:RouteModules")
            .Get<List<RouteModuleMap>>() ?? [];
    }

    public async Task InvokeAsync(HttpContext context, IModuleAccessService moduleAccessService)
    {
        if (!_enabled)
        {
            await _next(context);
            return;
        }

        var requestPath = context.Request.Path.Value ?? string.Empty;

        if (!requestPath.StartsWith("/api", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        if (IsExcludedPath(requestPath))
        {
            await _next(context);
            return;
        }

        if (context.User.Identity?.IsAuthenticated != true)
        {
            await _next(context);
            return;
        }

        var nomepack = ResolveModuleByPath(requestPath);
        if (string.IsNullOrWhiteSpace(nomepack))
        {
            await _next(context);
            return;
        }

        var hasAccess = await moduleAccessService.CanAccessModuleAsync(
            context.User,
            nomepack,
            context.RequestAborted);

        if (hasAccess)
        {
            await _next(context);
            return;
        }

        _logger.LogWarning(
            "Module access denied. Path: {Path}, Module: {Module}, User: {User}",
            requestPath,
            nomepack,
            context.User.Identity?.Name ?? "unknown");

        var problemResult = Results.Problem(
            statusCode: StatusCodes.Status403Forbidden,
            title: "Forbidden",
            detail: $"Sem acesso ao módulo '{nomepack}'");

        await problemResult.ExecuteAsync(context);
    }

    private bool IsExcludedPath(string path)
        => _excludedPrefixes.Any(prefix => path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));

    private string? ResolveModuleByPath(string path)
    {
        var configured = _routeModules
            .FirstOrDefault(x => path.StartsWith(x.Prefix, StringComparison.OrdinalIgnoreCase));

        return configured?.nomepack ?? _defaultModule;
    }

    private sealed class RouteModuleMap
    {
        public string Prefix { get; set; } = string.Empty;
        public string nomepack { get; set; } = string.Empty;
    }
}
