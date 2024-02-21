using System.Text;
using Microsoft.Extensions.Logging;

namespace LightsCameraAction;

public class ActionExecutor
{
    private readonly ILogger _logger;
    private readonly Stack<ExecuteHistory> _history = new();

    public ActionExecutor(ILogger logger)
    {
        _logger = logger;
    }

    public TOut Execute<TResult, TOut>(Action<TResult> action, Func<TResult, TOut> onSuccess, Func<TOut> onFailure)
    {
        using (_logger.BeginScope(action.GetType().Name))
        {
            _logger.LogTrace("Running {ActionType}", action.GetType().Name);

            var result = action.Run();

            _history.Push(new ExecuteHistory(action.GetType().Name, result.IsSuccess));

            return result.Match(onSuccess, onFailure);
        }
    }

    public TOut Execute<TResult, TOut>(string name, Action<TResult> action, Func<TResult, TOut> onSuccess, Func<TOut> onFailure)
    {
        using (_logger.BeginScope(action.GetType().Name))
        {
            _logger.LogTrace("Running {Name}:{ActionType}", name, action.GetType().Name);

            var result = action.Run();

            _history.Push(new ExecuteHistory(name, action.GetType().Name, result.IsSuccess));

            return result.Match(onSuccess, onFailure);
        }
    }

    public TrueExecutor<TResult> If<TResult>(string name, Func<bool> result)
    {
        using (_logger.BeginScope(nameof(If)))
        {
            _logger.LogInformation("{Name}: Running {ControlFlow}", name, nameof(If));
            if (result())
            {
                _history.Push(new ExecuteHistory(name, "If", true));
                return new TrueExecutor<TResult>(true, this, _logger);
            }

            _history.Push(new ExecuteHistory(name, "If", false));
            return new TrueExecutor<TResult>(false, this, _logger);
        }
    }

    public ForEachExecutor<T> ForEach<T, TResult>(IEnumerable<T> enumerable)
    {
        using (_logger.BeginScope(nameof(ForEach)))
        {
            return new ForEachExecutor<T>(enumerable, _logger);
        }
    }

    public string RenderHistory(bool successful)
    {
        var sb = new StringBuilder("Start");
        sb.Append('|');
        sb.Append('|');
        sb.Append('V');
        while (_history.TryPop(out var history))
        {
            if (history.Name is not null)
            {
                sb.Append(history.Name);
            }

            sb.Append(history.Type);
            sb.Append('|');
            sb.Append(history.Successful);
            sb.Append('|');
            sb.Append('V');
        }

        sb.Append(successful ? "Workflow Terminated Successfully" : "Workflow Terminated Unsuccessfully");
        return sb.ToString();
    }

    private struct ExecuteHistory
    {
        public ExecuteHistory(string type, bool success)
        {
            Name = null;
            Type = type;
            Successful = success;
        }

        public ExecuteHistory(string name, string type, bool success)
        {
            Name = name;
            Type = type;
            Successful = success;
        }

        public string? Name { get; set; }
        public string Type { get; set; }
        public bool Successful { get; }
    }
}

public class TrueExecutor<TResult>
{
    private readonly bool _runBranch;
    private readonly ActionExecutor _executor;
    private readonly ILogger _logger;

    public TrueExecutor(bool runBranch, ActionExecutor executor, ILogger logger)
    {
        _runBranch = runBranch;
        _executor = executor;
        _logger = logger;
    }

    public FalseExecutor<TResult> OnTrue(System.Func<ActionExecutor, TResult> f)
    {
        TResult result = default;
        if (_runBranch)
        {
            _logger.LogTrace("Running If True Branch");
            result = f(_executor);
        }

        return new FalseExecutor<TResult>(_executor, result, _logger);
    }
}

public class FalseExecutor<TResult>
{
    private readonly TResult? _trueResult;
    private readonly ILogger _logger;
    private readonly ActionExecutor _executor;

    public FalseExecutor(ActionExecutor executor, TResult? trueResult, ILogger logger)
    {
        _trueResult = trueResult;
        _logger = logger;
        _executor = executor;
    }

    public TResult OnFalse(System.Func<ActionExecutor, TResult> f)
    {
        var result = _trueResult;
        if (result is null)
        {
            _logger.LogTrace("Running If True Branch");
            result = f(_executor);
        }

        return result;
    }
}

public class ForEachExecutor<T>
{
    private readonly IEnumerable<T> _enumerable;
    private readonly ILogger _logger;
    private int _index;
    private readonly int _maxIndex;

    public ForEachExecutor(IEnumerable<T> enumerable, ILogger logger)
    {
        _enumerable = enumerable;
        _logger = logger;
        _index = 0;
        _maxIndex = _enumerable.Count();
    }

    public IEnumerable<TOut> Map<TOut>(Func<T, TOut> f, int startIndex, int endIndex)
    {
        foreach (var e in _enumerable)
        {
            if (_index < startIndex)
            {
                _index++;
                continue;
            }

            if (_index >= endIndex)
            {
                _index++;
                break;
            }

            _logger.LogTrace("On iteration {I}/{MaxIteration}", _index, _maxIndex);
            yield return f(e);
            _index++;
        }
    }

    public IEnumerable<TOut> Map<TOut>(Func<T, TOut> f)
    {
        foreach (var e in _enumerable)
        {
            _logger.LogTrace("On iteration {I}/{MaxIteration}", _index, _maxIndex);
            yield return f(e);
            _index++;
        }
    }
}