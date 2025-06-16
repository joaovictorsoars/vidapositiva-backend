using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VidaPositiva.Api.Entities;

[Table("users")]
public class User
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    [Column("user_id")]
    public int Id { get; set; }
    
    [Column("email")]
    [MaxLength(255)]
    public required string Email { get; set; }

    [Column("name")]
    [MaxLength(255)]
    public required string Name { get; set; }

    [Column("google_id")] [MaxLength(255)] public string? GoogleId { get; set; }

    [Column("public_id")]
    [MaxLength(255)]
    public required string PublicId { get; set; }
    
    [Column("picture_url")]
    [MaxLength(255)]
    public string? PictureUrl { get; set; }
    
    [Column("last_login")]
    public required DateTime LastLogin { get; set; }
    
    [Column("created_at")]
    public required DateTime CreatedAt { get; set; }
    
    [Column("updated_at")]
    public required DateTime UpdatedAt { get; set; }
}