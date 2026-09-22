using Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Persistence;

/// <summary>
/// Primary DbContext for the main database
/// This context connects to the main database (DBconnect in appSettings)
/// and manages AppLicense entities which store tenant database credentials
/// 
/// This is separate from AuthDbContext which manages ASP.NET Identity
/// </summary>
public class PrimaryDbContext : DbContext
{
    public PrimaryDbContext(DbContextOptions<PrimaryDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// AppLicense table - stores license information and database credentials for each tenant
    /// Maps to dbo.u_applicense
    /// </summary>
    public DbSet<AppLicense> AppLicenses { get; set; } = null!;

    /// <summary>
    /// Linhas de licenciamento por AppLicense (ex.: "API PHC" concede acesso geral à API)
    /// Maps to dbo.u_applicl
    /// </summary>
    public DbSet<AppLicenseLine> AppLicenseLines { get; set; } = null!;

    /// <summary>
    /// Tabela de módulos permitidos por linha de licença
    /// Maps to dbo.u_apilic
    /// </summary>
    public DbSet<UApiLic> u_apilic { get; set; } = null!;

    /// <summary>
    /// Additional field definitions per tenant/module/table
    /// Maps to dbo.u_addfields
    /// </summary>
    public DbSet<UserField> UserFields { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure AppLicense entity mapping
        ConfigureAppLicense(builder);
        ConfigureAppLicenseLine(builder);
        ConfigureUApilic(builder);
        ConfigureUserField(builder);
    }

    private static void ConfigureAppLicense(ModelBuilder builder)
    {
        var appLicenseBuilder = builder.Entity<AppLicense>();

        // Table name
        appLicenseBuilder.ToTable("u_applicense", "dbo");

        // Primary key
        appLicenseBuilder.HasKey(al => al.Stamp)
            .HasName("pk_u_applicense");

        // Property mappings
        appLicenseBuilder.Property(al => al.Stamp)
            .HasColumnName("u_applicensestamp")
            .HasMaxLength(25)
            .IsRequired();

        appLicenseBuilder.Property(al => al.Inactive)
            .HasColumnName("inativo")
            .HasDefaultValue(false);

        appLicenseBuilder.Property(al => al.Name)
            .HasColumnName("nome")
            .HasMaxLength(55)
            .IsRequired();

        appLicenseBuilder.Property(al => al.CompanyNo)
            .HasColumnName("no")
            .HasPrecision(10, 0);

        appLicenseBuilder.Property(al => al.EstablishmentNo)
            .HasColumnName("estab")
            .HasPrecision(10, 0);

        appLicenseBuilder.Property(al => al.Email)
            .HasColumnName("email")
            .HasMaxLength(100)
            .IsRequired();

        appLicenseBuilder.Property(al => al.CreationDate)
            .HasColumnName("creationdate");

        // Audit fields - creation
        appLicenseBuilder.Property(al => al.CreatedByUsername)
            .HasColumnName("ousrinis")
            .HasMaxLength(30)
            .IsRequired();

        appLicenseBuilder.Property(al => al.CreatedDate)
            .HasColumnName("ousrdata");

        appLicenseBuilder.Property(al => al.CreatedTime)
            .HasColumnName("ousrhora")
            .HasMaxLength(8)
            .IsRequired();

        // Audit fields - updates
        appLicenseBuilder.Property(al => al.UpdatedByUsername)
            .HasColumnName("usrinis")
            .HasMaxLength(30)
            .IsRequired();

        appLicenseBuilder.Property(al => al.UpdatedDate)
            .HasColumnName("usrdata");

        appLicenseBuilder.Property(al => al.UpdatedTime)
            .HasColumnName("usrhora")
            .HasMaxLength(8)
            .IsRequired();

        appLicenseBuilder.Property(al => al.Marked)
            .HasColumnName("marcada")
            .HasDefaultValue(false);

        // Database credentials mapping
        appLicenseBuilder.Property(al => al.DbServer)
            .HasColumnName("csserver")
            .HasMaxLength(100)
            .IsRequired();

        appLicenseBuilder.Property(al => al.DbDatabase)
            .HasColumnName("csdatabase")
            .HasMaxLength(100)
            .IsRequired();

        appLicenseBuilder.Property(al => al.DbUserId)
            .HasColumnName("csuserid")
            .HasMaxLength(100)
            .IsRequired();

        appLicenseBuilder.Property(al => al.DbPassword)
            .HasColumnName("cspassword")
            .HasMaxLength(100)
            .IsRequired();

        appLicenseBuilder.Property(al => al.IntranetLink)
            .HasColumnName("linkintranet")
            .HasMaxLength(200)
            .IsRequired();

        // PHC WEB service credentials (distintas das credenciais SQL acima)
        appLicenseBuilder.Property(al => al.WebServiceUsername)
            .HasColumnName("username")
            .HasMaxLength(50)
            .IsRequired();

        appLicenseBuilder.Property(al => al.WebServicePassword)
            .HasColumnName("password")
            .HasMaxLength(50)
            .IsRequired();

        appLicenseBuilder.Property(al => al.WebServiceAuthToken)
            .HasColumnName("tokenauth")
            .HasColumnType("text")
            .IsRequired();

        // Indexes for performance
        appLicenseBuilder.HasIndex(al => al.Name)
            .HasDatabaseName("idx_applicense_name");

        appLicenseBuilder.HasIndex(al => al.CompanyNo)
            .HasDatabaseName("idx_applicense_companyno");

        appLicenseBuilder.HasIndex(al => al.Inactive)
            .HasDatabaseName("idx_applicense_inactive");
    }

    private static void ConfigureAppLicenseLine(ModelBuilder builder)
    {
        var lineBuilder = builder.Entity<AppLicenseLine>();

        lineBuilder.ToTable("u_applicl", "dbo");

        lineBuilder.HasKey(x => x.Stamp)
            .HasName("pk_u_applicl");

        lineBuilder.Property(x => x.Stamp)
            .HasColumnName("u_appliclstamp")
            .HasMaxLength(25)
            .IsRequired();

        lineBuilder.Property(x => x.Type)
            .HasColumnName("tipo")
            .HasMaxLength(50)
            .IsRequired();

        lineBuilder.Property(x => x.AppLicenseStamp)
            .HasColumnName("u_applicensestamp")
            .HasMaxLength(25)
            .IsRequired();

        lineBuilder.Property(x => x.SubType)
            .HasColumnName("subtipo")
            .HasMaxLength(50)
            .IsRequired();

        lineBuilder.Property(x => x.Name)
            .HasColumnName("nome")
            .HasMaxLength(100)
            .IsRequired();

        lineBuilder.Property(x => x.StartDate)
            .HasColumnName("dinicio");

        lineBuilder.Property(x => x.EndDate)
            .HasColumnName("dfim");

        lineBuilder.Property(x => x.Quantity)
            .HasColumnName("qtt")
            .HasPrecision(10, 0);

        lineBuilder.Property(x => x.CreatedByUsername)
            .HasColumnName("ousrinis")
            .HasMaxLength(30)
            .IsRequired();

        lineBuilder.Property(x => x.CreatedDate)
            .HasColumnName("ousrdata");

        lineBuilder.Property(x => x.CreatedTime)
            .HasColumnName("ousrhora")
            .HasMaxLength(8)
            .IsRequired();

        lineBuilder.Property(x => x.UpdatedByUsername)
            .HasColumnName("usrinis")
            .HasMaxLength(30)
            .IsRequired();

        lineBuilder.Property(x => x.UpdatedDate)
            .HasColumnName("usrdata");

        lineBuilder.Property(x => x.UpdatedTime)
            .HasColumnName("usrhora")
            .HasMaxLength(8)
            .IsRequired();

        lineBuilder.Property(x => x.Marked)
            .HasColumnName("marcada")
            .HasDefaultValue(false);

        lineBuilder.Property(x => x.DivisionName)
            .HasColumnName("nomediv")
            .HasMaxLength(50)
            .IsRequired();

        lineBuilder.Property(x => x.EstablishmentNo)
            .HasColumnName("estab")
            .HasPrecision(10, 0);

        lineBuilder.Property(x => x.CompanyNo)
            .HasColumnName("no")
            .HasPrecision(10, 0);

        lineBuilder.Property(x => x.Inactive)
            .HasColumnName("inactivo")
            .HasDefaultValue(false);

        // Consulta principal: encontrar a linha "API PHC" de uma licença
        lineBuilder.HasIndex(x => new { x.AppLicenseStamp, x.Type })
            .HasDatabaseName("idx_u_applicl_license_type");
    }

    private static void ConfigureUApilic(ModelBuilder builder)
    {
        var appLicenseModuleBuilder = builder.Entity<UApiLic>();

        appLicenseModuleBuilder.ToTable("u_apilic", "dbo");

        appLicenseModuleBuilder.HasKey(x => x.UApilicStamp)
            .HasName("pk_u_apilic");

        appLicenseModuleBuilder.Property(x => x.UApilicStamp)
            .HasColumnName("u_apilicstamp")
            .HasMaxLength(25)
            .IsRequired();

        appLicenseModuleBuilder.Property(x => x.AppLicenseLineStamp)
            .HasColumnName("u_appliclstamp")
            .HasMaxLength(25)
            .IsRequired();

        appLicenseModuleBuilder.Property(x => x.Nomepack)
            .HasColumnName("nomepack")
            .HasMaxLength(15);

        appLicenseModuleBuilder.Property(x => x.Ousrinis)
            .HasColumnName("ousrinis")
            .HasMaxLength(30);

        appLicenseModuleBuilder.Property(x => x.Ousrdata)
            .HasColumnName("ousrdata");

        appLicenseModuleBuilder.Property(x => x.Ousrhora)
            .HasColumnName("ousrhora")
            .HasMaxLength(8);

        appLicenseModuleBuilder.Property(x => x.Usrinis)
            .HasColumnName("usrinis")
            .HasMaxLength(30);

        appLicenseModuleBuilder.Property(x => x.Usrdata)
            .HasColumnName("usrdata");

        appLicenseModuleBuilder.Property(x => x.Usrhora)
            .HasColumnName("usrhora")
            .HasMaxLength(8);

        appLicenseModuleBuilder.Property(x => x.Marked)
            .HasColumnName("marcada")
            .HasDefaultValue(false);

        appLicenseModuleBuilder.Property(x => x.FinalDate)
            .HasColumnName("dfinal");

        appLicenseModuleBuilder.Property(x => x.InitialDate)
            .HasColumnName("dinicial");

        appLicenseModuleBuilder.HasIndex(x => new { x.AppLicenseLineStamp, x.Nomepack })
            .HasDatabaseName("idx_u_apilic_line_module");
    }

    private static void ConfigureUserField(ModelBuilder builder)
    {
        var b = builder.Entity<UserField>();

        b.ToTable("u_addfields", "dbo");

        b.HasKey(f => f.Stamp)
            .HasName("pk_u_addfields");

        b.Property(f => f.Stamp)
            .HasColumnName("u_addfieldsstamp")
            .HasMaxLength(25)
            .IsRequired();

        b.Property(f => f.AppLicenseStamp)
            .HasColumnName("u_applicensestamp")
            .HasMaxLength(25)
            .IsRequired();

        b.Property(f => f.ModuleName)
            .HasColumnName("modulename")
            .HasMaxLength(100)
            .IsRequired();

        b.Property(f => f.TableName)
            .HasColumnName("tablename")
            .HasMaxLength(50)
            .IsRequired();

        b.Property(f => f.FieldAlias)
            .HasColumnName("fieldalias")
            .HasMaxLength(100)
            .IsRequired();

        b.Property(f => f.FieldColumn)
            .HasColumnName("fieldcolumn")
            .HasMaxLength(100)
            .IsRequired();

        b.Property(f => f.FieldType)
            .HasColumnName("fieldtype")
            .HasMaxLength(50)
            .IsRequired();

        b.Property(f => f.IsReadOnly)
            .HasColumnName("readonly")
            .HasDefaultValue(false);

        b.Property(f => f.Inactive)
            .HasColumnName("inactive")
            .HasDefaultValue(false);

        b.HasIndex(f => new { f.AppLicenseStamp, f.ModuleName, f.TableName })
            .HasDatabaseName("idx_u_addfields_license_module_table");
    }
}
