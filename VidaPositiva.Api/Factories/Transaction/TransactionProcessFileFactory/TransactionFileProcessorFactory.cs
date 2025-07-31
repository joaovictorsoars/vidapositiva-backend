using VidaPositiva.Api.Services.NotificationService;
using VidaPositiva.Api.Strategies.Transaction.ProcessTransactionFileStrategy;

namespace VidaPositiva.Api.Factories.Transaction.TransactionProcessFileFactory;

public class TransactionFileProcessorFactory(
    INotificationService notificationService) : ITransactionFileProcessorFactory
{
    public IProcessTransactionFileStrategy? Creator(string fileExtension)
    {
        return fileExtension switch
        {
            ".csv" => new ProcessTransactionCsvFileStrategy(notificationService),
            ".xls" => new ProcessTransactionExcelXlsFileStrategy(notificationService),
            _ => null
        };
    }
}