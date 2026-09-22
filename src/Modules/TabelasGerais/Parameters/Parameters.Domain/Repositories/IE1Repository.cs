using Parameters.Domain.Entities;

namespace Parameters.Domain.Repositories;


public interface IPara1Repository
{
    Task<Para1?> GetByStampAsync(string para1Stamp, CancellationToken cancellationToken = default);
    Task<Para1?> GetByDescricaoAsync(string descricao, CancellationToken cancellationToken = default);
    Task<IEnumerable<Para1>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default);
    Task<IEnumerable<Para1>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string para1Stamp, CancellationToken cancellationToken = default);
    Task<Para1> AddAsync(Para1 para1, CancellationToken cancellationToken = default);
    Task UpdateAsync(Para1 para1, CancellationToken cancellationToken = default);
    Task DeleteAsync(Para1 para1, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
}
