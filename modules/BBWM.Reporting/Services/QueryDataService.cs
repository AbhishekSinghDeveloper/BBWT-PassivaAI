using BBWM.Core.Data;
using BBWM.Core.Exceptions;
using BBWM.Core.Filters;
using BBWM.Core.Web.Extensions;
using BBWM.DbDoc.DbSchemas.Interfaces;
using BBWM.DbDoc.DbSchemas.SchemaModels;
using BBWM.DbDoc.Interfaces;
using BBWM.DbDoc.Model;
using BBWM.Reporting.DTO;
using BBWM.Reporting.Extensions;
using BBWM.Reporting.Interfaces;
using BBWM.Reporting.Model;
using BBWM.Reporting.Providers;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using SqlKata.Compilers;
using SqlKata.Execution;
using System.Data.Common;
using System.Text.RegularExpressions;

using Z.EntityFramework.Plus;

namespace BBWM.Reporting.Services
{
    public class QueryDataService : IQueryDataService
    {
        private readonly string _currentUserId;
        private readonly IDbContext _mainDbContext;
        private readonly IDbDocFolderService _dbDocFolderService;
        private readonly IQueryTablesSchemaService _tablesSchemaService;
        private readonly IDbSchemaManager _dbSchemaManager;

        public QueryDataService(
            IHttpContextAccessor httpContextAccessor,
            IDbContext mainDbContext,
            IDbDocFolderService dbDocFolderService,
            IQueryTablesSchemaService tablesSchemaService,
            IDbSchemaManager dbSchemaManager
            )
        {
            _currentUserId = httpContextAccessor.HttpContext.GetUserId();
            _mainDbContext = mainDbContext;
            _dbDocFolderService = dbDocFolderService;
            _tablesSchemaService = tablesSchemaService;
            _dbSchemaManager = dbSchemaManager;
        }


        public async Task<string> GetSqlQuery(Query query, bool reduceSyntax = false, CancellationToken ct = default)
        {
            if (query?.QueryTables is null || !query.QueryTables.Any())
                return null;

            var queryFactory = GetQueryFactory(query);
            var tablesSchema = await _tablesSchemaService.BuildTablesSchema(query, ct);
            var sqlKataQuery = await GetQuery(query, tablesSchema, null, ct);

            var columns = query.QueryTables
                .SelectMany(x => x.Columns)
                .Select(x => (x.QueryTable, tablesSchema.GetColumn(x)));

            var tables = query.QueryTables.Select(x => (x, tablesSchema.GetTable(x)));

            IEnumerable<string> selectColumns;

            HashSet<string> idValuesToUnwrap = new();
            HashSet<string> dbNamePrefixesToRemove = new();

            if (reduceSyntax)
            {
                var isSingleTable = query.QueryTables.Count == 1;
                selectColumns = columns.Select(x =>
                    string.IsNullOrEmpty(x.QueryTable.Alias)
                        ? ((isSingleTable ? "" : $"{x.Item2.ParentTableName}.") + x.Item2.PropertyName)
                        : $"{x.QueryTable.Alias}.{x.Item2.PropertyName}");

                foreach (var x in columns)
                {
                    idValuesToUnwrap.Add(x.Item2.PropertyName);
                }

                foreach (var x in tables)
                {
                    idValuesToUnwrap.Add(x.Item2.TableName);
                    idValuesToUnwrap.Add(x.Item2.DbName);
                    if (!string.IsNullOrEmpty(x.Item2.Schema))
                    {
                        idValuesToUnwrap.Add(x.Item2.Schema);
                    }

                    // TODO: db schema can be removed when single in DB
                    dbNamePrefixesToRemove.Add(x.Item2.DbName);
                }
            }
            else
            {
                selectColumns = columns.Select(x => $"{(string.IsNullOrEmpty(x.QueryTable.Alias) ? string.Empty : x.QueryTable.Alias + ".")}{x.Item2.QueryName}");
            }

            sqlKataQuery.Select(selectColumns.ToArray());

            var compile = queryFactory.Compiler.Compile(sqlKataQuery);
            var sql = compile.Sql;

            if (reduceSyntax)
            {
                // We don't remove prefixes when multiple DBs used (to keep distinguising DBs)
                if (dbNamePrefixesToRemove.Count == 1)
                {
                    var dbName = dbNamePrefixesToRemove.First();
                    var w = queryFactory.Compiler.WrapValue(dbName);
                    // TODO: improvement: we can detect when value is within quotes (string value of SQL script)
                    // and skip replacing.
                    sql = sql.Replace(w + ".", "");
                }

                foreach (var x in idValuesToUnwrap.Where(y => Regex.IsMatch(y, "^[a-zA-Z0-9_]*$")))
                {
                    var w = queryFactory.Compiler.WrapValue(x);
                    sql = sql.Replace(w, x);
                }
            }

            return sql;
        }

