using System.ComponentModel.DataAnnotations;
using VidaPositiva.Api.Enums.Transaction;

namespace VidaPositiva.Api.DTOs.Inputs.Transaction;

public record TransactionCreationInputDto
{
    public TransactionTypeEnum Type { get; init; }
    
    [MaxLength(150)]
    public required string Title { get; init; }
    
    [MaxLength(500)]
    public string? Description { get; init; }
    
    public DateTime AccrualDate { get; init; }
    public DateTime CashDate { get; init; }
    public decimal Amount { get; init; }
    public int Installments { get; init; }
    public int? PoteId { get; init; }
    public int? CategoryId { get; init; }
    public int? SubcategoryId { get; init; }
    public string? CategoryName { get; init; }
    public string? SubcategoryName { get; init; }
};