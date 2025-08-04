using System.Data.SqlTypes;
using System.Text;

namespace BBWM.DataProcessing.Export;

public class CsvExport
{
    /// <summary>
    /// To keep the ordered list of column names
    /// </summary>
    readonly List<string> _fields = new List<string>();

    /// <summary>
    /// The list of rows
    /// </summary>
    readonly List<Dictionary<string, object>> _rows = new List<Dictionary<string, object>>();

    /// <summary>
    /// The current row
    /// </summary>
    Dictionary<string, object> _currentRow { get { return _rows[_rows.Count - 1]; } }

    /// <summary>
    /// The string used to separate columns in the output
    /// </summary>
    private readonly string _columnSeparator;

    /// <summary>
    /// Whether to include the preamble that declares which column separator is used in the output
    /// </summary>
    private readonly bool _includeColumnSeparatorDefinitionPreamble;
    private readonly int _cellLengthLimit;

    /// <summary>
    /// Initializes a new instance of the <see cref="Jitbit.Utils.CsvExport"/> class.
    /// </summary>
    /// <param name="columnSeparator">
    /// The string used to separate columns in the output.
    /// By default this is a comma so that the generated output is a CSV file.
    /// </param>
    /// <param name="includeColumnSeparatorDefinitionPreamble">
    /// Whether to include the preamble that declares which column separator is used in the output.
    /// By default this is <c>true</c> so that Excel can open the generated CSV
    /// without asking the user to specify the delimiter used in the file.
    /// </param>
    public CsvExport(string columnSeparator = ",", bool includeColumnSeparatorDefinitionPreamble = true,
        int cellLengthLimit = 30000)
    {
        _columnSeparator = columnSeparator;
        _includeColumnSeparatorDefinitionPreamble = includeColumnSeparatorDefinitionPreamble;
        _cellLengthLimit = cellLengthLimit;
    }

    /// <summary>
    /// Set a value on this column
    /// </summary>
    public object this[string field]
    {
        set
        {
            // Keep track of the field names, because the dictionary loses the ordering
            if (!_fields.Contains(field)) _fields.Add(field);
            _currentRow[field] = value;
        }
    }

    /// <summary>
    /// Call this before setting any fields on a row
    /// </summary>
    public void AddRow()
    {
        _rows.Add(new Dictionary<string, object>());
    }

    /// <summary>
    /// Add a list of typed objects, maps object properties to CsvFields
    /// </summary>
    public void AddRows<T>(IEnumerable<T> list)
    {
        if (list.Any())
        {
            foreach (var obj in list)
            {
                AddRow();
                var values = obj.GetType().GetProperties();
                foreach (var value in values)
                {
                    this[value.Name] = value.GetValue(obj, null);
                }
            }
        }
    }

    /// <summary>
    /// Converts a value to how it should output in a CSV file
    ///     Lint rules:
    ///     - a cell is always wrapped with double quotes
    ///     - all double quotes within a cell value are replaced with two double quotes ("")
    ///       E.g. "Dangerous Dan" McGrew -> """Dangerous Dan"" McGrew"
    /// Note! The method doesn't sanitize data for Excel purposes. For Excel there should be additional measures taken.
    /// Useful references:
    ///     - https://owasp.org/www-community/attacks/CSV_Injection
    ///     - https://csvlint.io 
    /// </summary>
    private string SanitizeCsvValue(object value)
    {
        if (value is null) return "";
        if (value is INullable nullable && nullable.IsNull) return "";

        var output = value.ToString();

        if (output.Length > _cellLengthLimit)
            output = output[.._cellLengthLimit];

        output = '"' + output.Replace("\"", "\"\"") + '"';

        return output;
    }

    /// <summary>
    /// Outputs all rows as a CSV, returning one string at a time
    /// </summary>
    private IEnumerable<string> ExportToLines(bool includeHeader = false)
    {
        if (_includeColumnSeparatorDefinitionPreamble) yield return "sep=" + _columnSeparator;

        // The header
        if (includeHeader)
            yield return string.Join(_columnSeparator, _fields
                .Select(f => SanitizeCsvValue(f)));

        // The rows
        foreach (var row in _rows)
        {
            foreach (var k in _fields.Where(f => !row.ContainsKey(f)))
            {
                row[k] = null;
            }
            yield return string.Join(_columnSeparator, _fields
                .Select(field => SanitizeCsvValue(row[field])));
        }
    }

    /// <summary>
    /// Output all rows as a CSV returning a string
    /// </summary>
    public string Export(bool includeHeader = false)
    {
        var sb = new StringBuilder();

        foreach (var line in ExportToLines(includeHeader))
        {
            sb.AppendLine(line);
        }

        return sb.ToString();
    }

    /// <summary>
    /// Exports to a file
    /// </summary>
    public void ExportToFile(string path, bool includeHeader = false)
    {
        File.WriteAllLines(path, ExportToLines(includeHeader), Encoding.UTF8);
    }

    /// <summary>
    /// Exports as raw UTF8 bytes
    /// </summary>
    public byte[] ExportToBytes(bool includeHeader = false)
    {
        var data = Encoding.Default.GetBytes(Export(includeHeader));
        return Encoding.Default.GetPreamble().Concat(data).ToArray();
    }

    public CsvExport Merge(CsvExport detailsReport, string columnSeparator = ",", bool includeColumnSeparatorDefinitionPreamble = true)
    {
        var result = new CsvExport(columnSeparator, includeColumnSeparatorDefinitionPreamble);

        void AddResultRows(List<Dictionary<string, object>> _rows)
        {
            foreach (var row in _rows)
            {
                result.AddRow();
                var keys = row.Keys.ToArray();
                for (var i = 1; i <= row.Count; i++)
                {
                    result[i.ToString()] = row[keys[i - 1]];
                }
            }
        }

        AddResultRows(_rows);
        AddResultRows(detailsReport._rows);

        return result;
    }
}
