namespace VidaPositiva.Api.DTOs.Outputs.Transaction;

public record TransactionDataOutputDto
{
    public required string PublicId { get; init; }
    public required string Type { get; init; }
    public required string Title { get; init; }
    public string? Description { get; init; }
    public DateTime AccrualDate { get; init; }
    public DateTime CashDate { get; init; }
    public decimal Amount { get; init; }
    public decimal Installments { get; init; }
};