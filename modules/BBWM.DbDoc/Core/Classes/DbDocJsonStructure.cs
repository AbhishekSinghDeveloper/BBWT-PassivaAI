using BBWM.DbDoc.Model;

namespace BBWM.DbDoc.Core.Classes;

public class DbDocJsonStructure
{
    public IEnumerable<Folder> Folders { get; set; } = new List<Folder>();

    public IEnumerable<ColumnType> ColumnTypes { get; set; } = new List<ColumnType>();
}
