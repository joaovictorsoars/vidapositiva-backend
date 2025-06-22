namespace VidaPositiva.Api.ValueObjects.Validation;

public record FieldValidationError :  ValidationError
{
    public FieldError[] FieldErrors { get; init; } = [];
    
    public FieldValidationError()
    {
        Code = "invalid_fields";
        Message = "Alguns campos retornaram inválidos";
    }

    public override object GetActionResultPayload()
    {
        return new
        {
            Code,
            Message,
            FieldErrors
        };
    }
}