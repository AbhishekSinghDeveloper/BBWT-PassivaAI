namespace BBWM.DataProcessing.Export;

public class ReportCSVService<T>
{
    public CsvExport GetCsvFile(
        List<T> data,
        List<ColumnSetting<T>> settings,
        Action<List<ColumnSetting<T>>, CsvExport, List<T>> footer = null,
        string columnSeparator = ",",
        bool includeColumnSeparatorDefinitionPreamble = true)
    {
        var csvExport = new CsvExport(columnSeparator, includeColumnSeparatorDefinitionPreamble);

        //Create Header
        csvExport.AddRow();
        foreach (var column in settings)
        {
            csvExport[column.Header] = column.Header;
        }

        foreach (var row in data)
        {
            csvExport.AddRow();
            foreach (var column in settings)
            {
                csvExport[column.Header] = column.GetValue(row);
            }
        }

        //Footer
        if (footer is not null)
        {
            csvExport.AddRow();
            footer(settings, csvExport, data);
        }

        return csvExport;
    }
}
