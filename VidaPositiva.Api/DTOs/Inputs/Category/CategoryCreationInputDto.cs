namespace VidaPositiva.Api.DTOs.Inputs.Category;

public record CategoryCreationInputDto
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public string? PictureUrl { get; init; }
    public int? ParentCategoryId { get; init; }
    public int? PoteId { get; init; }
};