using Invoices.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Kernel.MultiTenancy;

namespace Invoices.Infrastructure.Persistence
{
    public class FaturasDbContext : DbContext
    {
        private readonly ITenantContext _tenantContext;

        public FaturasDbContext(
            DbContextOptions<FaturasDbContext> options,
            ITenantContext tenantContext)
            : base(options)
        {
            _tenantContext = tenantContext;
        }

        public DbSet<Ft> Faturas { get; set; } = null!;
        public DbSet<Fi> FaturaLinhas { get; set; } = null!;
        public DbSet<FT2> FaturaDadosSecundarios { get; set; } = null!;
        public DbSet<FT3> FaturaDadosAdicionais { get; set; } = null!;
        public DbSet<Fi2> FaturaLinhasDadosAdicionais { get; set; } = null!;
        public DbSet<TD> TD { get; set; } = null!;
        public DbSet<Cl> Cl { get; set; } = null!;
        public DbSet<St> St { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (_tenantContext?.HasDatabaseCredentials == true)
            {
                optionsBuilder.UseSqlServer(_tenantContext.GetConnectionString());
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Ft>(entity =>
            {
                entity.ToTable("ft");
                entity.HasKey(x => new { x.Ndoc, x.Fno, x.FtAno });
                entity.HasIndex(x => x.FtStamp).IsUnique();

                entity.Property(x => x.FtStamp).HasColumnName("ftstamp").HasMaxLength(25).IsFixedLength();
                entity.Property(x => x.Pais).HasColumnName("pais");
                entity.Property(x => x.NmDoc).HasColumnName("nmdoc").HasMaxLength(20);
                entity.Property(x => x.Fno).HasColumnName("fno");
                entity.Property(x => x.No).HasColumnName("no");
                entity.Property(x => x.Nome).HasColumnName("nome").HasMaxLength(55);
                entity.Property(x => x.FData).HasColumnName("fdata");
                entity.Property(x => x.FtAno).HasColumnName("ftano");
                entity.Property(x => x.Ndoc).HasColumnName("ndoc");
                entity.Property(x => x.Moeda).HasColumnName("moeda").HasMaxLength(11);
                entity.Property(x => x.Estab).HasColumnName("estab");
                entity.Property(x => x.Total).HasColumnName("total").HasPrecision(18, 5);
                entity.Property(x => x.TotalMoeda).HasColumnName("totalmoeda").HasPrecision(15, 3);
                entity.Property(x => x.TtIva).HasColumnName("ttiva").HasPrecision(18, 5);
                entity.Property(x => x.TMIva).HasColumnName("tmiva").HasPrecision(15, 3);
                entity.Property(x => x.OUsrInis).HasColumnName("ousrinis").HasMaxLength(30);
                entity.Property(x => x.OUsrData).HasColumnName("ousrdata");
                entity.Property(x => x.OUsrHora).HasColumnName("ousrhora").HasMaxLength(8);
                entity.Property(x => x.UsrInis).HasColumnName("usrinis").HasMaxLength(30);
                entity.Property(x => x.UsrData).HasColumnName("usrdata");
                entity.Property(x => x.UsrHora).HasColumnName("usrhora").HasMaxLength(8);
                entity.Property(x => x.Marcada).HasColumnName("marcada");
            });

            modelBuilder.Entity<Fi>(entity =>
            {
                entity.ToTable("fi");
                entity.HasKey(x => x.Fistamp);

                entity.Property(x => x.Fistamp).HasColumnName("fistamp").HasMaxLength(25).IsFixedLength();
                entity.Property(x => x.Nmdoc).HasColumnName("nmdoc").HasMaxLength(20);
                entity.Property(x => x.Fno).HasColumnName("fno").HasPrecision(10, 2);
                entity.Property(x => x.Ref).HasColumnName("ref").HasMaxLength(18);
                entity.Property(x => x.Design).HasColumnName("design").HasMaxLength(60);
                entity.Property(x => x.Qtt).HasColumnName("qtt").HasPrecision(12, 2);
                entity.Property(x => x.Tiliquido).HasColumnName("tiliquido").HasPrecision(18, 2);
                entity.Property(x => x.Etiliquido).HasColumnName("etiliquido").HasPrecision(19, 2);
                entity.Property(x => x.Iva).HasColumnName("iva").HasPrecision(5, 2);
                entity.Property(x => x.Ivaincl).HasColumnName("ivaincl");
                entity.Property(x => x.Tabiva).HasColumnName("tabiva").HasPrecision(1, 0);
                entity.Property(x => x.Ndoc).HasColumnName("ndoc").HasPrecision(3, 0);
                entity.Property(x => x.Armazem).HasColumnName("armazem").HasPrecision(5, 0);
                entity.Property(x => x.Lote).HasColumnName("lote").HasMaxLength(30);
                entity.Property(x => x.Usr1).HasColumnName("usr1").HasMaxLength(100);
                entity.Property(x => x.Usr2).HasColumnName("usr2").HasMaxLength(100);
                entity.Property(x => x.Usr3).HasColumnName("usr3").HasMaxLength(35);
                entity.Property(x => x.Usr4).HasColumnName("usr4").HasMaxLength(20);
                entity.Property(x => x.Usr5).HasColumnName("usr5").HasMaxLength(120);
                entity.Property(x => x.Usr6).HasColumnName("usr6").HasMaxLength(30);
                entity.Property(x => x.Ftstamp).HasColumnName("ftstamp").HasMaxLength(25).IsFixedLength();
                entity.Property(x => x.Pv).HasColumnName("pv").HasPrecision(18, 0);
                entity.Property(x => x.Pvmoeda).HasColumnName("pvmoeda").HasPrecision(19, 0);
                entity.Property(x => x.Epv).HasColumnName("epv").HasPrecision(19, 0);
                entity.Property(x => x.Tmoeda).HasColumnName("tmoeda").HasPrecision(13, 0);
                entity.Property(x => x.Ousrinis).HasColumnName("ousrinis").HasMaxLength(30);
                entity.Property(x => x.Ousrdata).HasColumnName("ousrdata");
                entity.Property(x => x.Ousrhora).HasColumnName("ousrhora").HasMaxLength(8);
                entity.Property(x => x.Usrinis).HasColumnName("usrinis").HasMaxLength(30);
                entity.Property(x => x.Usrdata).HasColumnName("usrdata");
                entity.Property(x => x.Usrhora).HasColumnName("usrhora").HasMaxLength(8);
                entity.Property(x => x.Marcada).HasColumnName("marcada");
            });

            modelBuilder.Entity<FT2>(entity =>
            {
                entity.ToTable("ft2");
                entity.HasKey(x => x.Ft2Stamp);
                
                entity.Property(x => x.Ft2Stamp).HasColumnName("ft2stamp").HasMaxLength(25).IsFixedLength();
                entity.Property(x => x.FormaPag).HasColumnName("formapag").HasMaxLength(3);
                entity.Property(x => x.LocalEntrega).HasColumnName("localentrega").HasMaxLength(60);
                entity.Property(x => x.MoradaEntrega).HasColumnName("moradaentrega").HasMaxLength(60);
                entity.Property(x => x.LocalLocEnt).HasColumnName("locallocent").HasMaxLength(60);
                entity.Property(x => x.CodPEntrega).HasColumnName("codpentrega").HasMaxLength(60);
                entity.Property(x => x.ObsDoc).HasColumnName("obsdoc");
                entity.Property(x => x.DataEntrega).HasColumnName("dataentrega");
                entity.Property(x => x.HoraEntrega).HasColumnName("horaentrega").HasMaxLength(5);
                entity.Property(x => x.Ousrinis).HasColumnName("ousrinis").HasMaxLength(30);
                entity.Property(x => x.Ousrdata).HasColumnName("ousrdata");
                entity.Property(x => x.Ousrhora).HasColumnName("ousrhora").HasMaxLength(8);
                entity.Property(x => x.Usrinis).HasColumnName("usrinis").HasMaxLength(30);
                entity.Property(x => x.Usrdata).HasColumnName("usrdata");
                entity.Property(x => x.Usrhora).HasColumnName("usrhora").HasMaxLength(8);
                entity.Property(x => x.Marcada).HasColumnName("marcada");
            });

            modelBuilder.Entity<FT3>(entity =>
            {
                entity.ToTable("ft3");
                entity.HasKey(x => x.Ft3Stamp);

                entity.Property(x => x.Ft3Stamp).HasColumnName("ft3stamp").HasMaxLength(25).IsFixedLength();
                entity.Property(x => x.OUsrInis).HasColumnName("ousrinis").HasMaxLength(30);
                entity.Property(x => x.OUsrData).HasColumnName("ousrdata");
                entity.Property(x => x.OUsrHora).HasColumnName("ousrhora").HasMaxLength(8);
                entity.Property(x => x.UsrInis).HasColumnName("usrinis").HasMaxLength(30);
                entity.Property(x => x.UsrData).HasColumnName("usrdata");
                entity.Property(x => x.UsrHora).HasColumnName("usrhora").HasMaxLength(8);
                entity.Property(x => x.Marcada).HasColumnName("marcada");
            });

            modelBuilder.Entity<Fi2>(entity =>
            {
                entity.ToTable("fi2");
                entity.HasKey(x => new { x.Fi2Stamp });

                entity.Property(x => x.Fi2Stamp).HasColumnName("fi2stamp").HasMaxLength(25).IsFixedLength();
                entity.Property(x => x.Ftstamp).HasColumnName("ftstamp").HasMaxLength(25).IsFixedLength();
                entity.Property(x => x.Ousrinis).HasColumnName("ousrinis").HasMaxLength(30);
                entity.Property(x => x.Ousrdata).HasColumnName("ousrdata");
                entity.Property(x => x.Ousrhora).HasColumnName("ousrhora").HasMaxLength(8);
                entity.Property(x => x.Usrinis).HasColumnName("usrinis").HasMaxLength(30);
                entity.Property(x => x.Usrdata).HasColumnName("usrdata");
                entity.Property(x => x.Usrhora).HasColumnName("usrhora").HasMaxLength(8);
                entity.Property(x => x.Marcada).HasColumnName("marcada");
            });

            modelBuilder.Entity<TD>(entity =>
            {
                entity.ToTable("td");
                entity.HasKey(x => x.Ndoc);

                entity.Property(x => x.Ndoc).HasColumnName("ndoc");
                entity.Property(x => x.NmDoc).HasColumnName("nmdoc").HasMaxLength(40);
                entity.Property(x => x.NmDocP).HasColumnName("nmdocp").HasMaxLength(40);
                entity.Property(x => x.NmDocA).HasColumnName("nmdoca").HasMaxLength(5);
                entity.Property(x => x.Serie).HasColumnName("serie");
                entity.Property(x => x.GuiaRemessa).HasColumnName("guiaremessa");
                entity.Property(x => x.AutoFat).HasColumnName("autofat");
                entity.Property(x => x.LancaCc).HasColumnName("lancacc");
                entity.Property(x => x.LancaSl).HasColumnName("lancasl");
                entity.Property(x => x.LancaOl).HasColumnName("lancaol");
                entity.Property(x => x.AutoMl).HasColumnName("automl");
                entity.Property(x => x.Fechada).HasColumnName("fechada");
                entity.Property(x => x.ExcluiSaft).HasColumnName("excluisaft");
                entity.Property(x => x.LimiteSimp).HasColumnName("limitesimp");
                entity.Property(x => x.PreDec).HasColumnName("predec");
                entity.Property(x => x.QttDec).HasColumnName("qttdec");
                entity.Property(x => x.OusrInis).HasColumnName("ousrinis").HasMaxLength(30);
                entity.Property(x => x.OusrData).HasColumnName("ousrdata");
                entity.Property(x => x.OusrHora).HasColumnName("ousrhora").HasMaxLength(8);
                entity.Property(x => x.AspNetUsersId).HasColumnName("aspnetusersid").HasMaxLength(450);
            });

            modelBuilder.Entity<Cl>(entity =>
            {
                entity.ToTable("cl");
                entity.HasKey(x => new { x.No, x.Estab });

                entity.Property(x => x.No).HasColumnName("no");
                entity.Property(x => x.Estab).HasColumnName("estab");
                entity.Property(x => x.Nome).HasColumnName("nome").HasMaxLength(55);
                entity.Property(x => x.Moeda).HasColumnName("moeda").HasMaxLength(3);
                entity.Property(x => x.ClStamp).HasColumnName("clstamp").HasMaxLength(25).IsFixedLength();
            });

            modelBuilder.Entity<St>(entity =>
            {
                entity.ToTable("st");
                entity.HasKey(x => x.Ref);

                entity.Property(x => x.Ref).HasColumnName("ref").HasMaxLength(25);
                entity.Property(x => x.Design).HasColumnName("design").HasMaxLength(60);
                entity.Property(x => x.StStamp).HasColumnName("ststamp").HasMaxLength(25).IsFixedLength();
            });
        }
    }
}
