using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using VidaPositiva.Api.DTOs.Inputs.Category;
using VidaPositiva.Api.DTOs.Inputs.Transaction;
using VidaPositiva.Api.DTOs.Outputs;
using VidaPositiva.Api.DTOs.Outputs.Transaction;
using VidaPositiva.Api.Entities;
using VidaPositiva.Api.Extensions.String;
using VidaPositiva.Api.Factories.Transaction.TransactionProcessFileFactory;
using VidaPositiva.Api.Persistence.Repository;
using VidaPositiva.Api.Persistence.UnitOfWork;
using VidaPositiva.Api.QueryParams.Transaction;
using VidaPositiva.Api.Services.CategoryService;
using VidaPositiva.Api.ValueObjects.Common;
using VidaPositiva.Api.ValueObjects.Validation;

namespace VidaPositiva.Api.Services.TransactionService;

public class TransactionService(
    IRepository<Transaction> repository,
    ITransactionFileProcessorFactory fileProcessorFactory,
    ICategoryService categoryService,
    IUnitOfWork unitOfWork) : ITransactionService
{
    private const double MinimumSimilarity = 0.4;
    
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

    public async Task<Either<FieldValidationError, string>> BulkCreate(TransactionCreationInputDto[] input, int userId,
        CancellationToken cancellationToken = default)
    {
        var (newCategories, newSubcategories) = await CreateCategoriesAndSubCategories(input, userId, cancellationToken);

        var transactionsWithCategories = input.Select(t =>
        {
            var nonExistingCategory = t.CategoryName != null;
            var nonExistingSubcategory = t.SubcategoryName != null;

            if (nonExistingCategory || nonExistingSubcategory)
            {
                return new TransactionCreationInputDto
                {
                    Title = t.Title,
                    Description = t.Description,
                    Type = t.Type,
                    AccrualDate = t.AccrualDate,
                    CashDate = t.CashDate,
                    Installments = t.Installments,
                    Amount = t.Amount,
                    PoteId = t.PoteId,
                    CategoryId = nonExistingCategory ? newCategories!.First(c => c.Name == t.CategoryName).Id : t.CategoryId,
                    SubcategoryId = nonExistingSubcategory ? newSubcategories!.First(c => c.Name == t.SubcategoryName).Id : t.SubcategoryId
                };
            }

            return t;
        });

        var transactions = transactionsWithCategories.Select(t => Transaction.FromDto(t, userId)).ToArray();
        
        var dbTransaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
        
        await repository.AddRangeAsync(transactions, cancellationToken);
        
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await dbTransaction.CommitAsync(cancellationToken);
        
        return Either<FieldValidationError, string>.FromRight(string.Empty);
    }

    private async Task<(Category[]? categories, Category[]? subcategories)> CreateCategoriesAndSubCategories(TransactionCreationInputDto[] input, int userId,
        CancellationToken cancellationToken = default)
    {
        var nonExistingCategories = input
            .Where(t => t.CategoryName != null)
            .Select(t => new CategoryCreationByNameInputDto
            {
                CategoryName = t.CategoryName!,
                ParentCategoryId = null,
                PoteId = t.PoteId!.Value
            })
            .ToArray();

        Category[]? categories = null;

        if (nonExistingCategories is { Length: > 0 })
            categories = await categoryService.BulkCreateByName(nonExistingCategories, userId, cancellationToken);
        
        var nonExistingSubcategories = input
            .Where(t => t.SubcategoryName != null)
            .Select(t => new CategoryCreationByNameInputDto
            {
                CategoryName = t.SubcategoryName!,
                ParentCategoryId = categories?.FirstOrDefault(c => c.Name == t.CategoryName)?.Id,
                PoteId = t.PoteId!.Value
            })
            .ToArray();
        
        Category[]? subcategories = null;

        if (nonExistingSubcategories is { Length: > 0 })
            subcategories = await categoryService.BulkCreateByName(nonExistingSubcategories, userId, cancellationToken);

        return (categories, subcategories);
    }

    public async Task<Either<ValidationError, (bool partiallyCategorized, IDictionary<string, IList<TransactionCreationInputDto>> transactions)>> Process(
        IList<IFormFile> files, string connectionId, int userId, CancellationToken cancellationToken = default)
    {
        var partiallyCategorizedTransactions = false;
        var transactionsByFileName = new Dictionary<string, IList<TransactionCreationInputDto>>();
        
        foreach (var file in files)
        {
            var fileExtension = Path.GetExtension(file.FileName);
            var processor = fileProcessorFactory.Creator(fileExtension);

            if (processor is null) continue;
            
            var fileTransactions = await processor.Process(file, connectionId, cancellationToken);
            
            if ( fileTransactions is { Count: 0 }) continue;

            var (partiallyCategorized, categorizedTransactions) = await FillTransactionsWithCategories(fileTransactions!, userId, cancellationToken);
            
            if (!partiallyCategorizedTransactions && partiallyCategorized)
                partiallyCategorizedTransactions = true;
            
            transactionsByFileName.TryAdd(file.FileName, categorizedTransactions);
        }

        if (transactionsByFileName.Count == 0)
            return Either<ValidationError, (bool partiallyCategorized, IDictionary<string, IList<TransactionCreationInputDto>> transactions)>.FromLeft(
                new ValidationError
                {
                    Code = "no_valid_transactions",
                    HttpCode = 400,
                    Message = "Nenhuma transação válida foi encontrada no(s) arquivo(s)."
                });
        
        return Either<ValidationError, (bool partiallyCategorized, IDictionary<string, IList<TransactionCreationInputDto>> transactions)>.FromRight((partiallyCategorizedTransactions, transactionsByFileName));
    }

    private async Task<(bool partiallyCategorizedTransactions, IList<TransactionCreationInputDto> transactionsWithCategories)> FillTransactionsWithCategories(IList<TransactionCreationInputDto> transactions, int userId, CancellationToken cancellationToken = default)
    {
        var transactionsWithCategories = new List<TransactionCreationInputDto>();
        var partiallyCategorizedTransactions = false;
        
        foreach (var transaction in transactions)
        {
            var title = transaction.Title;
            var description = transaction.Description;

            var bestSimilarTransaction = await FuzzySearchTransaction(title, description, userId, cancellationToken);

            if (bestSimilarTransaction is null)
            {
                partiallyCategorizedTransactions = true;
                transactionsWithCategories.Add(transaction);
                continue;
            }

            TransactionCreationInputDto categorizedTransaction;

            if (bestSimilarTransaction.UserId == userId)
            {
                categorizedTransaction = transaction with
                {
                    PoteId = bestSimilarTransaction.PoteId,
                    CategoryId = bestSimilarTransaction.CategoryId,
                    SubcategoryId = bestSimilarTransaction.SubcategoryId
                };
            }
            else
            {
                categorizedTransaction = transaction with
                {
                    PoteId = bestSimilarTransaction.PoteId,
                    CategoryName = bestSimilarTransaction.Category.Name,
                    SubcategoryName = bestSimilarTransaction.Subcategory.Name
                };
            }
            
            transactionsWithCategories.Add(categorizedTransaction);
        }

        return (partiallyCategorizedTransactions, transactionsWithCategories);
    }

    private async Task<Transaction?> FuzzySearchTransaction(string title, string? description, int userId, CancellationToken cancellationToken)
    {
        return await repository
            .Query()
            .Include(t => t.Category)
            .Include(t => t.Subcategory)
            .Where(t => EF.Functions.TrigramsSimilarity(t.Title, title) > MinimumSimilarity ||
                        (t.Description != null && description != null && EF.Functions.TrigramsSimilarity(t.Description, description) > MinimumSimilarity)  ||
                        EF.Functions.ILike(t.Title, $"%{title}%") ||
                        (t.Description != null && description != null && EF.Functions.ILike(t.Description, $"%{description}%"))
            )
            .OrderByDescending(t => EF.Functions.TrigramsSimilarity(t.Title, title))
            .ThenBy(t => EF.Functions.FuzzyStringMatchLevenshtein(t.Title, title))
            .ThenByDescending(t => t.UserId == userId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private IQueryable<TransactionDataOutputDto> ApplyGetAllSort(string sortBy, string sortDirection, IQueryable<TransactionDataOutputDto> query)
    {
        Expression<Func<TransactionDataOutputDto, int>> typeSortExpression = t => t.Type == "Expense" ? 1 :
            t.Type == "Income" ? 2 :
            t.Type == "Transfer" ? 3 : 4;
        
        return sortBy.Trim() switch
        {
            "type" => sortDirection == "asc" ? query.OrderBy(typeSortExpression) :  query.OrderByDescending(typeSortExpression),
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