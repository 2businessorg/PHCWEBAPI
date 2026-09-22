using Invoices.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Invoices.Domain.Repositories
{
    /// <summary>
    /// Interface para repositório de Faturas (FT)
    /// Define operações CRUD básicas para faturas
    /// </summary>
    public interface IFaturaRepository
    {
        /// <summary>
        /// Adiciona uma nova fatura ao repositório
        /// </summary>
        Task<Ft> AddAsync(Ft fatura);

        /// <summary>
        /// Recupera uma fatura pelo seu identificador único
        /// </summary>
        Task<Ft> GetByIdAsync(int ndoc, int nfo, int ftano);

        /// <summary>
        /// Recupera todas as faturas com paginação
        /// </summary>
        Task<IEnumerable<Ft>> GetAllAsync(int page = 1, int pageSize = 20);

        /// <summary>
        /// Recupera faturas de um cliente específico
        /// </summary>
        Task<IEnumerable<Ft>> GetByClientAsync(int no, int page = 1, int pageSize = 20);

        /// <summary>
        /// Atualiza uma fatura existente
        /// </summary>
        Task<Ft> UpdateAsync(Ft fatura);

        /// <summary>
        /// Remove uma fatura
        /// </summary>
        Task<bool> DeleteAsync(int ndoc, int nfo, int ftano);

        /// <summary>
        /// Verifica se uma fatura existe
        /// </summary>
        Task<bool> ExistsAsync(int ndoc, int nfo, int ftano);

        /// <summary>
        /// Recupera linhas (FI) da fatura pelo seu stamp
        /// </summary>
        Task<IEnumerable<Fi>> GetLinesAsync(string ftStamp);

        /// <summary>
        /// Recupera dados secundários (FT2) pelo stamp da fatura
        /// </summary>
        Task<FT2?> GetDadosSecundariosAsync(string ftStamp);

        /// <summary>
        /// Recupera linhas adicionais (FI2) da fatura
        /// </summary>
        Task<IEnumerable<Fi2>> GetLinesAdicionaisAsync(IEnumerable<string> fiStamps);

        /// <summary>
        /// Recupera o agregado completo da fatura
        /// </summary>
        Task<FaturaAggregate?> GetAggregateAsync(int ndoc, int nfo, int ftano);

        /// <summary>
        /// Lê valores de campos de utilizador de uma tabela pelo stamp
        /// </summary>
        Task<Dictionary<string, object?>> GetUserFieldValuesAsync(
            string tableName,
            string stampColumn,
            string stamp,
            IReadOnlyList<string> columns,
            CancellationToken cancellationToken = default);
    }
}
