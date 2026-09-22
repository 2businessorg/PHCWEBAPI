using Currencies.Domain.Repositories;
using Currencies.Domain.ValueObjects;
using Currencies.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Currencies.Infrastructure.Repositories;

/// <summary>
/// Repositório agregado de moedas (para1 + cb).
/// </summary>
public sealed class CurrencyRepositoryEFCore : ICurrencyRepository
{
    private const string BaseCurrencyDescription = "ge_pteoueuro";

    private readonly CurrenciesDbContext _context;

    /// <summary>
    /// Inicializa uma nova instância do repositório.
    /// </summary>
    public CurrencyRepositoryEFCore(CurrenciesDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtém todas as moedas agregadas (para1 + cb).
    /// </summary>
    public async Task<IReadOnlyList<CurrencyData>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        // Buscar moedas do CB com país
        var cbCurrencies = await _context.Cb
            .AsNoTracking()
            .Where(c => !string.IsNullOrWhiteSpace(c.Moeda))
            .Select(c => new { Moeda = c.Moeda.Trim(), Pais = c.Pais.Trim() })
            .ToListAsync(cancellationToken);

        // Buscar moeda base do para1
        var baseCurrencies = await _context.Para1
            .AsNoTracking()
            .Where(p => p.Descricao == BaseCurrencyDescription && !string.IsNullOrWhiteSpace(p.Valor))
            .Select(p => new { Moeda = p.Valor.Trim(), Pais = "Base" })
            .ToListAsync(cancellationToken);

        // Combinar e remover duplicatas no cliente
        var currencies = cbCurrencies
            .Union(baseCurrencies)
            .Distinct()
            .OrderBy(c => c.Moeda)
            .Select(c => new CurrencyData(c.Moeda, c.Pais))
            .ToList();

        return currencies;
    }

    /// <summary>
    /// Obtém uma moeda específica pelo código.
    /// </summary>
    public async Task<CurrencyData?> GetByCodeAsync(string moeda, CancellationToken cancellationToken = default)
    {
        var normalizedMoeda = moeda.Trim().ToUpper();

        var baseCurrency = await _context.Para1
            .AsNoTracking()
            .Where(p => p.Descricao == BaseCurrencyDescription && p.Valor.Trim().ToUpper() == normalizedMoeda)
            .Select(p => new CurrencyData(p.Valor.Trim(), "Base"))
            .FirstOrDefaultAsync(cancellationToken);

        if (baseCurrency is not null)
            return baseCurrency;

        var cbCurrency = await _context.Cb
            .AsNoTracking()
            .Where(c => c.Moeda.Trim().ToUpper() == normalizedMoeda)
            .Select(c => new CurrencyData(c.Moeda.Trim(), c.Pais.Trim()))
            .FirstOrDefaultAsync(cancellationToken);

        return cbCurrency;
    }

    /// <summary>
    /// Verifica se uma moeda existe.
    /// </summary>
    public async Task<bool> ExistsAsync(string moeda, CancellationToken cancellationToken = default)
    {
        var normalizedMoeda = moeda.Trim().ToUpper();

        var existsInPara1 = await _context.Para1
            .AsNoTracking()
            .AnyAsync(p => p.Descricao == BaseCurrencyDescription && p.Valor.Trim().ToUpper() == normalizedMoeda, cancellationToken);

        if (existsInPara1)
            return true;

        var existsInCb = await _context.Cb
            .AsNoTracking()
            .AnyAsync(c => c.Moeda.Trim().ToUpper() == normalizedMoeda, cancellationToken);

        return existsInCb;
    }

    /// <summary>
    /// Obtém taxas de conversão por código de moeda.
    /// </summary>
    public async Task<IReadOnlyList<CurrencyExchangeRateData>> GetExchangeRatesByCodeAsync(string moeda, CancellationToken cancellationToken = default)
    {
        var normalizedMoeda = moeda.Trim().ToUpper();

        var rates = await _context.Cb
            .AsNoTracking()
            .Where(c => c.Moeda.Trim().ToUpper() == normalizedMoeda)
            .OrderByDescending(c => c.Data)
            .Select(c => new CurrencyExchangeRateData(
                c.Moeda.Trim(),
                c.Pais.Trim(),
                c.UCambioC,
                c.UCambioV,
                c.Data))
            .ToListAsync(cancellationToken);

        return rates;
    }

    /// <summary>
    /// Obtém todas as taxas de conversão.
    /// </summary>
    public async Task<IReadOnlyList<CurrencyExchangeRateData>> GetAllExchangeRatesAsync(CancellationToken cancellationToken = default)
    {
        var rates = await _context.Cb
            .AsNoTracking()
            .Where(c => !string.IsNullOrWhiteSpace(c.Moeda))
            .OrderByDescending(c => c.Data)
            .ThenBy(c => c.Moeda)
            .Select(c => new CurrencyExchangeRateData(
                c.Moeda.Trim(),
                c.Pais.Trim(),
                c.UCambioC,
                c.UCambioV,
                c.Data))
            .ToListAsync(cancellationToken);

        return rates;
    }

    /// <summary>
    /// Verifica se existe uma taxa de conversão com a moeda e país especificados.
    /// </summary>
    public async Task<bool> ExchangeRateExistsAsync(string moeda, string pais, CancellationToken cancellationToken = default)
    {
        var normalizedMoeda = moeda.Trim().ToUpper();
        var normalizedPais = pais.Trim().ToUpper();

        return await _context.Cb
            .AsNoTracking()
            .AnyAsync(c => c.Moeda.Trim().ToUpper() == normalizedMoeda && 
                          c.Pais.Trim().ToUpper() == normalizedPais, 
                     cancellationToken);
    }
}
