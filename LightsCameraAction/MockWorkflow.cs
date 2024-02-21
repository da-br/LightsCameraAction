using Microsoft.Extensions.Logging;

namespace LightsCameraAction;

public class MockWorkflow
{
    private readonly ILogger _logger;
    private readonly ActionExecutor _actionExecutor;

    public MockWorkflow(ILogger logger)
    {
        _logger = logger;
        _actionExecutor = new ActionExecutor(logger);
    }

    public void Execute(CancellationToken cancellationToken)
    {
        var action1 = new ActionSuccess("this can be done through DI", 1);
        
        var res1 = _actionExecutor.Execute(() => action1.Run("SomeString"),
            onSuccess: s => s,
            onFailure: () =>
            {
                _logger.LogInformation("This failed");
                Unsuccessful(cancellationToken);
                return "";
            });

        var answer = _actionExecutor.If<string>("first if", () => true)
            .OnTrue(ae => { return ae.Execute(new ActionFail(), s => s, () => ""); })
            .OnFalse(_ =>
            {
                var this1 = "";
                return $"{this1} this2";
            });
    }

    private void Unsuccessful(CancellationToken cancellationToken)
    {
        // Terminate workflow as failure 
    }

    private void Successful(CancellationToken cancellationToken)
    {
        // Terminate workflow as as success 
    }
}