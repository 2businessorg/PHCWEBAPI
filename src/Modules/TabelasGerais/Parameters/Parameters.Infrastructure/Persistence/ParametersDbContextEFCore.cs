using Microsoft.EntityFrameworkCore;
using Parameters.Domain.Entities;
using Shared.Kernel.MultiTenancy;

namespace Parameters.Infrastructure.Persistence;

/// <summary>
/// DbContext EF Core para o módulo de Parâmetros - uses TenantContext for dynamic connection
/// </summary>
public class ParametersDbContextEFCore : DbContext
{
    private readonly ITenantContext _tenantContext;

    public ParametersDbContextEFCore(
        DbContextOptions<ParametersDbContextEFCore> options,
        ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    public DbSet<Para1> Para1 { get; set; } = null!;
    public DbSet<Cb> Cb { get; set; } = null!;

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
        modelBuilder.ApplyConfiguration(new Para1ConfigurationEFCore());
        modelBuilder.ApplyConfiguration(new CbConfigurationEFCore());
    }
}