        public async Task<IEnumerable<dynamic>> GetData(Query query, QueryCommand queryCommand = null, CancellationToken ct = default)
        {
            if (query?.QueryTables is null || !query.QueryTables.Any())
                return null;

            var queryFactory = GetQueryFactory(query);
            var tablesSchema = await _tablesSchemaService.BuildTablesSchema(query, ct);
            var sqlKataQuery = await GetQuery(query, tablesSchema, queryCommand, ct);

            var columnHeaders = new List<(string field, string header)>();

            foreach (var queryTable in query.QueryTables)
            {
                foreach (var queryTableColumn in queryTable.Columns)
                {
                    var schemaColumn = tablesSchema.GetColumn(queryTableColumn);
                    columnHeaders.Add(string.IsNullOrEmpty(queryTable.Alias)
                        ? (schemaColumn.QueryName, schemaColumn.GetQueryAlias())
                        : ($"{queryTable.Alias}.{schemaColumn.ColumnName}", $"{queryTable.Alias}_{schemaColumn.ColumnName}"));
                }
            }

            sqlKataQuery.Select(columnHeaders.Select(x => $"{x.field} AS {x.header}").ToArray());

            if (queryCommand?.Skip != null)
                sqlKataQuery.Skip(queryCommand.Skip.Value);
            if (queryCommand?.Take != null)
                sqlKataQuery.Take(queryCommand.Take.Value);

            return queryFactory.FromQuery(sqlKataQuery).Get();
        }

        public async Task<IEnumerable<DropDownOption>> GetDataAsOptions(QueryBuilderOptionsRequest request, Query query, CancellationToken ct = default)
        {
            var tablesSchema = await _tablesSchemaService.BuildTablesSchema(query, ct);

            var tableStaticData = tablesSchema.GetTable(request.SourceTableId)
                ?? throw new ObjectNotExistsException("The table static data was not found for the specified unique table ID.");

            var labelSchemaColumn = tablesSchema.GetColumn(request.SourceTableId, request.LabelColumnId)
                ?? throw new ObjectNotExistsException("The option's label column static data was not found for the specified unique column ID.");

            var valueSchemaColumn = tablesSchema.GetColumn(request.SourceTableId, request.LabelColumnId)
                ?? throw new ObjectNotExistsException("The option's value column static data was not found for the specified unique column ID.");

            var sqlKataQuery = GetQueryFactory(query).Query(tableStaticData.QueryName);

            if (labelSchemaColumn.ParentTableName != tableStaticData.TableName ||
                valueSchemaColumn.ParentTableName != tableStaticData.TableName)
            {
                throw new BusinessException("There is no relation between the source table and label or value columns.");
            }

            sqlKataQuery.Select(new[]
                {
                    $"{labelSchemaColumn.QueryName} AS Label",
                    $"{valueSchemaColumn.QueryName} AS Value"
                })
                .OrderBy(labelSchemaColumn.QueryName)
                .Distinct();

            //This limitation is a guard against stupid report querying, on one hand. On the other hand, if dropdown
            //must contain > N records, then limiting doesn't make ANY sense - we lose records. Here we set a limit to
            //some value where a decision of the report user to switch to another filter type would become reasonable.
            //For example, the user should use a string input filter or a combination of a dropdown with inline search.
            sqlKataQuery.Limit(4000);

            return await sqlKataQuery.GetAsync<DropDownOption>();
        }

