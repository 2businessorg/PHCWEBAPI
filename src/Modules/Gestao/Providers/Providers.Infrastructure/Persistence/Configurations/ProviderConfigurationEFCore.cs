using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Providers.Domain.Entities;

namespace Providers.Infrastructure.Persistence;

/// <summary>
/// Configuração EF Core para entidade Provider
/// </summary>
public class ProviderConfigurationEFCore : IEntityTypeConfiguration<Provider>
{
    public void Configure(EntityTypeBuilder<Provider> builder)
    {
        builder.ToTable("u_provider");

        builder.HasKey(p => p.UProviderStamp);

        builder.Property(p => p.UProviderStamp)
            .HasColumnName("u_providerstamp")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.Codigo)
            .HasColumnName("codigo")
            .IsRequired();

        builder.Property(p => p.Provedor)
            .HasColumnName("provedor")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.Environment)
            .HasColumnName("environment")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.Descricao)
            .HasColumnName("descricao")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(p => p.Ativo)
            .HasColumnName("ativo")
            .IsRequired();

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
            .HasMaxLength(50);

        builder.Property(p => p.UsrData)
            .HasColumnName("usrdata")
            .IsRequired(); // ✅ Obrigatório - sempre preenchido na criação

        builder.Property(p => p.UsrHora)
            .HasColumnName("usrhora")
            .HasMaxLength(8)
            .IsRequired(); // ✅ Obrigatório - sempre preenchido na criação

        builder.Property(p => p.UsrInis)
            .HasColumnName("usrinis")
            .HasMaxLength(50);

        // Relacionamento com ProviderValues
        builder.HasMany(p => p.Values)
            .WithOne(pv => pv.Provider)
            .HasForeignKey(pv => pv.UProviderStamp)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices
        builder.HasIndex(p => new { p.Provedor, p.Environment })
            .IsUnique()
            .HasDatabaseName("UQ_u_provider_provedor_env");

        builder.HasIndex(p => new { p.Codigo, p.Environment })
            .HasDatabaseName("IX_u_provider_codigo_env");
    }
}
