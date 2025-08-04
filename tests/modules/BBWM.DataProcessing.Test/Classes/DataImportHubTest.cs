using BBWM.Core.Tasks;
using BBWM.DataProcessing.Classes;

using Moq;

using Xunit;

namespace BBWM.DataProcessing.Test.Classes;

public class DataImportHubTest
{
    [Fact]
    public void Stop()
    {
        // Arrange
        Mock<IBackgroundTaskQueue> queue = new();
        queue.Setup(q => q.CancelWorkItem()).Verifiable();

        DataImportHub dataImportHub = new(queue.Object);

        // Act
        dataImportHub.Stop();

        // Assert
        queue.Verify();
    }
}