        public async Task<int> GetTotal(Query query, QueryCommand queryCommand = null, CancellationToken ct = default)
        {
            if (query?.QueryTables is null || !query.QueryTables.Any())
                return 0;

            var queryFactory = GetQueryFactory(query);
            var tablesSchema = await _tablesSchemaService.BuildTablesSchema(query, ct);
            var sqlKataQuery = await GetQuery(query, tablesSchema, queryCommand, ct);
            var sqlResult = queryFactory.Compiler.Compile(sqlKataQuery.AsCount());

            queryFactory.Connection.Open();
            var result = await queryFactory.Connection.ExecuteScalarAsync<int>(
                new CommandDefinition(sqlResult.Sql, sqlResult.NamedBindings, cancellationToken: ct));
            queryFactory.Connection.Close();

            return result;
        }

        public async Task<dynamic> GetAggregations(
            Query query,
            IEnumerable<dynamic> aggregations,
            QueryCommand queryCommand = null,
            CancellationToken ct = default)
        {
            if (query?.QueryTables is null || !query.QueryTables.Any() ||
                aggregations is null || !aggregations.Any())
            {
                return null;
            }

            var queryFactory = GetQueryFactory(query);
            var tablesSchema = await _tablesSchemaService.BuildTablesSchema(query, ct);
            var sqlKataQuery = await GetQuery(query, tablesSchema, queryCommand, ct);
            var sqlKataQueryResult = sqlKataQuery.Clone();

            var sqlAliases = new Dictionary<string, IList<string>>();
            foreach (var columnAggregations in aggregations)
            {
                var aggregationExpressions = (string[])columnAggregations.Expressions;
                var columnStaticData = tablesSchema.GetColumn("TODO: set tableId", (string)columnAggregations.ColumnId);

                var columnQueryAlias = columnStaticData.GetQueryAlias();
                sqlAliases[columnQueryAlias] = new List<string>();
                var index = 0;
                foreach (var aggregationExpression in aggregationExpressions)
                {
                    var sqlExpressionAlias = $"{columnQueryAlias}_{index++}";
                    sqlAliases[columnQueryAlias].Add(sqlExpressionAlias);

                    if (IsPredefinedAggregationFunction(aggregationExpression))
                    {
                        sqlKataQueryResult.Select(
                            sqlKataQuery.Clone().AsAggregate(aggregationExpression, new[] { columnStaticData.QueryName }),
                            sqlExpressionAlias);
                    }
                    else
                    {

                        sqlKataQueryResult.Select
                            (sqlKataQuery.Clone().SelectRaw(await FixParamNamesForCustomAggregateExpression(
                                query,
                                tablesSchema,
                                aggregationExpression.Replace("@column", $"@{columnStaticData.ParentTableName}.{columnStaticData.ColumnName}"),
                                ct)),
                            sqlExpressionAlias);
                    }
                }
            }

            var sqlResult = queryFactory.FromQuery(sqlKataQueryResult.Limit(1)).Get().FirstOrDefault()
                as IDictionary<string, object> ?? new Dictionary<string, object>();

            var result = new Dictionary<string, object>();
            foreach (var sqlAliasesGroup in sqlAliases)
            {
                result[sqlAliasesGroup.Key] = sqlAliasesGroup.Value
                    .Select(x => sqlResult.Any() ? sqlResult[x] : string.Empty)
                    .ToArray();
            }

            return result;
        }


        private async Task<string> FixParamNamesForCustomAggregateExpression(Query query, QueryTablesSchema tablesSchema, string expression, CancellationToken ct)
        {
            var queryTableColumns = query.QueryTables.SelectMany(x => x.Columns);
            var matches = new Regex("@([\\w\\d\\.]+)").Matches(expression);

            if (matches.Count != 0)
            {
                var queryTableSchemaColumns = queryTableColumns.Select(tablesSchema.GetColumn);

                foreach (Match match in matches)
                {
                    var paramName = match.Groups[1].Value;
                    var relatedSchemaColumn = queryTableSchemaColumns
                        .SingleOrDefault(x => $"{x.ParentTableName}.{x.ColumnName}" == paramName);
                    if (relatedSchemaColumn is not null)
                    {
                        expression = expression.Replace(
                            match.Value,
                            MakeColumnNameUniversalForKata(relatedSchemaColumn.QueryName));
                    }
                }
            }

            return expression;
        }

