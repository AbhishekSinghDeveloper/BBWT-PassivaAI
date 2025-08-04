namespace BBWM.DbDoc;

public class DbDocSettings
{
    public string FilePath { get; set; } = "/data/dbdoc/dbdoc.json";

    public bool ShowTableData { get; set; } = false;

    public bool ReadOnlyTableData { get; set; } = true;
}
