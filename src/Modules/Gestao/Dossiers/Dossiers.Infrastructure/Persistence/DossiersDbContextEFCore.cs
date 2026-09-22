using Dossiers.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Kernel.MultiTenancy;

namespace Dossiers.Infrastructure.Persistence;

/// <summary>
/// DbContext EF Core para o módulo Dossiers - uses TenantContext for dynamic connection
/// </summary>
public class DossiersDbContextEFCore : DbContext
{
    private readonly ITenantContext _tenantContext;

    public DossiersDbContextEFCore(
        DbContextOptions<DossiersDbContextEFCore> options,
        ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    public virtual DbSet<Bo> Bo { get; set; } = null!;
    public virtual DbSet<Bo2> Bo2 { get; set; } = null!;
    public virtual DbSet<Bo3> Bo3 { get; set; } = null!;
    public virtual DbSet<Bi> Bi { get; set; } = null!;
    public virtual DbSet<Bi2> Bi2 { get; set; } = null!;
    public virtual DbSet<Bot> Bot { get; set; } = null!;
    public virtual DbSet<Cl> Cl { get; set; } = null!;
    public virtual DbSet<Cl2> Cl2 { get; set; } = null!;
    public virtual DbSet<Fl> Fl { get; set; } = null!;
    public virtual DbSet<Ag> Ag { get; set; } = null!;
    public virtual DbSet<Em> Em { get; set; } = null!;
    public virtual DbSet<St> St { get; set; } = null!;
    public virtual DbSet<Stfami> Stfami { get; set; } = null!;
    public virtual DbSet<Taxasiva> Taxasiva { get; set; } = null!;
    public virtual DbSet<Ts> Ts { get; set; } = null!;

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

        modelBuilder.Entity<Bo>(entity =>
        {
            entity.ToTable("bo");
            entity.HasKey(e => new { e.Ndos, e.Obrano, e.Boano }).HasName("pk_bo").IsClustered(false);
            entity.HasIndex(e => e.Bostamp, "in_bo_stamp").IsUnique();

            entity.Property(e => e.Bostamp).HasMaxLength(25).IsUnicode(false).HasColumnName("bostamp").IsFixedLength();
            entity.Property(e => e.Ndos).HasColumnType("numeric(3, 0)").HasColumnName("ndos");
            entity.Property(e => e.Obrano).HasColumnType("numeric(10, 0)").HasColumnName("obrano");
            entity.Property(e => e.Boano).HasColumnType("numeric(4, 0)").HasColumnName("boano");
            entity.Property(e => e.No).HasColumnType("numeric(10, 0)").HasColumnName("no");
            entity.Property(e => e.Estab).HasColumnType("numeric(3, 0)").HasColumnName("estab");
            entity.Property(e => e.Nome).HasMaxLength(55).IsUnicode(false).HasColumnName("nome");
            entity.Property(e => e.Nmdos).HasMaxLength(24).IsUnicode(false).HasColumnName("nmdos");
            entity.Property(e => e.Dataobra).HasColumnType("datetime").HasColumnName("dataobra");
            entity.Property(e => e.Moeda).HasMaxLength(11).IsUnicode(false).HasColumnName("moeda");
            entity.Property(e => e.UBotot).HasColumnType("numeric(12, 2)").HasColumnName("u_botot");

            // Campos de auditoria
            entity.Property(e => e.Ousrinis).HasMaxLength(20).IsUnicode(false).HasColumnName("ousrinis");
            entity.Property(e => e.Ousrdata).HasColumnType("datetime").HasColumnName("ousrdata");
            entity.Property(e => e.Ousrhora).HasMaxLength(8).IsUnicode(false).HasColumnName("ousrhora");
            entity.Property(e => e.Usrinis).HasMaxLength(20).IsUnicode(false).HasColumnName("usrinis");
            entity.Property(e => e.Usrdata).HasColumnType("datetime").HasColumnName("usrdata");
            entity.Property(e => e.Usrhora).HasMaxLength(8).IsUnicode(false).HasColumnName("usrhora");
        });

        modelBuilder.Entity<Bo2>(entity =>
        {
            entity.ToTable("bo2");
            entity.HasKey(e => e.Bo2stamp).HasName("pk_bo2");

            entity.Property(e => e.Bo2stamp).HasMaxLength(25).IsUnicode(false).HasColumnName("bo2stamp").IsFixedLength();
            entity.Property(e => e.Totalciva).HasColumnType("numeric(18, 5)").HasColumnName("totalciva");

            // Campos de auditoria
            entity.Property(e => e.Ousrinis).HasMaxLength(20).IsUnicode(false).HasColumnName("ousrinis");
            entity.Property(e => e.Ousrdata).HasColumnType("datetime").HasColumnName("ousrdata");
            entity.Property(e => e.Ousrhora).HasMaxLength(8).IsUnicode(false).HasColumnName("ousrhora");
            entity.Property(e => e.Usrinis).HasMaxLength(20).IsUnicode(false).HasColumnName("usrinis");
            entity.Property(e => e.Usrdata).HasColumnType("datetime").HasColumnName("usrdata");
            entity.Property(e => e.Usrhora).HasMaxLength(8).IsUnicode(false).HasColumnName("usrhora");
        });

        modelBuilder.Entity<Bo3>(entity =>
        {
            entity.ToTable("bo3");
            entity.HasKey(e => e.Bo3stamp).HasName("pk_bo3");
            entity.Property(e => e.Bo3stamp).HasMaxLength(25).IsUnicode(false).HasColumnName("bo3stamp").IsFixedLength();

            // Campos de auditoria
            entity.Property(e => e.Ousrinis).HasMaxLength(20).IsUnicode(false).HasColumnName("ousrinis");
            entity.Property(e => e.Ousrdata).HasColumnType("datetime").HasColumnName("ousrdata");
            entity.Property(e => e.Ousrhora).HasMaxLength(8).IsUnicode(false).HasColumnName("ousrhora");
            entity.Property(e => e.Usrinis).HasMaxLength(20).IsUnicode(false).HasColumnName("usrinis");
            entity.Property(e => e.Usrdata).HasColumnType("datetime").HasColumnName("usrdata");
            entity.Property(e => e.Usrhora).HasMaxLength(8).IsUnicode(false).HasColumnName("usrhora");

        });

        modelBuilder.Entity<Bi>(entity =>
        {
            entity.ToTable("bi");
            entity.HasKey(e => e.Bistamp).HasName("pk_bi");

            entity.Property(e => e.Bistamp).HasMaxLength(25).IsUnicode(false).HasColumnName("bistamp").IsFixedLength();
            entity.Property(e => e.Bostamp).HasMaxLength(25).IsUnicode(false).HasColumnName("bostamp").IsFixedLength();
            entity.Property(e => e.Ref).HasMaxLength(18).IsUnicode(false).HasColumnName("ref");
            entity.Property(e => e.Design).HasMaxLength(60).IsUnicode(false).HasColumnName("design");
            entity.Property(e => e.Qtt).HasColumnType("numeric(18, 6)").HasColumnName("qtt");
            entity.Property(e => e.Tabiva).HasColumnType("numeric(2, 0)").HasColumnName("tabiva");
            entity.Property(e => e.Iva).HasColumnType("numeric(5, 2)").HasColumnName("iva");
            entity.Property(e => e.Ivaincl).HasColumnName("ivaincl");
            entity.Property(e => e.Debito).HasColumnType("numeric(18, 6)").HasColumnName("debito");
            entity.Property(e => e.Ttdeb).HasColumnType("numeric(18, 6)").HasColumnName("ttdeb");

            // Campos de auditoria
            entity.Property(e => e.Ousrinis).HasMaxLength(20).IsUnicode(false).HasColumnName("ousrinis");
            entity.Property(e => e.Ousrdata).HasColumnType("datetime").HasColumnName("ousrdata");
            entity.Property(e => e.Ousrhora).HasMaxLength(8).IsUnicode(false).HasColumnName("ousrhora");
            entity.Property(e => e.Usrinis).HasMaxLength(20).IsUnicode(false).HasColumnName("usrinis");
            entity.Property(e => e.Usrdata).HasColumnType("datetime").HasColumnName("usrdata");
            entity.Property(e => e.Usrhora).HasMaxLength(8).IsUnicode(false).HasColumnName("usrhora");
        });

        modelBuilder.Entity<Bi2>(entity =>
        {
            entity.ToTable("bi2");
            entity.HasKey(e => e.Bi2stamp).HasName("pk_bi2");
            entity.Property(e => e.Bi2stamp).HasMaxLength(25).IsUnicode(false).HasColumnName("bi2stamp").IsFixedLength();
            entity.Property(e => e.Bostamp).HasMaxLength(25).IsUnicode(false).HasColumnName("bostamp").IsFixedLength();

            // Campos de auditoria
            entity.Property(e => e.Ousrinis).HasMaxLength(20).IsUnicode(false).HasColumnName("ousrinis");
            entity.Property(e => e.Ousrdata).HasColumnType("datetime").HasColumnName("ousrdata");
            entity.Property(e => e.Ousrhora).HasMaxLength(8).IsUnicode(false).HasColumnName("ousrhora");
            entity.Property(e => e.Usrinis).HasMaxLength(20).IsUnicode(false).HasColumnName("usrinis");
            entity.Property(e => e.Usrdata).HasColumnType("datetime").HasColumnName("usrdata");
            entity.Property(e => e.Usrhora).HasMaxLength(8).IsUnicode(false).HasColumnName("usrhora");
        });

        modelBuilder.Entity<Bot>(entity =>
        {
            entity.ToTable("bot");
            entity.HasKey(e => e.Botstamp).HasName("pk_bot");
            entity.Property(e => e.Botstamp).HasMaxLength(25).IsUnicode(false).HasColumnName("botstamp").IsFixedLength();
            entity.Property(e => e.Bostamp).HasMaxLength(25).IsUnicode(false).HasColumnName("bostamp").IsFixedLength();
            entity.Property(e => e.Codigo).HasColumnType("numeric(2, 0)").HasColumnName("codigo");
            entity.Property(e => e.Taxa).HasColumnType("numeric(7, 3)").HasColumnName("taxa");
            entity.Property(e => e.Baseinc).HasColumnType("numeric(18, 5)").HasColumnName("baseinc");
            entity.Property(e => e.Ebaseinc).HasColumnType("numeric(19, 6)").HasColumnName("ebaseinc");
            entity.Property(e => e.Valor).HasColumnType("numeric(18, 5)").HasColumnName("valor");
            entity.Property(e => e.Evalor).HasColumnType("numeric(19, 6)").HasColumnName("evalor");

            // Campos de auditoria
            entity.Property(e => e.Ousrinis).HasMaxLength(20).IsUnicode(false).HasColumnName("ousrinis");
            entity.Property(e => e.Ousrdata).HasColumnType("datetime").HasColumnName("ousrdata");
            entity.Property(e => e.Ousrhora).HasMaxLength(8).IsUnicode(false).HasColumnName("ousrhora");
            entity.Property(e => e.Usrinis).HasMaxLength(20).IsUnicode(false).HasColumnName("usrinis");
            entity.Property(e => e.Usrdata).HasColumnType("datetime").HasColumnName("usrdata");
            entity.Property(e => e.Usrhora).HasMaxLength(8).IsUnicode(false).HasColumnName("usrhora");
        });

        modelBuilder.Entity<Cl>(entity =>
        {
            entity.ToTable("cl");
            entity.HasKey(e => new { e.No, e.Estab }).HasName("pk_cl").IsClustered(false);
            entity.Property(e => e.No).HasColumnType("numeric(10, 0)").HasColumnName("no");
            entity.Property(e => e.Estab).HasColumnType("numeric(3, 0)").HasColumnName("estab");
            entity.Property(e => e.Clstamp).HasMaxLength(25).IsUnicode(false).HasColumnName("clstamp").IsFixedLength();
            entity.Property(e => e.Nome).HasMaxLength(55).IsUnicode(false).HasColumnName("nome");
            entity.Property(e => e.Ncont).HasMaxLength(20).IsUnicode(false).HasColumnName("ncont");
            entity.Property(e => e.Preco).HasColumnType("numeric(1, 0)").HasColumnName("preco");
            entity.Property(e => e.Morada).HasMaxLength(40).IsUnicode(false).HasColumnName("morada");
            entity.Property(e => e.Local).HasMaxLength(25).IsUnicode(false).HasColumnName("local");
            entity.Property(e => e.Codpost).HasMaxLength(10).IsUnicode(false).HasColumnName("codpost");
            entity.Property(e => e.Segmento).HasMaxLength(30).IsUnicode(false).HasColumnName("segmento");
            entity.Property(e => e.Telefone).HasMaxLength(20).IsUnicode(false).HasColumnName("telefone");
            entity.Property(e => e.Contacto).HasMaxLength(20).IsUnicode(false).HasColumnName("contacto");
            entity.Property(e => e.Email).HasMaxLength(80).IsUnicode(false).HasColumnName("email");
        });

        modelBuilder.Entity<Cl2>(entity =>
        {
            entity.ToTable("cl2");
            entity.HasKey(e => e.Cl2stamp).HasName("pk_cl2");
            entity.Property(e => e.Cl2stamp).HasMaxLength(25).IsUnicode(false).HasColumnName("cl2stamp").IsFixedLength();
            entity.Property(e => e.Codpais).HasMaxLength(3).IsUnicode(false).HasColumnName("codpais");
            entity.Property(e => e.Descpais).HasMaxLength(60).IsUnicode(false).HasColumnName("descpais");
        });

        modelBuilder.Entity<St>(entity =>
        {
            entity.ToTable("st");
            entity.HasKey(e => e.Ststamp).HasName("pk_st");
            entity.Property(e => e.Ststamp).HasMaxLength(25).IsUnicode(false).HasColumnName("ststamp").IsFixedLength();
            entity.Property(e => e.Ref).HasMaxLength(18).IsUnicode(false).HasColumnName("ref");
            entity.Property(e => e.Design).HasMaxLength(60).IsUnicode(false).HasColumnName("design");
            entity.Property(e => e.Epv1).HasColumnName("epv1");
            entity.Property(e => e.Epv2).HasColumnName("epv2");
            entity.Property(e => e.Epv3).HasColumnName("epv3");
            entity.Property(e => e.Epv4).HasColumnName("epv4");
            entity.Property(e => e.Epv5).HasColumnName("epv5");
            entity.Property(e => e.Inactivo).HasColumnName("inactivo");
            entity.Property(e => e.Iva1Incl).HasColumnName("iva1incl");
            entity.Property(e => e.Iva2Incl).HasColumnName("iva2incl");
            entity.Property(e => e.Iva3Incl).HasColumnName("iva3incl");
            entity.Property(e => e.Iva4Incl).HasColumnName("iva4incl");
            entity.Property(e => e.Iva5Incl).HasColumnName("iva5incl");
            entity.Property(e => e.Ivaincl).HasColumnName("ivaincl");
            entity.Property(e => e.IvapcIncl).HasColumnName("ivapcincl");
            entity.Property(e => e.Pv1).HasColumnName("pv1");
            entity.Property(e => e.Pv2).HasColumnName("pv2");
            entity.Property(e => e.Pv3).HasColumnName("pv3");
            entity.Property(e => e.Pv4).HasColumnName("pv4");
            entity.Property(e => e.Pv5).HasColumnName("pv5");
            entity.Property(e => e.Qttacin).HasColumnName("qttacin");
            entity.Property(e => e.Qttacout).HasColumnName("qttacout");
            entity.Property(e => e.Qttcat).HasColumnName("qttcat");
            entity.Property(e => e.Qttcli).HasColumnName("qttcli");
            entity.Property(e => e.Qttesp).HasColumnName("qttesp");
            entity.Property(e => e.Qttfor).HasColumnName("qttfor");
            entity.Property(e => e.Qttrec).HasColumnName("qttrec");
            entity.Property(e => e.Qtttouch).HasColumnName("qtttouch");
            entity.Property(e => e.Qttvend).HasColumnName("qttvend");
            entity.Property(e => e.Stns).HasColumnName("stns");
            entity.Property(e => e.Stmax).HasColumnName("stmax");
            entity.Property(e => e.Stmin).HasColumnName("stmin");
            entity.Property(e => e.Stock).HasColumnName("stock");
            entity.Property(e => e.Stocktch).HasColumnName("stocktch");
            entity.Property(e => e.Tabiva).HasColumnType("numeric(1, 0)").HasColumnName("tabiva");
            entity.Property(e => e.Familia).HasMaxLength(18).IsUnicode(false).HasColumnName("familia");
            entity.Property(e => e.Faminome).HasMaxLength(25).IsUnicode(false).HasColumnName("faminome");
            entity.Property(e => e.Pcusto).HasColumnType("numeric(18, 6)").HasColumnName("pcusto");
            entity.Property(e => e.Epcusto).HasColumnType("numeric(19, 6)").HasColumnName("epcusto");
            entity.Property(e => e.Pcpond).HasColumnType("numeric(18, 6)").HasColumnName("pcpond");
            entity.Property(e => e.Epcpond).HasColumnType("numeric(19, 6)").HasColumnName("epcpond");
            entity.Property(e => e.Pcult).HasColumnType("numeric(18, 6)").HasColumnName("pcult");
            entity.Property(e => e.Epcult).HasColumnType("numeric(19, 6)").HasColumnName("epcult");
            entity.Property(e => e.Cpoc).HasColumnType("numeric(6, 0)").HasColumnName("cpoc");
        });

        modelBuilder.Entity<Stfami>(entity =>
        {
            entity.ToTable("stfami");
            entity.HasKey(e => e.Stfamistamp).HasName("pk_stfami");
            entity.Property(e => e.Stfamistamp).HasMaxLength(25).IsUnicode(false).HasColumnName("stfamistamp").IsFixedLength();
            entity.Property(e => e.Ref).HasMaxLength(18).IsUnicode(false).HasColumnName("ref");
            entity.Property(e => e.Nome).HasMaxLength(25).IsUnicode(false).HasColumnName("nome");
        });

        modelBuilder.Entity<Taxasiva>(entity =>
        {
            entity.ToTable("taxasiva");
            entity.HasKey(e => e.Codigo).HasName("pk_taxasiva");
            entity.Property(e => e.Codigo).HasColumnType("numeric(2, 0)").HasColumnName("codigo");
            entity.Property(e => e.Taxa).HasColumnType("numeric(7, 3)").HasColumnName("taxa");
        });

        modelBuilder.Entity<Ts>(entity =>
        {
            entity.ToTable("ts");
            entity.HasKey(e => e.Ndos).HasName("pk_ts");
            entity.Property(e => e.Ndos).HasColumnType("numeric(3, 0)").HasColumnName("ndos");
            entity.Property(e => e.Nmdos).HasMaxLength(24).IsUnicode(false).HasColumnName("nmdos");
            entity.Property(e => e.Bdempresas).HasMaxLength(2).IsUnicode(false).HasColumnName("bdempresas");
            entity.Property(e => e.Qpreco).HasColumnType("numeric(1, 0)").HasColumnName("qpreco");
            entity.Property(e => e.Qprecocusto).HasColumnType("numeric(1, 0)").HasColumnName("qprecocusto");
        });

        modelBuilder.Entity<Fl>(entity =>
        {
            entity.ToTable("fl");
            entity.HasKey(e => new { e.No, e.Estab }).HasName("pk_fl");
            entity.Property(e => e.No).HasColumnType("numeric(10, 0)").HasColumnName("no");
            entity.Property(e => e.Estab).HasColumnType("numeric(3, 0)").HasColumnName("estab");
            entity.Property(e => e.Nome).HasMaxLength(55).IsUnicode(false).HasColumnName("nome");
            entity.Property(e => e.Ncont).HasMaxLength(20).IsUnicode(false).HasColumnName("ncont");
            entity.Property(e => e.Morada).HasMaxLength(55).IsUnicode(false).HasColumnName("morada");
            entity.Property(e => e.Local).HasMaxLength(43).IsUnicode(false).HasColumnName("local");
            entity.Property(e => e.Codpost).HasMaxLength(45).IsUnicode(false).HasColumnName("codpost");
            entity.Property(e => e.Preco).HasColumnType("numeric(1, 0)").HasColumnName("preco");
            entity.Property(e => e.Segmento).HasMaxLength(25).IsUnicode(false).HasColumnName("segmento");
            entity.Property(e => e.Telefone).HasMaxLength(60).IsUnicode(false).HasColumnName("telefone");
            entity.Property(e => e.Contacto).HasMaxLength(30).IsUnicode(false).HasColumnName("contacto");
            entity.Property(e => e.Email).HasMaxLength(100).IsUnicode(false).HasColumnName("email");
            entity.Property(e => e.Flstamp).HasMaxLength(25).IsUnicode(false).HasColumnName("flstamp").IsFixedLength();
        });

        modelBuilder.Entity<Ag>(entity =>
        {
            entity.ToTable("ag");
            entity.HasKey(e => e.No).HasName("pk_ag");
            entity.Property(e => e.No).HasColumnType("numeric(10, 0)").HasColumnName("no");
            entity.Property(e => e.Nome).HasMaxLength(55).IsUnicode(false).HasColumnName("nome");
            entity.Property(e => e.Ncont).HasMaxLength(20).IsUnicode(false).HasColumnName("ncont");
            entity.Property(e => e.Morada).HasMaxLength(55).IsUnicode(false).HasColumnName("morada");
            entity.Property(e => e.Local).HasMaxLength(43).IsUnicode(false).HasColumnName("local");
            entity.Property(e => e.Codpost).HasMaxLength(45).IsUnicode(false).HasColumnName("codpost");
            entity.Property(e => e.Telefone).HasMaxLength(60).IsUnicode(false).HasColumnName("telefone");
            entity.Property(e => e.Contacto).HasMaxLength(40).IsUnicode(false).HasColumnName("contacto");
            entity.Property(e => e.Email).HasMaxLength(100).IsUnicode(false).HasColumnName("email");
            entity.Property(e => e.Agstamp).HasMaxLength(25).IsUnicode(false).HasColumnName("agstamp").IsFixedLength();
        });

        modelBuilder.Entity<Em>(entity =>
        {
            entity.ToTable("em");
            entity.HasKey(e => e.No).HasName("pk_em");
            entity.Property(e => e.No).HasColumnType("numeric(10, 0)").HasColumnName("no");
            entity.Property(e => e.Nome).HasMaxLength(55).IsUnicode(false).HasColumnName("nome");
            entity.Property(e => e.Ncont).HasMaxLength(20).IsUnicode(false).HasColumnName("ncont");
            entity.Property(e => e.Morada).HasMaxLength(55).IsUnicode(false).HasColumnName("morada");
            entity.Property(e => e.Local).HasMaxLength(43).IsUnicode(false).HasColumnName("local");
            entity.Property(e => e.Codpost).HasMaxLength(45).IsUnicode(false).HasColumnName("codpost");
            entity.Property(e => e.Cpostl).HasMaxLength(25).IsUnicode(false).HasColumnName("cpostl");
            entity.Property(e => e.Preco).HasColumnType("numeric(1, 0)").HasColumnName("preco");
            entity.Property(e => e.Segmento).HasMaxLength(25).IsUnicode(false).HasColumnName("segmento");
            entity.Property(e => e.Telefone).HasMaxLength(60).IsUnicode(false).HasColumnName("telefone");
            entity.Property(e => e.Contacto).HasMaxLength(45).IsUnicode(false).HasColumnName("ctacto");
            entity.Property(e => e.Email).HasMaxLength(100).IsUnicode(false).HasColumnName("email");
            entity.Property(e => e.Emstamp).HasMaxLength(25).IsUnicode(false).HasColumnName("emstamp").IsFixedLength();
        });
    }
}
