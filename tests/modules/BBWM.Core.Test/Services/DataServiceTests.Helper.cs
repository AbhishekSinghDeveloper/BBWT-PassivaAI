using BBWM.AWS.EventBridge.Model;

using Bogus;

namespace BBWM.Core.Test.Services;

public class DataServiceTestHelper
{
    public static EventBridgeRunningJob CreateEntity(int id = 1)
    {
        var runningJob = new Faker<EventBridgeRunningJob>();
        runningJob
            .RuleFor(p => p.Id, f => id)
            .RuleFor(p => p.JobId, f => $"{f.Random.String(5, 10, 'a', 'z')}-job")
            .RuleFor(p => p.RuleId, f => $"{f.Random.String(5, 10, 'a', 'z')}-rule");
        return runningJob.Generate();
    }

    public static EventBridgeRunningJob[] CreateEntities(int count = 10)
        => Enumerable.Range(1, count).Select(CreateEntity).ToArray();
}
