using Microsoft.EntityFrameworkCore;
using Shared.Kernel.MultiTenancy;
using Treasury.Domain.Entities;

namespace Treasury.Infrastructure.Persistence;

/// <summary>
/// EF Core DbContext for Treasury (PHC tables <c>bl</c>, <c>ba</c>, <c>br</c>).
/// Database-first: tables already exist.
/// </summary>
public class TreasuryDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;

    public TreasuryDbContext(
        DbContextOptions<TreasuryDbContext> options,
        ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    public DbSet<TreasuryAccount> TreasuryAccounts { get; set; } = null!;
    public DbSet<TreasuryAccountMovement> TreasuryAccountMovements { get; set; } = null!;
    public DbSet<ImportedBankMovement> ImportedBankMovements { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (_tenantContext?.HasDatabaseCredentials == true)
        {
            optionsBuilder.UseSqlServer(_tenantContext.GetConnectionString());
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TreasuryAccount>(e =>
        {
            e.ToTable("bl");
            e.HasKey(x => x.AccountCode);
            e.Property(x => x.AccountCode).HasColumnName("noconta");
            e.Property(x => x.Name).HasColumnName("banco");
            e.Property(x => x.AccountNumber).HasColumnName("conta");
            e.Property(x => x.Inactive).HasColumnName("inactivo");
            e.Property(x => x.Currency).HasColumnName("moeda");
            e.Property(x => x.Balance).HasColumnName("saldo").HasPrecision(18, 2);
        });

        modelBuilder.Entity<TreasuryAccountMovement>(e =>
        {
            e.ToTable("ba");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("bastamp").HasMaxLength(25);
            e.Property(x => x.Date).HasColumnName("data");
            e.Property(x => x.ValueDate).HasColumnName("dvalor");
            e.Property(x => x.Document).HasColumnName("documento");
            e.Property(x => x.Description).HasColumnName("descricao");
            e.Property(x => x.Inflow).HasColumnName("entrada").HasPrecision(18, 2);
            e.Property(x => x.Outflow).HasColumnName("saida").HasPrecision(18, 2);
            e.Property(x => x.Cheque).HasColumnName("cheque");
            e.Property(x => x.AccountCode).HasColumnName("contado");
            e.Property(x => x.Reconciled).HasColumnName("reco");
        });

        modelBuilder.Entity<ImportedBankMovement>(e =>
        {
            e.ToTable("br");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("brstamp").HasMaxLength(25);
            e.Property(x => x.Date).HasColumnName("data");
            e.Property(x => x.ValueDate).HasColumnName("dvalor");
            e.Property(x => x.Document).HasColumnName("documento");
            e.Property(x => x.Description).HasColumnName("descricao");
            e.Property(x => x.Amount).HasColumnName("valor").HasPrecision(18, 2);
            e.Property(x => x.Cheque).HasColumnName("cheque");
            e.Property(x => x.AccountCode).HasColumnName("contado");
            e.Property(x => x.Reconciled).HasColumnName("reco");
            e.Property(x => x.Ignored).HasColumnName("ignorado");
        });
    }
}
