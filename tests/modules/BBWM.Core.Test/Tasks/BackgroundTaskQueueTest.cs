using BBWM.Core.Tasks;

using Xunit;

namespace BBWM.Core.Test.Tasks;

public class BackgroundTaskQueueTest
{
    public BackgroundTaskQueueTest()
    {
    }

    private static IBackgroundTaskQueue GetService()
    {
        return new BackgroundTaskQueue();
    }

    [Fact]
    public async Task Queue_Background_Work_Item_Test()
    {
        // Arrange
        var service = GetService();

        // Act
        for (int i = 1; i <= 5; i++)
        {
            var iClosure = i;
            Func<CancellationToken, Task<object>> backgroundItem = (ct) => Task.FromResult<object>($"R{iClosure}");
            service.QueueBackgroundWorkItem(backgroundItem);
        }

        // Assert
        for (int i = 1; i <= 5; i++)
        {
            await service.Dequeue(CancellationToken.None);
            Assert.Equal($"R{i}", service.GetLastResult<string>());
        }
    }
}
