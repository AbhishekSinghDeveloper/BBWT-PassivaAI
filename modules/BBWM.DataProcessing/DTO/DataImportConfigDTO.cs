using System.Text.Json.Nodes;

namespace BBWM.DataProcessing.DTO;

/// <summary>
/// DTO for DataImportConfig
/// </summary>
public class DataImportConfigDTO
{
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
    public int? MaxErrorsCount { get; set; } = 10;

    public bool SkipInvalidRows { get; set; }

    /// <summary>
    /// The definition of each readed row
    /// </summary>
    public IEnumerable<ColumnDefinitionDTO> ColumnDefinitions { get; set; }

    /// <summary>
    /// The name of file with extenstion
    /// </summary>
    public string FileName { get; set; }

    /// <summary>
    /// The file stream
    /// </summary>
    public Stream FileStream { get; set; }

    public string UserId { get; set; }
}