namespace VidaPositiva.Api.ValueObjects.Common;

public class Either<TLeft, TRight>(TLeft? left, TRight? right, bool isLeft, bool isRight)
{
    public TLeft? Left => left;
    public TRight? Right => right;
    
    public bool IsLeft => isLeft;
    public bool IsRight => isRight;

    public static Either<TLeft, TRight> FromLeft(TLeft left)
    {
        return new Either<TLeft, TRight>(left, default, true, false);
    }

    public static Either<TLeft, TRight> FromRight(TRight right)
    {
        return new Either<TLeft, TRight>(default, right, false, true);
    }

    public T Fold<T>(Func<TLeft, T> transformLeft, Func<TRight, T> transformRight)
    {
        return IsLeft ? transformLeft(Left!) : transformRight(Right!);
    }

    public Either<TLeft, TNewRight> Map<TNewRight>(Func<TRight, TNewRight> transformRight)
    {
        return IsRight
            ? new Either<TLeft, TNewRight>(default, transformRight(Right!), false, true)
            : new Either<TLeft, TNewRight>(Left, default, true, false);
    }
    
    public Either<TNewLeft, TRight> MapLeft<TNewLeft>(Func<TLeft, TNewLeft> transformLeft)
    {
        return IsLeft
            ? new Either<TNewLeft, TRight>(transformLeft(Left!), default, true, false)
            : new Either<TNewLeft, TRight>(default, Right, false, true);
    }

    public TRight GetOrElse(Func<TRight> fallbackRight)
    {
        return IsLeft ? fallbackRight() : Right!;
    }
}