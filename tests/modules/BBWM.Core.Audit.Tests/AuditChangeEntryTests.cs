using BBWM.Core.Membership.Model;
using BBWM.Core.Test;
using BBWM.Core.Test.Utils;

using Xunit;

namespace BBWM.Core.Audit.Tests;

public class AuditChangeEntryTests
{
    public AuditChangeEntryTests()
    {
    }

    private static AuditChangeEntry GetService()
    {
        DataContext dataContext = SutDataHelper.CreateEmptyContext<DataContext>();

        return new AuditChangeEntry(dataContext.Entry(new User()));
    }

    [Fact]
    public void ToAudit_StateUnderTest_ExpectedBehavior()
    {
        // Arrange
        var service = GetService();

        var reuslt = service.ToAudit();

        Assert.NotNull(reuslt);
    }
}
