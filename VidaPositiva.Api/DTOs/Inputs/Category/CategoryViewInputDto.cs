namespace VidaPositiva.Api.DTOs.Inputs.Category;

public record CategoryViewInputDto
{
    public int Id { get; init; }
    public int? ParentCategoryId { get; init; }
    public int? PoteId { get; init; }
}