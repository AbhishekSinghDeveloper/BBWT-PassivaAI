namespace BBWT.InitialData;

public interface IDatabaseInitializerService
{
    /// <summary>
    /// Initialize/sync database data on application start.
    /// </summary>
    /// <param name="context">Database context</param>
    /// <param name="includingOnceSeededData">When True means that application durring current start up
    /// is performing the one-off initialization of data that supposed to be seeded only once ever.
    /// For example, you may seed demo users once, but then that users can be removed from app UI and
    /// the app should NOT re-create them on further start up.
    /// </param>
    void EnsureInitialData(bool includingOnceSeededData);
}
