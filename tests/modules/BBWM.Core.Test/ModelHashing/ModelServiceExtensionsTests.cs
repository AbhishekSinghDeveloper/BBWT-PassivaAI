using BBWM.Core.Membership.DTO;
using BBWM.Core.ModelHashing;

using Moq;

using Xunit;

namespace BBWM.Core.Test.ModelHashing;

public class ModelServiceExtensionsTests
{
    public ModelServiceExtensionsTests()
    {
    }

    [Fact]
    public void HashProperty_Test()
    {
        var modelServices = new Mock<IModelHashingService>();

        var modelServiceExt = ModelServiceExtensions.HashProperty<UserDTO>(modelServices.Object, new UserDTO(), "FirstName");
        var modelServiceExt2 = ModelServiceExtensions.HashProperty<UserDTO>(modelServices.Object, "FirstName", 1);

        Assert.Null(modelServiceExt);
        Assert.Null(modelServiceExt2);
    }
}
