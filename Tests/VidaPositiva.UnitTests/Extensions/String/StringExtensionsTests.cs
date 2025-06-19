using VidaPositiva.Api.Extensions.String;

namespace VidaPositiva.UnitTests.Extensions.String;

public class StringExtensionsTests
{
    [Fact]
    public void NormalizeWhitespaces_WithAStringWithWhitespaces_ReturnsStringWithNormalizedWhitespaces()
    {
        var expectedResult = "Eu sou uma string com espaços em branco sobressalentes.";
        var input = "Eu sou     uma string    com espaços em branco              sobressalentes.               ";

        var result = input.NormalizeWhitespaces();

        Assert.IsType<string>(result);
        Assert.Equal(expectedResult, result);
    }
}