using LightsCameraAction;
using Microsoft.Extensions.Logging;

Console.WriteLine("Start");

var diContainer = new DiContainer();

var logger = diContainer.GetLogger()!;

var actionExecutor = diContainer.Build<ActionExecutor>();

// simple run
var action1 = diContainer.Build<ActionSuccess>();
var res1 = actionExecutor.Execute("Action with pararm", () => action1.Run("SomeString"));

var action1Again = diContainer.Build<ActionFail>();
var res1Again = actionExecutor.Execute("call .Run inside", action1Again);
var afterMatch = res1Again.Match(
    s =>
    {
        logger.LogInformation("The following word should be false: {Res}", s);
        return s;
    },
    () =>
    {
        logger.LogInformation("The following word should be failed: {Res}", "failed");
        // Fail workflow somehow
        return "failed";
    });

// run with bind
var action2 = diContainer.Build<ActionSuccess>();
var res2 = actionExecutor.Bind("bind 1", res1, s => action2.Run(s));

var action3 = diContainer.Build<ActionFail>();
var res3 = actionExecutor.Bind("bind 2", res2, _ => action3.Run());

logger.LogInformation("The following word should be false: {Res}", res3.IsSuccess);

// specify failed action runs explicitly
var action4 = new ActionFail(new SecretRepository(), new OtherService());
var res4 = actionExecutor.Execute<string, string>(
    "Run with match",
    action4,
    result => result,
    () => "failed"
);

// control flow
var answer = actionExecutor
    .If<string>("first if", () => true)
    .OnTrue(ae =>
    {
        return ae.Execute("on true", new ActionFail(new SecretRepository(), new OtherService()))
            .Match(s => s, () => "");
    })
    .OnFalse(_ =>
    {
        var this1 = "";
        return $"{this1} this2";
    });


// provide constructor args
var complex = new ActionComplex.ComplexReturnType { Prop = "prop", Thing = 42 };
var action5 = diContainer.Build<ActionComplex>(complex);

var res5 = actionExecutor.Execute("Run with complex", action5,
    result => result.Prop == "shhh",
    () => false
);
logger.LogInformation("The following word should be true: {Res}", res5);


logger.LogInformation("{History}", actionExecutor.RenderHistory(true));
logger.LogInformation("Finished");
