using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Audit.Domain.Entities;

namespace Audit.Infrastructure.Persistence;


public class AuditLogConfigurationEFCore : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        // Tabela
        builder.ToTable("u_logs");

        // Chave primária
        builder.HasKey(a => a.ULogsstamp)
            .HasName("PK__u_logs__9803C30F60140409");

        // Propriedades
        builder.Property(a => a.ULogsstamp)
            .HasColumnName("u_logsstamp")
            .HasMaxLength(50)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(a => a.RequestId)
            .HasColumnName("requestId")
            .IsUnicode(false);

        builder.Property(a => a.Data)
            .HasColumnName("data")
            .HasColumnType("datetime");

        builder.Property(a => a.Code)
            .HasColumnName("code")
            .IsUnicode(false);

        builder.Property(a => a.Content)
            .HasColumnName("content")
            .IsUnicode(false);

        builder.Property(a => a.Ip)
            .HasColumnName("ip")
            .IsUnicode(false);

        builder.Property(a => a.ResponseDesc)
            .HasColumnName("responseDesc")
            .IsUnicode(false);

        builder.Property(a => a.ResponseText)
            .HasColumnName("responsetext")
            .IsUnicode(false);

        builder.Property(a => a.Operation)
            .HasColumnName("operation")
            .IsUnicode(false);

        // ✅ Identificação do usuário
        builder.Property(a => a.UserId)
            .HasColumnName("userId")
            .HasMaxLength(450)
            .IsUnicode(false);

        builder.Property(a => a.Username)
            .HasColumnName("username")
            .HasMaxLength(256)
            .IsUnicode(false);

        // Índices para performance
        builder.HasIndex(a => a.RequestId)
            .HasDatabaseName("idx_ulogs_requestid");

        builder.HasIndex(a => a.Data)
            .HasDatabaseName("idx_ulogs_data");

        builder.HasIndex(a => a.Code)
            .HasDatabaseName("idx_ulogs_code");

        builder.HasIndex(a => a.Operation)
            .HasDatabaseName("idx_ulogs_operation");

        // ✅ Índices para consultas por usuário
        builder.HasIndex(a => new { a.Username, a.UserId })
            .HasDatabaseName("idx_ulogs_user");

        builder.HasIndex(a => a.UserId)
            .HasDatabaseName("idx_ulogs_userid");
    }
}
