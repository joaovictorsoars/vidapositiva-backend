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
}