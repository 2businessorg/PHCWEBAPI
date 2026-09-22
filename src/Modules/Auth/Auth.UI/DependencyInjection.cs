using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Auth.UI;

/// <summary>
/// Configuração de dependências do módulo Auth.UI
/// Fornece interface web para gestão de usuários e autenticação
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adiciona serviços do Identity UI ao pipeline
    /// Usa as páginas oficiais embeddadas da Microsoft (Microsoft.AspNetCore.Identity.UI)
    /// Páginas disponíveis:
    /// - /Identity/Account/Login
    /// - /Identity/Account/Register
    /// - /Identity/Account/Logout
    /// - /Identity/Account/ForgotPassword
    /// - /Identity/Account/ResetPassword
    /// - /Identity/Account/Manage/Index (Profile)
    /// - /Identity/Account/Manage/ChangePassword
    /// - /Identity/Account/Manage/Email
    /// - /Identity/Account/Manage/TwoFactorAuthentication
    /// E muitas outras...
    /// </summary>
    public static IServiceCollection AddAuthUI(this IServiceCollection services)
    {
        // Adiciona suporte a Razor Pages para Identity UI
        services.AddRazorPages();
        
        // ❌ Cookie Configuration removida - agora é configurada pelo Admin.UI
        // O Admin.UI tem uma página de login customizada em /Account/Login
        // e sobrescreve estas configurações para fornecer uma experiência moderna
        
        // services.ConfigureApplicationCookie(options =>
        // {
        //     options.LoginPath = "/Identity/Account/Login";
        //     options.LogoutPath = "/Identity/Account/Logout";
        //     options.AccessDeniedPath = "/Identity/Account/AccessDenied";
        //     options.SlidingExpiration = true;
        //     options.ExpireTimeSpan = TimeSpan.FromDays(7);
        // });

        return services;
    }

    /// <summary>
    /// Configura os endpoints do Identity UI no pipeline da aplicação
    /// DEVE ser chamado APÓS app.UseAuthentication() e app.UseAuthorization()
    /// </summary>
    public static IApplicationBuilder UseAuthUI(this IApplicationBuilder app)
    {
        // Nota: MapRazorPages() deve ser chamado diretamente no Program.cs
        // Este método agora é apenas um placeholder para consistência com outros módulos
        return app;
    }
    
    /// <summary>
    /// Mapeia os endpoints das Razor Pages do Auth.UI
    /// DEVE ser chamado no final do pipeline, junto com MapControllers()
    /// </summary>
    public static IEndpointRouteBuilder MapAuthUI(this IEndpointRouteBuilder endpoints)
    {
        // Mapeia as páginas Razor do Identity UI
        // Disponibiliza rotas como:
        // - /Identity/Account/Login
        // - /Identity/Account/Logout  
        // - /Identity/Account/Manage
        endpoints.MapRazorPages();
        
        return endpoints;
    }
}
