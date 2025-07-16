using Microsoft.EntityFrameworkCore;
using VidaPositiva.Api.DTOs.Inputs.Transaction;
using VidaPositiva.Api.DTOs.Outputs;
using VidaPositiva.Api.DTOs.Outputs.Transaction;
using VidaPositiva.Api.Entities;
using VidaPositiva.Api.Extensions.String;
using VidaPositiva.Api.Persistence.Repository;
using VidaPositiva.Api.Persistence.UnitOfWork;
using VidaPositiva.Api.QueryParams.Transaction;
using VidaPositiva.Api.ValueObjects.Common;
using VidaPositiva.Api.ValueObjects.Validation;

namespace VidaPositiva.Api.Services.TransactionService;

public class TransactionService(IRepository<Transaction> repository, IUnitOfWork unitOfWork) : ITransactionService
{
    public async Task<Paginated<IList<TransactionDataOutputDto>>> List(int userId, TransactionGetAllQueryParams queryParams, CancellationToken cancellationToken = default)
    {
        var page = queryParams.Page ?? 0;
        var pageSize = queryParams.PageSize ?? 10;
        
        var startId = page * pageSize;

        var totalTransactions = await repository.Query().Where(t => t.UserId == userId).CountAsync(cancellationToken);
        var query = repository.Query()
            .Where(t => t.Id > startId && t.UserId == userId)
            .Select(t => new TransactionDataOutputDto
            {
                PublicId = t.PublicId,
                Title = t.Title,
                Description = t.Description,
                Type = t.Type,
                AccrualDate = t.AccrualDate,
                CashDate = t.CashDate,
                Amount = t.Amount,
                Installments = t.Installments
            })
            .Take(pageSize);

        query = ApplyGetAllSort(queryParams.SortBy ?? "", queryParams.SortDirection ?? "", query);
        
        var transactions = await query.ToListAsync(cancellationToken);

        return new Paginated<IList<TransactionDataOutputDto>>
        {
            Total = totalTransactions,
            Items = transactions
        };
    }

    public async Task<Either<FieldValidationError, string>> Create(TransactionCreationInputDto input, int userId, CancellationToken cancellationToken = default)
    {
        var normalizedDto = input with
        {
            Title = input.Title.NormalizeWhitespaces(),
            Description = input.Description?.NormalizeWhitespaces()
        };
        
        var transaction = Transaction.FromDto(normalizedDto, userId);

        var dbTransaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        await repository.AddAsync(transaction, cancellationToken);
        
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await dbTransaction.CommitAsync(cancellationToken);
        
        return Either<FieldValidationError, string>.FromRight(transaction.PublicId);
    }

    private IQueryable<TransactionDataOutputDto> ApplyGetAllSort(string sortBy, string sortDirection, IQueryable<TransactionDataOutputDto> query)
    {
        return sortBy.Trim() switch
        {
            "type" => sortDirection == "asc" ? query.OrderBy(t => t.Type) :  query.OrderByDescending(t => t.Type),
            "title" => sortDirection == "asc"
                ? query.OrderBy(t => t.Title)
                : query.OrderByDescending(t => t.Title),
            "accrualDate" => sortDirection == "asc"
                ? query.OrderBy(t => t.AccrualDate)
                : query.OrderByDescending(t => t.AccrualDate),
            "cashDate" => sortDirection == "asc"
                ? query.OrderBy(t => t.CashDate)
                : query.OrderByDescending(t => t.CashDate),
            "amount" => sortDirection == "asc"
                ? query.OrderBy(t => t.Amount)
                : query.OrderByDescending(t => t.Amount),
            "installments" => sortDirection == "asc"
                ? query.OrderBy(t => t.Installments)
                : query.OrderByDescending(t => t.Installments),
            _ => query.OrderByDescending(t => t.Title),
        };
    }
}