using Microsoft.EntityFrameworkCore;
using Providers.Domain.Entities;
using Shared.Kernel.MultiTenancy;

namespace Providers.Infrastructure.Persistence;

/// <summary>
/// DbContext EF Core para o módulo de Providers - uses TenantContext for dynamic connection
/// </summary>
public class ProvidersDbContextEFCore : DbContext
{
    private readonly ITenantContext _tenantContext;

    public ProvidersDbContextEFCore(
        DbContextOptions<ProvidersDbContextEFCore> options,
        ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    public DbSet<Provider> Providers { get; set; } = null!;
    public DbSet<ProviderValue> ProviderValues { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // ✅ MULTI-TENANCY: Get connection string from TenantContext
        // If TenantContext has credentials, use the tenant's database
        if (_tenantContext?.HasDatabaseCredentials == true)
        {
            optionsBuilder.UseSqlServer(_tenantContext.GetConnectionString());
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations
        modelBuilder.ApplyConfiguration(new ProviderConfigurationEFCore());
        modelBuilder.ApplyConfiguration(new ProviderValueConfigurationEFCore());
    }
}
