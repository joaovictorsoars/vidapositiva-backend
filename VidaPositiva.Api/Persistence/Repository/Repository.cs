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

    public void AddRange(IEnumerable<T> entities)
    {
        context.Set<T>().AddRange(entities);
    }

    public void Update(T entity)
    {
        context.Set<T>().Update(entity);
    }

    public void Remove(T entity)
    {
        context.Set<T>().Remove(entity);
    }
}