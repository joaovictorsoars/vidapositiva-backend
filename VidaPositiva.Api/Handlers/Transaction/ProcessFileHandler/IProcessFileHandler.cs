using System.Data;
using VidaPositiva.Api.DTOs.Inputs.Transaction;

namespace VidaPositiva.Api.Handlers.Transaction.ProcessFileHandler;

public interface IProcessFileHandler
{
    IProcessFileHandler SetNext(IProcessFileHandler handler);
        
    Task<IList<TransactionCreationInputDto>?> Handle(string fileName, string connectionId, DataTable request);
}