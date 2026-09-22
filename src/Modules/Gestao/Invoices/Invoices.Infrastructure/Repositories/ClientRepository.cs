using Invoices.Domain.Entities;
using Invoices.Domain.Repositories;
using Invoices.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Invoices.Infrastructure.Repositories;

/// <summary>
/// Implementação do repositório de clientes (CL) usando EF Core
/// </summary>
public class ClientRepository : IClientRepository
{
    private readonly FaturasDbContext _context;

    public ClientRepository(FaturasDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Verifica se um cliente existe pela combinação No + Estab
    /// </summary>
    public async Task<bool> ExistsByNoAndEstabAsync(decimal no, decimal estab, CancellationToken cancellationToken = default)
    {
        return await _context.Cl
            .AsNoTracking()
            .AnyAsync(c => c.No == no && c.Estab == estab, cancellationToken);
    }

    /// <summary>
    /// Obtém um cliente pela combinação No + Estab
    /// </summary>
    public async Task<Cl?> GetByNoAndEstabAsync(decimal no, decimal estab, CancellationToken cancellationToken = default)
    {
        return await _context.Cl
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.No == no && c.Estab == estab, cancellationToken);
    }

    /// <summary>
    /// Adiciona um novo cliente
    /// </summary>
    public async Task AddAsync(Cl cliente, CancellationToken cancellationToken = default)
    {
        if (cliente == null)
            throw new ArgumentNullException(nameof(cliente));

        _context.Cl.Add(cliente);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Atualiza um cliente existente
    /// </summary>
    public async Task UpdateAsync(Cl cliente, CancellationToken cancellationToken = default)
    {
        if (cliente == null)
            throw new ArgumentNullException(nameof(cliente));

        _context.Cl.Update(cliente);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Remove um cliente
    /// </summary>
    public async Task<bool> DeleteAsync(decimal no, decimal estab, CancellationToken cancellationToken = default)
    {
        var cliente = await _context.Cl
            .FirstOrDefaultAsync(c => c.No == no && c.Estab == estab, cancellationToken);

        if (cliente == null)
            return false;

        _context.Cl.Remove(cliente);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
