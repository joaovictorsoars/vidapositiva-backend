using System.Data;
using System.Globalization;
using VidaPositiva.Api.DTOs.Inputs.Transaction;
using VidaPositiva.Api.DTOs.Outputs.Transaction;
using VidaPositiva.Api.Enums.Transaction;
using VidaPositiva.Api.Extensions.String;
using VidaPositiva.Api.Services.NotificationService;

namespace VidaPositiva.Api.Handlers.Transaction.ProcessFileHandler.Xls;

public class ProcessItauCardBillXlsHandler(INotificationService notificationService) : ProcessFileHandler
{
    private readonly string[] _columnHeaders = ["DATA", "LANÇAMENTO", "VALOR"];
    private const string ItauHeaderTerm = "LOGOTIPO ITAÚ";
    private const string DateRowFormat = "dd/MM/yyyy";
    private const string StopTerm = "TOTAL";
    public override async Task<IList<TransactionCreationInputDto>?> Handle(string fileName, string connectionId,
        DataTable request)
    {
        var logoCell = request.Rows[0][0].ToString()?.ToUpper();
        
        if (logoCell !=  ItauHeaderTerm)
            return await base.Handle(fileName, connectionId, request);
        
        var headerRow = request.Rows[26];
        
        var headerRowItems = headerRow.ItemArray
            .Where(i => !string.IsNullOrEmpty(i as string))
            .Select(i => i!.ToString()?.ToUpper())
            .ToArray();
        
        var hasAllHeaders = _columnHeaders.All(h => headerRowItems.Contains(h));
        
        if (!hasAllHeaders)
            return await base.Handle(fileName, connectionId, request);
        
        var rows = request.AsEnumerable().Select((row, index) => (row, index)).ToArray();
        var totalRows = rows.Length;
        
        var transactions = new List<TransactionCreationInputDto>();

        foreach (var (row, rowIndex) in rows.Skip(26))
        {
            int progress;
            var dateStr = row[0].ToString();

            if (dateStr?.ToUpper().Contains(StopTerm, StringComparison.CurrentCultureIgnoreCase) == true)
            {
                progress = 90;
                await notificationService.NotifyProgressAsync(connectionId,
                    new ProcessFileProgress(fileName, progress, "PROCESSING"));
                break;
            }

            if (!DateTime.TryParseExact(dateStr, DateRowFormat, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var date))
                continue;

            var title = row[1].ToString();

            if (title is null)
                continue;
            
            var amountStr = row[3].ToString()?.Replace(".", "").Replace(",", ".");
            
            if (!decimal.TryParse(amountStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var amount))
                continue;
            
            var type = amount < 0 ? TransactionTypeEnum.Income : TransactionTypeEnum.Expense;
            
            transactions.Add(new TransactionCreationInputDto
            {
                Type = type,
                Title = title.NormalizeWhitespaces(),
                Description = null,
                AccrualDate = date,
                CashDate = date,
                Amount = Math.Abs(amount),
                Installments = 1
            });
            
            progress = (rowIndex + 1) * 100 / totalRows;
            await notificationService.NotifyProgressAsync(connectionId,
                new ProcessFileProgress(fileName, progress, "PROCESSING"));
        }

        return transactions;
    }
}