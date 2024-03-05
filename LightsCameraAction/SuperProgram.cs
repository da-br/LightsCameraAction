using Microsoft.Extensions.Logging;

namespace LightsCameraAction;

public class SuperProgram
{
    public static void Main()
    {
        var diContainer = new DiContainer();

        var logger = diContainer.GetLogger()!;

        var actionExecutor = diContainer.Build<SuperActionExecutor>(diContainer);

        var input = new ActionComplex.ComplexReturnType
        {
            Prop = "prop",
            Thing = 42
        };
        var result = actionExecutor.Execute<ActionComplex, ActionComplex.ComplexReturnType>("complex", input);
        logger.LogInformation(result.Match(s => s.Thing.ToString("000"), () => "failed"));

    }
}
