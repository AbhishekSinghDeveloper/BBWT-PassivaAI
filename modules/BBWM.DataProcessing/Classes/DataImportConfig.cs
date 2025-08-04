using System.Text.Json.Nodes;

namespace BBWM.DataProcessing.Classes;

/// <summary>
/// Data import configuration
/// </summary>
public class DataImportConfig
{
    /// <summary>
    /// The name of file with extenstion
    /// </summary>
    public string FileName { get; set; }

    /// <summary>
    /// The file stream
    /// </summary>
    public Stream FileStream { get; set; }

    /// <summary>
    /// The definition of each readed row
    /// </summary>
    public ColumnsDefinitionsCollection ColumnDefinitions { get; set; }

    /// <summary>
    /// The first row from which a file is read
    /// </summary>
    public int FirstRow { get; set; }

    public int? LastRow { get; set; }

    public string SheetName { get; set; }

    public JsonObject Data { get; set; }

    /// <summary>
    /// The maximum number of errors after which processing stops. If it is not defined, the processing is not stops;
    /// </summary>
    public int? MaxErrorsCount { get; set; }

    public bool SkipInvalidRows { get; set; }

    public string UserId { get; set; }
}