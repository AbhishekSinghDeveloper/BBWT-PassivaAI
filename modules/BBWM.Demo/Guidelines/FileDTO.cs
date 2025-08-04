using BBWM.Core.DTO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BBWM.Demo.Guidelines;

public class FileDTO : IDTO
{
    /// <summary>
    /// ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Label
    /// </summary>
    public string Label { get; set; }

    /// <summary>
    /// Data
    /// </summary>
    public string Data { get; set; }

    /// <summary>
    /// Type (0: Folder, 1: File)
    /// </summary>
    public int Type { get; set; }

    /// <summary>
    /// ExpandedIcon
    /// </summary>
    public string Icon { get; set; }

    /// <summary>
    /// ExpandedIcon
    /// </summary>
    public string ExpandedIcon { get; set; }

    /// <summary>
    /// CollapsedIcon
    /// </summary>
    public string CollapsedIcon { get; set; }

    /// <summary>
    /// Expanded
    /// </summary>
    public bool Expanded { get; set; }

    /// <summary>
    /// ParentId
    /// </summary>
    public int? ParentId { get; set; }

    /// <summary>
    /// Parent
    /// </summary>
    [JsonIgnore]
    public virtual FileDTO Parent { get; set; }

    /// <summary>
    /// Children
    /// </summary>
    public virtual IEnumerable<FileDTO> Children { get; set; }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}