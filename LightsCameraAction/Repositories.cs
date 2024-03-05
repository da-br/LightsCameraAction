namespace LightsCameraAction;

public interface IRepository<in TEntity>
{
    bool TryAdd(TEntity entity);
}

public class ResourceRepository : IRepository<string>
{
    public bool TryAdd(string entity)
    {
        return true;
    }
}

public class SecretRepository : IRepository<string>
{
    public bool TryAdd(string entity)
    {
        return true;
    }

    public string Get()
    {
        return "shhh";
    }
}

public interface IService
{
}

public class SomeService : IService
{
}

public class OtherService : IService
{
}
