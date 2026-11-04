using MinimalApiVersioningCodeGenerated.Common.Interfaces;
using MinimalApiVersioningCodeGenerated.Common.Models.Dto;

namespace MinimalApiVersioningCodeGenerated.Feature.TestFeature.Persistence.TestRepository;


public sealed class InMemoryRepository<T>: IRepository<T> where T : class, IEntity
{
    private readonly Dictionary<Guid, T> _store = [];
    private readonly Lock _lock = new();

    public Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        lock (_lock)
            return Task.FromResult(_store.TryGetValue(id, out var e) ? e : null);
    }

    public Task<PagedResult<T>> GetPagedAsync(int page, int pageSize, Func<T, bool>? filter = null, CancellationToken ct = default)
    {
        lock (_lock)
        {
            var query = _store.Values.AsEnumerable();
            if (filter is not null)
                query = query.Where(filter);

            var total = query.Count();
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            // Order by Id for deterministic paging between requests
            var items = query.OrderBy(e => e.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Task.FromResult(new PagedResult<T>(items, page, pageSize, total));
        }
    }

    public Task<T> AddAsync(T entity, CancellationToken ct = default)
    {
        lock (_lock)
            _store[entity.Id] = entity;
        return Task.FromResult(entity);
    }

    public Task<bool> UpdateAsync(T entity, CancellationToken ct = default)
    {
        lock (_lock)
        {
            if (!_store.ContainsKey(entity.Id))
                return Task.FromResult(false);
            _store[entity.Id] = entity;
            return Task.FromResult(true);
        }
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        lock (_lock)
            return Task.FromResult(_store.Remove(id));
    }

    public void Seed(params T[] items)
    {
        lock (_lock)
            foreach (var item in items)
                _store[item.Id] = item;
    }
}
