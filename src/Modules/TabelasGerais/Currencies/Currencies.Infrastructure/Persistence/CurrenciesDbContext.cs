using Currencies.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Kernel.MultiTenancy;

namespace Currencies.Infrastructure.Persistence;

/// <summary>
/// DbContext EF Core para o módulo Currencies.
/// </summary>
public class CurrenciesDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;

    public CurrenciesDbContext(
        DbContextOptions<CurrenciesDbContext> options,
        ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    public DbSet<Para1> Para1 { get; set; } = null!;
    public DbSet<Cb> Cb { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (_tenantContext?.HasDatabaseCredentials == true)
        {
            optionsBuilder.UseSqlServer(_tenantContext.GetConnectionString());
        }

        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new Configurations.Para1Configuration());
        modelBuilder.ApplyConfiguration(new Configurations.CbConfiguration());
    }
}
