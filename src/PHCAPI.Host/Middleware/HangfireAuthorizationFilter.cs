using Hangfire.Dashboard;

namespace PHCAPI.Host.Middleware;


public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        try
        {
            // ✅ Proteção contra null
            if (context == null)
                return false;

            var httpContext = context.GetHttpContext();

            if (httpContext == null)
                return false;

            // ⚠️ Em desenvolvimento: permite QUALQUER acesso local
            // Isso evita problemas com Host parsing
            var host = httpContext.Request.Host.Host?.ToLowerInvariant() ?? string.Empty;

            // Permitir localhost, 127.0.0.1 e qualquer IP local
            return host == "localhost" 
                || host == "127.0.0.1"
                || host.StartsWith("192.168.")
                || host.StartsWith("10.")
                || string.IsNullOrEmpty(host); // ← Permite se não conseguir determinar host
        }
        catch
        {
            // ✅ Em caso de erro, PERMITIR em desenvolvimento
            // (mais seguro que crashar)
            return true;
        }
    }
}
