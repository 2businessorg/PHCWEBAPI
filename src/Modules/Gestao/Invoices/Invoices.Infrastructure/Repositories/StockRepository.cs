using Invoices.Domain.Entities;
using Invoices.Domain.Repositories;
using Invoices.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Invoices.Infrastructure.Repositories;

/// <summary>
/// Implementação do repositório de artigos/produtos (ST) usando EF Core
/// </summary>
public class StockRepository : IStockRepository
{
    private readonly FaturasDbContext _context;

    public StockRepository(FaturasDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Verifica se um artigo existe pela sua referência
    /// </summary>
    public async Task<bool> ExistsByRefAsync(string ref_code, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(ref_code))
            return false;

        return await _context.St
            .AsNoTracking()
            .AnyAsync(s => s.Ref == ref_code.Trim(), cancellationToken);
    }

    /// <summary>
    /// Obtém um artigo pela sua referência
    /// </summary>
    public async Task<St?> GetByRefAsync(string ref_code, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(ref_code))
            return null;

        return await _context.St
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Ref == ref_code.Trim(), cancellationToken);
    }

    /// <summary>
    /// Adiciona um novo artigo
    /// </summary>
    public async Task AddAsync(St artigo, CancellationToken cancellationToken = default)
    {
        if (artigo == null)
            throw new ArgumentNullException(nameof(artigo));

        _context.St.Add(artigo);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Atualiza um artigo existente
    /// </summary>
    public async Task UpdateAsync(St artigo, CancellationToken cancellationToken = default)
    {
        if (artigo == null)
            throw new ArgumentNullException(nameof(artigo));

        _context.St.Update(artigo);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Remove um artigo
    /// </summary>
    public async Task<bool> DeleteAsync(string ref_code, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(ref_code))
            return false;

        var artigo = await _context.St
            .FirstOrDefaultAsync(s => s.Ref == ref_code.Trim(), cancellationToken);

        if (artigo == null)
            return false;

        _context.St.Remove(artigo);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
