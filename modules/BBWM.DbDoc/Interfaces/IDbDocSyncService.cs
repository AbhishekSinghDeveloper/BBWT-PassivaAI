namespace BBWM.DbDoc.Interfaces;

public interface IDbDocSyncService
{
    /// <summary>
    /// Synchronizes data from the JSON file and DB.
    /// </summary>
    Task Synchronize();
}
