using VidaPositiva.Api.DTOs.Inputs.Transaction;
using VidaPositiva.Api.DTOs.Outputs;
using VidaPositiva.Api.DTOs.Outputs.Transaction;
using VidaPositiva.Api.QueryParams.Transaction;
using VidaPositiva.Api.ValueObjects.Common;
using VidaPositiva.Api.ValueObjects.Validation;

namespace VidaPositiva.Api.Services.TransactionService;

public interface ITransactionService
{
    Task<Paginated<IList<TransactionDataOutputDto>>> List(int userId, TransactionGetAllQueryParams queryParams, CancellationToken cancellationToken = default);
    Task<Either<FieldValidationError, string>> Create(TransactionCreationInputDto input, int userId, CancellationToken cancellationToken = default);

    Task<Either<FieldValidationError, string>> BulkCreate(TransactionCreationInputDto[] input, int userId,
        CancellationToken cancellationToken = default);

    Task<Either<ValidationError, (bool partiallyCategorized, IDictionary<string, IList<TransactionCreationInputDto>>
        transactions)>> Process(IList<IFormFile> files, string connectionId, int userId,
        CancellationToken cancellationToken = default);
}