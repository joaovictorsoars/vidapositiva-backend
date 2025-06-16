namespace VidaPositiva.Api.DTOs.Outputs.User;

public record UserInfoOutputDto
{
    public required string Name { get; init; }
    public required string Email { get; init; }
    public string? PictureUrl { get; init; }
    public required string PublicId { get; init; }
};