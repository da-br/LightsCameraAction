using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LightsCameraAction;

public class DiContainer
{
    private readonly ServiceProvider _serviceProvider;

    public DiContainer()
    {
        // done in startup
        _serviceProvider = new ServiceCollection()
            .AddTransient<IRepository<string>, ResourceRepository>()
            .AddTransient<SecretRepository>()
            .AddTransient<OtherService>()
            .AddTransient<SomeService>()
            .AddLogging(builder => { builder.AddConsole(); })
            .BuildServiceProvider();
    }

    public ILogger? GetLogger()
    {
        return _serviceProvider.GetService<ILogger<Program>>(); //hack
    }

    public TAction Build<TAction>()
    {
        return ActivatorUtilities.CreateInstance<TAction>(_serviceProvider);
    }

    public TAction Build<TAction>(params object[] args)
    {
        return ActivatorUtilities.CreateInstance<TAction>(_serviceProvider, args);
    }
}
