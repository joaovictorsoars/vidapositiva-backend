namespace VidaPositiva.Api.DTOs.Outputs;

public record Paginated<T>
{
    public int Total { get; init; }
    public required T Items { get; init; }
};