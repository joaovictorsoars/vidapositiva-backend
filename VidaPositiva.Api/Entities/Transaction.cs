using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VidaPositiva.Api.DTOs.Inputs.Transaction;

namespace VidaPositiva.Api.Entities;

[Table("transactions")]
public class Transaction
{
    public static Transaction FromDto(TransactionCreationInputDto transactionDto, int userId)
    {
        var publicId = Guid.NewGuid().ToString();
        
        return new Transaction
        {
            Type = transactionDto.Type.ToString(),
            PublicId = publicId,
            Title = transactionDto.Title,
            Description = transactionDto.Description,
            AccrualDate = transactionDto.AccrualDate,
            CashDate = transactionDto.CashDate,
            Amount = transactionDto.Amount,
            Installments = transactionDto.Installments,
            PoteId = transactionDto.PoteId,
            CategoryId = transactionDto.CategoryId,
            SubcategoryId = transactionDto.SubCategoryId,
            UserId = userId
        };
    }
    
    [Key]
    [Column("transaction_id")] 
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)] 
    public int Id { get; set; }
        
    [Column("public_id")]
    [MaxLength(100)]
    public required string PublicId { get; set; }
    
    [Column("type")]
    [MaxLength(50)]
    public required string Type { get; set; }
    
    [Column("title")]
    [MaxLength(150)]
    public required string Title { get; set; }
        
    [Column("description")]
    [MaxLength(500)]
    public string? Description { get; set; }
        
    [Column("accrual_date")]
    public DateTime AccrualDate { get; set; }
    [Column("cash_date")]
    public DateTime CashDate { get; set; }
        
    [Column("amount")]
    public decimal Amount { get; set; }
    
    [Column("installments")]
    public int Installments { get; set; }
        
    [Column("user_id")]
    public int UserId { get; set;}
        
    [Column("pote_id")]
    public int PoteId {get; set; }
        
    [Column("category_id")]
    public int CategoryId { get; set; }
        
    [Column("subcategory_id")]
    public int SubcategoryId { get; set; }

    [ForeignKey(nameof(UserId))] public User User { get; set; } = null!;
    [ForeignKey(nameof(PoteId))] public Pote Pote { get; set; } = null!;
    [ForeignKey(nameof(CategoryId))] public Category Category { get; set; } = null!;
    [ForeignKey(nameof(SubcategoryId))] public Category Subcategory { get; set; } = null!;
}