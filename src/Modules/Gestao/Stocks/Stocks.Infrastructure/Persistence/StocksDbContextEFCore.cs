using Microsoft.EntityFrameworkCore;
using Stocks.Domain.Entities;
using Shared.Kernel.MultiTenancy;

namespace Stocks.Infrastructure.Persistence;

/// <summary>
/// DbContext EF Core para o módulo Stocks - uses TenantContext for dynamic connection
/// </summary>
public class StocksDbContextEFCore : DbContext
{
    private readonly ITenantContext _tenantContext;

    public StocksDbContextEFCore(
        DbContextOptions<StocksDbContextEFCore> options,
        ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    public virtual DbSet<St> St { get; set; } = null!;
    public virtual DbSet<Sa> Sa { get; set; } = null!;
    public virtual DbSet<Se> Se { get; set; } = null!;
    public virtual DbSet<Sal> Sal { get; set; } = null!;
    public virtual DbSet<Sz> Sz { get; set; } = null!;

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

        modelBuilder.Entity<St>(entity =>
        {
            entity.HasKey(e => e.Ref)
                .HasName("pk_st")
                .IsClustered(false);

            entity.ToTable("st");

            entity.HasIndex(e => e.Ref, "in_st_ref").HasFillFactor(70);
            entity.HasIndex(e => e.Design, "in_st_design").HasFillFactor(70);
            entity.HasIndex(e => e.Familia, "in_st_familia").HasFillFactor(70);
            entity.HasIndex(e => e.Ststamp, "in_st_stamp").IsUnique().HasFillFactor(70);

            entity.Property(e => e.Ref)
                .HasMaxLength(18)
                .IsUnicode(false)
                .HasColumnName("ref")
                .HasDefaultValueSql("('')")
                .IsFixedLength();

            entity.Property(e => e.Ststamp)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("ststamp")
                .HasDefaultValueSql("('')")
                .IsFixedLength();

            entity.Property(e => e.Design)
                .HasMaxLength(60)
                .IsUnicode(false)
                .HasColumnName("design")
                .HasDefaultValueSql("('')")
                .IsFixedLength();

            entity.Property(e => e.Stns).HasColumnName("stns");

            entity.Property(e => e.Familia)
                .HasMaxLength(18)
                .IsUnicode(false)
                .HasColumnName("familia")
                .HasDefaultValueSql("('')");

            entity.Property(e => e.Faminome)
                .HasMaxLength(60)
                .IsUnicode(false)
                .HasColumnName("faminome")
                .HasDefaultValueSql("('')");

            entity.Property(e => e.Epv1)
                .HasColumnType("numeric(19, 6)")
                .HasColumnName("epv1");

            entity.Property(e => e.Pv1)
                .HasColumnType("numeric(18, 5)")
                .HasColumnName("pv1");

            entity.Property(e => e.Iva1incl).HasColumnName("iva1incl");

            entity.Property(e => e.Epv2)
                .HasColumnType("numeric(19, 6)")
                .HasColumnName("epv2");

            entity.Property(e => e.Pv2)
                .HasColumnType("numeric(18, 5)")
                .HasColumnName("pv2");

            entity.Property(e => e.Iva2incl).HasColumnName("iva2incl");

            entity.Property(e => e.Epv3)
                .HasColumnType("numeric(19, 6)")
                .HasColumnName("epv3");

            entity.Property(e => e.Pv3)
                .HasColumnType("numeric(18, 5)")
                .HasColumnName("pv3");

            entity.Property(e => e.Iva3incl).HasColumnName("iva3incl");

            entity.Property(e => e.Epv4)
                .HasColumnType("numeric(19, 6)")
                .HasColumnName("epv4");

            entity.Property(e => e.Pv4)
                .HasColumnType("numeric(18, 5)")
                .HasColumnName("pv4");

            entity.Property(e => e.Iva4incl).HasColumnName("iva4incl");

            entity.Property(e => e.Epv5)
                .HasColumnType("numeric(19, 6)")
                .HasColumnName("epv5");

            entity.Property(e => e.Pv5)
                .HasColumnType("numeric(18, 5)")
                .HasColumnName("pv5");

            entity.Property(e => e.Iva5incl).HasColumnName("iva5incl");

            entity.Property(e => e.Epcusto)
                .HasColumnType("numeric(19, 6)")
                .HasColumnName("epcusto");

            entity.Property(e => e.Pcusto)
                .HasColumnType("numeric(18, 5)")
                .HasColumnName("pcusto");

            entity.Property(e => e.Stock)
                .HasColumnType("numeric(14, 3)")
                .HasColumnName("stock");

            entity.Property(e => e.Tabiva)
                .HasColumnType("numeric(3, 0)")
                .HasColumnName("tabiva");

            entity.Property(e => e.Obs)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("obs")
                .HasDefaultValueSql("('')");

            entity.Property(e => e.Inactivo).HasColumnName("inactivo");

            entity.Property(e => e.Ousrinis)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("ousrinis")
                .HasDefaultValueSql("('')");

            entity.Property(e => e.Ousrdata)
                .HasColumnType("datetime")
                .HasColumnName("ousrdata")
                .HasDefaultValueSql("(getdate())");

            entity.Property(e => e.Ousrhora)
                .HasMaxLength(8)
                .IsUnicode(false)
                .HasColumnName("ousrhora")
                .HasDefaultValueSql("('')");

            entity.Property(e => e.Usrinis)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("usrinis")
                .HasDefaultValueSql("('')");

            entity.Property(e => e.Usrdata)
                .HasColumnType("datetime")
                .HasColumnName("usrdata")
                .HasDefaultValueSql("(getdate())");

            entity.Property(e => e.Usrhora)
                .HasMaxLength(8)
                .IsUnicode(false)
                .HasColumnName("usrhora")
                .HasDefaultValueSql("('')");
        });

        modelBuilder.Entity<Sa>(entity =>
        {
            entity.HasKey(e => new { e.Ref, e.Armazem })
                .HasName("pk_sa");

            entity.ToTable("sa", "dbo");

            entity.Property(e => e.Ref)
                .HasMaxLength(18)
                .IsUnicode(false)
                .HasColumnName("ref");

            entity.Property(e => e.Armazem)
                .HasColumnType("numeric(5, 0)")
                .HasColumnName("armazem");

            entity.Property(e => e.Sastamp)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("sastamp");

            entity.Property(e => e.Stock)
                .HasColumnType("numeric(13, 3)")
                .HasColumnName("stock");

            entity.Property(e => e.Local)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("local");

            entity.Property(e => e.Optimo)
                .HasColumnType("numeric(13, 3)")
                .HasColumnName("optimo");

            entity.Property(e => e.Rescli)
                .HasColumnType("numeric(15, 3)")
                .HasColumnName("rescli");

            entity.Property(e => e.Resfor)
                .HasColumnType("numeric(15, 3)")
                .HasColumnName("resfor");

            entity.Property(e => e.Qttrec)
                .HasColumnType("numeric(15, 3)")
                .HasColumnName("qttrec");

            entity.Property(e => e.Eoq)
                .HasColumnType("numeric(15, 3)")
                .HasColumnName("eoq");

            entity.Property(e => e.Stmin)
                .HasColumnType("numeric(15, 3)")
                .HasColumnName("stmin");

            entity.Property(e => e.Consumo)
                .HasColumnType("numeric(15, 3)")
                .HasColumnName("consumo");

            entity.Property(e => e.Ptoenc)
                .HasColumnType("numeric(15, 3)")
                .HasColumnName("ptoenc");

            entity.Property(e => e.Qttacin)
                .HasColumnType("numeric(15, 3)")
                .HasColumnName("qttacin");

            entity.Property(e => e.Pcpond)
                .HasColumnType("numeric(18, 5)")
                .HasColumnName("pcpond");

            entity.Property(e => e.Epcpond)
                .HasColumnType("numeric(19, 6)")
                .HasColumnName("epcpond");

            entity.Property(e => e.Rescat)
                .HasColumnType("numeric(15, 3)")
                .HasColumnName("rescat");

            entity.Property(e => e.Epv)
                .HasColumnType("numeric(19, 6)")
                .HasColumnName("epv");

            entity.Property(e => e.Pv)
                .HasColumnType("numeric(18, 5)")
                .HasColumnName("pv");

            entity.Property(e => e.Ousrinis)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("ousrinis");

            entity.Property(e => e.Ousrdata)
                .HasColumnType("datetime")
                .HasColumnName("ousrdata");

            entity.Property(e => e.Ousrhora)
                .HasMaxLength(8)
                .IsUnicode(false)
                .HasColumnName("ousrhora");

            entity.Property(e => e.Usrinis)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("usrinis");

            entity.Property(e => e.Usrdata)
                .HasColumnType("datetime")
                .HasColumnName("usrdata");

            entity.Property(e => e.Usrhora)
                .HasMaxLength(8)
                .IsUnicode(false)
                .HasColumnName("usrhora");

            entity.Property(e => e.Marcada)
                .HasColumnName("marcada");
        });

        modelBuilder.Entity<Sal>(entity =>
        {
            entity.HasKey(e => new { e.Ref, e.Armazem, e.Lote })
                .HasName("pk_sal");

            entity.ToTable("sal", "dbo");

            entity.Property(e => e.Ref)
                .HasMaxLength(18)
                .IsUnicode(false)
                .HasColumnName("ref");

            entity.Property(e => e.Armazem)
                .HasColumnType("numeric(5, 0)")
                .HasColumnName("armazem");

            entity.Property(e => e.Lote)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("lote");

            entity.Property(e => e.Salstamp)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("salstamp");

            entity.Property(e => e.Stock)
                .HasColumnType("numeric(13, 3)")
                .HasColumnName("stock");

            entity.Property(e => e.Qttrec)
                .HasColumnType("numeric(15, 3)")
                .HasColumnName("qttrec");

            entity.Property(e => e.Qttacin)
                .HasColumnType("numeric(15, 3)")
                .HasColumnName("qttacin");

            entity.Property(e => e.Pcpond)
                .HasColumnType("numeric(18, 5)")
                .HasColumnName("pcpond");

            entity.Property(e => e.Epcpond)
                .HasColumnType("numeric(19, 6)")
                .HasColumnName("epcpond");

            entity.Property(e => e.Qttcat)
                .HasColumnType("numeric(15, 3)")
                .HasColumnName("qttcat");

            entity.Property(e => e.Ousrinis)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("ousrinis");

            entity.Property(e => e.Ousrdata)
                .HasColumnType("datetime")
                .HasColumnName("ousrdata");

            entity.Property(e => e.Ousrhora)
                .HasMaxLength(8)
                .IsUnicode(false)
                .HasColumnName("ousrhora");

            entity.Property(e => e.Usrinis)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("usrinis");

            entity.Property(e => e.Usrdata)
                .HasColumnType("datetime")
                .HasColumnName("usrdata");

            entity.Property(e => e.Usrhora)
                .HasMaxLength(8)
                .IsUnicode(false)
                .HasColumnName("usrhora");

            entity.Property(e => e.Marcada)
                .HasColumnName("marcada");
        });

        modelBuilder.Entity<Se>(entity =>
        {
            entity.HasKey(e => new { e.Lote, e.Ref })
                .HasName("pk_se");

            entity.ToTable("se", "dbo");

            entity.Property(e => e.Lote)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("lote");

            entity.Property(e => e.Ref)
                .HasMaxLength(18)
                .IsUnicode(false)
                .HasColumnName("ref");

            entity.Property(e => e.Sestamp)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("sestamp");

            entity.Property(e => e.Stock)
                .HasColumnType("numeric(13, 3)")
                .HasColumnName("stock");

            entity.Property(e => e.Validade)
                .HasColumnType("datetime")
                .HasColumnName("validade");

            entity.Property(e => e.Design)
                .HasMaxLength(60)
                .IsUnicode(false)
                .HasColumnName("design");

            entity.Property(e => e.Pcult)
                .HasColumnType("numeric(18, 5)")
                .HasColumnName("pcult");

            entity.Property(e => e.Pcpond)
                .HasColumnType("numeric(18, 5)")
                .HasColumnName("pcpond");

            entity.Property(e => e.Epcult)
                .HasColumnType("numeric(19, 6)")
                .HasColumnName("epcult");

            entity.Property(e => e.Epcpond)
                .HasColumnType("numeric(19, 6)")
                .HasColumnName("epcpond");

            entity.Property(e => e.Usaid)
                .HasColumnType("datetime")
                .HasColumnName("usaid");

            entity.Property(e => e.Uintr)
                .HasColumnType("datetime")
                .HasColumnName("uintr");

            entity.Property(e => e.Pcimp)
                .HasColumnType("numeric(18, 5)")
                .HasColumnName("pcimp");

            entity.Property(e => e.Pcmoe)
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("pcmoe");

            entity.Property(e => e.Pcusto)
                .HasColumnType("numeric(18, 5)")
                .HasColumnName("pcusto");

            entity.Property(e => e.Data)
                .HasColumnType("datetime")
                .HasColumnName("data");

            entity.Property(e => e.Qttacout)
                .HasColumnType("numeric(14, 3)")
                .HasColumnName("qttacout");

            entity.Property(e => e.Qttacin)
                .HasColumnType("numeric(14, 3)")
                .HasColumnName("qttacin");

            entity.Property(e => e.Datafact)
                .HasColumnType("datetime")
                .HasColumnName("datafact");

            entity.Property(e => e.Forref)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("forref");

            entity.Property(e => e.Forlote)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("forlote");

            entity.Property(e => e.Epcust)
                .HasColumnType("numeric(19, 6)")
                .HasColumnName("epcust");

            entity.Property(e => e.Epcimp)
                .HasColumnType("numeric(19, 6)")
                .HasColumnName("epcimp");

            entity.Property(e => e.Qttrec)
                .HasColumnType("numeric(15, 3)")
                .HasColumnName("qttrec");

            entity.Property(e => e.Tipoiect)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("tipoiect");

            entity.Property(e => e.Inactivo)
                .HasColumnName("inactivo");

            entity.Property(e => e.Ousrinis)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("ousrinis");

            entity.Property(e => e.Ousrdata)
                .HasColumnType("datetime")
                .HasColumnName("ousrdata");

            entity.Property(e => e.Ousrhora)
                .HasMaxLength(8)
                .IsUnicode(false)
                .HasColumnName("ousrhora");

            entity.Property(e => e.Usrinis)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("usrinis");

            entity.Property(e => e.Usrdata)
                .HasColumnType("datetime")
                .HasColumnName("usrdata");

            entity.Property(e => e.Usrhora)
                .HasMaxLength(8)
                .IsUnicode(false)
                .HasColumnName("usrhora");

            entity.Property(e => e.Marcada)
                .HasColumnName("marcada");
        });

        modelBuilder.Entity<Sz>(entity =>
        {
            entity.HasKey(e => e.No)
                .HasName("pk_sz")
                .IsClustered(false);

            entity.ToTable("sz", "dbo");

            entity.Property(e => e.No)
                .HasColumnType("numeric(5, 0)")
                .HasColumnName("no");

            entity.Property(e => e.Szstamp)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("szstamp");

            entity.Property(e => e.Nome)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("nome");

            entity.Property(e => e.Nomecl)
                .HasMaxLength(55)
                .IsUnicode(false)
                .HasColumnName("nomecl");

            entity.Property(e => e.Nocl)
                .HasColumnType("numeric(10, 0)")
                .HasColumnName("nocl");

            entity.Property(e => e.Estabcl)
                .HasColumnType("numeric(3, 0)")
                .HasColumnName("estabcl");

            entity.Property(e => e.Site)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("site");

            entity.Property(e => e.Ousrinis)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("ousrinis");

            entity.Property(e => e.Ousrdata)
                .HasColumnType("datetime")
                .HasColumnName("ousrdata");

            entity.Property(e => e.Ousrhora)
                .HasMaxLength(8)
                .IsUnicode(false)
                .HasColumnName("ousrhora");

            entity.Property(e => e.Usrinis)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("usrinis");

            entity.Property(e => e.Usrdata)
                .HasColumnType("datetime")
                .HasColumnName("usrdata");

            entity.Property(e => e.Usrhora)
                .HasMaxLength(8)
                .IsUnicode(false)
                .HasColumnName("usrhora");

            entity.Property(e => e.Marcada)
                .HasColumnName("marcada");
        });
    }
}
