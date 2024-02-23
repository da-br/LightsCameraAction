using Microsoft.Extensions.Logging;

namespace LightsCameraAction;

public class MockWorkflow
{
    private readonly ILogger _logger;
    private readonly ActionExecutor _actionExecutor;

    public MockWorkflow(ILogger logger)
    {
        _logger = logger;
    }

    public void Execute(CancellationToken cancellationToken)
    {
        var action1 = new ActionSuccess(null, null);
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