namespace VidaPositiva.Api.DTOs.Inputs.User;

public record UserCreationInputDto
{
    public required string UserId { get; init; }
    public required string Name { get; init; }
    public required string Email { get; init; }
    public string? PictureUrl { get; init; }
}