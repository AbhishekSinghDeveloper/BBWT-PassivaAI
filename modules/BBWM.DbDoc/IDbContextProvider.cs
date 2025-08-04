using BBWM.Core.Data;

namespace BBWM.DbDoc;

public interface IDbContextProvider
{
    void Register(Type type);
    IDbContext[] GetDbContexts(IServiceProvider serviceProvider);
}
