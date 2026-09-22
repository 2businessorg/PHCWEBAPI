using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Providers.Domain.Repositories;
using Providers.Infrastructure.Caching.Configuration;
using System.Diagnostics;

namespace Providers.Infrastructure.Caching;

/// <summary>
/// Background service that pre-warms the provider cache at application startup.
/// Pre-warming ensures frequently accessed configurations are cached before first request.
/// </summary>
public class ProviderCachePreWarmingService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ProviderCachePreWarmingService> _logger;
    private readonly ProviderCachingOptions _options;

    public ProviderCachePreWarmingService(
        IServiceProvider serviceProvider,
        ILogger<ProviderCachePreWarmingService> logger,
        IOptions<ProviderCachingOptions> options)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _options = options.Value;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_options.Enabled || !_options.EnablePreWarming || !_options.PreWarmProviders.Any())
        {
            _logger.LogInformation("Provider cache pre-warming is disabled or no providers configured");
            return;
        }

        _logger.LogInformation("Starting provider cache pre-warming for {Count} configurations", 
            _options.PreWarmProviders.Count);

        var stopwatch = Stopwatch.StartNew();
        var successCount = 0;
        var failureCount = 0;

        // Create a scope to resolve scoped dependencies
        using var scope = _serviceProvider.CreateScope();
        var providerRepository = scope.ServiceProvider.GetRequiredService<IProviderRepository>();

        foreach (var preWarmConfig in _options.PreWarmProviders)
        {
            try
            {
                _logger.LogDebug("Pre-warming cache: Provider={Provider}, OperationCode={OperationCode}, Environment={Environment}",
                    preWarmConfig.Provider, preWarmConfig.OperationCode ?? "ALL", preWarmConfig.Environment);

                // Call GetByProviderAndEnvironmentAsync which will trigger caching via decorator
                var provider = await providerRepository.GetByProviderAndEnvironmentAsync(
                    preWarmConfig.Provider,
                    preWarmConfig.Environment,
                    cancellationToken);

                if (provider != null)
                {
                    successCount++;
                    _logger.LogDebug("Successfully pre-warmed: {Provider} ({Environment})",
                        preWarmConfig.Provider, preWarmConfig.Environment);
                }
                else
                {
                    failureCount++;
                    _logger.LogWarning("Provider not found for pre-warming: {Provider} ({Environment})",
                        preWarmConfig.Provider, preWarmConfig.Environment);
                }
            }
            catch (Exception ex)
            {
                failureCount++;
                _logger.LogError(ex, "Error pre-warming cache for Provider={Provider}, Environment={Environment}",
                    preWarmConfig.Provider, preWarmConfig.Environment);
            }
        }

        stopwatch.Stop();

        _logger.LogInformation(
            "Provider cache pre-warming completed in {ElapsedMs}ms. Success: {Success}, Failed: {Failed}",
            stopwatch.ElapsedMilliseconds, successCount, failureCount);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Provider cache pre-warming service stopped");
        return Task.CompletedTask;
    }
}
