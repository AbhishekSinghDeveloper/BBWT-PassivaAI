using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

using System.Reflection;
using System.Text;

namespace BBWM.DataProcessing.Services;

public class GridService : IGridService
{
    private static IWorkbook CreateSpreadsheetDocument<T>(IEnumerable<T> data, GridData grid)
    {
        IWorkbook workbook = new XSSFWorkbook();
        var sheet = workbook.CreateSheet("export table");
        var column = 0;
        var line = 0;
        var header = sheet.CreateRow(0);

        Dictionary<string, PropertyInfo> columnMap = grid.GridTableColumns
            .Select(GetField)
            .Select(
                field => new
                {
                    Field = field,
                    Property = typeof(T).GetProperty(field, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)
                })
            .Where(item => item.Property != null)
            .ToDictionary(kv => kv.Field, kv => kv.Property);

        // header
        foreach (var h in grid.GridTableColumns)
        {
            if (!columnMap.ContainsKey(GetField(h))) continue;

            header.CreateCell(column).SetCellValue(new XSSFRichTextString(h.Header));
            column++;
        }

        foreach (var entry in data)
        {
            line++;
            column = 0;
            var row = sheet.CreateRow(line);

            foreach (var g in grid.GridTableColumns)
            {
                var cell = row.CreateCell(column);

                if (!columnMap.TryGetValue(GetField(g), out var property)) continue;

                var value = property.GetValue(entry);

                if (property.PropertyType == typeof(DateTime) && value is not null)
                {
                    cell.SetCellValue((DateTime)value);
                }

                if (property.PropertyType == typeof(double) && value is not null)
                {
                    cell.SetCellValue((double)value);
                }
                else
                {
                    cell.SetCellValue(new XSSFRichTextString(value is not null ? value.ToString() : string.Empty));
                }
                column++;
            }
        }

        return workbook;
    }

    private static string GetField(GridTableColumn column)
        => string.IsNullOrEmpty(column.SortField) ? column.Field : column.SortField;

    public byte[] PrintExcel<T>(IEnumerable<T> data, GridData grid)
    {
        var ExcelResult = CreateSpreadsheetDocument(data, grid);
        using MemoryStream stream = new();
        ExcelResult.Write(stream, false);
        return stream.ToArray();
    }

    public byte[] PrintCSV<T>(IEnumerable<T> data, GridData grid)
    {
        var csv = new StringBuilder("");
        grid.GridTableColumns.ForEach(x => csv.AppendFormat("{0},", x.Header));
        csv.Append("\r\n");

        foreach (var entry in data)
        {
            foreach (var g in grid.GridTableColumns)
            {
                var type = entry.GetType();
                var field = GetField(g);
                if (field == "id_original") field = "id";
                var property = type.GetProperty(field, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                var value = property.GetValue(entry, null);
                csv.AppendFormat("{0},", value);
            }

            csv.Append("\r\n");
        }

        return Encoding.ASCII.GetBytes(csv.ToString());

    }
}
