using Microsoft.EntityFrameworkCore;
using Audit.Domain.Entities;
using Shared.Kernel.MultiTenancy;

namespace Audit.Infrastructure.Persistence;


public class AuditDbContextEFCore : DbContext
{
    private readonly ITenantContext _tenantContext;

    public AuditDbContextEFCore(
        DbContextOptions<AuditDbContextEFCore> options,
        ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    public DbSet<AuditLog> AuditLogs { get; set; } = null!;

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
        modelBuilder.ApplyConfiguration(new AuditLogConfigurationEFCore());
    }
}
