using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Providers.Domain.Entities;

namespace Providers.Infrastructure.Persistence;

/// <summary>
/// Configuração EF Core para entidade ProviderValue
/// </summary>
public class ProviderValueConfigurationEFCore : IEntityTypeConfiguration<ProviderValue>
{
    public void Configure(EntityTypeBuilder<ProviderValue> builder)
    {
        builder.ToTable("u_providervalues");

        builder.HasKey(pv => pv.UProviderValuesStamp);

        builder.Property(pv => pv.UProviderValuesStamp)
            .HasColumnName("u_providervaluesstamp")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(pv => pv.UProviderStamp)
            .HasColumnName("u_providerstamp")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(pv => pv.OperationCode)
            .HasColumnName("operationcode")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(pv => pv.Chave)
            .HasColumnName("chave")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(pv => pv.Valor)
            .HasColumnName("valor")
            .HasColumnType("NVARCHAR(MAX)")
            .IsRequired();

        builder.Property(pv => pv.Encriptado)
            .HasColumnName("encriptado")
            .IsRequired();

        builder.Property(pv => pv.Ordem)
            .HasColumnName("ordem")
            .IsRequired();

        builder.Property(pv => pv.Ativo)
            .HasColumnName("ativo")
            .IsRequired();

        // Campos de auditoria PHC
        builder.Property(pv => pv.OUsrData)
            .HasColumnName("ousrdata")
            .IsRequired();

        builder.Property(pv => pv.OUsrHora)
            .HasColumnName("ousrhora")
            .HasMaxLength(8)
            .IsRequired();

        builder.Property(pv => pv.OUsrInis)
            .HasColumnName("ousrinis")
            .HasMaxLength(50);

        builder.Property(pv => pv.UsrData)
            .HasColumnName("usrdata")
            .IsRequired(); // ✅ Obrigatório - sempre preenchido na criação

        builder.Property(pv => pv.UsrHora)
            .HasColumnName("usrhora")
            .HasMaxLength(8)
            .IsRequired(); // ✅ Obrigatório - sempre preenchido na criação

        builder.Property(pv => pv.UsrInis)
            .HasColumnName("usrinis")
            .HasMaxLength(50);

        // Índices
        builder.HasIndex(pv => new { pv.UProviderStamp, pv.OperationCode, pv.Chave })
            .IsUnique()
            .HasDatabaseName("UQ_u_providervalues_provider_op_key");

        builder.HasIndex(pv => new { pv.UProviderStamp, pv.OperationCode })
            .HasDatabaseName("IX_u_providervalues_provider");
    }
}
