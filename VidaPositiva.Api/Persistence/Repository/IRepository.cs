namespace VidaPositiva.Api.Persistence.Repository;

public interface IRepository<T> where T : class
{
    IQueryable<T>  Query();
    void Add(T entity);
    void AddRange(IEnumerable<T> entities);
    void Update(T entity);
    void Remove(T entity);
}