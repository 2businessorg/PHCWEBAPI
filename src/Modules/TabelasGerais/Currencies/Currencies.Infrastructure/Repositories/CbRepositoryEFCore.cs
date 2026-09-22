using Currencies.Domain.Entities;
using Currencies.Domain.Repositories;
using Currencies.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Currencies.Infrastructure.Repositories;

/// <summary>
/// Repositório EF Core da tabela cb.
/// </summary>
public sealed class CbRepositoryEFCore : ICbRepository
{
    private readonly CurrenciesDbContext _context;

    /// <summary>
    /// Inicializa uma nova instância do repositório.
    /// </summary>
    public CbRepositoryEFCore(CurrenciesDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtém lista distinta de moedas.
    /// </summary>
    public async Task<IReadOnlyList<string>> GetDistinctMoedasAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Cb
            .AsNoTracking()
            .Where(c => !string.IsNullOrWhiteSpace(c.Moeda))
            .Select(c => c.Moeda.Trim())
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Cria uma nova moeda na tabela cb.
    /// </summary>
    public async Task CreateAsync(string pais, string moeda, string? createdBy = null, CancellationToken cancellationToken = default)
    {
        var now = DateTime.Now;
        var cbStamp = $"{Guid.NewGuid():N}".Substring(0, 25);
        var hour = now.ToString("HH:mm:ss");
        var user = string.IsNullOrWhiteSpace(createdBy) ? "PHCAPI" : createdBy.Trim();

        var cb = new Cb
        {
            CbStamp = cbStamp,
            Pais = pais.Trim(),
            Moeda = moeda.Trim().ToUpper(),
            Data = now,
            UCambioC = 0,
            UCambioV = 0,
            Cambio = 0,
            Obs = string.Empty,
            Cambio2 = 0,
            Ecambio = 0,
            Ecambio2 = 0,
            Zonaeuro = false,
            Unisg = string.Empty,
            Unipl = string.Empty,
            Centsg = string.Empty,
            Centpl = string.Empty,
            Taxa = 0,
            Taxa2 = 0,
            Cambioinvertido = 0,
            Cambioinvertido2 = 0,
            Ecambioinvertido = 0,
            Ecambioinvertido2 = 0,
            OusrInis = user,
            OusrData = now.Date,
            OusrHora = hour,
            UsrInis = user,
            UsrData = now.Date,
            UsrHora = hour,
            Marcada = false
        };

        _context.Cb.Add(cb);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Atualiza uma moeda existente.
    /// </summary>
    public async Task<bool> UpdateAsync(string moeda, string pais, string? updatedBy = null, CancellationToken cancellationToken = default)
    {
        var normalizedMoeda = moeda.Trim().ToUpper();

        var cb = await _context.Cb
            .FirstOrDefaultAsync(c => c.Moeda.Trim().ToUpper() == normalizedMoeda, cancellationToken);

        if (cb is null)
            return false;

        var now = DateTime.Now;

        cb.Pais = pais.Trim();
        cb.UsrInis = string.IsNullOrWhiteSpace(updatedBy) ? "PHCAPI" : updatedBy.Trim();
        cb.UsrData = now.Date;
        cb.UsrHora = now.ToString("HH:mm:ss");

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    /// <summary>
    /// Deleta uma moeda existente.
    /// </summary>
    public async Task<bool> DeleteAsync(string moeda, CancellationToken cancellationToken = default)
    {
        var normalizedMoeda = moeda.Trim().ToUpper();

        var cb = await _context.Cb
            .FirstOrDefaultAsync(c => c.Moeda.Trim().ToUpper() == normalizedMoeda, cancellationToken);

        if (cb is null)
            return false;

        _context.Cb.Remove(cb);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    /// <summary>
    /// Cria uma taxa de conversão na tabela cb.
    /// </summary>
    public async Task CreateExchangeRateAsync(
        string pais,
        string moeda,
        decimal buyRate,
        decimal sellRate,
        DateTime data,
        string? createdBy = null,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.Now;
        var cbStamp = $"{Guid.NewGuid():N}".Substring(0, 25);
        var hour = now.ToString("HH:mm:ss");
        var user = string.IsNullOrWhiteSpace(createdBy) ? "PHCAPI" : createdBy.Trim();
        var normalizedMoeda = moeda.Trim().ToUpper();
        var normalizedPais = pais.Trim();

        // Lógica PHC (conforme script):
        // vrCambV = 1 / u_cambiov
        // vrCambC = 1 / u_cambioc
        // ecambio/ecambio2/cambio/cambio2 recebem os valores invertidos
        var vrCambV = 1m / sellRate;
        var vrCambC = 1m / buyRate;

        var existingRows = await _context.Cb
            .Where(c => c.Moeda.Trim().ToUpper() == normalizedMoeda)
            .ToListAsync(cancellationToken);

        var shouldUpdateSingleRowWithZeroRate =
            existingRows.Count == 1 &&
            (existingRows[0].UCambioV == 0m || existingRows[0].UCambioC == 0m);

        if (shouldUpdateSingleRowWithZeroRate)
        {
            var existing = existingRows[0];

            existing.Pais = normalizedPais;
            existing.Data = data;
            existing.UCambioC = buyRate;
            existing.UCambioV = sellRate;
            existing.Cambio = vrCambV;
            existing.Cambio2 = vrCambC;
            existing.Ecambio = vrCambV;
            existing.Ecambio2 = vrCambC;

            existing.UsrInis = user;
            existing.UsrData = now.Date;
            existing.UsrHora = hour;

            await _context.SaveChangesAsync(cancellationToken);
            return;
        }

        var cb = new Cb
        {
            CbStamp = cbStamp,
            Pais = normalizedPais,
            Moeda = normalizedMoeda,
            Data = data,
            UCambioC = buyRate,
            UCambioV = sellRate,
            Cambio = vrCambV,
            Obs = string.Empty,
            Cambio2 = vrCambC,
            Ecambio = vrCambV,
            Ecambio2 = vrCambC,
            Zonaeuro = false,
            Unisg = string.Empty,
            Unipl = string.Empty,
            Centsg = string.Empty,
            Centpl = string.Empty,
            Taxa = 0,
            Taxa2 = 0,
            Cambioinvertido = 0,
            Cambioinvertido2 = 0,
            Ecambioinvertido = 0,
            Ecambioinvertido2 = 0,
            OusrInis = user,
            OusrData = now.Date,
            OusrHora = hour,
            UsrInis = user,
            UsrData = now.Date,
            UsrHora = hour,
            Marcada = false
        };

        _context.Cb.Add(cb);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
