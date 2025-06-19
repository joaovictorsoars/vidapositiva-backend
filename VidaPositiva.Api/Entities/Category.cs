using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VidaPositiva.Api.DTOs.Inputs.Category;

namespace VidaPositiva.Api.Entities;

[Table("categories")]
public class Category
{
    public static Category FromDto(CategoryCreationInputDto poteDto, int userId)
    {
        return new Category
        {
            Name = poteDto.Name,
            Description = poteDto.Description,
            PictureUrl = poteDto.PictureUrl,
            ParentId = poteDto.ParentCategoryId,
            PoteId = poteDto.PoteId,
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
    
    [Column("picture_url")]
    [MaxLength(255)]
    public string? PictureUrl { get; set; }
    
    [Column("parent_id")]
    public int? ParentId { get; set; }
    
    [Column("pote_id")]
    public int? PoteId { get; set; }
    
    [Column("user_id")]
    public int? UserId { get; set; }

    [ForeignKey(nameof(PoteId))] public Pote? Pote { get; set; }
}