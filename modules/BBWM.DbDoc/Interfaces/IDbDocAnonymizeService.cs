namespace BBWM.DbDoc.Interfaces;

public interface IDbDocAnonymizeService
{
    /// <summary>
    /// Gets XML file of DB documenting tables for anonymization functional
    /// </summary>
    Task<byte[]> GetAnonymizationXml(Guid folderId, CancellationToken ct);
}
