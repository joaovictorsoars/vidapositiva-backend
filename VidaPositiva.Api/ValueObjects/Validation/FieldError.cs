namespace VidaPositiva.Api.ValueObjects.Validation;

public record FieldError
{
    public string Field { get; init; }
    public string Message { get; init; }
}