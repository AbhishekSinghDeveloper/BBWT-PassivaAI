using BBWM.SystemSettings;

namespace BBWM.RuntimeEditor;

// Settings stored in the settings JSON file
public class RuntimeEditorSettings : IMutableSystemConfigurationSettings
{
    private string editsGitBranch;
    private string editsCommitName;
    private string editJsonFilesPath;
    private string dictionaryFilePath;
    private string editionFilePath;

    /// <summary>
    /// Git branch where JSON files with edits come to.
    /// </summary>
    public string EditsGitBranch
    {
        get => string.IsNullOrWhiteSpace(editsGitBranch) ? Defaults.EditsGitBranch : editsGitBranch;
        set => editsGitBranch = value;
    }

    /// <summary>
    /// Folder where JSON files with edits come to.
    /// </summary>
    public string EditJsonFilesPath
    {
        get => (string.IsNullOrWhiteSpace(editJsonFilesPath) ?
                    Defaults.EditJsonFilesPath :
                    editJsonFilesPath)
                .TrimEnd(new char[] { '/', '\\' }) + "/";
        set => editJsonFilesPath = value;
    }

    /// <summary>
    /// Name of commit with edits JSON files.
    /// </summary>
    public string EditsCommitName
    {
        get => string.IsNullOrWhiteSpace(editsCommitName) ? Defaults.EditsCommitName : editsCommitName;
        set => editsCommitName = value;
    }

    /// <summary>
    /// Folder where a JSON file with the RTE phrases dictionary stored.
    /// The folder is relative to the server's content root path (/BBWT.Server/).
    /// </summary>
    public string DictionaryFilePath
    {
        get => string.IsNullOrWhiteSpace(dictionaryFilePath) ? Defaults.DictionaryFilePath : dictionaryFilePath;
        set => dictionaryFilePath = value;
    }

    /// <summary>
    /// Folder where a JSON file with the RTE phrases dictionary stored.
    /// The folder is relative to the server's content root path (/BBWT.Server/).
    /// </summary>
    public string EditionFilePath
    {
        get => string.IsNullOrWhiteSpace(editionFilePath) ? Defaults.EditionFilePath : editionFilePath;
        set => editionFilePath = value;
    }

    private static RuntimeEditorSettings Defaults =>
        new()
        {
            DictionaryFilePath = "/data/runtime-editor/dictionary.json",
            EditionFilePath = "/data/runtime-editor/edition.json",
            EditJsonFilesPath = "scripts/RuntimeEditorData/Edits/",
            EditsCommitName = "#nt Runtime Editor - website edit(s)",
            EditsGitBranch = "develop"
        };
}
