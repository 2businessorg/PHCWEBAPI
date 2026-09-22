using Invoices.Domain.Entities;
using Invoices.Domain.Repositories;
using Invoices.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Invoices.Infrastructure.Repositories
{
    /// <summary>
    /// Implementação do repositório de Faturas
    /// Utiliza Entity Framework Core para acesso aos dados
    /// </summary>
    public class FaturaRepository : IFaturaRepository
    {
        private readonly FaturasDbContext _context;

        public FaturaRepository(FaturasDbContext context)
        {
            _context = context ?? throw new System.ArgumentNullException(nameof(context));
        }

        public async Task<Ft> AddAsync(Ft fatura)
        {
            if (fatura == null)
                throw new System.ArgumentNullException(nameof(fatura));

            var entry = await _context.Faturas.AddAsync(fatura);
            return entry.Entity;
        }

        public async Task<Ft> GetByIdAsync(int ndoc, int fno, int ftano)
        {
            return await _context.Faturas
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Ndoc == ndoc && f.Fno == fno && f.FtAno == ftano);
        }

        public async Task<IEnumerable<Ft>> GetAllAsync(int page = 1, int pageSize = 20)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;

            return await _context.Faturas
                .AsNoTracking()
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Ft>> GetByClientAsync(int no, int page = 1, int pageSize = 20)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;

            return await _context.Faturas
                .Where(f => f.No == no)
                .AsNoTracking()
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<Ft> UpdateAsync(Ft fatura)
        {
            if (fatura == null)
                throw new System.ArgumentNullException(nameof(fatura));

            _context.Faturas.Update(fatura);
            return fatura;
        }

        public async Task<bool> DeleteAsync(int ndoc, int fno, int ftano)
        {
            var fatura = await _context.Faturas
                .FirstOrDefaultAsync(f => f.Ndoc == ndoc && f.Fno == fno && f.FtAno == ftano);

            if (fatura == null)
                return false;

            _context.Faturas.Remove(fatura);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int ndoc, int fno, int ftano)
        {
            return await _context.Faturas
                .AnyAsync(f => f.Ndoc == ndoc && f.Fno == fno && f.FtAno == ftano);
        }

        public async Task<IEnumerable<Fi>> GetLinesAsync(string ftStamp)
        {
            if (string.IsNullOrWhiteSpace(ftStamp))
                return new List<Fi>();

            return await _context.FaturaLinhas
                .AsNoTracking()
                .Where(l => l.Ftstamp == ftStamp)
                .OrderBy(l => l.Fistamp)
                .ToListAsync();
        }

        public async Task<FT2?> GetDadosSecundariosAsync(string ftStamp)
        {
            if (string.IsNullOrWhiteSpace(ftStamp))
                return null;

            return await _context.FaturaDadosSecundarios
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Ft2Stamp == ftStamp);
        }

        public async Task<IEnumerable<FT3>> GetDadosAdicionaisAsync(string ftStamp)
        {
            if (string.IsNullOrWhiteSpace(ftStamp))
                return new List<FT3>();

            return await _context.FaturaDadosAdicionais
                .AsNoTracking()
                .Where(x => x.Ft3Stamp == ftStamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<Fi2>> GetLinesAdicionaisAsync(IEnumerable<string> fiStamps)
        {
            return await _context.FaturaLinhasDadosAdicionais
                .AsNoTracking()
                .Where(x => fiStamps.Contains(x.Fi2Stamp))
                .ToListAsync();
        }

        public async Task<FaturaAggregate?> GetAggregateAsync(int ndoc, int fno, int ftano)
        {
            var fatura = await GetByIdAsync(ndoc, fno, ftano);
            if (fatura == null)
                return null;

            var linhas = await GetLinesAsync(fatura.FtStamp);
            var fiStamps = linhas.Select(l => l.Fistamp).ToList();
            var linhasAdicionais = await GetLinesAdicionaisAsync(fiStamps);
            var dadosSecundarios = await GetDadosSecundariosAsync(fatura.FtStamp);
            var dadosAdicionais = await GetDadosAdicionaisAsync(fatura.FtStamp);

            return new FaturaAggregate
            {
                Fatura = fatura,
                DadosSecundarios = dadosSecundarios ?? new FT2(),
                DadosAdicionais = dadosAdicionais.FirstOrDefault() ?? new FT3(),
                Linhas = linhas.ToList(),
                LinhasAdicionais = linhasAdicionais.ToList()
            };
        }

        public async Task<Dictionary<string, object?>> GetUserFieldValuesAsync(
            string tableName,
            string stampColumn,
            string stamp,
            IReadOnlyList<string> columns,
            CancellationToken cancellationToken = default)
        {
            var safeColumns = GetSafeColumns(columns);
            if (safeColumns.Count == 0 || string.IsNullOrWhiteSpace(stamp))
                return new Dictionary<string, object?>();

            var connection = (SqlConnection)_context.Database.GetDbConnection();
            var wasOpen = connection.State == ConnectionState.Open;
            if (!wasOpen)
                await connection.OpenAsync(cancellationToken);

            try
            {
                var existingColumns = await GetTableColumnsAsync(connection, tableName, cancellationToken);
                var validColumns = safeColumns.Where(existingColumns.Contains).ToList();
                if (validColumns.Count == 0)
                    return new Dictionary<string, object?>();

                var select = string.Join(", ", validColumns.Select(c => $"[{c}]"));
                var sql = $"SELECT {select} FROM [dbo].[{tableName}] WHERE [{stampColumn}] = @stamp";

                await using var command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@stamp", stamp);

                await using var reader = await command.ExecuteReaderAsync(cancellationToken);
                if (!await reader.ReadAsync(cancellationToken))
                    return new Dictionary<string, object?>();

                var result = new Dictionary<string, object?>(validColumns.Count, StringComparer.OrdinalIgnoreCase);
                for (var i = 0; i < validColumns.Count; i++)
                    result[validColumns[i]] = reader.IsDBNull(i) ? null : reader.GetValue(i);

                return result;
            }
            finally
            {
                if (!wasOpen)
                    await connection.CloseAsync();
            }
        }

        private static List<string> GetSafeColumns(IReadOnlyList<string> columns)
            => columns
                .Where(c => !string.IsNullOrWhiteSpace(c) && c.All(ch => char.IsLetterOrDigit(ch) || ch == '_'))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

        private static async Task<HashSet<string>> GetTableColumnsAsync(
            SqlConnection connection,
            string tableName,
            CancellationToken cancellationToken)
        {
            const string sql = "SELECT [name] FROM sys.columns WHERE object_id = OBJECT_ID(@objName)";
            await using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@objName", $"dbo.{tableName}");

            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
                result.Add(reader.GetString(0));

            return result;
        }

    }
}
