using Advances.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Kernel.MultiTenancy;

namespace Advances.Infrastructure.Persistence;

/// <summary>
/// DbContext EF Core para o módulo Advances (tabelas rd, tsrd, cl, bl)
/// </summary>
public class AdvancesDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;

    public AdvancesDbContext(
        DbContextOptions<AdvancesDbContext> options,
        ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    public DbSet<Rd> Rd { get; set; } = null!;
    public DbSet<Tsrd> Tsrd { get; set; } = null!;
    public DbSet<Cl> Cl { get; set; } = null!;
    public DbSet<Bl> Bl { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (_tenantContext?.HasDatabaseCredentials == true)
            optionsBuilder.UseSqlServer(_tenantContext.GetConnectionString());
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // rd: chave composta (ndoc, rno, rdano)
        modelBuilder.Entity<Rd>(e =>
        {
            e.ToTable("rd");
            e.HasKey(x => new { x.Ndoc, x.Rno, x.Rdano });
            e.Property(x => x.Rdstamp).HasColumnName("rdstamp").HasMaxLength(25);
            e.Property(x => x.Nmdoc).HasColumnName("nmdoc");
            e.Property(x => x.Rno).HasColumnName("rno");
            e.Property(x => x.Rdata).HasColumnName("rdata");
            e.Property(x => x.Nome).HasColumnName("nome");
            e.Property(x => x.Morada).HasColumnName("morada");
            e.Property(x => x.Local).HasColumnName("local");
            e.Property(x => x.Codpost).HasColumnName("codpost");
            e.Property(x => x.Ncont).HasColumnName("ncont");
            e.Property(x => x.Zona).HasColumnName("zona");
            e.Property(x => x.Nib).HasColumnName("nib");
            e.Property(x => x.Total).HasColumnName("total");
            e.Property(x => x.Etotal).HasColumnName("etotal");
            e.Property(x => x.Base).HasColumnName("base");
            e.Property(x => x.Ebase).HasColumnName("ebase");
            e.Property(x => x.Ndoc).HasColumnName("ndoc");
            e.Property(x => x.No).HasColumnName("no");
            e.Property(x => x.Rdano).HasColumnName("rdano");
            e.Property(x => x.Olcodigo).HasColumnName("olcodigo");
            e.Property(x => x.Moeda).HasColumnName("moeda");
            e.Property(x => x.Contado).HasColumnName("contado");
            e.Property(x => x.Ollocal).HasColumnName("ollocal");
            e.Property(x => x.Cm).HasColumnName("cm");
            e.Property(x => x.Cmdesc).HasColumnName("cmdesc");
            e.Property(x => x.Descricao).HasColumnName("descricao");
            e.Property(x => x.Ousrinis).HasColumnName("ousrinis");
            e.Property(x => x.Ousrdata).HasColumnName("ousrdata");
            e.Property(x => x.Ousrhora).HasColumnName("ousrhora");
            e.Property(x => x.Usrinis).HasColumnName("usrinis");
            e.Property(x => x.Usrdata).HasColumnName("usrdata");
            e.Property(x => x.Usrhora).HasColumnName("usrhora");
        });

        // tsrd: chave primária ndoc
        modelBuilder.Entity<Tsrd>(e =>
        {
            e.ToTable("tsrd");
            e.HasKey(x => x.Ndoc);
            e.Property(x => x.Tsrdstamp).HasColumnName("tsrdstamp").HasMaxLength(25);
            e.Property(x => x.Ndoc).HasColumnName("ndoc");
            e.Property(x => x.Nmdoc).HasColumnName("nmdoc");
            e.Property(x => x.Cmcc).HasColumnName("cmcc");
            e.Property(x => x.Cmccn).HasColumnName("cmccn");
        });

        // cl: chave composta (no, estab)
        modelBuilder.Entity<Cl>(e =>
        {
            e.ToTable("cl");
            e.HasKey(x => new { x.No, x.Estab });
            e.Property(x => x.Clstamp).HasColumnName("clstamp").HasMaxLength(25);
            e.Property(x => x.No).HasColumnName("no");
            e.Property(x => x.Estab).HasColumnName("estab");
            e.Property(x => x.Nome).HasColumnName("nome");
        });

        // bl: chave primária noconta
        modelBuilder.Entity<Bl>(e =>
        {
            e.ToTable("bl");
            e.HasKey(x => x.Noconta);
            e.Property(x => x.Noconta).HasColumnName("noconta");
            e.Property(x => x.Banco).HasColumnName("banco");
            e.Property(x => x.Conta).HasColumnName("conta");
            e.Property(x => x.Inactivo).HasColumnName("inactivo");
        });
    }
}
