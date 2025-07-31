using System.Text;
using ExcelDataReader;
using VidaPositiva.Api.DTOs.Inputs.Transaction;
using VidaPositiva.Api.Handlers.Transaction.ProcessFileHandler.Xls;
using VidaPositiva.Api.Services.NotificationService;

namespace VidaPositiva.Api.Strategies.Transaction.ProcessTransactionFileStrategy;

public class ProcessTransactionExcelXlsFileStrategy(
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
        
        using var reader = ExcelReaderFactory.CreateReader(fileStream, readerConfig);
        var dataset = reader.AsDataSet();
        var tables = dataset.Tables;
        var table = tables[0];

        var itauCardBillHandler = new ProcessItauCardBillXlsHandler(notificationService);
        
        return await itauCardBillHandler.Handle(file.FileName, connectionId, table);
    }
}