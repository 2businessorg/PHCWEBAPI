using Currencies.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Currencies.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuração EF Core da tabela cb.
/// </summary>
public class CbConfiguration : IEntityTypeConfiguration<Cb>
{
    public void Configure(EntityTypeBuilder<Cb> builder)
    {
        builder.ToTable("cb", tableBuilder =>
        {
            tableBuilder.HasTrigger("tr_cb_update");
        });

        builder.HasKey(x => x.CbStamp)
            .HasName("pk_cb");

        builder.Property(x => x.CbStamp)
            .HasColumnName("cbstamp")
            .HasColumnType("char(25)")
            .IsRequired();

        builder.Property(x => x.Pais)
            .HasColumnName("pais")
            .HasColumnType("varchar(12)")
            .IsRequired();

        builder.Property(x => x.Moeda)
            .HasColumnName("moeda")
            .HasColumnType("varchar(3)")
            .IsRequired();

        builder.Property(x => x.Data)
            .HasColumnName("data")
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(x => x.UCambioC)
            .HasColumnName("u_cambioc")
            .HasColumnType("numeric(19,12)")
            .IsRequired();

        builder.Property(x => x.UCambioV)
            .HasColumnName("u_cambiov")
            .HasColumnType("numeric(19,12)")
            .IsRequired();

        builder.Property(x => x.Cambio)
            .HasColumnName("cambio")
            .HasColumnType("numeric(19,12)")
            .IsRequired();

        builder.Property(x => x.Obs)
            .HasColumnName("obs")
            .HasColumnType("varchar(10)")
            .IsRequired();

        builder.Property(x => x.Cambio2)
            .HasColumnName("cambio2")
            .HasColumnType("numeric(19,12)")
            .IsRequired();

        builder.Property(x => x.Ecambio)
            .HasColumnName("ecambio")
            .HasColumnType("numeric(20,12)")
            .IsRequired();

        builder.Property(x => x.Ecambio2)
            .HasColumnName("ecambio2")
            .HasColumnType("numeric(20,12)")
            .IsRequired();

        builder.Property(x => x.Zonaeuro)
            .HasColumnName("zonaeuro")
            .HasColumnType("bit")
            .IsRequired();

        builder.Property(x => x.Unisg)
            .HasColumnName("unisg")
            .HasColumnType("varchar(20)")
            .IsRequired();

        builder.Property(x => x.Unipl)
            .HasColumnName("unipl")
            .HasColumnType("varchar(20)")
            .IsRequired();

        builder.Property(x => x.Centsg)
            .HasColumnName("centsg")
            .HasColumnType("varchar(20)")
            .IsRequired();

        builder.Property(x => x.Centpl)
            .HasColumnName("centpl")
            .HasColumnType("varchar(20)")
            .IsRequired();

        builder.Property(x => x.Taxa)
            .HasColumnName("taxa")
            .HasColumnType("numeric(19,12)")
            .IsRequired();

        builder.Property(x => x.Taxa2)
            .HasColumnName("taxa2")
            .HasColumnType("numeric(19,12)")
            .IsRequired();

        builder.Property(x => x.Cambioinvertido)
            .HasColumnName("cambioinvertido")
            .HasColumnType("numeric(19,12)")
            .IsRequired();

        builder.Property(x => x.Cambioinvertido2)
            .HasColumnName("cambioinvertido2")
            .HasColumnType("numeric(19,12)")
            .IsRequired();

        builder.Property(x => x.Ecambioinvertido)
            .HasColumnName("ecambioinvertido")
            .HasColumnType("numeric(20,10)")
            .IsRequired();

        builder.Property(x => x.Ecambioinvertido2)
            .HasColumnName("ecambioinvertido2")
            .HasColumnType("numeric(20,10)")
            .IsRequired();

        builder.Property(x => x.OusrInis)
            .HasColumnName("ousrinis")
            .HasColumnType("varchar(30)")
            .IsRequired();

        builder.Property(x => x.OusrData)
            .HasColumnName("ousrdata")
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(x => x.OusrHora)
            .HasColumnName("ousrhora")
            .HasColumnType("varchar(8)")
            .IsRequired();

        builder.Property(x => x.UsrInis)
            .HasColumnName("usrinis")
            .HasColumnType("varchar(30)")
            .IsRequired();

        builder.Property(x => x.UsrData)
            .HasColumnName("usrdata")
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(x => x.UsrHora)
            .HasColumnName("usrhora")
            .HasColumnType("varchar(8)")
            .IsRequired();

        builder.Property(x => x.Marcada)
            .HasColumnName("marcada")
            .HasColumnType("bit")
            .IsRequired();
    }
}
