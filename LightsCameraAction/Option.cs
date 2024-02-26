using System.Diagnostics.CodeAnalysis;

namespace LightsCameraAction;

public class Option<TResult>
{
    private readonly TResult? _result;
    public bool IsSuccess => _result is not null;

    public static Option<TResult> Success(TResult result) => new(result);
    public static Option<TResult> Fail() => new();

    private Option([NotNull] TResult result)
    {
        _result = result;
    }

    private Option()
    {
        _result = default;
    }

    public TOut Match<TOut>(Func<TResult, TOut> onSuccess, Func<TOut> onFail)
    {
        if (_result is not null)
        {
            return onSuccess(_result);
        }

        return onFail();
    }

    public Option<TOut> Bind<TOut>(Func<TResult, Option<TOut>> onSuccess)
    {
        return Match(onSuccess, Option<TOut>.Fail);
    }
}