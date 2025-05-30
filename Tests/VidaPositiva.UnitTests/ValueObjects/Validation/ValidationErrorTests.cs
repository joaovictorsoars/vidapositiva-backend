using VidaPositiva.Api.ValueObjects.Validation;
using Microsoft.AspNetCore.Mvc;

namespace VidaPositiva.UnitTests.ValueObjects.Validation;

public class ValidationErrorTests
{
    public static IList<object[]> GetActionResultPayloadCases => [
        [
            new ValidationError { 
                Code = "validation_error_code", 
                Message = "Validation error" 
            }, 
            new { 
                Code =  "validation_error_code", 
                Message = "Validation error" 
            }
        ],
        [
            new ValidationError { 
                Code = "second_validation_error_code",
                Message = "Validation error" 
            }, 
            new { 
                Code =  "second_validation_error_code",
                Message = "Validation error" 
            }
        ]
    ];
    
    public static IList<object[]> AsActionResultCases => [
        [
            new ValidationError { 
                Code = "validation_error_code",
                HttpCode = 400,
                Message = "Bad request" 
            }, 
            new BadRequestObjectResult(new
            {
                Code =  "validation_error_code",
                Message =  "Bad request"
            })
        ],
    ];
    
    [Fact]
    public void Constructor_WhenInitializedWithValidParameters_ReturnsValidationErrorObject()
    {
        var validationError = new ValidationError
        {
            Code = "validation_error_code",
            HttpCode = 400,
            Message = "Validation error"
        };

        Assert.NotNull(validationError);
        Assert.IsType<ValidationError>(validationError);
    }

    [Theory, MemberData(nameof(GetActionResultPayloadCases))]
    public void GetActionResultPayload_WithValidationError_ReturnsEquivalentPayloadObjects(ValidationError input, object expectedResult)
    {
        var payload = input.GetActionResultPayload();
    
        Assert.Equivalent(expectedResult, payload);
    }

    [Theory, MemberData(nameof(AsActionResultCases))]
    public void AsActionResult_WithValidationError_ReturnsEqualStatusCodeAndEquivalentActionResultObject(ValidationError input, ObjectResult expectedResult)
    {
        var expectedStatusCode = expectedResult.StatusCode;
        var expectedValue = expectedResult.Value;
        
        var inputObjectResult = input.AsActionResult() as ObjectResult;
        var inputStatusCode = inputObjectResult!.StatusCode;
        var inputValue = inputObjectResult.Value;
        
        Assert.Equal(expectedStatusCode, inputStatusCode);
        Assert.Equivalent(expectedValue, inputValue, true);
    }
}