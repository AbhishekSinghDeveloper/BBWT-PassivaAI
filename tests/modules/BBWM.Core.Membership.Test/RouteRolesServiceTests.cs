using AutoMapper;
using BBWM.Core.Membership.DTO;
using BBWM.Core.Membership.Interfaces;
using BBWM.Core.Membership.Model;
using BBWM.Core.Membership.Services;
using BBWM.Core.Test;
using BBWM.Core.Web.Filters;
using Bogus;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using System.Reflection;
using Xunit;

namespace BBWM.Core.Membership.Test;

public class RouteRolesServiceTests
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;
    private Mock<IActionDescriptorCollectionProvider> _action;

    public RouteRolesServiceTests()
    {
        _context = InMemoryDataContext.GetContext(Guid.NewGuid().ToString());
        _mapper = AutoMapperConfig.CreateMapper();
    }

    private RouteRolesService GetService<TContext>(TContext context, Mock<IUserService> userService = default)
    {
        if (context is not DataContext ctx)
        {
            throw new InvalidCastException();
        }

        var userManager = ServicesFactory.GetUserManager(ctx);

        userService ??= new Mock<IUserService>(MockBehavior.Loose);
        var routes = new Mock<IEnumerable<IRouteRolesModule>>(MockBehavior.Strict);
        var mock = new List<IRouteRolesModule>();
        var pageInfoDtoList = new List<PageInfoDTO>()
            {
                new PageInfoDTO(Routes.Users, AggregatedRoles.Authenticated),
                new PageInfoDTO(Routes.Users, Core.Roles.SystemAdminRole),
            };
        mock.ForEach(p => p.GetRouteRoles().AddRange(pageInfoDtoList));

        var contollerType = typeof(HomeController);
        var actionMethod = contollerType.GetMethod("Index");

        _action = new Mock<IActionDescriptorCollectionProvider>();
        _action.Setup(p => p.ActionDescriptors).Returns(new ActionDescriptorCollection(
            new ActionDescriptor[]
            {
                   new ControllerActionDescriptor
                   {
                       DisplayName = "Microsoft.AspNetCore.Mvc.Routing.AttributeRoutingTest+HomeController.Index",
                       ControllerTypeInfo = contollerType.GetTypeInfo(),
                       ActionName = "Index",
                       MethodInfo = actionMethod,
                       RouteValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                       {
                           { "controller", "Home" },
                           { "action", "Index" },
                       },
                       AttributeRouteInfo = new AttributeRouteInfo() { Template = "{controller}/{action}" },
                       BoundProperties = new List<ParameterDescriptor>(),
                       Parameters = new List<ParameterDescriptor>()
                       {
                          new ParameterDescriptor
                          {
                              Name = "value",
                              ParameterType = typeof(int),
                              BindingInfo = new Microsoft.AspNetCore.Mvc.ModelBinding.BindingInfo(),
                          },
                       },
                       EndpointMetadata = new List<object>() { new AuthorizeAttribute { } },
                       ActionConstraints = new List<IActionConstraintMetadata>() { },
                       FilterDescriptors = new List<FilterDescriptor>() { },
                   },
          }, 0));

        var controllerActionDesc = new ControllerActionDescriptor();
        controllerActionDesc.ActionName = "Index";
        controllerActionDesc.ControllerName = "Home";
        controllerActionDesc.DisplayName = "Microsoft.AspNetCore.Mvc.Routing.AttributeRoutingTest+HomeController.Index";
        controllerActionDesc.ControllerTypeInfo = contollerType.GetTypeInfo();
        controllerActionDesc.MethodInfo = actionMethod;
        controllerActionDesc.ActionConstraints = new List<IActionConstraintMetadata>() { };
        controllerActionDesc.EndpointMetadata = new List<object>() {
                new ReadWriteAuthorizeAttribute { ReadRoles = Roles.SuperAdminRole },
            };
        controllerActionDesc.FilterDescriptors = new List<FilterDescriptor>() {
                new FilterDescriptor(new AuthorizeFilter(), 0),
            };
        controllerActionDesc.RouteValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
              {
                  { "controller", "Home" },
                  { "action", "Index" },
              };
        controllerActionDesc.AttributeRouteInfo = new AttributeRouteInfo();
        controllerActionDesc.AttributeRouteInfo.Template = "{controller}/{action}";

        var service = CreateService(controllerActionDesc);

        routes.Setup(s => s.GetEnumerator()).Returns(mock.GetEnumerator());

        return new RouteRolesService(
            _action.Object,
            userService.Object,
            routes.Object,
            userManager);
    }

    private IServiceProvider CreateService(params ActionDescriptor[] actions)
    {
        var serviceProvider = new ServiceProvider();

        var collection = new ActionDescriptorCollection(actions, version: 0);

        _action = new Mock<IActionDescriptorCollectionProvider>();
        _action.Setup(p => p.ActionDescriptors).Returns(collection);

        var routes = new Mock<IOptions<RouteOptions>>();
        routes.SetupGet(p => p.Value).Returns(new RouteOptions());

        var inlineConstraintResolver = new DefaultInlineConstraintResolver(routes.Object, serviceProvider);

        var services = new ServiceCollection().AddSingleton<IInlineConstraintResolver>(inlineConstraintResolver);

        services.AddRouting();
        services.AddOptions();
        services.AddLogging();

        return services.AddSingleton(_action.Object).BuildServiceProvider();
    }

    private class ServiceProvider : IServiceProvider
    {
        public object GetService(Type serviceType)
        {
            return serviceType;
        }
    }

    [Authorize]
    private class HomeController
    {
        [Authorize]
        public static void Index() { }
    }

    [Fact]
    public void GetApiRoutesRoles_StateUnderTest_ExpectedBehavior()
    {
        // Arrange
        var service = GetService(_context);

        // Act
        var test = service.GetApiRoutesRoles().ToList();

        // Assert
        Assert.NotEmpty(test.First().Roles);
    }

    [Fact]
    public async Task GetPagesRoutes_StateUnderTest_ExpectedBehavior()
    {
        // Arrange
        var service = this.GetService(_context);

        var pageInfoDto = new Mock<IRouteRolesModule>();
        pageInfoDto.Setup(s => s.GetRouteRoles()).Returns(new List<PageInfoDTO>()
            {
                new PageInfoDTO(Routes.Users, Core.Roles.SuperAdminRole) { Permissions = new List<string>() { "test1", "test2" } },
                new PageInfoDTO(Routes.Roles, Core.Roles.SystemAdminRole) { Permissions = new List<string>() { "test1", "test2" } },
                new PageInfoDTO(Routes.Roles, AggregatedRoles.Authenticated) { Permissions = new List<string>() { "test1", "test2" } },
            });

        // Act
        var routes = service.GetPagesRoutes().ToList();

        // Assert
        Assert.NotNull(routes);
        Assert.NotNull(pageInfoDto);
    }

    [Fact]
    public async Task GetPageRoutesForUser_StateUnderTest_ExpectedBehavior()
    {
        // Arrange
        var permissions = new Faker<PermissionDTO>()
            .RuleFor(p => p.Id, s => s.Random.Int())
            .RuleFor(p => p.Name, s => s.Random.AlphaNumeric(7))
            .Generate(5);

        var groups = new Faker<GroupDTO>()
            .RuleFor(p => p.Id, s => s.Random.Int())
            .RuleFor(p => p.Name, s => s.Random.AlphaNumeric(7))
            .Generate(5);

        var user = new Faker<UserDTO>()
              .RuleFor(p => p.Id, s => s.Random.Int().ToString())
              .RuleFor(p => p.FirstName, s => s.Person.FirstName)
              .RuleFor(p => p.LastName, s => s.Person.LastName)
              .RuleFor(p => p.Email, (s, p) => s.Internet.Email(p.FirstName, p.LastName))
              .RuleFor(p => p.UserName, (s, p) => p.Email)
              .RuleFor(p => p.Password, s => s.Internet.Password())
              .RuleFor(p => p.ConfirmPassword, (s, p) => p.Password)
              .RuleFor(p => p.TwoFactorEnabled, p => false)
              .RuleFor(p => p.Permissions, s => permissions)
              .RuleFor(p => p.Groups, s => groups)
              .Generate(5);

        await _context.Set<User>().AddRangeAsync(_mapper.Map<List<User>>(user));
        await _context.SaveChangesAsync();

        var userService = new Mock<IUserService>();
        userService
            .Setup(u => u.GetAllUserPermissions(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(permissions);
        var service = this.GetService(_context, userService);

        // Act
        foreach (var p in user)
        {
            await service.GetPageRoutesForUser(p.Id, CancellationToken.None);
        }

        // Assert
        Assert.NotNull(permissions);
        Assert.NotNull(groups);
        Assert.NotNull(user);
    }
}
