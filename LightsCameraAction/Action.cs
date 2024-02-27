namespace LightsCameraAction;

public abstract class Action<TResult>
{
    public abstract Option<TResult> Run();
}

public abstract class Action<TRunArgs, TResult>
{
    public abstract Option<TResult> Run(TRunArgs args);
}

public class ActionSuccess : Action<string, string>
{
    public ActionSuccess(IRepository<string> repository, SomeService initArgs2)
    {
    }

    public override Option<string> Run(string input)
    {
        var rnd = new Random((int)DateTime.UtcNow.Ticks);
        Thread.Sleep(rnd.Next(3000));
        return Option<string>.Success($"winner winner chicken dinner; {input}");
    }
}

public class ActionFail : Action<string>
{
    public ActionFail(SecretRepository secretRepository, OtherService service)
    {
    }

    public override Option<string> Run()
    {
        var rnd = new Random((int)DateTime.UtcNow.Ticks);
        Thread.Sleep(rnd.Next(3000));
        return Option<string>.Fail();
    }
}

public class ActionComplex : Action<ActionComplex.ComplexReturnType>
{
    public override Option<ComplexReturnType> Run()
    {
        return Option<ComplexReturnType>.Success(new ComplexReturnType
        {
            Prop = "A string",
            Thing = -1
        });
    }

    public struct ComplexReturnType
    {
        public string Prop { get; init; }
        public int Thing { get; init; }
    }
}
