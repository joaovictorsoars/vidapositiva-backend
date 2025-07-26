using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace VidaPositiva.Api.Persistence.Repository;

public class Repository<T>(Context context)  : IRepository<T> where T : class
{
    public IQueryable<T> Query()
    {
        return context.Set<T>().AsQueryable();
    }

    public void Add(T entity)
    {
        context.Set<T>().Add(entity);
    }

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await context.Set<T>().AddAsync(entity, cancellationToken);
    }

    public void AddRange(IEnumerable<T> entities)
    {
        context.Set<T>().AddRange(entities);
    }
    
    public async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        await context.Set<T>().AddRangeAsync(entities, cancellationToken);
    }


    public void Update(T entity)
    {
        context.Set<T>().Update(entity);
    }

    public void Remove(T entity)
    {
        context.Set<T>().Remove(entity);
    }

    public async Task RemoveByCondition(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var condition = await context.Set<T>().Where(predicate).ToListAsync(cancellationToken);
        context.Set<T>().RemoveRange(condition);
    }
}