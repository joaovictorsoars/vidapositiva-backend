using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VidaPositiva.Api.DTOs.Inputs.Pote;

namespace VidaPositiva.Api.Entities;

[Table("potes")]
public class Pote
{
    public static Pote FromDto(PoteCreationInputDto poteDto)
    {
        return new Pote
        {
            Name = poteDto.Name,
            PictureUrl = poteDto.PictureUrl
        };
    }
    
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    [Column("pote_id")]
    public int Id { get; set; }
    
    [Column("name")]
    [MaxLength(150)]
    public required string Name { get; set; }
    
    [Column("picture_url")]
    [MaxLength(255)]
    public string? PictureUrl { get; set; }
}