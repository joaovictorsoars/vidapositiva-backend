namespace VidaPositiva.Api.DTOs.Inputs.Pote;

public record PoteCreationInputDto
{
    public required string Name { get; init; }
    public string? PictureUrl { get; init; }
};