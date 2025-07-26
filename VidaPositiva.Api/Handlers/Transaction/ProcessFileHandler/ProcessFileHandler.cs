using System.Data;
using VidaPositiva.Api.DTOs.Inputs.Transaction;
using VidaPositiva.Api.Services.NotificationService;

namespace VidaPositiva.Api.Handlers.Transaction.ProcessFileHandler;

public abstract class ProcessFileHandler : IProcessFileHandler
{
    private IProcessFileHandler? _nextHandler;

    public IProcessFileHandler SetNext(IProcessFileHandler handler)
    {
        _nextHandler = handler;
        return handler;
    }

    public virtual async Task<IList<TransactionCreationInputDto>?> Handle(string fileName, string connectionId, DataTable request)
    {
        return await _nextHandler?.Handle(fileName, connectionId, request)!;
    }
}