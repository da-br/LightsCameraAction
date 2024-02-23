using System.ComponentModel;
using System.Text;
using Microsoft.Extensions.Logging;

namespace LightsCameraAction;

public class ActionExecutor
{
    private readonly ILogger _logger;
    private readonly Queue<ExecuteHistory> _history = new();

    public ActionExecutor(ILogger<ActionExecutor> logger)
    {
        _logger = logger;
    }

    public TOut Execute<TResult, TOut>(string name, Func<Option<TResult>> action, Func<TResult, TOut> onSuccess, Func<TOut> onFailure)
    {
        using (_logger.BeginScope(action.GetType().Name))
        {
            _logger.LogTrace("Running {Name}:{ActionType}", name, action.GetType().Name);

            var result = action();

            _history.Enqueue(new ExecuteHistory(name, action.GetType().Name, result.IsSuccess));

            return result.Match(onSuccess, onFailure);
        }
    }

    public TOut Execute<TResult, TOut>(Action<TResult> action, Func<TResult, TOut> onSuccess, Func<TOut> onFailure)
    {
        using (_logger.BeginScope(action.GetType().Name))
        {
            _logger.LogTrace("Running {ActionType}", action.GetType().Name);

            var result = action.Run();

            _history.Enqueue(new ExecuteHistory(action.GetType().Name, result.IsSuccess));

            return result.Match(onSuccess, onFailure);
        }
    }

    public TOut Execute<TResult, TOut>(string name, Action<TResult> action, Func<TResult, TOut> onSuccess, Func<TOut> onFailure)
    {
        using (_logger.BeginScope(action.GetType().Name))
        {
            _logger.LogTrace("Running {Name}:{ActionType}", name, action.GetType().Name);

            var result = action.Run();

            _history.Enqueue(new ExecuteHistory(name, action.GetType().Name, result.IsSuccess));

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
                _history.Enqueue(new ExecuteHistory(name, "If", true));
                return new TrueExecutor<TResult>(true, this, _logger);
            }

            _history.Enqueue(new ExecuteHistory(name, "If", false));
            return new TrueExecutor<TResult>(false, this, _logger);
        }
    }

    public ForEachExecutor<T, TResult> ForEach<T, TResult>(IEnumerable<T> enumerable, Action<T, TResult> action)
    {
        using (_logger.BeginScope(nameof(ForEach)))
        {
            return new ForEachExecutor<T, TResult>(enumerable, action, _logger);
        }
    }

    public string RenderHistory(bool successful)
    {
        var sb = new StringBuilder("Start");
        sb.AppendLine();
        sb.Append('|');
        sb.AppendLine();
        sb.Append('|');
        sb.AppendLine();
        sb.Append('V');
        sb.AppendLine();
        while (_history.TryDequeue(out var history))
        {
            if (history.Name is not null)
            {
                sb.Append(history.Name);
                sb.AppendLine();
            }

            sb.Append(history.Type);
            sb.AppendLine();
            sb.Append('|');
            sb.AppendLine();
            sb.Append(history.Successful);
            sb.AppendLine();
            sb.Append('|');
            sb.AppendLine();
            sb.Append('V');
            sb.AppendLine();
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

public class ForEachExecutor<TInput, TResult>
{
    private readonly IEnumerable<TInput> _enumerable;
    private readonly Action<TInput, TResult> _action;
    private readonly ILogger _logger;
    private int _index;
    private readonly int _maxIndex;

    public ForEachExecutor(IEnumerable<TInput> enumerable, Action<TInput, TResult> action, ILogger logger)
    {
        _enumerable = enumerable;
        _action = action;
        _logger = logger;
        _index = 0;
        _maxIndex = _enumerable.Count();
    }

    public IEnumerable<TOut> Map<TOut>(Func<Option<TResult>, TOut> f)
    {
        foreach (var e in _enumerable)
        {
            _logger.LogTrace("On iteration {I}/{MaxIteration}", _index, _maxIndex);
            yield return f(_action.Run(e));
            _index++;
        }
    }

    public IEnumerable<Option<TResult>> Map()
    {
        foreach (var e in _enumerable)
        {
            _logger.LogTrace("On iteration {I}/{MaxIteration}", _index, _maxIndex);
            yield return _action.Run(e);
            _index++;
        }
    }
}