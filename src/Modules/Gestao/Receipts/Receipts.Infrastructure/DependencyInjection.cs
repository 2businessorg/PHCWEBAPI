using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Receipts.Domain.ExternalServices;
using Receipts.Domain.Repositories;
using Receipts.Infrastructure.ExternalServices;
using Receipts.Infrastructure.Persistence;
using Receipts.Infrastructure.Repositories;
using Shared.Abstractions.ExternalServices;

namespace Receipts.Infrastructure;

/// <summary>
/// Registro de dependências da camada Infrastructure do módulo Receipts
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddReceiptsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ReceiptsDbContext>((sp, options) =>
        {
            var environment = sp.GetRequiredService<IHostEnvironment>();
            var fallbackConnectionString = configuration.GetConnectionString("DBconnect");
            options.UseSqlServer(fallbackConnectionString)
                .EnableSensitiveDataLogging(environment.IsDevelopment())
                .EnableDetailedErrors(environment.IsDevelopment());
        });

        // PHC WEB integration
        services.AddHttpClient<IPhcWebServiceReceipts, PhcWebService>()
            .ConfigureHttpClient(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
            });

        services.AddScoped<IPhcWebCredentialsProvider, PhcWebCredentialsProvider>();

        // Repositories
        services.AddScoped<IReceiptRepository, ReceiptRepositoryEFCore>();
        services.AddScoped<IReceiptTypeRepository, ReceiptTypeRepositoryEFCore>();
        services.AddScoped<IClientRepository, ClientRepositoryEFCore>();
        services.AddScoped<ICcRepository, CcRepositoryEFCore>();

        return services;
    }
}
