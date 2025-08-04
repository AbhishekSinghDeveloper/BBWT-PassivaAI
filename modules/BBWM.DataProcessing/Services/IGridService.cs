namespace BBWM.DataProcessing.Services;

public interface IGridService
{
    byte[] PrintExcel<T>(IEnumerable<T> data, GridData grid);

    byte[] PrintCSV<T>(IEnumerable<T> data, GridData grid);
}

public class GridTableColumn
{
    public string Field { get; set; }

    public string Header { get; set; }

    public string SortField { get; set; }
}

public class GridData
{
    public List<GridTableColumn> GridTableColumns { get; set; }

    public List<int> Ids { get; set; }
}
