using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.Configuration;
using Admin.UI.Settings;

namespace Admin.UI;

/// <summary>
/// Admin UI Module - Dependency Injection Configuration
/// Provides a modern SPA-like interface for user and role management
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddAdminUI(this IServiceCollection services, IConfiguration configuration)
    {
        // Register AdminUI settings (logo, colors, etc.)
        services.Configure<AdminUISettings>(configuration.GetSection("AdminUI"));
        
        // ✅ Add HttpClient for API calls
        services.AddHttpClient();
        
        // Add Razor Pages
        services.AddRazorPages(options =>
        {
            // ✅ Routes are defined in @page directives of each .cshtml file
            // All pages use /Admin prefix for administration panel
            
            // Require authentication for all admin pages (except login/logout/access denied)
            options.Conventions.AuthorizeFolder("/Users", "AdminOnly");
            options.Conventions.AuthorizeFolder("/Roles", "AdminOnly");
            options.Conventions.AuthorizeFolder("/Providers", "AdminOnly"); // ✅ Providers management
            options.Conventions.AuthorizeFolder("/Logs", "InternalOnly"); // ✅ Logs: Admin + AuditViewer
            options.Conventions.AllowAnonymousToPage("/Account/Login");
            options.Conventions.AllowAnonymousToPage("/Account/Logout");
            options.Conventions.AllowAnonymousToPage("/Account/AccessDenied");
            
            // Admin area prefix
            options.RootDirectory = "/Pages";
        });

        // Add Session for SPA-like experience
        services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(30);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });

        // ✅ Cookie Authentication já está configurado no Auth.Infrastructure
        // Paths: /Admin/Account/Login, /Admin/Account/Logout, /Admin/Account/AccessDenied

        return services;
    }

    public static IApplicationBuilder UseAdminUI(this IApplicationBuilder app)
    {
        app.UseSession();
        return app;
    }

    public static IEndpointRouteBuilder MapAdminUI(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapRazorPages();
        return endpoints;
    }
}
