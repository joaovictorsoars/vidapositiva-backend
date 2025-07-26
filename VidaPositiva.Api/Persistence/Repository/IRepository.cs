using System.Linq.Expressions;

namespace VidaPositiva.Api.Persistence.Repository;

public interface IRepository<T> where T : class
{
    IQueryable<T>  Query();
    void Add(T entity);
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    void AddRange(IEnumerable<T> entities);
    Task AddRangeAsync(IEnumerable<T> entities,  CancellationToken cancellationToken = default);
    void Update(T entity);
    void Remove(T entity);
    Task RemoveByCondition(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
}