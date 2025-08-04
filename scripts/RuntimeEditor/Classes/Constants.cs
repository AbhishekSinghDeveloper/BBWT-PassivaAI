namespace RuntimeEditor.Classes;

public static class Constants
{
    /// <summary>
    /// Name of the RTE attribute that contains a unique ID value.
    /// </summary>
    public static readonly string RteAttrName = "rte";

    /// <summary>
    /// Settings file for links embedding
    /// </summary>
    public static readonly string LinksEmbedSettingsFilePath = "scripts/RuntimeEditor/links-embed-settings.json";

    /// <summary>
    /// Global counter of repo template's IDs.
    /// </summary>
    public static readonly string LinksCounterFilePath = "scripts/RuntimeEditorData/rte-links-counter.json";

    /// <summary>
    /// Folder where JSON files with edits come to.
    /// </summary>
    public static readonly string EditJsonFilesPath = "scripts/RuntimeEditorData/Edits/";

    /// <summary>
    /// Setting file for dictiony generating
    /// </summary>
    public static readonly string DictionarySettingsFilePath = "scripts/RuntimeEditor/dictionary-settings.json";

    /// <summary>
    /// Folder where JSON files with edits come to.
    /// </summary>
    public static readonly string DictionaryFilePath = "project/BBWT.Server/data/runtime-editor/dictionary.json";
}