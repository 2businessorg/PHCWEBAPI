using Microsoft.AspNetCore.RateLimiting;
using PHCAPI.Host.Configuration;
using Serilog;
using System.Threading.RateLimiting;

namespace PHCAPI.Host.Extensions;

/// <summary>
/// Extension methods for configuring rate limiting from appsettings.json
/// 🛡️ SECURITY: Centralized rate limiting configuration
/// </summary>
public static class RateLimitingExtensions
{
    /// <summary>
    /// Adds rate limiting policies from configuration
    /// </summary>
    public static IServiceCollection AddConfigurableRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var rateLimitingOptions = configuration
            .GetSection(RateLimitingOptions.SectionName)
            .Get<RateLimitingOptions>() ?? new RateLimitingOptions();

        services.AddRateLimiter(rateLimiterOptions =>
        {
            // 🔴 CRITICAL: Login endpoint (Anti-Brute Force)
            if (rateLimitingOptions.LoginEndpoint.Enabled)
            {
                rateLimiterOptions.AddPolicy("login-endpoint", context =>
                {
                    var config = rateLimitingOptions.LoginEndpoint;
                    var ip = context.Connection.RemoteIpAddress?.ToString() ??
                             context.Connection.LocalIpAddress?.ToString() ?? "unknown";
                    var username = context.Request.Headers["X-Username"].ToString();
                    var partitionKey = string.IsNullOrEmpty(username) ? ip : $"{ip}_{username}";

                    return CreateRateLimiter(partitionKey, config);
                });
            }

            // 🟠 HIGH: Parameter Create endpoint
            if (rateLimitingOptions.ParametersCreate.Enabled)
            {
                rateLimiterOptions.AddPolicy("parameters-create", context =>
                {
                    var config = rateLimitingOptions.ParametersCreate;
                    var partitionKey = GetPartitionKey(context);
                    return CreateRateLimiter(partitionKey, config);
                });
            }

            // 🔴 CRITICAL: Parameter Delete endpoint
            if (rateLimitingOptions.ParametersDelete.Enabled)
            {
                rateLimiterOptions.AddPolicy("parameters-delete", context =>
                {
                    var config = rateLimitingOptions.ParametersDelete;
                    var partitionKey = GetPartitionKey(context);
                    return CreateRateLimiter(partitionKey, config);
                });
            }

            // 🟡 MEDIUM: Parameter Update endpoint
            if (rateLimitingOptions.ParametersUpdate.Enabled)
            {
                rateLimiterOptions.AddPolicy("parameters-update", context =>
                {
                    var config = rateLimitingOptions.ParametersUpdate;
                    var partitionKey = GetPartitionKey(context);
                    return CreateRateLimiter(partitionKey, config);
                });
            }

            // 🟢 LOW: Parameter Query endpoints
            if (rateLimitingOptions.ParametersQuery.Enabled)
            {
                rateLimiterOptions.AddPolicy("parameters-query", context =>
                {
                    var config = rateLimitingOptions.ParametersQuery;
                    var partitionKey = GetPartitionKey(context);
                    return CreateRateLimiter(partitionKey, config);
                });
            }

            // 🟠 HIGH: Client Create endpoint
            if (rateLimitingOptions.ClientsCreate.Enabled)
            {
                rateLimiterOptions.AddPolicy("clients-create", context =>
                {
                    var config = rateLimitingOptions.ClientsCreate;
                    var partitionKey = GetPartitionKey(context);
                    return CreateRateLimiter(partitionKey, config);
                });
            }

            // 🔴 CRITICAL: Client Delete endpoint
            if (rateLimitingOptions.ClientsDelete.Enabled)
            {
                rateLimiterOptions.AddPolicy("clients-delete", context =>
                {
                    var config = rateLimitingOptions.ClientsDelete;
                    var partitionKey = GetPartitionKey(context);
                    return CreateRateLimiter(partitionKey, config);
                });
            }

            // 🟡 MEDIUM: Client Update endpoint
            if (rateLimitingOptions.ClientsUpdate.Enabled)
            {
                rateLimiterOptions.AddPolicy("clients-update", context =>
                {
                    var config = rateLimitingOptions.ClientsUpdate;
                    var partitionKey = GetPartitionKey(context);
                    return CreateRateLimiter(partitionKey, config);
                });
            }

            // 🟠 HIGH: Client Bulk Create endpoint
            if (rateLimitingOptions.ClientsBulkCreate.Enabled)
            {
                rateLimiterOptions.AddPolicy("clients-bulk-create", context =>
                {
                    var config = rateLimitingOptions.ClientsBulkCreate;
                    var partitionKey = GetPartitionKey(context);
                    return CreateRateLimiter(partitionKey, config);
                });
            }

            // 🟢 LOW: Client Query endpoints
            if (rateLimitingOptions.ClientsQuery.Enabled)
            {
                rateLimiterOptions.AddPolicy("clients-query", context =>
                {
                    var config = rateLimitingOptions.ClientsQuery;
                    var partitionKey = GetPartitionKey(context);
                    return CreateRateLimiter(partitionKey, config);
                });
            }

            // 🟠 HIGH: Dossier Create endpoint
            if (rateLimitingOptions.DossiersCreate.Enabled)
            {
                rateLimiterOptions.AddPolicy("dossiers-create", context =>
                {
                    var config = rateLimitingOptions.DossiersCreate;
                    var partitionKey = GetPartitionKey(context);
                    return CreateRateLimiter(partitionKey, config);
                });
            }

            // 🔴 CRITICAL: Dossier Delete endpoint
            if (rateLimitingOptions.DossiersDelete.Enabled)
            {
                rateLimiterOptions.AddPolicy("dossiers-delete", context =>
                {
                    var config = rateLimitingOptions.DossiersDelete;
                    var partitionKey = GetPartitionKey(context);
                    return CreateRateLimiter(partitionKey, config);
                });
            }

            // 🟠 HIGH: Dossier Bulk Create endpoint
            if (rateLimitingOptions.DossiersBulkCreate.Enabled)
            {
                rateLimiterOptions.AddPolicy("dossiers-bulk-create", context =>
                {
                    var config = rateLimitingOptions.DossiersBulkCreate;
                    var partitionKey = GetPartitionKey(context);
                    return CreateRateLimiter(partitionKey, config);
                });
            }

            // 🟢 LOW: Dossier Query endpoints
            if (rateLimitingOptions.DossiersQuery.Enabled)
            {
                rateLimiterOptions.AddPolicy("dossiers-query", context =>
                {
                    var config = rateLimitingOptions.DossiersQuery;
                    var partitionKey = GetPartitionKey(context);
                    return CreateRateLimiter(partitionKey, config);
                });
            }

            // 🟠 HIGH: Receipts Create endpoint
            if (rateLimitingOptions.ReceiptsCreate.Enabled)
            {
                rateLimiterOptions.AddPolicy("receipts-create", context =>
                {
                    var config = rateLimitingOptions.ReceiptsCreate;
                    var partitionKey = GetPartitionKey(context);
                    return CreateRateLimiter(partitionKey, config);
                });
            }

            // 🟢 LOW: Receipts Query endpoints
            if (rateLimitingOptions.ReceiptsQuery.Enabled)
            {
                rateLimiterOptions.AddPolicy("receipts-query", context =>
                {
                    var config = rateLimitingOptions.ReceiptsQuery;
                    var partitionKey = GetPartitionKey(context);
                    return CreateRateLimiter(partitionKey, config);
                });
            }

            // 🟢 LOW: Stocks Query endpoints
            if (rateLimitingOptions.StocksQuery.Enabled)
            {
                rateLimiterOptions.AddPolicy("stocks-query", context =>
                {
                    var config = rateLimitingOptions.StocksQuery;
                    var partitionKey = GetPartitionKey(context);
                    return CreateRateLimiter(partitionKey, config);
                });
            }

            // 🟠 HIGH: Stocks Create endpoint
            if (rateLimitingOptions.StocksCreate.Enabled)
            {
                rateLimiterOptions.AddPolicy("stocks-create", context =>
                {
                    var config = rateLimitingOptions.StocksCreate;
                    var partitionKey = GetPartitionKey(context);
                    return CreateRateLimiter(partitionKey, config);
                });
            }

            // 🟡 MEDIUM: Stocks Update endpoint
            if (rateLimitingOptions.StocksUpdate.Enabled)
            {
                rateLimiterOptions.AddPolicy("stocks-update", context =>
                {
                    var config = rateLimitingOptions.StocksUpdate;
                    var partitionKey = GetPartitionKey(context);
                    return CreateRateLimiter(partitionKey, config);
                });
            }

            // 🔴 CRITICAL: Stocks Delete endpoint
            if (rateLimitingOptions.StocksDelete.Enabled)
            {
                rateLimiterOptions.AddPolicy("stocks-delete", context =>
                {
                    var config = rateLimitingOptions.StocksDelete;
                    var partitionKey = GetPartitionKey(context);
                    return CreateRateLimiter(partitionKey, config);
                });
            }

            // 🟢 LOW: VAT Taxes Query endpoints
            if (rateLimitingOptions.VatTaxesQuery.Enabled)
            {
                rateLimiterOptions.AddPolicy("vat-taxes-query", context =>
                {
                    var config = rateLimitingOptions.VatTaxesQuery;
                    var partitionKey = GetPartitionKey(context);
                    return CreateRateLimiter(partitionKey, config);
                });
            }

            // 🟡 MEDIUM: VAT Taxes Update endpoint
            if (rateLimitingOptions.VatTaxesUpdate.Enabled)
            {
                rateLimiterOptions.AddPolicy("vat-taxes-update", context =>
                {
                    var config = rateLimitingOptions.VatTaxesUpdate;
                    var partitionKey = GetPartitionKey(context);
                    return CreateRateLimiter(partitionKey, config);
                });
            }

            // 🟠 HIGH: Advances Create endpoint
            if (rateLimitingOptions.AdvancesCreate.Enabled)
            {
                rateLimiterOptions.AddPolicy("advances-create", context =>
                {
                    var config = rateLimitingOptions.AdvancesCreate;
                    var partitionKey = GetPartitionKey(context);
                    return CreateRateLimiter(partitionKey, config);
                });
            }

            // 🟢 LOW: Advances Query endpoints
            if (rateLimitingOptions.AdvancesQuery.Enabled)
            {
                rateLimiterOptions.AddPolicy("advances-query", context =>
                {
                    var config = rateLimitingOptions.AdvancesQuery;
                    var partitionKey = GetPartitionKey(context);
                    return CreateRateLimiter(partitionKey, config);
                });
            }

            if (rateLimitingOptions.TreasuryQuery.Enabled)
            {
                rateLimiterOptions.AddPolicy("treasury-query", context =>
                {
                    var config = rateLimitingOptions.TreasuryQuery;
                    var partitionKey = GetPartitionKey(context);
                    return CreateRateLimiter(partitionKey, config);
                });
            }

            if (rateLimitingOptions.AiChat.Enabled)
            {
                rateLimiterOptions.AddPolicy("ai-chat", context =>
                {
                    var config = rateLimitingOptions.AiChat;
                    var partitionKey = GetPartitionKey(context);
                    return CreateRateLimiter(partitionKey, config);
                });
            }

            // ⚠️ Response when rate limit is exceeded
            rateLimiterOptions.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.ContentType = "application/json";

                var retryAfter = 60;
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfterMetadata))
                {
                    retryAfter = (int)retryAfterMetadata.TotalSeconds;
                }

