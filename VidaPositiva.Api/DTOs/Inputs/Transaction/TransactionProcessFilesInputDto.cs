namespace VidaPositiva.Api.DTOs.Inputs.Transaction;

public record TransactionProcessFilesInputDto(IList<IFormFile> Files, string ConnectionId);