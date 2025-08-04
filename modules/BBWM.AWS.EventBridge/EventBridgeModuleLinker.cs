using BBWM.AWS.EventBridge.Interfaces;
using BBWM.Core.ModuleLinker;

namespace BBWM.AWS.EventBridge;

internal static class EventBridgeModuleLinker
{
    /// <summary>
    /// Finds all Event Bridge jobs across the application
    /// </summary>
    /// <returns>Returns a list of tuples with both the job's type and job metadata's type</returns>
    public static List<(Type JobType, Type JobMetadata)> GetEventBridgeJobImplementors()
    {
        var allTypes = ModuleLinker.GetBbAssemblies().SelectMany(assembly => assembly.GetTypes()).ToList();

        Type getMetadata(Type jt)
            => allTypes.FirstOrDefault(tm => typeof(IEventBridgeJobMetadata<>).MakeGenericType(jt).IsAssignableFrom(tm));

        return allTypes
            .Where(t => !t.IsAbstract && typeof(IEventBridgeJob).IsAssignableFrom(t))
            .Select(jt => (JobType: jt, JobMetadata: getMetadata(jt)))
            .Where(i => i.JobType is not null && i.JobMetadata is not null)
            .ToList();
    }
}
