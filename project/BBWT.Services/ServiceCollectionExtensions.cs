using Autofac;

using BBWM.Core.Autofac;

namespace BBWT.Services;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Use to manually register you project services.
    /// (by default services are supposed to be placed into BBWT.Services project).
    /// <para>For example, <c>builder.RegisterService&lt;IMyFeatureService, MyFeatureService&gt;()</c></para>
    /// </summary>
    /// <remarks>
    /// IMPORTANT! Note that the application's core, by default, is set up to register services automatically!
    /// It gets rid a developer from registering services manually, which means in most cases you
    /// don't need to do any action.
    /// <see href="https://wiki.bbconsult.co.uk/display/BLUEB/Code+Automation#CodeAutomation-AutomaticServicesRegistration">
    /// See more details on Wiki page.</see>.
    /// Also see <see cref="NonRegisteredServicesRegistrationSource"/>
    /// </remarks>
    /// <param name="builder">Used for services registrations.</param>
    public static void RegisterProjectServices(this ContainerBuilder builder)
    {
    }
}
