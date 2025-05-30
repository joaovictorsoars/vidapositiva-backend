using VidaPositiva.Api.ValueObjects.Common;

namespace VidaPositiva.UnitTests.ValueObjects.Common;

public class EitherTests
{
    public static IList<object[]> FoldCases =>
        new List<object[]>
        {
            new object[] { Either<int, string>.FromLeft(10), 11 },
            new object[] { Either<int, string>.FromRight("123"), 123 },
            new object[] { Either<int, string>.FromRight("11"), 11 },
            new object[] { Either<int, string>.FromLeft(42), 43 }
        };
    
    public static IList<object[]> MapCases =>
        new List<object[]>
        {
            new object[] { Either<int, string>.FromLeft(10), 10 },
            new object[] { Either<int, string>.FromRight("123"), 246 },
            new object[] { Either<int, string>.FromRight("11"), 22 },
            new object[] { Either<int, string>.FromLeft(42), 42 }
        };
    
    public static IList<object[]> LeftMapCases =>
        new List<object[]>
        {
            new object[] { Either<string, int>.FromRight(10), 10 },
            new object[] { Either<string, int>.FromLeft("123"), 246 },
            new object[] { Either<string, int>.FromLeft("11"), 22 },
            new object[] { Either<string, int>.FromRight(42), 42 }
        };

    
    [Fact]
    public void FromLeft_IntegerLeftValue_ReturnsLeftIntegerResult()
    {
        var result = Either<int, string>.FromLeft(0);

        Assert.True(result.IsLeft, "result.IsLeft");
        Assert.False(result.IsRight, "result.IsRight");
        Assert.Equal(0, result.Left);
        Assert.Null(result.Right);
    }

    [Fact]
    public void FromRight_IntegerRightValue_ReturnsRightIntegerResult()
    {
        var result = Either<int, string>.FromRight("bar");
        
        Assert.False(result.IsLeft, "result.IsLeft");
        Assert.True(result.IsRight,  "result.IsRight");
        Assert.Equal("bar", result.Right);
        Assert.Equal(0,  result.Left);
    }

    [Theory, MemberData(nameof(FoldCases))]
    public void Fold_WithLeftOrRight_AppliesCorrectFunctionAndReturnsExpected(Either<int, string> sut, int expectedResult)
    {
        int Succ(int v) => v + 1;
        
        Assert.Equal(expectedResult, sut.Fold(Succ, int.Parse));
    }

    [Theory, MemberData(nameof(MapCases))]
    public void Map_WithParsableStringRight_ReturnsTransformedRight(Either<int, string> sut, int expectedResult)
    {
        var result = sut.Map(r => int.Parse(r) * 2);
        var compare = result.IsRight ? result.Right : result.Left;
        
        Assert.Equal(expectedResult, compare);
    }
    
    [Theory, MemberData(nameof(LeftMapCases))]
    public void LeftMap_WithParsableStringRight_ReturnsTransformedLeft(Either<string, int> sut, int expectedResult)
    {
        var result = sut.MapLeft(l => int.Parse(l) * 2);
        var compare = result.IsLeft ? result.Left : result.Right;
        
        Assert.Equal(expectedResult, compare);
    }

    [Fact]
    public void GetOrElse_WhenEitherHasLeftOrRight_ReturnsRightOrFallbackValue()
    {
        var resultLeft = Either<string, string>.FromLeft("bar");
        var resultRight = Either<string, string>.FromRight("bar");

        var leftGetOrElse = resultLeft.GetOrElse(() => "foo");
        var rightGetOrElse = resultRight.GetOrElse(() => "foo");
        
        Assert.Equal("foo", leftGetOrElse);
        Assert.Equal("bar", rightGetOrElse);
    }
}