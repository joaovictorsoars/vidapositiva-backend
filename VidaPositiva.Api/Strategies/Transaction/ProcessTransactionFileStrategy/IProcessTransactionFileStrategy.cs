using VidaPositiva.Api.DTOs.Inputs.Transaction;

namespace VidaPositiva.Api.Strategies.Transaction.ProcessTransactionFileStrategy;

public interface IProcessTransactionFileStrategy
{
    Task<IList<TransactionCreationInputDto>?> Process(IFormFile file, string connectionId, CancellationToken cancellationToken = default);
}