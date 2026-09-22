using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Shared.Abstractions.Entities;

namespace Shared.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Interceptor EF Core compartilhado para preencher automaticamente campos de auditoria
/// em todas as entidades que herdam de AuditableEntity.
/// 
/// Este interceptor é reutilizável em todos os módulos que usam EF Core.
/// </summary>
public class AuditableEntityInterceptorEFCore : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        UpdateAuditableEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        UpdateAuditableEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void UpdateAuditableEntities(DbContext? context)
    {
        if (context == null) return;

        var now = DateTime.UtcNow;
        var timeString = now.ToString("HH:mm:ss");

        foreach (var entry in context.ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                // ✅ Criação - preenche se ainda não foi preenchido pela entidade
                if (entry.Entity.OUsrData == default)
                {
                    entry.Entity.GetType()
                        .GetProperty(nameof(AuditableEntity.OUsrData))!
                        .SetValue(entry.Entity, now);
                }

                if (string.IsNullOrEmpty(entry.Entity.OUsrHora))
                {
                    entry.Entity.GetType()
                        .GetProperty(nameof(AuditableEntity.OUsrHora))!
                        .SetValue(entry.Entity, timeString);
                }

                // ✅ Garantir que UsrData e UsrHora também sejam preenchidos na criação
                if (entry.Entity.UsrData == default)
                {
                    entry.Entity.GetType()
                        .GetProperty(nameof(AuditableEntity.UsrData))!
                        .SetValue(entry.Entity, now);
                }

                if (string.IsNullOrEmpty(entry.Entity.UsrHora))
                {
                    entry.Entity.GetType()
                        .GetProperty(nameof(AuditableEntity.UsrHora))!
                        .SetValue(entry.Entity, timeString);
                }
            }
            else if (entry.State == EntityState.Modified)
            {
                // ✅ Atualização - sempre sobrescreve UsrData/UsrHora/UsrInis
                entry.Entity.GetType()
                    .GetProperty(nameof(AuditableEntity.UsrData))!
                    .SetValue(entry.Entity, now);

                entry.Entity.GetType()
                    .GetProperty(nameof(AuditableEntity.UsrHora))!
                    .SetValue(entry.Entity, timeString);
            }
        }
    }
}
