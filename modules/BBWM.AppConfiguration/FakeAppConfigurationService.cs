namespace BBWM.AppConfiguration;

public class FakeAppConfigurationService : IAppConfigurationService
{
    public Task Delete(string name, CancellationToken cancellationToken = default) => null;

    public Task<IEnumerable<Parameter>> GetAll(CancellationToken cancellationToken = default)
        => Task.FromResult((IEnumerable<Parameter>)new List<Parameter>());

    public Task<Parameter> GetByName(string name, CancellationToken cancellationToken = default) => null;

    public Task Put(Parameter parameter, CancellationToken cancellationToken = default) => null;
}
