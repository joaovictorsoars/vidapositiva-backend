using System.Data;
using System.Globalization;
using VidaPositiva.Api.DTOs.Inputs.Transaction;
using VidaPositiva.Api.DTOs.Outputs.Transaction;
using VidaPositiva.Api.Enums.Transaction;
using VidaPositiva.Api.Extensions.String;
using VidaPositiva.Api.Services.NotificationService;

namespace VidaPositiva.Api.Handlers.Transaction.ProcessFileHandler.Csv;

public sealed class ProcessBradescoAccountStatementCsvFileHandler(
    INotificationService notificationService) : ProcessFileHandler
{
    private readonly string[] _columnHeaders = ["Data", "Histórico", "Docto.", "Crédito (R$)", "Débito (R$)", "Saldo (R$)"];
    private const string RowTitleStop = "Total";
    private const string TransferTitle = "TRANSF";
    private const string PreviousBalanceTitle = "SALDO ANTERIOR";
    
    public override async Task<IList<TransactionCreationInputDto>?> Handle(string fileName, string connectionId, DataTable request)
    {
        var headerRow = request.Rows[1];

        var hasAllHeaders = headerRow.ItemArray
            .Where(i => !string.IsNullOrEmpty(i as string))
            .Select(i => i!.ToString()).All(i => _columnHeaders.Contains(i));

        if (!hasAllHeaders)
        {
            return await base.Handle(fileName, connectionId, request);
        }

        var rows = request.AsEnumerable().Select((row, index) => (row, index)).ToArray();
        var totalRows = rows.Length;
        
        var transactions = new List<TransactionCreationInputDto>();

        foreach (var (row, rowIndex) in rows.Skip(2))
        {
            var isValidDate = DateTime.TryParseExact(row[0].ToString(), "dd/MM/yy", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var date);
            var title = row[1].ToString();

            if (title == RowTitleStop)
                break;
            
            var isPreviousBalance = title?.Contains(PreviousBalanceTitle) ?? false;
            
            if (!isValidDate || isPreviousBalance || string.IsNullOrEmpty(title))
                continue;

            var incomeStr = row[3].ToString();
            var expenseStr = row[4].ToString();
            
            var isValidIncome = decimal.TryParse(incomeStr, NumberStyles.Any, new CultureInfo("pt-BR"), out var income);
            var isValidExpense = decimal.TryParse(expenseStr, NumberStyles.Any, new CultureInfo("pt-BR"), out var expense);

            if (!isValidIncome && !isValidExpense)
                continue;
            
            var isTransfer = title.ToUpper().Contains(TransferTitle) && isValidExpense;
            var type = isTransfer ? TransactionTypeEnum.Transfer :
                !string.IsNullOrEmpty(incomeStr) ? TransactionTypeEnum.Income : TransactionTypeEnum.Expense;

            var amount = isValidIncome ? income : expense;
            
            var nextRow = rows.FirstOrDefault(i => i.index == rowIndex + 1).row;
            var nextRowHasDate = DateTime.TryParseExact(nextRow?[0].ToString(), "dd/MM/yy", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out _);
            var nextRowTitle = row[1].ToString();

            string? description = null;

            if (!nextRowHasDate && nextRowTitle != RowTitleStop)
                description = nextRow?[1].ToString();
            
            transactions.Add(new TransactionCreationInputDto
            {
                Type = type,
                Title = title.NormalizeWhitespaces(),
                Description = description?.NormalizeWhitespaces(),
                AccrualDate = date,
                CashDate = date,
                Amount = Math.Abs(amount),
                Installments = 1
            });
            
            var progress = (rowIndex + 1) * 100 / totalRows;
            await notificationService.NotifyProgressAsync(connectionId,
                new ProcessFileProgress(fileName, progress, "PROCESSING"));
        }
        
        return transactions;
    }
}