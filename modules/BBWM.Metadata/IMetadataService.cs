namespace BBWM.Metadata;

public interface IMetadataService
{
    MetadataDTO GetByKey(string key);
    MetadataDTO Save(MetadataDTO dto);
    MetadataDTO Save(string key, string value);
    void LockUnlockRecord(string key, bool isLocked);
}
