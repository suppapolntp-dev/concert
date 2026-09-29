namespace TicketWorld.Domain.Repositories;

using TicketWorld.Domain.Common;

/// <summary>
/// Generic repository interface for entities with a typed identifier.
/// </summary>
public interface IRepository<T, TId> where T : BaseEntity<TId>
{
    Task<T?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);
    void Add(T entity);
    void Update(T entity);
    void Remove(T entity);
}

/// <summary>
/// Default generic repository interface for entities with a Guid identifier.
/// </summary>
public interface IRepository<T> : IRepository<T, Guid> where T : BaseEntity<Guid>
{
}
