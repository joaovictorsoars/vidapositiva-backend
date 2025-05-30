namespace VidaPositiva.Api.ValueObjects.Common;

public readonly struct Option<T>
{
    private readonly T _value;
    private readonly bool _hasValue;

    private Option(T value)
    {
        _value = value;
        _hasValue = true;
    }
    
    public bool IsSome => _hasValue;
    public bool IsNone => !_hasValue;

    public T Value => _value;

    public static Option<T> Some(T value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value), "Não se pode criar Some com valor nulo. Use None()");
            
        return new Option<T>(value);
    }
    
    public static Option<T> None() => default;

    public static Option<T> FromNullable(T? value)
        => value != null ? Some(value) : None();

    public bool Exists(Func<T, bool> predicate)
        => _hasValue && predicate(_value);
    
    public Option<TU> Map<TU>(Func<T, TU> mapper)
        => _hasValue ? Option<TU>.Some(mapper(_value)) : Option<TU>.None();

    public T GetOrElse(T defaultValue)
        => _hasValue ? _value : defaultValue;

    public T GetOrElse(Func<T> defaultValueProvider)
        => _hasValue ? _value : defaultValueProvider();

    public T? GetOrDefault()
        => _hasValue ? _value : default;

    
    public Option<T> Filter(Func<T, bool> predicate)
        => _hasValue && predicate(_value) ? this : None();

    public TU Match<TU>(Func<T, TU> someFunc, Func<TU> noneFunc)
        => _hasValue ? someFunc(_value) : noneFunc();

    public void Match(Action<T> someAction, Action noneAction)
    {
        if (_hasValue)
            someAction(_value);
        else
            noneAction();
    }
}