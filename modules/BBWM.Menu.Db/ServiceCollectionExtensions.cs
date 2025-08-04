using Autofac;

using BBWM.Core.Autofac;

namespace BBWM.Menu.Db;

public static class ServiceCollectionExtensions
{
    public static void RegisterMenuDbProviders(this ContainerBuilder builder)
    {
        builder.RegisterService<IMenuDataProvider, DbMenuDataProvider>();
        builder.RegisterService<IFooterMenuDataProvider, DbFooterMenuDataProvider>();
    }
}
