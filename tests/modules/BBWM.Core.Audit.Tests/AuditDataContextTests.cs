using BBWM.Core.Membership.Model;
using BBWM.Core.ModelHashing;
using Microsoft.EntityFrameworkCore;

using Moq;

using Xunit;

namespace BBWM.Core.Audit.Tests;

public class AuditDataContextTests
{
    public AuditDataContextTests()
    {
    }


    [Fact]
    public void Find_Keys_Test()
    {
        // Arrange
        var service = GetService();
        Action test = () => service.FindKeys(typeof(User));

        Assert.Throws<InvalidOperationException>(test);
    }

    [Fact]
    public void Filter_StateUnderTest_ExpectedBehavior()
    {
        // Arrange
        var service = GetService();

        var obj = new Mock<Func<IQueryable<User>, IQueryable<User>>>();
    }

    [Fact]
    public void Filter_StateUnderTest_ExpectedBehavior1()
    {
        // Arrange
    }


    private AuditContext GetService()
    {
        DbContextOptions<AuditContext> ctx = new DbContextOptions<AuditContext>();

        return new AuditContext(ctx);
    }
}
