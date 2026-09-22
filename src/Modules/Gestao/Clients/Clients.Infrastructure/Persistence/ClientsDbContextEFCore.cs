using Microsoft.EntityFrameworkCore;
using Clients.Domain.Entities;
using Shared.Kernel.MultiTenancy;

namespace Clients.Infrastructure.Persistence;

/// <summary>
/// DbContext EF Core para o módulo Clients
/// Database First approach - uses TenantContext for dynamic connection
/// </summary>
public class ClientsDbContextEFCore : DbContext
{
    private readonly ITenantContext _tenantContext;

    public ClientsDbContextEFCore(
        DbContextOptions<ClientsDbContextEFCore> options,
        ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    public virtual DbSet<Cl> Cl { get; set; } = null!;
    public virtual DbSet<Cl2> Cl2 { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // MULTI-TENANCY: Get connection string from TenantContext
        // If TenantContext has credentials, use the tenant's database
        // Otherwise, the connection string was already set during AddDbContext (fallback to primary DB)
        if (_tenantContext?.HasDatabaseCredentials == true)
        {
            optionsBuilder.UseSqlServer(_tenantContext.GetConnectionString());
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureCl(modelBuilder);
        ConfigureCl2(modelBuilder);
    }

    private static void ConfigureCl(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cl>(entity =>
        {
            // Chave primária composta
            entity.HasKey(e => new { e.No, e.Estab })
                .HasName("pk_cl")
                .IsClustered(false);

            // Nome da tabela
            entity.ToTable("cl");

            // Índices essenciais
            entity.HasIndex(e => new { e.Nome, e.No, e.Estab, e.Clstamp }, "in_cl_cllist")
                .HasFillFactor(70);

            entity.HasIndex(e => e.Ncont, "in_cl_ncont")
                .HasFillFactor(70);

            entity.HasIndex(e => e.No, "in_cl_no")
                .HasFillFactor(70);

            entity.HasIndex(e => e.Nome, "in_cl_nome")
                .HasFillFactor(70);

            entity.HasIndex(e => e.Clstamp, "in_cl_stamp")
                .IsUnique()
                .HasFillFactor(70);

            // Propriedades com configuração de coluna
            entity.Property(e => e.No)
                .HasColumnType("numeric(10, 0)")
                .HasColumnName("no");

            entity.Property(e => e.Estab)
                .HasColumnType("numeric(3, 0)")
                .HasColumnName("estab");

            entity.Property(e => e.Nome)
                .HasMaxLength(55)
                .IsUnicode(false)
                .HasColumnName("nome");

            entity.Property(e => e.Ncont)
                .HasMaxLength(9)
                .IsUnicode(false)
                .HasColumnName("ncont");

            entity.Property(e => e.Telefone)
                .HasMaxLength(60)
                .IsUnicode(false)
                .HasColumnName("telefone")
                .HasDefaultValueSql("('')");

            entity.Property(e => e.Morada)
                .HasMaxLength(55)
                .IsUnicode(false)
                .HasColumnName("morada")
                .HasDefaultValueSql("('')");

            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("email")
                .HasDefaultValueSql("('')");

            entity.Property(e => e.Clstamp)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("clstamp")
                .HasDefaultValueSql("('')")
                .IsFixedLength();

            entity.Property(e => e.Inactivo)
                .HasColumnName("inactivo");

            // Auditoria
            entity.Property(e => e.Usrinis)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("usrinis")
                .HasDefaultValueSql("('')");

            entity.Property(e => e.Usrdata)
                .HasColumnType("datetime")
                .HasColumnName("usrdata")
                .HasDefaultValueSql("(CONVERT([datetime],'19000101'))");

            entity.Property(e => e.Usrhora)
                .HasMaxLength(8)
                .IsUnicode(false)
                .HasColumnName("usrhora")
                .HasDefaultValueSql("('00:00:00')");

            entity.Property(e => e.Ousrinis)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("ousrinis")
                .HasDefaultValueSql("('')");

            entity.Property(e => e.Ousrdata)
                .HasColumnType("datetime")
                .HasColumnName("ousrdata")
                .HasDefaultValueSql("(CONVERT([datetime],'19000101'))");

            entity.Property(e => e.Ousrhora)
                .HasMaxLength(8)
                .IsUnicode(false)
                .HasColumnName("ousrhora")
                .HasDefaultValueSql("('00:00:00')");

            entity.Property(e => e.Marcada)
                .HasColumnName("marcada");
        });
    }

    private static void ConfigureCl2(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cl2>(entity =>
        {
            // Chave primária
            entity.HasKey(e => e.Cl2stamp)
                .HasName("pk_cl2");

            // Nome da tabela
            entity.ToTable("cl2");

            // Propriedades
            entity.Property(e => e.Cl2stamp)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("cl2stamp")
                .HasDefaultValueSql("('')")
                .IsFixedLength();

            // Auditoria
            entity.Property(e => e.Usrinis)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("usrinis")
                .HasDefaultValueSql("('')");

            entity.Property(e => e.Usrdata)
                .HasColumnType("datetime")
                .HasColumnName("usrdata")
                .HasDefaultValueSql("(CONVERT([datetime],'19000101'))");

            entity.Property(e => e.Usrhora)
                .HasMaxLength(8)
                .IsUnicode(false)
                .HasColumnName("usrhora")
                .HasDefaultValueSql("('00:00:00')");

            entity.Property(e => e.Ousrinis)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("ousrinis")
                .HasDefaultValueSql("('')");

            entity.Property(e => e.Ousrdata)
                .HasColumnType("datetime")
                .HasColumnName("ousrdata")
                .HasDefaultValueSql("(CONVERT([datetime],'19000101'))");

            entity.Property(e => e.Ousrhora)
                .HasMaxLength(8)
                .IsUnicode(false)
                .HasColumnName("ousrhora")
                .HasDefaultValueSql("('00:00:00')");

            entity.Property(e => e.Marcada)
                .HasColumnName("marcada");
        });
    }
}
