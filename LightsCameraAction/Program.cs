using LightsCameraAction;
using Microsoft.Extensions.Logging;

Console.WriteLine("Start");

var diContainer = new DiContainer();

var logger = diContainer.GetLogger()!;

var actionExecutor = diContainer.Build<ActionExecutor>();

var action1 = diContainer.Build<ActionSuccess>();

var res1 = actionExecutor.Execute("Action with pararm", () => action1.Run("SomeString"),
    onSuccess: s => s,
    onFailure: () =>
    {
        logger.LogInformation("This failed");
        return "";
    });

var answer = actionExecutor.If<string>("first if", () => true)
    .OnTrue(ae => { return ae.Execute(new ActionFail(null, null), s => s, () => ""); })
    .OnFalse(_ =>
    {
        var this1 = "";
        return $"{this1} this2";
    });

logger.LogInformation("{History}", actionExecutor.RenderHistory(true));

logger.LogInformation("Finished");