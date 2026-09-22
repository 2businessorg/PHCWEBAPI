using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Parameters.Domain.Entities;

namespace Parameters.Infrastructure.Persistence;

/// <summary>
/// Configuração EF Core para entidade Para1
/// </summary>
public class Para1ConfigurationEFCore : IEntityTypeConfiguration<Para1>
{
    public void Configure(EntityTypeBuilder<Para1> builder)
    {
        builder.ToTable("para1");

        builder.HasKey(p => p.Para1Stamp);

        builder.Property(p => p.Para1Stamp)
            .HasColumnName("para1stamp")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.Descricao)
            .HasColumnName("descricao")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.Valor)
            .HasColumnName("valor")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(p => p.Tipo)
            .HasColumnName("tipo")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.Dec)
            .HasColumnName("dec");

        builder.Property(p => p.Tam)
            .HasColumnName("tam");

        // Campos de auditoria PHC
        builder.Property(p => p.OUsrData)
            .HasColumnName("ousrdata")
            .IsRequired();

        builder.Property(p => p.OUsrHora)
            .HasColumnName("ousrhora")
            .HasMaxLength(8)
            .IsRequired();

        builder.Property(p => p.OUsrInis)
            .HasColumnName("ousrinis")
            .HasMaxLength(100);

        builder.Property(p => p.UsrData)
            .HasColumnName("usrdata")
            .IsRequired(); // ✅ Obrigatório - sempre preenchido na criação

        builder.Property(p => p.UsrHora)
            .HasColumnName("usrhora")
            .HasMaxLength(8)
            .IsRequired(); // ✅ Obrigatório - sempre preenchido na criação

        builder.Property(p => p.UsrInis)
            .HasColumnName("usrinis")
            .HasMaxLength(100);

   
    }
}
