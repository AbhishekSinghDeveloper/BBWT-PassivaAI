using BBWM.Core.Filters;

namespace BBWM.DbDoc.Core.Classes;

public class TableDump
{
    public IList<Tuple<string, string>> Columns { get; set; }

    public PageResult<dynamic> Data { get; set; }
}
