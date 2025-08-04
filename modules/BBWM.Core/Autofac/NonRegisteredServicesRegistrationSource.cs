using Autofac;
using Autofac.Builder;
using Autofac.Core;

using System.Reflection;
using System.Text.RegularExpressions;

namespace BBWM.Core.Autofac;

/// <summary>
/// Automatically registers all non-registered services with Autofac based on a convention that targets all
/// interfaces matching the pattern <b>I[service-name]Service</b> and meet one of the following conditions:
/// <list type="bullet">
/// <item>
/// The interface has one implementation only matching the pattern <b>[service-name]Service</b>, e.g.,
/// <code>OrderService : IOrderService</code>
/// Note that in this case is doesn't matter how many interfaces are implemented by <b>[service-name]Service</b>.
/// </item>
/// <item>
/// The interface has one implementation only with a 1:1 relationship, i.e., the implementation only implements
/// one interface and that is <b>I[service-name]Service</b>, e.g.,
/// <code>MyDummyService : ISmartService</code>
/// Note that in this case there's no need for the implementation to be suffixed by <b>Service</b>.
/// </item>
/// </list>
/// </summary>
/// <remarks>
/// <para>
/// This approach removes the need of manually registering services when they follow the mentioned convention, which
/// should be the case most of times. Also, it should save the developer some time, increasing this way the productivity
/// on other tasks.
/// </para>
/// <para>
/// Another advantage of this approach is that service registration occurs on demand, i.e, when requested by Autofac
/// during activation.
/// </para>
/// <para>
/// Some examples of services that will be registered automatically with Autofac are:
/// <list type="bullet">
/// <item><code>OrderService : IOrderService</code></item>
/// <item><code>OrderService : IOrderService, ISecondInterface</code></item>
/// <item><code>SecondOrderService : IOrderService</code></item>
/// <item><code>Orders : IOrderService</code></item>
/// </list>
/// </para>
/// <para><see href="https://wiki.bbconsult.co.uk/display/BLUEB/Code+Automation#CodeAutomation-AutomaticServicesRegistration">See Wiki page</see></para>
/// </remarks>
public class NonRegisteredServicesRegistrationSource : IRegistrationSource
{
    private static readonly Regex interfaceNameRegex = new(@"^I(.+?)Service$");
    private static readonly Regex serviceNameRegex = new(@"^(.+?)Service$");
    private readonly HashSet<Type> allTypes;

    public bool IsAdapterForIndividualComponents => false;

    /// <summary>
    /// Initializes a new instance of the class <see cref="NonRegisteredServicesRegistrationSource"/>
    /// </summary>
    /// <param name="scanImplementors">Assemblies for implementation lookup</param>
    public NonRegisteredServicesRegistrationSource(IEnumerable<Assembly> scanImplementors)
    {
        scanImplementors ??= Array.Empty<Assembly>();
        allTypes = new HashSet<Type>(
            scanImplementors
                .SelectMany(assembly => assembly.GetTypes())
                .Where(t => t.IsPublic && !t.IsAbstract && t.IsClass && !t.IsGenericType));
    }

    public IEnumerable<IComponentRegistration> RegistrationsFor(
        Service service, Func<Service, IEnumerable<ServiceRegistration>> registrationAccessor)
    {
        if (service is not IServiceWithType serviceWithType ||
            registrationAccessor(new TypedService(serviceWithType.ServiceType)).Any())
        {
            return Empty;
        }

        var interfaceType = serviceWithType.ServiceType;
        var interfaceNameMatch = interfaceNameRegex.Match(interfaceType.Name);

        if (!interfaceType.IsInterface || !interfaceNameMatch.Success)
        {
            return Empty;
        }

        var implementorType = FindImplementorService(interfaceType);
        if (implementorType == default)
        {
            return Empty;
        }

        var directInterfaces = GetDirectInterfaces(implementorType);
        if (!directInterfaces.Contains(interfaceType))
        {
            return Empty;
        }

        var serviceNameMatch = serviceNameRegex.Match(implementorType.Name);
        if ((serviceNameMatch.Success &&
             serviceNameMatch.Groups[1].Value == interfaceNameMatch.Groups[1].Value) ||
            directInterfaces.Length == 1)
        {
            return Register(implementorType, interfaceType);
        }

        return Empty;
    }

    private Type FindImplementorService(Type interfaceType)
    {
        var implementors = allTypes.Where(interfaceType.IsAssignableFrom).ToArray();
        return implementors?.Length == 1
            ? implementors[0]
            : default;
    }

    private static IEnumerable<IComponentRegistration> Register(Type concrete, Type @interface)
        => new[] { RegistrationBuilder.ForType(concrete).As(@interface).CreateRegistration() };

    private static IEnumerable<IComponentRegistration> Empty => Enumerable.Empty<IComponentRegistration>();

    private static Type[] GetDirectInterfaces(Type baseType)
    {
        var implementedInterfaces = baseType.GetInterfaces();

        return implementedInterfaces
            .Except(implementedInterfaces.SelectMany(type => type.GetInterfaces()))
            .ToArray();
    }
}
