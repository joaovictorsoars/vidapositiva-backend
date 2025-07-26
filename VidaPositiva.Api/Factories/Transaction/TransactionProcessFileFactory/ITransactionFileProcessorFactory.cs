using VidaPositiva.Api.Strategies.Transaction.ProcessTransactionFileStrategy;

namespace VidaPositiva.Api.Factories.Transaction.TransactionProcessFileFactory;

public interface ITransactionFileProcessorFactory
{
    IProcessTransactionFileStrategy? Creator(string fileName);
}