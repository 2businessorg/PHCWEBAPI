using Agent.Application;
using Agent.Application.Abstractions;
using Agent.Infrastructure;
using Agent.Presentation.MCP;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Agent.Presentation;

/// <summary>
/// Entry point that registers Agent layers, MCP tools, and the HTTP MCP server.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddAgentPresentation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAgentApplication();
        services.AddAgentInfrastructure(configuration);

        services.AddScoped<ReconciliationMcpTools>();
        services.AddScoped<ReconciliationSummaryMcpTool>();
        services.AddScoped<ReconciliationMatchesMcpTool>();
        services.AddScoped<ListTreasuryAccountsMcpTool>();
        services.AddScoped<GetTreasuryAccountMcpTool>();
        services.AddScoped<IMcpToolHandler>(sp => sp.GetRequiredService<ReconciliationMcpTools>());
        services.AddScoped<IMcpToolHandler>(sp => sp.GetRequiredService<ReconciliationSummaryMcpTool>());
        services.AddScoped<IMcpToolHandler>(sp => sp.GetRequiredService<ReconciliationMatchesMcpTool>());
        services.AddScoped<IMcpToolHandler>(sp => sp.GetRequiredService<ListTreasuryAccountsMcpTool>());
        services.AddScoped<IMcpToolHandler>(sp => sp.GetRequiredService<GetTreasuryAccountMcpTool>());

        services.AddMcpServer()
            .WithHttpTransport(options => options.Stateless = true)
            .WithTools<ReconciliationMcpTools>()
            .WithTools<ReconciliationSummaryMcpTool>()
            .WithTools<ReconciliationMatchesMcpTool>()
            .WithTools<ListTreasuryAccountsMcpTool>()
            .WithTools<GetTreasuryAccountMcpTool>();

        return services;
    }

    /// <summary>
    /// Maps the Streamable HTTP MCP endpoint at <c>/mcp</c>.
    /// </summary>
    public static WebApplication MapAgentMcp(this WebApplication app)
    {
        app.MapMcp("/mcp");
        return app;
    }
}
