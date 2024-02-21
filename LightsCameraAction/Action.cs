namespace LightsCameraAction;

public abstract class Action<TResult>
{
    public abstract Option<TResult> Run();
}

public class ActionSuccess : Action<string>
{
    public override Option<string> Run()
    {
        return Option<string>.Success("winner winner chicken dinner");
    }
}

public class ActionFail : Action<string>
{
    public override Option<string> Run()
    {
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