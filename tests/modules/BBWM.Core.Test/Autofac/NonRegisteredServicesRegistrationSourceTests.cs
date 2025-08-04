using Autofac.Core;

using BBWM.Core.Autofac;

using System.Reflection;

using Xunit;

namespace BBWM.Core.Test.Autofac;

public class NonRegisteredServicesRegistrationSourceTests
{
    [Fact]
    public void RegistrationsFor_Should_Return_Empty_On_Wrong_Service()
    {
        // Arrange
        var service = new InvalidService();
        var nonRegisteredSource = new NonRegisteredServicesRegistrationSource(default);

        // Act
        var registrations = nonRegisteredSource.RegistrationsFor(service, default);

        // Assert
        Assert.Empty(registrations);
    }

    [Fact]
    public void RegistrationsFor_Should_Return_Empty_On_Already_Registered()
    {
        // Arrange
        var (service, nonRegisteredSource) = CreateSut<IMatchingNameMultipleDirectInterfacesService>();
        Func<Service, IEnumerable<ServiceRegistration>> registrationAccessor = s => new[] { new ServiceRegistration() };

        // Act
        var registrations = nonRegisteredSource.RegistrationsFor(service, registrationAccessor);

        // Assert
        Assert.Empty(registrations);
    }

    [Theory]
    [InlineData(typeof(IWrongTypeService))]
    [InlineData(typeof(WrongServiceName))]
    public void RegistrationsFor_Should_Return_Empty_On_Invalid_Type(Type serviceType)
    {
        // Arrange
        var (service, nonRegisteredSource) = CreateSut(serviceType, default);

        // Act
        var registrations = nonRegisteredSource.RegistrationsFor(service, EmptyRegistrations);

        // Assert
        Assert.Empty(registrations);
    }

    [Theory]
    [InlineData(typeof(INoImplementorsService))]
    [InlineData(typeof(IMultipleImplementorsService))]
    public void RegistrationsFor_Should_Return_Empty_On_Missing_Implementor(Type serviceType)
    {
        // Arrange
        var (service, nonRegisteredSource) = CreateSut(serviceType, ServicesAssemblies);

        // Act
        var registrations = nonRegisteredSource.RegistrationsFor(service, EmptyRegistrations);

        // Assert
        Assert.Empty(registrations);
    }

    [Fact]
    public void RegistrationsFor_Should_Return_Empty_On_Indirect_Implementation()
    {
        // Arrange
        var (service, nonRegisteredSource) = CreateSut<IIndirectImplementationService>(ServicesAssemblies);

        // Act
        var registrations = nonRegisteredSource.RegistrationsFor(service, EmptyRegistrations);

        // Assert
        Assert.Empty(registrations);
    }

    [Fact]
    public void RegistrationsFor_Should_Return_Empty_On_Invalid_Name_Match_And_Multiple_Direct_Interfaces()
    {
        // Arrange
        var (service, nonRegisteredSource) = CreateSut<IInvalidNameMultipleDirectInterfacesService>(ServicesAssemblies);

        // Act
        var registrations = nonRegisteredSource.RegistrationsFor(service, EmptyRegistrations);

        // Assert
        Assert.Empty(registrations);
    }

    [Theory]
    [InlineData(typeof(IMatchingNameMultipleDirectInterfacesService))]
    [InlineData(typeof(IUnmatchedNameSingleDirectInterfaceService))]
    public void RegistrationsFor_Should_Return_Registration(Type serviceType)
    {
        // Arrange
        var (service, nonRegisteredSource) = CreateSut(serviceType, ServicesAssemblies);

        // Act
        var registrations = nonRegisteredSource.RegistrationsFor(service, EmptyRegistrations);

        // Assert
        Assert.Single(registrations);
    }

    private IEnumerable<Assembly> ServicesAssemblies => new[] { typeof(IMatchingNameMultipleDirectInterfacesService).Assembly };

    private IEnumerable<ServiceRegistration> EmptyRegistrations(Service service) => Enumerable.Empty<ServiceRegistration>();

    private static (Service, NonRegisteredServicesRegistrationSource) CreateSut(
        Type serviceType, IEnumerable<Assembly> scanImplementors)
    {
        var service = new TypedService(serviceType);
        return (service, new NonRegisteredServicesRegistrationSource(scanImplementors));
    }

    private static (Service, NonRegisteredServicesRegistrationSource) CreateSut<TService>(
        IEnumerable<Assembly> scanImplementors = default)
        => CreateSut(typeof(TService), scanImplementors);

    // Helper service
    private class InvalidService : Service
    {
        public override string Description => "Invalid service";
    }
}
