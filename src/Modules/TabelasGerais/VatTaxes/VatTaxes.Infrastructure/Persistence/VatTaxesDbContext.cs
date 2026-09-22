using Microsoft.EntityFrameworkCore;
using VatTaxes.Domain;
using Shared.Kernel.MultiTenancy;

namespace VatTaxes.Infrastructure.Persistence;

/// <summary>
/// DbContext EF Core para o módulo VatTaxes - usa TenantContext para conexão dinâmica
/// </summary>
public class VatTaxesDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;

    public VatTaxesDbContext(
        DbContextOptions<VatTaxesDbContext> options,
        ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    public DbSet<TaxasIva> TaxasIva { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure TaxasIva entity
        modelBuilder.Entity<TaxasIva>(entity =>
        {
            entity.ToTable("taxasiva");
            entity.HasKey(e => e.TaxasIvaStamp);

            entity.Property(e => e.TaxasIvaStamp)
                .HasColumnName("taxasivastamp")
                .HasMaxLength(25)
                .IsRequired();

            entity.Property(e => e.Codigo)
                .HasColumnName("codigo")
                .HasPrecision(3, 0)
                .IsRequired();

            entity.Property(e => e.Taxa)
                .HasColumnName("taxa")
                .HasPrecision(5, 2)
                .IsRequired();

            entity.Property(e => e.Ref)
                .HasColumnName("ref")
                .HasMaxLength(18);

            entity.Property(e => e.Design)
                .HasColumnName("design")
                .HasMaxLength(60);

            entity.Property(e => e.UsrInis)
                .HasColumnName("usrinis")
                .HasMaxLength(30);

            entity.Property(e => e.UsrData)
                .HasColumnName("usrdata")
                .HasColumnType("datetime");

            entity.Property(e => e.UsrHora)
                .HasColumnName("usrhora")
                .HasMaxLength(8);

            entity.Property(e => e.OusrInis)
                .HasColumnName("ousrinis")
                .HasMaxLength(30);

            entity.Property(e => e.OusrData)
                .HasColumnName("ousrdata")
                .HasColumnType("datetime");

            entity.Property(e => e.OusrHora)
                .HasColumnName("ousrhora")
                .HasMaxLength(8);

            entity.Property(e => e.Marcada)
                .HasColumnName("marcada");
        });
    }
}
