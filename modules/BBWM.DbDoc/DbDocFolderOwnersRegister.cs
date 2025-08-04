namespace BBWM.DbDoc;

public static class DbDocFolderOwnersRegister
{
    private static readonly IList<string> allOwners = new List<string>();
    private static readonly IList<string> ownersAutoAddedToMainDbFolder = new List<string>();

    public static IEnumerable<string> GetAllOwners() =>
        allOwners.AsReadOnly();

    public static IEnumerable<string> GetOwnersAutoAddedToMainDbFolder() =>
        ownersAutoAddedToMainDbFolder.AsReadOnly();

    /// <summary>
    /// Adds an owner name to the list of possible owners of DBDoc folder.
    /// Features (like reports) can register to be an owner and then the feature's name will be shown in the
    /// DbDoc's list of folder owners (folder settings in the DbDoc tables tree). Then the feature
    /// (like reporting) can filter out folders (within its own UI) which belong to it and use those only folders
    /// for its purposes. For example, a user of reporting may create a custom folder with tables required for the
    /// reporting only and so he avoid exposing other tables.
    /// </summary>
    public static void RegisterFolderOwnerType(string owner, bool autoAddedOnMainDbFolderSeeding = false)
    {
        allOwners.Add(owner);

        if (autoAddedOnMainDbFolderSeeding)
        {
            ownersAutoAddedToMainDbFolder.Add(owner);
        }
    }
}
