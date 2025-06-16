using Microsoft.EntityFrameworkCore.Storage;

namespace VidaPositiva.Api.Persistence.UnitOfWork;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    void Detach<T>(T entry) where T : class;
    void BulkDetach<T>(IEnumerable<T> entries) where T : class;
    void DetachAll();
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}