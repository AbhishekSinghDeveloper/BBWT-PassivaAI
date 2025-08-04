using Autofac;
using Autofac.Builder;
using Autofac.Extras.DynamicProxy;

using Microsoft.Extensions.DependencyInjection;

using Interceptor = BBWM.Core.Autofac.LoggingInterceptor;

namespace BBWM.Core.Autofac;

public static class AutofacExtensions
{
    public static void RegisterLoggingInterceptor(this ContainerBuilder builder)
        => builder.RegisterType<Interceptor>().AsSelf();

    public static ContainerBuilder RegisterService<TService, TImplementation>(this ContainerBuilder builder,
        bool enableInterceptors = false, bool useClassInterceptors = false, ServiceLifetime? lifetime = null)
        where TService : class
        where TImplementation : class, TService
        => builder.RegisterService(typeof(TService), typeof(TImplementation), enableInterceptors, useClassInterceptors, lifetime);

    public static ContainerBuilder RegisterService<TService, TImplementation>(this ContainerBuilder builder, ServiceLifetime? lifetime)
        where TService : class
        where TImplementation : class, TService
        => builder.RegisterService(typeof(TService), typeof(TImplementation), false, false, lifetime);

    public static ContainerBuilder RegisterService<TService>(this ContainerBuilder builder,
        bool enableInterceptors = false, ServiceLifetime? lifetime = null)
        where TService : class
        => builder.RegisterService(typeof(TService), enableInterceptors, lifetime);

    private static ContainerBuilder RegisterService(this ContainerBuilder builder, Type serviceType, Type implementationType,
        bool enableInterceptors = false, bool useClassInterceptors = false, ServiceLifetime? lifetime = null)
    {
        var a = builder.RegisterType(implementationType).As(serviceType);
        ApplyLifetime(a, lifetime);

        if (enableInterceptors)
        {
            a = a.InterceptedBy(typeof(Interceptor));

            if (useClassInterceptors)
            {
                a.EnableClassInterceptors();
            }
            else
            {
                a.EnableInterfaceInterceptors();
            }
        }

        return builder;
    }

    private static ContainerBuilder RegisterService(this ContainerBuilder builder, Type serviceType, bool enableInterceptors = true,
        ServiceLifetime? lifetime = null)
    {
        var a = builder.RegisterType(serviceType);
        ApplyLifetime(a, lifetime);

        if (enableInterceptors)
        {
            a.EnableClassInterceptors().InterceptedBy(typeof(Interceptor));
        }

        return builder;
    }

    private static void ApplyLifetime(
        IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> builder,
        ServiceLifetime? lifetime)
    {
        switch (lifetime)
        {
            case ServiceLifetime.Singleton: builder.SingleInstance(); break;
            case ServiceLifetime.Transient: builder.InstancePerDependency(); break;
            case ServiceLifetime.Scoped: builder.InstancePerRequest(); break;
            default:
                break;
        }
    }
}