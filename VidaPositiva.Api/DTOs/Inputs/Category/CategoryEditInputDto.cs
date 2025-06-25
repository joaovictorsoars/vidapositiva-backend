namespace VidaPositiva.Api.DTOs.Inputs.Category;

public record CategoryEditInputDto
{
    public required int CategoryId { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
};