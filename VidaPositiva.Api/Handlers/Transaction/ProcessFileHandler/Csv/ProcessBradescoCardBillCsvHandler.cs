using System.Data;
using System.Globalization;
using VidaPositiva.Api.DTOs.Inputs.Transaction;
using VidaPositiva.Api.Enums.Transaction;
using VidaPositiva.Api.Extensions.String;
using VidaPositiva.Api.Services.NotificationService;

namespace VidaPositiva.Api.Handlers.Transaction.ProcessFileHandler.Csv;

public class ProcessBradescoCardBillCsvHandler(INotificationService notificationService) : ProcessFileHandler
{
    private readonly string[] _columnHeaders = ["Data", "Histórico", "Valor(US$)", "Valor(R$)"];
    private readonly string[] _termsToIgnore = ["PAGTO", "SALDO ANTERIOR"];
    private const string DateHeaderRowFormat = "dd/MM/yyyy HH:mm:ss";
    private const string DateRowFormat = "dd/MM/yyyy";
    
    public override async Task<IList<TransactionCreationInputDto>?> Handle(string fileName, string connectionId,
        DataTable request)
    {
        var dateHeaderRow = request.Rows[0][0].ToString()?.Replace("Data:", "").Trim();

        if (!DateTime.TryParseExact(dateHeaderRow, DateHeaderRowFormat, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var headerDate))
        {
            return await base.Handle(fileName, connectionId, request);
        }
        
        var headerRow = request.Rows[5];
        var headerRowItems = headerRow.ItemArray
            .Where(i => !string.IsNullOrEmpty(i as string))
            .Select(i => i!.ToString())
            .ToArray();
        
        var hasAllHeaders = _columnHeaders.All(h => headerRowItems.Contains(h));

        if (!hasAllHeaders)
        {
            return await base.Handle(fileName, connectionId, request);
        }
        
        var rows = request.AsEnumerable().Select((row, index) => (row, index)).ToArray();
        var totalRows = rows.Length;
        
        var transactions = new List<TransactionCreationInputDto>();

        foreach (var (row, rowIndex) in rows.Skip(6))
        {
            var rowDayMonth = row[0].ToString();
            var rowDateStr = $"{rowDayMonth}/{headerDate.Year}";

            if (!DateTime.TryParseExact(rowDateStr, DateRowFormat, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var date))
                continue;

            var title = row[1].ToString();

            if (title is null || _termsToIgnore.Any(term => title.ToUpper().Contains(term)))
                continue;
            
            var rowAmount = row[3].ToString()?.Replace(".", "").Replace(",", ".");

            if (!decimal.TryParse(rowAmount, NumberStyles.Any, CultureInfo.InvariantCulture, out var amount))
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
        }
        
        return transactions;
    }
}