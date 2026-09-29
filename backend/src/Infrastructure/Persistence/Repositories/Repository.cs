namespace TicketWorld.Infrastructure.Persistence.Repositories;

using Microsoft.EntityFrameworkCore;
using TicketWorld.Domain.Common;
using TicketWorld.Domain.Repositories;

public class Repository<T, TId> : IRepository<T, TId> where T : BaseEntity<TId>
{
    protected readonly ApplicationDbContext Context;
    protected readonly DbSet<T> DbSet;

    public Repository(ApplicationDbContext context)
    {
        Context = context;
        DbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FindAsync([id], cancellationToken);
    }

    public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.ToListAsync(cancellationToken);
    }

    public virtual void Add(T entity)
    {
        DbSet.Add(entity);
    }

    public virtual void Update(T entity)
    {
        DbSet.Update(entity);
    }

    public virtual void Remove(T entity)
    {
        DbSet.Remove(entity);
    }
}

public class Repository<T> : Repository<T, Guid>, IRepository<T> where T : BaseEntity<Guid>
{
    public Repository(ApplicationDbContext context) : base(context) { }
}
