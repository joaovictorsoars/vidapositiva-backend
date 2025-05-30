using VidaPositiva.Api.ValueObjects.Common;

namespace VidaPositiva.UnitTests.ValueObjects.Common;

public class OptionTests
{
    [Fact]
    public void Some_WithValidValue_ReturnsOption()
    {
        var option = Option<int>.Some(42);
        
        Assert.True(option.IsSome);
        Assert.False(option.IsNone);
        Assert.Equal(42, option.Value);
    }

    [Fact]
    public void Some_WithNullValue_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => Option<string>.Some(null!));
    }

    [Fact]
    public void None_WithoutValue_ReturnsEmptyOption()
    {
        var option = Option<int>.None();
        
        Assert.False(option.IsSome);
        Assert.True(option.IsNone);
    }

    [Fact]
    public void Value_WhenNone_ReturnsDefaultValue()
    {
        var option = Option<int>.None();
        
        var value = option.Value;
        
        Assert.Equal(0, value);
    }

    [Fact]
    public void FromNullable_WithNonNullReferenceType_ReturnsSome()
    {
        string value = "test";
        
        var option = Option<string>.FromNullable(value);
        
        Assert.True(option.IsSome);
        Assert.Equal("test", option.Value);
    }

    [Fact]
    public void FromNullable_WithNullReferenceType_ReturnsNone()
    {
        string value = null!;
        
        var option = Option<string>.FromNullable(value);
        
        Assert.True(option.IsNone);
    }

    [Fact]
    public void Map_WithSome_ReturnsApplyFunction()
    {
        var option = Option<int>.Some(42);
        
        var result = option.Map(x => x * 2);
        
        Assert.True(result.IsSome);
        Assert.Equal(84, result.Value);
    }

    [Fact]
    public void Map_WithNone_ReturnsNone()
    {
        var option = Option<int>.None();
        
        var result = option.Map(x => x * 2);
        
        Assert.True(result.IsNone);
    }

    [Fact]
    public void GetOrElse_WithSome_ReturnsValue()
    {
        var option = Option<int>.Some(42);
        
        var result = option.GetOrElse(0);
        
        Assert.Equal(42, result);
    }

    [Fact]
    public void GetOrElse_WithNone_ReturnsDefaultValue()
    {
        var option = Option<int>.None();
        
        var result = option.GetOrElse(0);
        
        Assert.Equal(0, result);
    }

    [Fact]
    public void GetOrElse_WithNoneAndFunc_ReturnsFuncResult()
    {
        var option = Option<int>.None();
        int called = 0;
        
        var result = option.GetOrElse(() =>
        {
            called++;
            return 99;
        });
        
        Assert.Equal(99, result);
        Assert.Equal(1, called);
    }

    [Fact]
    public void GetOrElse_WithSomeAndFunc_ReturnsNotCallFunc()
    {
        var option = Option<int>.Some(42);
        int called = 0;
        
        var result = option.GetOrElse(() =>
        {
            called++;
            return 99;
        });
        
        Assert.Equal(42, result);
        Assert.Equal(0, called);
    }

    [Fact]
    public void GetOrDefault_WithSome_ReturnsValue()
    {
        var option = Option<int>.Some(42);
        
        var result = option.GetOrDefault();
        
        Assert.Equal(42, result);
    }

    [Fact]
    public void GetOrDefault_WithNone_ReturnsDefault()
    {
        var option = Option<int>.None();
        
        var result = option.GetOrDefault();
        
        Assert.Equal(0, result);
    }

    [Fact]
    public void Exists_WithSomeMatchingPredicate_ReturnsTrue()
    {
        var option = Option<int>.Some(42);
        
        var result = option.Exists(x => x % 2 == 0);
        
        Assert.True(result);
    }

    [Fact]
    public void Exists_WithSomeNotMatchingPredicate_ReturnsFalse()
    {
        var option = Option<int>.Some(43);
        
        var result = option.Exists(x => x % 2 == 0);
        
        Assert.False(result);
    }

    [Fact]
    public void Exists_WithNone_ReturnsFalse()
    {
        var option = Option<int>.None();
        
        var result = option.Exists(_ => true);
        
        Assert.False(result);
    }

    [Fact]
    public void Filter_WithSomeMatchingPredicate_ReturnsSome()
    {
        var option = Option<int>.Some(42);
        
        var result = option.Filter(x => x % 2 == 0);
        
        Assert.True(result.IsSome);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void Filter_WithSomeNotMatchingPredicate_ReturnsNone()
    {
        var option = Option<int>.Some(43);
        
        var result = option.Filter(x => x % 2 == 0);
        
        Assert.True(result.IsNone);
    }

    [Fact]
    public void Filter_WithNone_ReturnsNone()
    {
        var option = Option<int>.None();
        
        var result = option.Filter(x => x % 2 == 0);
        
        Assert.True(result.IsNone);
    }

    [Fact]
    public void Match_WithSome_ReturnsSomeFuncValue()
    {
        var option = Option<int>.Some(42);
        
        var result = option.Match(
            someFunc: x => $"Value: {x}",
            noneFunc: () => "No value"
        );
        
        Assert.Equal("Value: 42", result);
    }

    [Fact]
    public void Match_WithNone_ReturnsNoneFuncValue()
    {
        var option = Option<int>.None();
        
        var result = option.Match(
            someFunc: x => $"Value: {x}",
            noneFunc: () => "No value"
        );
        
        Assert.Equal("No value", result);
    }

    [Fact]
    public void Match_WithSomeAndActions_ReturnsSomeAction()
    {
        var option = Option<int>.Some(42);
        var someCalled = false;
        var noneCalled = false;
        
        option.Match(
            someAction: _ => someCalled = true,
            noneAction: () => noneCalled = true
        );
        
        Assert.True(someCalled);
        Assert.False(noneCalled);
    }

    [Fact]
    public void Match_WithNoneAndActions_ReturnsNoneAction()
    {
        var option = Option<int>.None();
        var someCalled = false;
        var noneCalled = false;
        
        option.Match(
            someAction: _ => someCalled = true,
            noneAction: () => noneCalled = true
        );
        
        Assert.False(someCalled);
        Assert.True(noneCalled);
    }

    [Fact]
    public void MultipleOperations_ReturnsChainCorrectlyValue()
    {
        var option = Option<int>.Some(42);
        
        var result = option
            .Filter(x => x > 20)
            .Map(x => x * 2)
            .Filter(x => x < 100)
            .GetOrElse(-1);
        
        Assert.Equal(84, result);
    }

    [Fact]
    public void MultipleOperations_WithFilteringOut_ReturnsGetOrElseDefault()
    {
        var option = Option<int>.Some(42);
        
        var result = option
            .Filter(x => x > 50)
            .Map(x => x * 2)
            .GetOrElse(-1);
        
        Assert.Equal(-1, result);
    }
}