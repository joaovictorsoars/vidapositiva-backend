using System.Text;
using ExcelDataReader;
using VidaPositiva.Api.DTOs.Inputs.Transaction;
using VidaPositiva.Api.Handlers.Transaction.ProcessFileHandler.Csv;
using VidaPositiva.Api.Services.NotificationService;

namespace VidaPositiva.Api.Strategies.Transaction.ProcessTransactionFileStrategy;

public class ProcessTransactionCsvFileStrategy(
    INotificationService notificationService) : IProcessTransactionFileStrategy
{
    public async Task<IList<TransactionCreationInputDto>?> Process(IFormFile file, string connectionId, CancellationToken cancellationToken = default)
    {
        using var fileStream = new MemoryStream();
        await file.CopyToAsync(fileStream, cancellationToken);
        
        ExcelReaderConfiguration readerConfig = new()
        {
            FallbackEncoding = Encoding.GetEncoding("ISO-8859-1"),
            AutodetectSeparators = [',', ';'],
        };
        
        using var reader = ExcelReaderFactory.CreateCsvReader(fileStream, readerConfig);
        var dataset = reader.AsDataSet();
        var tables = dataset.Tables;
        var table = tables[0];

        var bradescoAccountStatementFileHandler =
            new ProcessBradescoAccountStatementCsvFileHandler(notificationService);
        var bradescoCardBillFileHandler = new ProcessBradescoCardBillCsvHandler(notificationService);

        bradescoAccountStatementFileHandler
            .SetNext(bradescoCardBillFileHandler);

        return await bradescoAccountStatementFileHandler.Handle(file.FileName, connectionId, table);
    }
}