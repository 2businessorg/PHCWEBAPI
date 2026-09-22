using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Parameters.Domain.Entities;

namespace Parameters.Infrastructure.Persistence;

/// <summary>
/// Configuração EF Core para a entidade Cb (tabela cb — taxas de câmbio / moedas).
/// </summary>
public class CbConfigurationEFCore : IEntityTypeConfiguration<Cb>
{
    public void Configure(EntityTypeBuilder<Cb> builder)
    {
        builder.ToTable("cb");

        builder.HasKey(c => c.Cbstamp);

        builder.Property(c => c.Cbstamp)
            .HasColumnName("cbstamp")
            .HasMaxLength(25)
            .IsRequired();

        builder.Property(c => c.Moeda)
            .HasColumnName("moeda")
            .HasMaxLength(11)
            .IsRequired();

        builder.Property(c => c.Descricao)
            .HasColumnName("descricao")
            .HasMaxLength(55);

        builder.Property(c => c.Taxac)
            .HasColumnName("taxac")
            .HasColumnType("numeric(18,5)");

        builder.Property(c => c.Taxav)
            .HasColumnName("taxav")
            .HasColumnType("numeric(18,5)");
    }
}
