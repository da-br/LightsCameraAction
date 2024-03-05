using Microsoft.Extensions.Logging;

namespace LightsCameraAction;

public class SuperActionExecutor
{
    private readonly DiContainer _diContainer;
    private readonly ILogger _logger;

    public SuperActionExecutor(DiContainer diContainer, ILogger<ActionExecutor> logger)
    {
        _diContainer = diContainer;
        _logger = logger;
    }


    public Option<TResult> Execute<TAction, TResult>(string name, params object[] ctorArgs) where TAction : Action<TResult>
    {
        using (_logger.BeginScope(typeof(TAction).Name))
        {
            _logger.LogTrace("Running {Name}:{ActionType}", name, typeof(TAction).Name);

            var action = _diContainer.Build<TAction>(ctorArgs);

            var result = action.Run();

            return result;
        }
    }

    public Option<TResult> Execute<TAction, TRunArgs, TResult>(string name, TRunArgs runArgs, params object[] ctorArgs) where TAction : Action<TRunArgs, TResult>
    {
        using (_logger.BeginScope(typeof(TAction).Name))
        {
            _logger.LogTrace("Running {Name}:{ActionType}", name, typeof(TAction).Name);

            var action = _diContainer.Build<TAction>(ctorArgs);

            var result = action.Run(runArgs);

            return result;
        }
    }

    public Option<TOut> Bind<TAction, TResult, TOut>(string name, Option<TResult> previousResult, Func<TAction, Func<TResult, Option<TOut>>> curryBinder) where TAction : Action<TResult>
    {
        using (_logger.BeginScope(name))
        {
            _logger.LogTrace("Running {Name}", name);

            var action = _diContainer.Build<TAction>();

            var binder = curryBinder(action);
            var result = previousResult.Bind(binder);

            return result;
        }
    }
}
