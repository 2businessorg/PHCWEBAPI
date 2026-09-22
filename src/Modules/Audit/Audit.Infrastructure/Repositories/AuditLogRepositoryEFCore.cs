using Microsoft.EntityFrameworkCore;
using Audit.Domain.Entities;
using Audit.Domain.Repositories;
using Audit.Infrastructure.Persistence;

namespace Audit.Infrastructure.Repositories;


public class AuditLogRepositoryEFCore : IAuditLogRepository
{
    private readonly AuditDbContextEFCore _context;

    public AuditLogRepositoryEFCore(AuditDbContextEFCore context)
    {
        _context = context;
    }

    public void Add(AuditLog auditLog)
    {
        _context.AuditLogs.Add(auditLog);
        _context.SaveChanges();
    }

    public async Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
    {
        await _context.AuditLogs.AddAsync(auditLog, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<AuditLog>> GetByRequestIdAsync(string requestId, CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs
            .Where(a => a.RequestId == requestId)
            .OrderByDescending(a => a.Data)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AuditLog>> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs
            .Where(a => a.Code == code)
            .OrderByDescending(a => a.Data)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs
            .Where(a => a.Data >= startDate && a.Data <= endDate)
            .OrderByDescending(a => a.Data)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AuditLog>> GetByOperationAsync(string operation, CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs
            .Where(a => a.Operation == operation)
            .OrderByDescending(a => a.Data)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AuditLog>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs.Take(10)
            .OrderByDescending(a => a.Data)
            .ToListAsync(cancellationToken);
    }

    public async Task<AuditLog?> GetByStampAsync(string stamp, CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs
            .FirstOrDefaultAsync(a => a.ULogsstamp == stamp, cancellationToken);
    }

    public async Task<IEnumerable<AuditLog>> GetByCorrelationIdAsync(string correlationId, CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs
            .Where(a => a.RequestId == correlationId)
            .OrderByDescending(a => a.Data)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// ✅ NOVO: Busca paginada com filtros aplicados NO BANCO
    /// </summary>
    public async Task<(IEnumerable<AuditLog> logs, int totalCount)> GetPagedAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? correlationId = null,
        string? operation = null,
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        // ✅ Query base com AsNoTracking para performance
        var query = _context.AuditLogs.AsNoTracking();

        // ✅ Aplicar filtros NO BANCO (não em memória)
        if (startDate.HasValue)
        {
            query = query.Where(a => a.Data >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(a => a.Data <= endDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(correlationId))
        {
            query = query.Where(a => a.RequestId == correlationId);
        }

        if (!string.IsNullOrWhiteSpace(operation))
        {
            query = query.Where(a => a.Operation != null && a.Operation.Contains(operation));
        }

        // ✅ Contar total ANTES da paginação (query separada otimizada)
        var totalCount = await query.CountAsync(cancellationToken);

        // ✅ Aplicar ordenação e paginação
        var logs = await query
            .OrderByDescending(a => a.Data)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (logs, totalCount);
    }

    /// <summary>
    /// ✅ NOVO: Busca avançada com TODOS os filtros dinâmicos (otimizada para tabelas grandes)
    /// </summary>
    public async Task<(IEnumerable<AuditLog> logs, int totalCount)> GetAdvancedPagedAsync(
        string? requestId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? code = null,
        string? content = null,
        string? ip = null,
        string? operation = null,
        string? responseDesc = null,
        string? responseText = null,
        bool useExactMatch = false,
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        // ✅ Query base com AsNoTracking para MÁXIMA performance (não rastreia mudanças)
        var query = _context.AuditLogs.AsNoTracking();

        // ✅ Filtros aplicados NO BANCO (WHERE clauses otimizadas)

        // Filtro por RequestId (Correlation ID)
        if (!string.IsNullOrWhiteSpace(requestId))
        {
            if (useExactMatch)
            {
                query = query.Where(a => a.RequestId == requestId);
            }
            else
            {
                query = query.Where(a => a.RequestId != null && a.RequestId.Contains(requestId));
            }
        }

        // Filtro por intervalo de Data/Hora
        if (startDate.HasValue)
        {
            query = query.Where(a => a.Data >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            // Incluir o dia inteiro (até 23:59:59)
            var endDateInclusive = endDate.Value.Date.AddDays(1).AddSeconds(-1);
            query = query.Where(a => a.Data <= endDateInclusive);
        }

        // Filtro por Code (geralmente exato)
        if (!string.IsNullOrWhiteSpace(code))
        {
            query = query.Where(a => a.Code == code);
        }

        // Filtro por Content (sempre LIKE/Contains para JSON)
        if (!string.IsNullOrWhiteSpace(content))
        {
            query = query.Where(a => a.Content != null && a.Content.Contains(content));
        }

        // Filtro por IP
        if (!string.IsNullOrWhiteSpace(ip))
        {
            if (useExactMatch)
            {
                query = query.Where(a => a.Ip == ip);
            }
            else
            {
                query = query.Where(a => a.Ip != null && a.Ip.Contains(ip));
            }
        }

        // Filtro por Operation
        if (!string.IsNullOrWhiteSpace(operation))
        {
            if (useExactMatch)
            {
                query = query.Where(a => a.Operation == operation);
            }
            else
            {
                query = query.Where(a => a.Operation != null && a.Operation.Contains(operation));
            }
        }

        // Filtro por ResponseDesc (sempre LIKE/Contains)
        if (!string.IsNullOrWhiteSpace(responseDesc))
        {
            query = query.Where(a => a.ResponseDesc != null && a.ResponseDesc.Contains(responseDesc));
        }

        // Filtro por ResponseText (sempre LIKE/Contains para JSON)
        if (!string.IsNullOrWhiteSpace(responseText))
        {
            query = query.Where(a => a.ResponseText != null && a.ResponseText.Contains(responseText));
        }

        // ✅ Contar total ANTES da paginação (query COUNT otimizada pelo SQL Server)
        var totalCount = await query.CountAsync(cancellationToken);

        // ✅ Aplicar ordenação (índice em Data) e paginação
        var logs = await query
            .OrderByDescending(a => a.Data) // Usa índice IX_u_logs_Data
            .ThenByDescending(a => a.ULogsstamp) // Desempate por stamp
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (logs, totalCount);
    }
}
