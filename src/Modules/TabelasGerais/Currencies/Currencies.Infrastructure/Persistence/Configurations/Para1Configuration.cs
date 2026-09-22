using Currencies.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Currencies.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuração EF Core da tabela para1.
/// </summary>
public class Para1Configuration : IEntityTypeConfiguration<Para1>
{
    public void Configure(EntityTypeBuilder<Para1> builder)
    {
        builder.ToTable("para1");

        builder.HasKey(x => x.Descricao)
            .HasName("pk_para1");

        builder.Property(x => x.Para1Stamp)
            .HasColumnName("para1stamp")
            .HasColumnType("char(25)")
            .IsRequired();

        builder.Property(x => x.Descricao)
            .HasColumnName("descricao")
            .HasColumnType("char(200)")
            .IsRequired();

        builder.Property(x => x.Valor)
            .HasColumnName("valor")
            .HasColumnType("char(200)")
            .IsRequired();
    }
}
