namespace VidaPositiva.Api.DTOs.Inputs.Category;

public record CategoryCreationByNameInputDto
{
    public required string CategoryName { get; init; }
    public int? ParentCategoryId { get; init; }
    public int PoteId { get; init; }
};