using BBWM.Core.Data;
using BBWM.Core.Services;
using BBWM.DbDoc.DbSchemas.SchemaModels;
using BBWM.FormIO.DTO;
using BBWM.FormIO.Extensions;
using BBWM.FormIO.Models;
using BBWM.Reporting.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Text.RegularExpressions;
using QueryableTable = BBWM.Reporting.Interfaces.QueryableTable;

namespace BBWM.FormIO.Connectors.ReportingV2;

public class FormsQueryableTableProvider : IFormsQueryableTablesProvider
{
    private readonly IDataService _dataService;
    private readonly IDbContext dbContext;
    private readonly DatabaseType _dbType;

    public FormsQueryableTableProvider(IDataService dataService,
        IDbContext dbContext,
        IConfiguration configuration)
    {
        _dataService = dataService;
        this.dbContext = dbContext;
        _dbType = configuration.GetDatabaseConnectionSettings().DatabaseType;
    }

    public string SourceCode => "form";

    public async Task<QueryableTableSource> GetQueryableTableSource(CancellationToken ct)
    {
        var tables = await GetVirtualTables(ct);

        // In this version of Forms provider for the reporting, we use the main DB context, therefore use dbContext
        // object to get DB schema details. In the future when forms are taken from passed db doc folder,
        // we can get from DB DOC folder's db source.
        var dbConnection = dbContext.Database.GetDbConnection();

        return new QueryableTableSource
        {
            SourceCode = SourceCode,
            SourceName = "Forms",
            Tables = tables.Select(x => new QueryableTable
            {
                // For showing in the forms list imported to RB query)
                FriendlyName = x.FriendlyName,
                // TODO: maybe we calc it from SourceTableName (for showing in the RB tables tree as '[FriendlyName as [SourceTableAlias]')
                SourceTableAlias = x.TableName,

                SchemaTable = new DbSchemaTable
                {
                    TableId = x.TableName,
                    TableName = x.TableName,
                    DbName = dbConnection.Database,
                    QueryName = dbConnection is Microsoft.Data.SqlClient.SqlConnection
                        ? $"{dbConnection.Database}.dbo.{x.TableName}"
                        : $"{dbConnection.Database}.{x.TableName}",
                    Schema = dbConnection is Microsoft.Data.SqlClient.SqlConnection ? "dbo" : "",
                },

                SchemaColumns = x.Columns.Select(y => new DbSchemaColumn
                {
                    TableId = x.TableName,
                    ParentTableName = x.TableName,
                    ColumnId = y.Name,
                    ColumnName = y.Name,
                    PropertyName = y.Name,
                    Type = y.Type,
                    IsForeignKey = y.IsForeingKey,
                    IsPrimaryKey = y.IsPrimaryKey,
                    QueryName = dbConnection is Microsoft.Data.SqlClient.SqlConnection
                        ? $"{dbConnection.Database}.dbo.{x.TableName}.{y.Name}"
                        : $"{dbConnection.Database}.{x.TableName}.{y.Name}",
                }).ToList()
            }).ToList()
        };
    }

    // TODO: should be refactored. FormTable - not necessary we need this transitional class
    private async Task<IEnumerable<FormTable>> GetVirtualTables(CancellationToken ct = default)
    {
        // Get table name from entity type.
        //var tableName = _dataService.Context.Model.FindEntityType(typeof(FormDefinition))?.GetTableName();
        //if (tableName == null) throw new InvalidOperationException("There is no FormDefinition table.");

        var definitions = await _dataService.GetAll<FormDefinition, FormDefinitionDTO>(ct);
        if (definitions == null) return new List<FormTable>();

        var tables =
            from definition in definitions
            let table = GetTable(definition)
            where table != null
            select table;

        return tables;
    }