        private QueryFactory GetQueryFactory(Query query)
        {
            var databaseSource = GetDatabaseSourceByQuery(query);
            var dbConnection = CreateDbConnection(databaseSource.ConnectionString, databaseSource.DatabaseType);

            var compiler = dbConnection switch
            {
                MySqlConnection _ => new MySqlCompiler(),
                Microsoft.Data.SqlClient.SqlConnection _ => (Compiler)new SqlServerCompiler(),
                _ => new SqliteCompiler() // For testing purposes
            };
            return new QueryFactory(dbConnection, compiler);
        }

        private DatabaseSource GetDatabaseSourceByQuery(Query query)
        {
            var firstTable = query.QueryTables.FirstOrDefault();
            if (firstTable == default)
                throw new ConflictException("The parent query does not contain tables.");

            DbSchema dbSchema = null;

            // If query table has no source code, then by default it's DB DOC schema's table and we get it
            // from DB DOC schema runtime storage
            if (string.IsNullOrEmpty(firstTable.SourceCode))
            {
                dbSchema = _dbSchemaManager.GetTableDbSchema(firstTable.SourceTableId);
            }

            // Otherwise if source code is set (e.g. 'form'), we suppose it should be the main database context.
            // At least we only use the main context for THIS version, because the only queryable tables provider
            // (Forms) fetches form tables from the main context. When we make Forms to fetch queryable tables
            // fro external DBs of DB DOC, then here we will search corresponding DB schema.
            else
            {
                dbSchema = _dbSchemaManager.GetTableDbSchema(firstTable.SourceTableId);
                //TODO: check what is the fix here
                //dbSchema = _dbSchemaManager.GetDbSchemaOfContextModels();
            }

            if (dbSchema == default)
                throw new ConflictException("DB schema not found for the query tables.");

            return dbSchema.DatabaseSource;
        }

        private async Task<SqlKata.Query> GetQuery(Query query,
            QueryTablesSchema tablesSchema,
            QueryCommand queryCommand = null,
            CancellationToken ct = default)
        {
            var sqlKataQuery = new BaseQueryProvider().BuildBaseQuery(query, tablesSchema);

            #region can be reworked to new approach with @user variable added to the query filters 
            // TODO: Recover
            //if (_mainDbContext.Database.GetDbConnection().Database ==
            //    (await _dbDocService.GetTableMetadata(query.QueryTables.First().DbDocTableId, ct)).StaticData.DbName &&
            //    query.ForEndUserOnly)
            //{
            //    var usersTable = await GetUsersTable(ct);

            //    if (usersTable == null)
            //        throw new ConflictException("Users table is not found.");

            //    var authClauseProvider = new CurrentUserAuthProvider(
            //        _dbSchemaManager,
            //        _currentUserId,
            //        usersTable);
            //    await authClauseProvider.Apply(sqlKataQuery, query, ct);
            //}
            #endregion

            new OrderActionProvider(tablesSchema).Apply(sqlKataQuery, queryCommand);
            new FilterActionProvider(query, tablesSchema).Apply(sqlKataQuery, queryCommand);

            return sqlKataQuery;
        }

        private static DbConnection CreateDbConnection(string connectionString, DatabaseType databaseType)
        {
            var builder = new DbContextOptionsBuilder<DbContext>();
            switch (databaseType)
            {
                case DatabaseType.MsSql:
                    builder.UseSqlServer(connectionString);
                    break;

                case DatabaseType.MySql:
                    builder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
                    break;

                default: throw new Exception($"DbConnection for database type {databaseType} not implemented.");
            }

            return new DbContext(builder.Options).Database.GetDbConnection();
        }

        //private async Task<TableMetadataDTO> GetUsersTable(CancellationToken ct) =>
        //    (await _dbDocFolderService.GetDefaultFolder(ct)).Tables.SingleOrDefault(x =>
        //        x.StaticData.ClrType == typeof(User).FullName);

        private bool IsPredefinedAggregationFunction(string expression) =>
            expression.Equals("avg", StringComparison.CurrentCultureIgnoreCase) ||
            expression.Equals("sum", StringComparison.CurrentCultureIgnoreCase) ||
            expression.Equals("min", StringComparison.CurrentCultureIgnoreCase) ||
            expression.Equals("max", StringComparison.CurrentCultureIgnoreCase);

        private static string MakeColumnNameUniversalForKata(string columnName) =>
            string.Join('.', columnName.Split('.').Select(x => $"[{x}]"));
    }
}