                context.HttpContext.Response.Headers.RetryAfter = retryAfter.ToString();

                var endpoint = context.HttpContext.GetEndpoint()?.DisplayName ?? "Unknown";
                var method = context.HttpContext.Request.Method;
                var path = context.HttpContext.Request.Path;
                var ipAddress = context.HttpContext.Connection.RemoteIpAddress?.ToString() ??
                               context.HttpContext.Connection.LocalIpAddress?.ToString() ?? "Unknown";
                var user = context.HttpContext.User?.Identity?.Name ?? "Anonymous";

                Log.Warning(
                    "🚨 RATE LIMIT EXCEEDED: User={User}, IP={IP}, Method={Method}, Path={Path}, Endpoint={Endpoint}, RetryAfter={RetryAfter}s",
                    user, ipAddress, method, path, endpoint, retryAfter);

                await context.HttpContext.Response.WriteAsJsonAsync(new
                {
                    error = "Rate limit exceeded",
                    message = $"Too many requests. Please try again in {retryAfter} seconds.",
                    retryAfter,
                    statusCode = 429,
                    endpoint = path.ToString()
                }, cancellationToken: cancellationToken);
            };

            // 🌍 Global fallback rate limiter
            if (rateLimitingOptions.GlobalLimit.Enabled)
            {
                rateLimiterOptions.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                {
                    var config = rateLimitingOptions.GlobalLimit;
                    var ipAddress = context.Connection.RemoteIpAddress?.ToString() ??
                                   context.Connection.LocalIpAddress?.ToString() ?? "unknown";

                    return CreateRateLimiter(ipAddress, config);
                });
            }

            Log.Information(
                "✅ Rate Limiting configured: Login({LoginLimit}/{LoginWindow}s), " +
                "Create({CreateLimit}/{CreateWindow}s), Delete({DeleteLimit}/{DeleteWindow}s), " +
                "Update({UpdateLimit}/{UpdateWindow}s), Query({QueryLimit}/{QueryWindow}s), " +
                "ClientsCreate({ClientsCreateLimit}/{ClientsCreateWindow}s), ClientsDelete({ClientsDeleteLimit}/{ClientsDeleteWindow}s), " +
                "ClientsUpdate({ClientsUpdateLimit}/{ClientsUpdateWindow}s), ClientsBulkCreate({ClientsBulkCreateLimit}/{ClientsBulkCreateWindow}s), ClientsQuery({ClientsQueryLimit}/{ClientsQueryWindow}s), " +
                "DossiersCreate({DossiersCreateLimit}/{DossiersCreateWindow}s), DossiersDelete({DossiersDeleteLimit}/{DossiersDeleteWindow}s), " +
                "DossiersBulkCreate({DossiersBulkCreateLimit}/{DossiersBulkCreateWindow}s), DossiersQuery({DossiersQueryLimit}/{DossiersQueryWindow}s), " +
                "StocksQuery({StocksQueryLimit}/{StocksQueryWindow}s), " +
                "VatTaxesQuery({VatTaxesQueryLimit}/{VatTaxesQueryWindow}s), " +
                "Global({GlobalLimit}/{GlobalWindow}s)",
                rateLimitingOptions.LoginEndpoint.PermitLimit, rateLimitingOptions.LoginEndpoint.WindowInSeconds,
                rateLimitingOptions.ParametersCreate.PermitLimit, rateLimitingOptions.ParametersCreate.WindowInSeconds,
                rateLimitingOptions.ParametersDelete.PermitLimit, rateLimitingOptions.ParametersDelete.WindowInSeconds,
                rateLimitingOptions.ParametersUpdate.PermitLimit, rateLimitingOptions.ParametersUpdate.WindowInSeconds,
                rateLimitingOptions.ParametersQuery.PermitLimit, rateLimitingOptions.ParametersQuery.WindowInSeconds,
                rateLimitingOptions.ClientsCreate.PermitLimit, rateLimitingOptions.ClientsCreate.WindowInSeconds,
                rateLimitingOptions.ClientsDelete.PermitLimit, rateLimitingOptions.ClientsDelete.WindowInSeconds,
                rateLimitingOptions.ClientsUpdate.PermitLimit, rateLimitingOptions.ClientsUpdate.WindowInSeconds,
                rateLimitingOptions.ClientsBulkCreate.PermitLimit, rateLimitingOptions.ClientsBulkCreate.WindowInSeconds,
                rateLimitingOptions.ClientsQuery.PermitLimit, rateLimitingOptions.ClientsQuery.WindowInSeconds,
                rateLimitingOptions.DossiersCreate.PermitLimit, rateLimitingOptions.DossiersCreate.WindowInSeconds,
                rateLimitingOptions.DossiersDelete.PermitLimit, rateLimitingOptions.DossiersDelete.WindowInSeconds,
                rateLimitingOptions.DossiersBulkCreate.PermitLimit, rateLimitingOptions.DossiersBulkCreate.WindowInSeconds,
                rateLimitingOptions.DossiersQuery.PermitLimit, rateLimitingOptions.DossiersQuery.WindowInSeconds,
                rateLimitingOptions.StocksQuery.PermitLimit, rateLimitingOptions.StocksQuery.WindowInSeconds,
                rateLimitingOptions.VatTaxesQuery.PermitLimit, rateLimitingOptions.VatTaxesQuery.WindowInSeconds,
                rateLimitingOptions.GlobalLimit.PermitLimit, rateLimitingOptions.GlobalLimit.WindowInSeconds);
        });

        return services;
    }

    /// <summary>
    /// Gets partition key based on user authentication status
    /// Authenticated users: partition by username
    /// Anonymous users: partition by IP address
    /// </summary>
    private static string GetPartitionKey(HttpContext context)
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ??
                 context.Connection.LocalIpAddress?.ToString() ?? "unknown";
        var user = context.User?.Identity?.Name ?? "anonymous";
        return context.User?.Identity?.IsAuthenticated == true ? user : ip;
    }

    /// <summary>
    /// Creates rate limiter based on configuration
    /// </summary>
    private static RateLimitPartition<string> CreateRateLimiter(
        string partitionKey,
        EndpointLimitOptions config)
    {
        return config.Algorithm == RateLimitAlgorithm.FixedWindow
            ? RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: partitionKey,
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = config.PermitLimit,
                    Window = TimeSpan.FromSeconds(config.WindowInSeconds),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 0
                })
            : RateLimitPartition.GetSlidingWindowLimiter(
                partitionKey: partitionKey,
                factory: _ => new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = config.PermitLimit,
                    Window = TimeSpan.FromSeconds(config.WindowInSeconds),
                    SegmentsPerWindow = config.SegmentsPerWindow,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 0
                });
    }
}
