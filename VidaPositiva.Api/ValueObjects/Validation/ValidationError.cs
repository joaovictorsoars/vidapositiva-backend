using Microsoft.AspNetCore.Mvc;

namespace VidaPositiva.Api.ValueObjects.Validation;

public sealed record ValidationError
{
    public string Code { get; init; } = null!;
    public int HttpCode { get; init; } = 400;
    public string Message { get; init; } = null!;

    public object GetActionResultPayload()
    {
        return new
        {
            Code,
            Message
        };
    }

    public IActionResult AsActionResult()
    {
        var payload = GetActionResultPayload();

        return HttpCode switch
        {
            401 => new UnauthorizedObjectResult(payload),
            400 => new BadRequestObjectResult(payload),
            404 => new NotFoundObjectResult(payload),
            409 => new ConflictObjectResult(payload),
            _ => new ObjectResult(payload)
            {
                StatusCode = HttpCode
            }
        };
    }
}
