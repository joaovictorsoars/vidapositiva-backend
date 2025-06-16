using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace VidaPositiva.Api.Persistence.UnitOfWork;

public class UnitOfWork(Context context) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return await context.SaveChangesAsync(cancellationToken);
    }

    public void Detach<T>(T entry)  where T : class
    {
        context.Entry(entry).State = EntityState.Detached;
    }

    public void BulkDetach<T>(IEnumerable<T> entries) where T : class
    {
        foreach (var entry in entries)
        {
            context.Entry(entry).State = EntityState.Detached;
        }
    }

    public void DetachAll()
    {
        foreach (var entry in context.ChangeTracker.Entries())
        {
            entry.State = EntityState.Detached;
        }
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken)
    {
        return await context.Database.BeginTransactionAsync(cancellationToken);
    }
}