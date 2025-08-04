using AutoMapper;

using BBWM.AWS.EventBridge.Test.Fixtures;

using Xunit;

namespace BBWM.AWS.EventBridge.Test;

[TestCaseOrderer(TestPriorityOrderer.TYPE_NAME, TestPriorityOrderer.ASSEMBLY_NAME)]
public partial class JobServiceAndWrapperTests : IClassFixture<MappingFixture>, IDisposable
{
    public JobServiceAndWrapperTests(MappingFixture mappingFixture)
        => Mapper = mappingFixture.Mapper;

    public IMapper Mapper { get; }

    public void Dispose()
    {
    }
}
