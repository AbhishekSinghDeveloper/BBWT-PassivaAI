using BBWM.Core.Data;

using Castle.DynamicProxy.Internal;

namespace BBWM.DbDoc;

public class DbContextProvider : IDbContextProvider
{
    private readonly List<Type> _types = new List<Type>();


    public IDbContext[] GetDbContexts(IServiceProvider serviceProvider) =>
        _types
        .Where(x => x is not null)
        .Select(x => (IDbContext)serviceProvider.GetService(x))
        .Where(x => x is not null)
        .ToArray();

    public void Register(Type type)
    {
        if (type is null) return;

        if (type.GetAllInterfaces().All(i => i != typeof(IDbContext)))
            throw new ApplicationException($"Type '{type.Name}' should implement IDbContext.");

        if (_types.Contains(type)) return;

        _types.Add(type);
    }
}
