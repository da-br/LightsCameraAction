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
    private readonly SecretRepository _repository;
    private readonly ComplexReturnType _complex;

    public ActionComplex(SecretRepository repository, ComplexReturnType complex)
    {
        _repository = repository;
        _complex = complex;
    }

    public override Option<ComplexReturnType> Run()
    {
        var rnd = new Random((int)DateTime.UtcNow.Ticks);
        Thread.Sleep(rnd.Next(3000));
        var result = new ComplexReturnType
        {
            Prop = _repository.Get(),
            Thing = rnd.Next()
        };

        return Option<ComplexReturnType>.Success(result);
    }

    public struct ComplexReturnType
    {
        public string Prop { get; init; }
        public int Thing { get; init; }
    }
}
