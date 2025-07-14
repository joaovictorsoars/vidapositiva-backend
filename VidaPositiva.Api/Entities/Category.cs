using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VidaPositiva.Api.DTOs.Inputs.Category;

namespace VidaPositiva.Api.Entities;

[Table("categories")]
public class Category
{
    public static Category FromDto(CategoryCreationInputDto categoryDto, int userId)
    {
        return new Category
        {
            Name = categoryDto.Name,
            Description = categoryDto.Description,
            ParentId = categoryDto.ParentCategoryId,
            PoteId = categoryDto.PoteId,
            UserId = userId
        };
    }
    
    [Key]
    [Column("category_id")]
    public int Id { get; set; }
    
    [Column("name")]
    [MaxLength(150)]
    public required string Name { get; set; }
    
    [Column("description")]
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [Column("parent_id")]
    public int? ParentId { get; set; }
    
    [Column("is_favorite")]
    public bool IsFavorite { get; set; }
    
    [Column("pote_id")]
    public int? PoteId { get; set; }
    
    [Column("user_id")]
    public int? UserId { get; set; }

    [ForeignKey(nameof(PoteId))] public Pote? Pote { get; set; }
    
    [ForeignKey(nameof(ParentId))] public Category? ParentCategory { get; set; }
}