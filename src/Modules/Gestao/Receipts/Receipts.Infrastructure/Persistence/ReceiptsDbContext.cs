using Microsoft.EntityFrameworkCore;
using Receipts.Domain.Entities;
using Shared.Kernel.MultiTenancy;

namespace Receipts.Infrastructure.Persistence;

/// <summary>
/// DbContext EF Core para o módulo Receipts (tabelas re, rl, tsre, cl, cc)
/// </summary>
public class ReceiptsDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;

    public ReceiptsDbContext(
        DbContextOptions<ReceiptsDbContext> options,
        ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    public DbSet<Re> Re { get; set; } = null!;
    public DbSet<Rl> Rl { get; set; } = null!;
    public DbSet<Tsre> Tsre { get; set; } = null!;
    public DbSet<Cl> Cl { get; set; } = null!;
    public DbSet<Cc> Cc { get; set; } = null!;
    public DbSet<Ft> Ft { get; set; } = null!;
    /// <summary>Contas bancárias/caixas</summary>
    public DbSet<Bl> Bl { get; set; } = null!;

    /// <summary>
    /// Configura as opções do DbContext incluindo a conexão multi-tenant
    /// </summary>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Multi-tenancy: use tenant DB when credentials are available.
        // Falls back to the connection string configured in DI when not available.
        if (_tenantContext?.HasDatabaseCredentials == true)
        {
            optionsBuilder.UseSqlServer(_tenantContext.GetConnectionString());
        }
    }

    /// <summary>
    /// Configura o modelo de dados e mapeamentos das entidades
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // re: chave composta (ndoc, rno, reano)
        modelBuilder.Entity<Re>(e =>
        {
            e.ToTable("re");
            e.HasKey(x => new { x.Ndoc, x.Rno, x.Reano });
            e.Property(x => x.Restamp).HasColumnName("restamp").HasMaxLength(25);
            e.Property(x => x.Nmdoc).HasColumnName("nmdoc");
            e.Property(x => x.Rno).HasColumnName("rno");
            e.Property(x => x.Rdata).HasColumnName("rdata");
            e.Property(x => x.Nome).HasColumnName("nome");
            e.Property(x => x.Total).HasColumnName("total");
            e.Property(x => x.Etotal).HasColumnName("etotal");
            e.Property(x => x.Ndoc).HasColumnName("ndoc");
            e.Property(x => x.No).HasColumnName("no");
            e.Property(x => x.Reano).HasColumnName("reano");
            e.Property(x => x.Olcodigo).HasColumnName("olcodigo");
            e.Property(x => x.Totalmoeda)
                .HasColumnName("totalmoeda")
                .HasPrecision(15, 2);
            e.Property(x => x.Moeda).HasColumnName("moeda");
            e.Property(x => x.Contado).HasColumnName("contado");
            e.Property(x => x.Ollocal).HasColumnName("ollocal");
            e.Property(x => x.Ccstamp).HasColumnName("ccstamp");
            e.Property(x => x.Ousrinis).HasColumnName("ousrinis");
            e.Property(x => x.Ousrdata).HasColumnName("ousrdata");
            e.Property(x => x.Ousrhora).HasColumnName("ousrhora");
            e.Property(x => x.Usrinis).HasColumnName("usrinis");
            e.Property(x => x.Usrdata).HasColumnName("usrdata");
            e.Property(x => x.Usrhora).HasColumnName("usrhora");
            e.Property(x => x.Marcada).HasColumnName("marcada");
        });

        // rl: chave primária rlstamp
        modelBuilder.Entity<Rl>(e =>
        {
            e.ToTable("rl");
            e.HasKey(x => x.Rlstamp);
            e.Property(x => x.Rlstamp).HasColumnName("rlstamp").HasMaxLength(25);
            e.Property(x => x.Ndoc).HasColumnName("ndoc");
            e.Property(x => x.Rno).HasColumnName("rno");
            e.Property(x => x.Cdesc).HasColumnName("cdesc");
            e.Property(x => x.Nrdoc).HasColumnName("nrdoc");
            e.Property(x => x.Rec).HasColumnName("rec");
            e.Property(x => x.Erec).HasColumnName("erec");
            e.Property(x => x.Eval).HasColumnName("eval");
            e.Property(x => x.Datalc).HasColumnName("datalc");
            e.Property(x => x.Dataven).HasColumnName("dataven");
            e.Property(x => x.Restamp).HasColumnName("restamp");
            e.Property(x => x.Ccstamp).HasColumnName("ccstamp");
            e.Property(x => x.Ousrinis).HasColumnName("ousrinis");
            e.Property(x => x.Ousrdata).HasColumnName("ousrdata");
            e.Property(x => x.Ousrhora).HasColumnName("ousrhora");
            e.Property(x => x.Usrinis).HasColumnName("usrinis");
            e.Property(x => x.Usrdata).HasColumnName("usrdata");
            e.Property(x => x.Usrhora).HasColumnName("usrhora");
            e.Property(x => x.Marcada).HasColumnName("marcada");
        });

        // tsre: chave primária ndoc
        modelBuilder.Entity<Tsre>(e =>
        {
            e.ToTable("tsre");
            e.HasKey(x => x.Ndoc);
            e.Property(x => x.Tsrestamp).HasColumnName("tsrestamp").HasMaxLength(25);
            e.Property(x => x.Nmdoc).HasColumnName("nmdoc");
            e.Property(x => x.Ndoc).HasColumnName("ndoc");
            e.Property(x => x.Ndino).HasColumnName("ndino");
            e.Property(x => x.Ndidesc).HasColumnName("ndidesc");
            e.Property(x => x.Ousrinis).HasColumnName("ousrinis");
            e.Property(x => x.Ousrdata).HasColumnName("ousrdata");
            e.Property(x => x.Ousrhora).HasColumnName("ousrhora");
            e.Property(x => x.Usrinis).HasColumnName("usrinis");
            e.Property(x => x.Usrdata).HasColumnName("usrdata");
            e.Property(x => x.Usrhora).HasColumnName("usrhora");
            e.Property(x => x.Marcada).HasColumnName("marcada");
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

        // cc: chave primária ccstamp
        modelBuilder.Entity<Cc>(e =>
        {
            e.ToTable("cc");
            e.HasKey(x => x.Ccstamp);
            e.Property(x => x.Ccstamp).HasColumnName("ccstamp").HasMaxLength(25);
            e.Property(x => x.No).HasColumnName("no");
            e.Property(x => x.Nrdoc).HasColumnName("nrdoc");
            e.Property(x => x.Deb).HasColumnName("deb");
            e.Property(x => x.Edeb).HasColumnName("edeb");
            e.Property(x => x.Datalc).HasColumnName("datalc");
        });

        // ft: chave ftstamp (usada para resolver dados da factura a partir do ccstamp)
        modelBuilder.Entity<Ft>(e =>
        {
            e.ToTable("ft");
            e.HasKey(x => x.Ftstamp);
            e.Property(x => x.Ftstamp).HasColumnName("ftstamp").HasMaxLength(25);
            e.Property(x => x.Fno).HasColumnName("fno");
            e.Property(x => x.Ftano).HasColumnName("ftano");
            e.Property(x => x.Ndoc).HasColumnName("ndoc");
            e.Property(x => x.Nmdoc).HasColumnName("nmdoc");
            e.Property(x => x.Series).HasColumnName("series");
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
