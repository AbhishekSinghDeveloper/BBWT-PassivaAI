using BBWM.Core.Data;
using BBWM.Core.Test;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Moq;

using Xunit;

namespace BBWM.Core.Membership.Test;

public class ServiceCollectionExtensionsTests
{
    private readonly IDbContext _context;

    public ServiceCollectionExtensionsTests()
    {
        _context = InMemoryDataContext.GetContext(Guid.NewGuid().ToString());
    }

    [Fact]
    public void Add_Sql_Server_Sign_In_Manager_Test()
    {
        var service = new Mock<ServiceCollection>();

        var actions = new Mock<Action<IdentityOptions>>();

        var addSqlServerSignInManager = ServiceCollectionExtensions.AddSqlServerSignInManager<DbContext>(service.Object, actions.Object);

        Assert.NotNull(addSqlServerSignInManager);
        Assert.NotNull(actions);
        Assert.NotNull(service);
    }

    [Fact]
    public void Add_MySql_Sign_In_Manager_Test()
    {
        var service = new Mock<ServiceCollection>();

        var actions = new Mock<Action<IdentityOptions>>();

        var addMySqlSignInManager = ServiceCollectionExtensions.AddMySqlSignInManager<DbContext>(service.Object, actions.Object);

        Assert.NotNull(addMySqlSignInManager);
        Assert.NotNull(actions);
        Assert.NotNull(service);
    }
}
