namespace LightsCameraAction;

public class Option<TResult>
{
    private readonly TResult? _result;
    public bool IsSuccess => _result is not null;

    public static Option<TResult> Success(TResult result) => new(result);
    public static Option<TResult> Fail() => new();

    private Option(TResult result)
    {
        _result = result;
    }

    private Option()
    {
        _result = default;
    }

    public TOut Match<TOut>(Func<TResult, TOut> success, Func<TOut> fail)
    {
        if (_result is not null)
        {
            return success(_result);
        }

        return fail();
    }
}