    private FormTable? GetTable(FormDefinitionDTO definition)
    {
        if (string.IsNullOrEmpty(definition.ActiveRevision.Json)) return null;

        var name = definition.Name;
        var sanitizedName = RemoveInvalidSqlNameCharacters(name);

        if (string.IsNullOrEmpty(sanitizedName)) return null;

        // Get table name from entity type.
        var tableName = _dataService.Context.Model.FindEntityType(typeof(FormData))?.GetTableName();
        // Get navigational property of type FormDefinition.
        var navigationProperty = typeof(FormData).GetProperties()
            .FirstOrDefault(property => property.PropertyType == typeof(FormDefinition));

        // If some parameter is missing, return null.
        if (tableName == null || navigationProperty == null) return null;

        // Get name of the FormDefinition foreign key.
        var foreignKey = navigationProperty.GetCustomAttributes<ForeignKeyAttribute>().FirstOrDefault()?.Name
                         ?? navigationProperty.Name + "Id";

        // Get this table columns.
        var columns = GetColumns(definition);

        var table = new FormTable
        {
            FriendlyName = name,
            TableName = sanitizedName,
            SourceTableName = tableName,
            SourceDefinitionField = foreignKey,
            SourceDefinitionValue = definition.Id,
            Columns = columns
        };

        return table;
    }

    private static string RemoveInvalidSqlNameCharacters(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        var regex = new Regex(@"[^a-zA-Z0-9_]");
        var sanitizedValue = regex.Replace(value, "");

        if (string.IsNullOrEmpty(sanitizedValue))
            return sanitizedValue;

        if (!char.IsLetter(sanitizedValue[0])) sanitizedValue = "_" + sanitizedValue;
        else sanitizedValue = char.ToLower(sanitizedValue[0]) + sanitizedValue[1..];

        return sanitizedValue;
    }

    private List<FormColumn> GetColumns(FormDefinitionDTO definition)
    {
        var json = JObject.Parse(definition.ActiveRevision.Json);

        // Mapping of types from form types to standard sql types.
        var mapping = new Dictionary<string, string>
        {
            { "checkbox", "BIT" },
            { "number", "DECIMAL(18, 6)" },
            { "currency", "DECIMAL(18, 6)" },
            { "day", "DATE" },
            { "time", "TIME" },
            { "datetime", _dbType == DatabaseType.MySql ? "DATETIME" : "DATETIME2" },
            { "textfield", _dbType == DatabaseType.MySql ? "VARCHAR(255)" : "NVARCHAR(255)" },
            { "textarea", _dbType == DatabaseType.MySql ? "VARCHAR(5000)" : "NVARCHAR(5000)" },
            { "password", _dbType == DatabaseType.MySql ? "VARCHAR(255)" : "NVARCHAR(255)" },
            { "selectboxes", _dbType == DatabaseType.MySql ? "VARCHAR(5000)" : "NVARCHAR(5000)" },
            { "select", _dbType == DatabaseType.MySql ? "VARCHAR(255)" : "NVARCHAR(255)" },
            { "radio", _dbType == DatabaseType.MySql ? "VARCHAR(255)" : "NVARCHAR(255)" },
            { "email", _dbType == DatabaseType.MySql ? "VARCHAR(255)" : "NVARCHAR(255)" },
            { "phoneNumber", _dbType == DatabaseType.MySql ? "VARCHAR(255)" : "NVARCHAR(255)" },
            { "tags", _dbType == DatabaseType.MySql ? "VARCHAR(255)" : "NVARCHAR(255)" },
            { "address", _dbType == DatabaseType.MySql ? "VARCHAR(255)" : "NVARCHAR(255)" },
        };

        // Get valid components from JSON.
        var tokens =
            json.First?.First?.GetInnerFormDefinitionComponents(token =>
            {
                var type = token.Value<string>("type");
                return type != null && mapping.ContainsKey(type);
            }) ?? new List<JToken>();

        var columns = (
            from token in tokens
            let label = token.Value<string>("label")
            let type = token.Value<string>("type")
            where !string.IsNullOrEmpty(label) && !string.IsNullOrEmpty(type)
            let sanitizedLabel = RemoveInvalidSqlNameCharacters(label)
            let mappedType = mapping[type]
            where !string.IsNullOrEmpty(label)
            select new FormColumn
            {
                Name = sanitizedLabel,
                Type = mappedType,
                Path = $"$.data.{sanitizedLabel}",
                IsPrimaryKey = false,
                IsForeingKey = false,
            }).ToList();

        return columns;
    }
}