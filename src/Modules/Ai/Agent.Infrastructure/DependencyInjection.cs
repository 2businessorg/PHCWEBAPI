using Agent.Application;
using Agent.Application.Abstractions;
using Agent.Infrastructure.LocalAi;
using Agent.Infrastructure.Mcp;
using Agent.Infrastructure.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Agent.Infrastructure;

/// <summary>
/// Infrastructure DI for the Agent module (Ollama or Qwen cloud + in-process MCP client).
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddAgentInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<AgentOptions>(configuration.GetSection(AgentOptions.SectionName));
        services.Configure<LocalAiOptions>(configuration.GetSection(LocalAiOptions.SectionName));

        var localAi = configuration.GetSection(LocalAiOptions.SectionName).Get<LocalAiOptions>()
            ?? new LocalAiOptions();

        if (IsQwenProvider(localAi.Provider))
        {
            services.AddHttpClient<ILocalChatModel, OpenAiCompatibleChatModel>(client =>
            {
                client.BaseAddress = new Uri(localAi.BaseUrl.TrimEnd('/') + "/");
                client.Timeout = TimeSpan.FromSeconds(Math.Max(5, localAi.TimeoutSeconds));
            });
        }
        else
        {
            services.AddHttpClient<ILocalChatModel, OllamaChatModel>(client =>
            {
                client.BaseAddress = new Uri(localAi.BaseUrl.TrimEnd('/') + "/");
                client.Timeout = TimeSpan.FromSeconds(Math.Max(5, localAi.TimeoutSeconds));
            });
        }

        services.AddScoped<IMcpClient, InProcessMcpClient>();

        return services;
    }

    private static bool IsQwenProvider(string? provider)
        => string.Equals(provider, "Qwen", StringComparison.OrdinalIgnoreCase)
           || string.Equals(provider, "OpenAICompatible", StringComparison.OrdinalIgnoreCase)
           || string.Equals(provider, "DashScope", StringComparison.OrdinalIgnoreCase);
}
