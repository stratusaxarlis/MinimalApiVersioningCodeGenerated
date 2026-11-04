using MinimalApiVersioningCodeGenerated.Common.Models.Dto;

namespace MinimalApiVersioningCodeGenerated.Common.Interfaces;

public interface IEntity
{
    Guid Id { get; }
}

public interface IRepository<T> where T : class, IEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<T>> GetPagedAsync(int page, int pageSize, Func<T, bool>? filter = null, CancellationToken ct = default);
    Task<T> AddAsync(T entity, CancellationToken ct = default);
    Task<bool> UpdateAsync(T entity, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    void Seed(params T[] items);
}