using AutoMapper;
using BBWM.Core.Data;
using BBWM.Core.Exceptions;
using BBWM.Core.Services;
using BBWM.DbDoc.DbGraph.Algorithms;
using BBWM.DbDoc.DbSchemas;
using BBWM.DbDoc.DbSchemas.Interfaces;
using BBWM.DbDoc.Enums;
using BBWM.DbDoc.Model;
using BBWM.Reporting.DTO;
using BBWM.Reporting.Enums;
using BBWM.Reporting.Interfaces;
using BBWM.Reporting.Model;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace BBWM.Reporting.Services
{
    public class QueryBuilderService : IQueryBuilderService
    {
        private readonly IDbContext _context;
        private readonly IMapper _mapper;
        private readonly IDataService _dataService;
        private readonly IDbSchemaManager _dbSchemaManager;


        public QueryBuilderService(
            IDbContext context,
            IMapper mapper,
            IDataService dataService,
            IDbSchemaManager dbSchemaManager)
        {
            _context = context;
            _mapper = mapper;
            _dataService = dataService;
            _dbSchemaManager = dbSchemaManager;
        }


        public static IEnumerable<SqlFilterCodeInsertDTO> GetSqlFilterCodeInserts(string sqlCode)
        {
            if (string.IsNullOrEmpty(sqlCode))
                return null;

            var inserts = new List<SqlFilterCodeInsertDTO>();

            var rgxControlName = new Regex(@"@[\w\.]+");

            foreach (Match match in rgxControlName.Matches(sqlCode))
            {
                if (match.Success)
                {
                    inserts.Add(new SqlFilterCodeInsertDTO
                    {
                        VariableType = match.Groups[0].Value.Contains('.') ?
                            SqlFilterVariableType.TableColumn :
                            SqlFilterVariableType.FilterControl,
                        VariableName = match.Groups[0].Value,
                        Position = match.Groups[0].Index
                    });
                }
            }

            inserts.Sort((a, b) => { return a.Position.CompareTo(b.Position); });

            return inserts;
        }

        public static QueryFilterSet MakeFilterSetsTree(IList<QueryFilterSet> sets, bool throwIfUnconnectedItems = false)
        {
            var root = sets.Single(x => x.ParentQueryId is not null);
            var result = Handle(root, sets);

            if (throwIfUnconnectedItems && result != sets.Count)
                throw new InvalidOperationException("The collection contains unconnected items.");

            return root;

            int Handle(QueryFilterSet set, IList<QueryFilterSet> sets)
            {
                var result = 1;
                foreach (var setItem in sets)
                {
                    if (setItem.ParentId == set.Id)
                    {
                        if (set.ChildSets.All(x => x.Id != setItem.Id))
                            set.ChildSets.Add(setItem);

                        setItem.Parent = set;
                        result += Handle(setItem, sets);
                    }
                }
                return result;
            }
        }

        public async Task<QueryTableJoin> AddDuplicateQueryTable(Query query, QueryTableJoinDTO join, CancellationToken ct = default)
        {
            if (query.DbDocFolderId is not null)
                await CheckQueryFolder((Guid)query.DbDocFolderId, ct);

            var fromQueryTable = query.QueryTables.Single(x => x.Id == join.FromQueryTableId);
            var tableMetadata = await _context.Set<TableMetadata>()
                .Include(x => x.Columns.Where(y => !y.Hidden))
                .SingleOrDefaultAsync(x => x.TableId == fromQueryTable.SourceTableId, ct);

            var newFromColumnsMetadata = tableMetadata.Columns
                .Where(x => fromQueryTable.Columns.All(y => y.SourceColumnId != x.ColumnId));
            if (newFromColumnsMetadata.Any())
            {
                foreach (var dbDocColumnMetadata in newFromColumnsMetadata)
                {
                    fromQueryTable.Columns.Add(new QueryTableColumn
                    {
                        SourceColumnId = dbDocColumnMetadata.ColumnId
                    });
                }
            }

            var toQueryTable = await CreateDuplicatedQueryTable(query, fromQueryTable.SourceTableId, ct);
            foreach (var dbDocColumnMetadata in tableMetadata.Columns)
            {
                toQueryTable.Columns.Add(new QueryTableColumn
                {
                    SourceColumnId = dbDocColumnMetadata.ColumnId
                });
            }

            var toQueryTableColumn = toQueryTable.Columns.Single(x => x.SourceColumnId == join.ToDbDocColumnId);
            var resultJoin = new QueryTableJoin
            {
                QueryId = query.Id,
                FromQueryTableId = fromQueryTable.Id,
                FromQueryTableColumn = fromQueryTable.Columns.Single(x => x.SourceColumnId == join.FromDbDocColumnId),
                ToQueryTable = toQueryTable,
                ToQueryTableColumn = toQueryTable.Columns.Single(x => x.SourceColumnId == join.ToDbDocColumnId)
            };

            await _context.Set<QueryTableJoin>().AddAsync(resultJoin, ct);
            await _context.SaveChangesAsync(ct);

            resultJoin.FromQueryTable = fromQueryTable;

            return resultJoin;
        }

        public async Task<QueryFilter> AddQueryFilter(QueryFilterDTO dto, CancellationToken ct = default)
        {
            if (await _context.Set<QueryFilterSet>().AllAsync(x => x.Id != dto.QueryFilterSetId, ct))
                throw new ObjectNotExistsException("The query filter set with specified ID doesn't exist.");

            var queryFilter = new QueryFilter
            {
                QueryFilterSetId = dto.QueryFilterSetId,
                QueryTableColumnId = dto.QueryTableColumnId
            };

            if (dto.QueryTableColumnId is not null) // Filter Control
            {
                if (await _context.Set<QueryTableColumn>().AllAsync(x => x.Id != dto.QueryTableColumnId, ct))
                    throw new ObjectNotExistsException("The related column with specified ID doesn't exist.");

                queryFilter.QueryRuleId = (await GetDefaultQueryRule(ct)).Id;
            }
            else // Custom SQL Filter
            {
                queryFilter.CustomSqlCodeTemplate = "";
            }

            if (dto.QueryFilterBindings.Any()) // Master Detail
            {
                var binding = dto.QueryFilterBindings.First();

                var masterDetailQueryTableColumn = await _context.Set<QueryTableColumn>()
                    .SingleOrDefaultAsync(x => x.Id == binding.MasterDetailQueryTableColumnId, ct);

                if (masterDetailQueryTableColumn is null)
                    throw new ObjectNotExistsException("The master-section's column with specified ID doesn't exist.");

                var masterDetailSection = await _context.Set<Section>()
                    .SingleOrDefaultAsync(x => x.Id == binding.MasterDetailSectionId, ct);

                if (masterDetailSection is null)
                    throw new ObjectNotExistsException("The master-section with specified section ID doesn't exist.");

                queryFilter.QueryFilterBindings = new List<QueryFilterBinding>
                {
                    new QueryFilterBinding
                    {
                        BindingType = QueryFilterBindingType.MasterDetailGrid,
                        QueryFilter = queryFilter,
                        MasterDetailQueryTableColumn = masterDetailQueryTableColumn,
                        MasterDetailSection = masterDetailSection
                    }
                };
            }

            await _context.Set<QueryFilter>().AddAsync(queryFilter, ct);
            await _context.SaveChangesAsync(ct);

            return queryFilter;
        }

        public async Task<QueryFilterSet> AddQueryFilterSet(int parentQueryFilterSetId, CancellationToken ct = default)
        {
            var parentQueryFilterSet = await _context.Set<QueryFilterSet>().SingleOrDefaultAsync(x => x.Id == parentQueryFilterSetId, ct);

            if (parentQueryFilterSet is null)
                throw new ObjectNotExistsException("The parent query filter set with specified ID doesn't exist.");

            var queryFilterSet = new QueryFilterSet
            {
                ParentId = parentQueryFilterSetId,
                QueryId = parentQueryFilterSet.QueryId
            };
            await _context.Set<QueryFilterSet>().AddAsync(queryFilterSet, ct);
            await _context.SaveChangesAsync(ct);

            return queryFilterSet;
        }

        public async Task<(QueryTable queryTable, IList<QueryTableJoin> joins)> AddQueryTable(
            Query query,
            int tableMetadataId,
            CancellationToken ct = default)
        {
            if (query.DbDocFolderId is not null)
                await CheckQueryFolder((Guid)query.DbDocFolderId, ct);

            var tableMetadata = await _context.Set<TableMetadata>()
                .Include(x => x.Columns.Where(y => !y.Hidden))
                .SingleOrDefaultAsync(x => x.Id == tableMetadataId, ct)
                ?? throw new ObjectNotExistsException($"The table with ID {tableMetadataId} doesn't exist.");

            if (query.DbDocFolderId != tableMetadata.FolderId)
            {
                if (await _context.Set<QueryTable>().AnyAsync(x => x.QueryId == query.Id, ct))
                    throw new BusinessException("You can't add tables from different folders.");
                else
                {
                    await CheckQueryFolder(tableMetadata.FolderId, ct);

                    query.DbDocFolderId = tableMetadata.FolderId;
                    await _context.SaveChangesAsync(ct);
                }
            }

            var queryTable = await GetOrCreateQueryTable(query, tableMetadata.TableId, null, ct);

            foreach (var queryTableColumn in queryTable.Columns)
            {
                queryTableColumn.OnlyForJoin = false;
            }

            var newColumnsMetadata = tableMetadata.Columns
                .Where(x => queryTable.Columns.All(y => y.SourceColumnId != x.ColumnId));
            if (newColumnsMetadata.Any())
            {
                foreach (var dbDocColumnMetadata in newColumnsMetadata)
                {
                    queryTable.Columns.Add(new QueryTableColumn
                    {
                        SourceColumnId = dbDocColumnMetadata.ColumnId
                    });
                }
            }

            if (queryTable.Id == default)
                await _context.Set<QueryTable>().AddAsync(queryTable, ct);

            await _context.SaveChangesAsync(ct);

            return (queryTable, await AddJoinsForQueryTable(query, queryTable, ct));
        }

        public async Task<IList<QueryTable>> AddQueryTablesFromSource(Query query, QueryableTableSource[] sources, CancellationToken ct = default)
        {
            IList<QueryTable> queryTables = new List<QueryTable>();

            // TODO: for this PoC code, table's schema is taken from request, but then we should then
            // request all registered sources by source code and table names and get table columns
            foreach (var source in sources)
            {
                foreach (var table in source.Tables)
                {
                    var queryTable = new QueryTable
                    {
                        Query = query,
                        SourceCode = source.SourceCode,
                        SourceTableId = table.SchemaTable.TableId
                    };

                    foreach (var column in table.SchemaColumns)
                    {
                        queryTable.Columns.Add(new QueryTableColumn
                        {
                            SourceColumnId = column.ColumnId
                        });
                    }

                    await _context.Set<QueryTable>().AddAsync(queryTable, ct);

                    queryTables.Add(queryTable);
                }
            }

            await _context.SaveChangesAsync(ct);

            return queryTables;
        }

        public async Task<(QueryTableColumn queryTableColumn, IEnumerable<QueryTableJoin> joins)> AddQueryTableColumn(
            Query query,
            int columnMetadataId,
            int? parentQueryTableId = null,
            CancellationToken ct = default)
        {
            if (query.DbDocFolderId is not null)
                await CheckQueryFolder((Guid)query.DbDocFolderId, ct);

            var сolumnMetadata = await _context.Set<ColumnMetadata>()
                .Include(x => x.Table)
                .SingleOrDefaultAsync(x => x.Id == columnMetadataId, ct);

            if (сolumnMetadata is null)
                throw new ObjectNotExistsException($"The column '{columnMetadataId}' doesn't exist.");

            if (query.DbDocFolderId != сolumnMetadata.Table.FolderId)
            {
                if (await _context.Set<QueryTable>().AnyAsync(x => x.QueryId == query.Id, ct))
                    throw new BusinessException("You can't add columns from different folders.");
                else
                {
                    await CheckQueryFolder(сolumnMetadata.Table.FolderId, ct);

                    query.DbDocFolderId = сolumnMetadata.Table.FolderId;
                    await _context.SaveChangesAsync(ct);
                }
            }

            var queryTable = await GetOrCreateQueryTable(query, сolumnMetadata.Table.TableId, parentQueryTableId, ct);

            if (queryTable is null)
                throw new ObjectNotExistsException("The duplicate query table with specified ID doesn't exist.");

            queryTable.OnlyForJoin = false;

            var queryTableColumn = queryTable.Columns.SingleOrDefault(x => x.SourceColumnId == сolumnMetadata.ColumnId);

            if (queryTableColumn != null)
            {
                if (queryTableColumn.OnlyForJoin)
                {
                    queryTableColumn.OnlyForJoin = false;
                    await _context.SaveChangesAsync(ct);
                    return (queryTableColumn, new List<QueryTableJoin>());
                }
                else
                {
                    throw new BusinessException("The query table column with the same table ID already exists.");
                }
            }

            queryTableColumn = new QueryTableColumn
            {
                SourceColumnId = сolumnMetadata.ColumnId
            };
            queryTable.Columns.Add(queryTableColumn);

            if (queryTable.Id == default)
                await _context.Set<QueryTable>().AddAsync(queryTable, ct);

            await _context.SaveChangesAsync(ct);

            return (queryTableColumn, await AddJoinsForQueryTable(query, queryTable, ct));
        }

        public async Task<QueryTableJoin> AddQueryTableJoin(QueryTableJoinDTO joinDto, CancellationToken ct = default)
        {
            var query = await _context.Set<Query>().SingleOrDefaultAsync(x => x.Id == joinDto.QueryId, ct);

            if (query is null)
                throw new ObjectNotExistsException("The related query with the specified ID doesn't exist.");

            return await SaveQueryTableJoin(query, joinDto, null, ct);
        }

        public async Task<Query> CreateQuery(Guid dbDocFolderId, CancellationToken ct = default)
        {
            await CheckQueryFolder(dbDocFolderId, ct);

            var queryFilterSet = new QueryFilterSet { ConditionalOperator = QueryConditionalOperator.And };
            var query = new Query
            {
                DbDocFolderId = dbDocFolderId,
                RootFilterSet = queryFilterSet,
                QueryFilterSets = new List<QueryFilterSet> { queryFilterSet }
            };

            return query;
        }

        public async Task DeleteQueryFilter(int queryFilterId, CancellationToken ct = default)
        {
            if (await _context.Set<QueryFilter>().AllAsync(x => x.Id != queryFilterId))
                throw new ObjectNotExistsException("The query filter with specified ID doesn't exist.");

            await _dataService.Delete<QueryFilter>(queryFilterId, ct);
        }

        public async Task DeleteQueryFilterSet(int id, CancellationToken ct = default)
        {
            var queryFilterSet = await _context.Set<QueryFilterSet>()
                .Include(x => x.Query).ThenInclude(x => x.QueryFilterSets)
                .SingleOrDefaultAsync(x => x.Id == id, ct);

            if (queryFilterSet is null)
                throw new ObjectNotExistsException("The query filter set with specified ID doesn't exist.");

            if (queryFilterSet.ParentQueryId is not null)
                throw new BusinessException("The root query filter set cannot be deleted.");

            DeleteQueryFilterSetTree(queryFilterSet, queryFilterSet.Query.QueryFilterSets);
            await _context.SaveChangesAsync(ct);


            void DeleteQueryFilterSetTree(QueryFilterSet node, IList<QueryFilterSet> allSets)
            {
                foreach (var queryFilterSetItem in allSets.Where(x => x.ParentId == node.Id))
                {
                    DeleteQueryFilterSetTree(queryFilterSetItem, allSets);
                }

                _context.Set<QueryFilterSet>().Remove(node);
            }
        }

        public async Task DeleteQueryTable(int queryTableId, CancellationToken ct = default)
        {
            var queryTable = await _context.Set<QueryTable>()
                .Include(x => x.Columns)
                .SingleOrDefaultAsync(x => x.Id == queryTableId, ct);

            if (queryTable is null)
                throw new ObjectNotExistsException("The query table with specified ID doesn't exist.");

            var queryId = queryTable.QueryId;
            var columnsIds = queryTable.Columns.Select(y => y.Id);

            await _dataService.Delete<QueryTable>(
                queryTableId,
                (entity, context) =>
                {
                    foreach (var queryTableJoin in _context.Set<QueryTableJoin>().Where(x => x.FromQueryTableId == entity.Id || x.ToQueryTableId == entity.Id))
                    {
                        _context.Set<QueryTableJoin>().Remove(queryTableJoin);
                    }

                    foreach (var filter in _context.Set<QueryFilter>().Where(x => columnsIds.Contains((int)x.QueryTableColumnId)))
                    {
                        _context.Set<QueryFilter>().Remove(filter);
                    }

                    foreach (var binding in _context.Set<QueryFilterBinding>()
                        .Include(x => x.QueryFilter)
                        .Where(x => x.MasterDetailQueryTableColumnId.HasValue && columnsIds.Contains(x.MasterDetailQueryTableColumnId.Value)))
                    {
                        _context.Set<QueryFilter>().Remove(binding.QueryFilter);
                    }
                },
                ct);
        }

        public async Task DeleteQueryTableColumn(int queryTableColumnId, CancellationToken ct = default)
        {
            var queryTableColumn = await _context.Set<QueryTableColumn>()
                .Include(x => x.QueryTable)
                .SingleOrDefaultAsync(x => x.Id == queryTableColumnId, ct);

            if (queryTableColumn is null)
                throw new ObjectNotExistsException("The query table column with specified ID doesn't exist.");

            var queryId = queryTableColumn.QueryTable.QueryId;

            var filters = _context.Set<QueryFilter>().Where(x => x.QueryTableColumnId == queryTableColumnId);
            foreach (var filter in filters)
            {
                _context.Set<QueryFilter>().Remove(filter);
            }

            await _dataService.Delete<QueryTableColumn>(
                queryTableColumnId,
                (entity, context) =>
                {
                    foreach (var queryTableJoin in _context.Set<QueryTableJoin>().Where(x => x.FromQueryTableColumnId == entity.Id || x.ToQueryTableColumnId == entity.Id))
                    {
                        _context.Set<QueryTableJoin>().Remove(queryTableJoin);
                    }

                    foreach (var binding in context.Set<QueryFilterBinding>().Include(x => x.QueryFilter).Where(x => x.MasterDetailQueryTableColumnId == queryTableColumnId))
                    {
                        context.Set<QueryFilter>().Remove(binding.QueryFilter);
                    }

                    if (context.Set<QueryTableColumn>().Count(x => x.QueryTableId == entity.QueryTableId) == 1)
                    {
                        _context.Set<QueryTable>().Remove(
                            _context.Set<QueryTable>().Single(x => x.Id == entity.QueryTableId));
                    }
                },
                ct);
        }

        public async Task DeleteQueryTableJoin(int queryTableJoinId, CancellationToken ct = default)
        {
            var queryTableJoin = await _context.Set<QueryTableJoin>()
                .Include(x => x.Query).ThenInclude(x => x.QueryTableJoins)
                .Include(x => x.FromQueryTable).ThenInclude(x => x.Columns)
                .Include(x => x.ToQueryTable).ThenInclude(x => x.Columns)
                .SingleOrDefaultAsync(x => x.Id == queryTableJoinId, ct);

            if (queryTableJoin is null)
                throw new ObjectNotExistsException("The query table join with specified ID doesn't exist.");

            _context.Entry(queryTableJoin).State = EntityState.Deleted;

            if (queryTableJoin.FromQueryTableColumn.OnlyForJoin && queryTableJoin.Query.QueryTableJoins.All(x => x.Id == queryTableJoin.Id ||
                x.FromQueryTableColumnId != queryTableJoin.FromQueryTableColumnId && x.ToQueryTableColumnId != queryTableJoin.FromQueryTableColumnId))
            {
                _context.Entry(queryTableJoin.FromQueryTableColumn).State = EntityState.Deleted;

                if (queryTableJoin.FromQueryTable.OnlyForJoin && queryTableJoin.Query.QueryTableJoins.All(x => x.Id == queryTableJoin.Id ||
                    x.FromQueryTableId != queryTableJoin.FromQueryTableId && x.ToQueryTableId != queryTableJoin.FromQueryTableId))
                {
                    _context.Entry(queryTableJoin.FromQueryTable).State = EntityState.Deleted;
                }
            }

            if (queryTableJoin.ToQueryTableColumn.OnlyForJoin && queryTableJoin.Query.QueryTableJoins.All(x => x.Id == queryTableJoin.Id ||
                x.FromQueryTableColumnId != queryTableJoin.ToQueryTableColumnId && x.ToQueryTableColumnId != queryTableJoin.ToQueryTableColumnId))
            {
                _context.Entry(queryTableJoin.ToQueryTableColumn).State = EntityState.Deleted;

                if (queryTableJoin.ToQueryTable.OnlyForJoin && queryTableJoin.Query.QueryTableJoins.All(x => x.Id == queryTableJoin.Id ||
                    x.FromQueryTableId != queryTableJoin.ToQueryTableId && x.ToQueryTableId != queryTableJoin.ToQueryTableId))
                {
                    _context.Entry(queryTableJoin.ToQueryTable).State = EntityState.Deleted;
                }
            }

            await _context.SaveChangesAsync(ct);
        }

        public IEnumerable<string> GetReachableTables(string uniqueTableId) =>
            _dbSchemaManager.GetTableDbSchema(uniqueTableId)
                .ToDijkstra(uniqueTableId)
                .GetReachableVertices()
                .Select(x => x.Data.Table.TableId);

        public async Task<QueryFilterBinding> UpdateMasterDetailQueryFilterBinding(int queryFilterBindingId, QueryFilterBindingDTO dto, CancellationToken ct = default)
        {
            var queryFilterBinding = await _context.Set<QueryFilterBinding>().SingleOrDefaultAsync(x => x.Id == queryFilterBindingId, ct);

            if (queryFilterBinding is null)
                throw new ObjectNotExistsException("The query filter binding with specified ID doesn't exist.");

            if (await _context.Set<Section>().AllAsync(x => x.Id != dto.MasterDetailSectionId, ct))
                throw new ObjectNotExistsException("The master-section with specified section ID doesn't exist.");

            if (await _context.Set<QueryTableColumn>().AllAsync(x => x.Id != dto.MasterDetailQueryTableColumnId, ct))
                throw new ObjectNotExistsException("The master-section's column with specified ID doesn't exist.");

            dto.Id = queryFilterBinding.Id;
            dto.QueryFilterId = queryFilterBinding.QueryFilterId;
            dto.BindingType = queryFilterBinding.BindingType;
            dto.FilterControlId = queryFilterBinding.FilterControlId;

            _mapper.Map(dto, queryFilterBinding);

            await _context.SaveChangesAsync(ct);

            return await _context.Set<QueryFilterBinding>()
                .Include(x => x.MasterDetailQueryTableColumn)
                .SingleOrDefaultAsync(x => x.Id == queryFilterBindingId, ct);
        }

        public async Task<QueryFilter> UpdateQueryFilter(int queryFilterId, QueryFilterDTO dto, CancellationToken ct = default)
        {
            var queryFilter = await _context.Set<QueryFilter>()
                .Include(x => x.QueryFilterSet)
                .SingleOrDefaultAsync(x => x.Id == queryFilterId, ct);

            if (queryFilter is null)
                throw new ObjectNotExistsException("The query filter with specified ID doesn't exist.");

            var relatedQueryTableColumn = await _context.Set<QueryTableColumn>()
                .Include(x => x.QueryTable).ThenInclude(x => x.Query)
                .SingleOrDefaultAsync(x => x.Id == dto.QueryTableColumnId, ct);

            if (relatedQueryTableColumn is null)
                throw new ObjectNotExistsException("The related column with specified ID doesn't exist.");

            var dbDocColumnStaticData = _dbSchemaManager.GetColumn(relatedQueryTableColumn.SourceColumnId);

            if (dbDocColumnStaticData is null)
                throw new ConflictException("The DBDoc static data service doesn't contain data about related column.");

            var queryRule = await _context.Set<QueryRule>()
                .Include(x => x.RuleTypes)
                .SingleOrDefaultAsync(x => x.Id == dto.QueryRuleId, ct);

            if (queryRule is null)
                throw new ObjectNotExistsException("The related query rule with specified ID doesn't exist.");

            if (!IsRuleCompatibleWithColumn(dbDocColumnStaticData.ClrTypeGroup, queryRule.RuleTypes))
                throw new BusinessException("The related query rule is incompatible with the column type.");

            dto.Id = queryFilter.Id;
            dto.QueryFilterSetId = queryFilter.QueryFilterSetId;

            _mapper.Map(dto, queryFilter);

            await _context.SaveChangesAsync(ct);

            return queryFilter;
        }

        public async Task<QueryFilter> UpdateSqlFilter(int queryFilterId, QueryFilterDTO dto, CancellationToken ct = default)
        {
            var queryFilter = await _context.Set<QueryFilter>()
                .Include(x => x.QueryFilterSet)
                .SingleOrDefaultAsync(x => x.Id == queryFilterId, ct);

            if (queryFilter is null)
                throw new ObjectNotExistsException("The query filter with specified ID doesn't exist.");

            dto.Id = queryFilter.Id;
            dto.QueryFilterSetId = queryFilter.QueryFilterSetId;

            _mapper.Map(dto, queryFilter);

            await _context.SaveChangesAsync(ct);

            return queryFilter;
        }

        public async Task<QueryFilterSet> UpdateQueryFilterSet(int id, QueryFilterSetDTO dto, CancellationToken ct = default)
        {
            var queryFilterSet = await _context.Set<QueryFilterSet>().SingleOrDefaultAsync(x => x.Id == id, ct);

            if (queryFilterSet is null)
                throw new ObjectNotExistsException("The query filter set with specified ID doesn't exist.");

            queryFilterSet.ConditionalOperator = dto.ConditionalOperator;
            await _context.SaveChangesAsync(ct);

            return queryFilterSet;
        }

        public async Task<QueryTableJoin> UpdateQueryTableJoin(QueryTableJoinDTO joinDto, CancellationToken ct = default)
        {
            var queryTableJoin = await _context.Set<QueryTableJoin>()
                .Include(x => x.Query).ThenInclude(x => x.QueryTableJoins).ThenInclude(x => x.FromQueryTable).ThenInclude(x => x.Columns)
                .Include(x => x.Query).ThenInclude(x => x.QueryTableJoins).ThenInclude(x => x.ToQueryTable).ThenInclude(x => x.Columns)
                .Include(x => x.FromQueryTable).ThenInclude(x => x.Columns)
                .Include(x => x.ToQueryTable).ThenInclude(x => x.Columns)
                .SingleOrDefaultAsync(x => x.Id == joinDto.Id, ct);

            if (queryTableJoin is null)
                throw new ObjectNotExistsException("The query table join with specified ID doesn't exist.");

            return await SaveQueryTableJoin(queryTableJoin.Query, joinDto, queryTableJoin, ct);
        }




        private async Task<IList<QueryTableJoin>> AddJoinsForQueryTable(Query query, QueryTable queryTable, CancellationToken ct = default)
        {
            if (query.QueryTableJoins.Any(x => x.ToQueryTableId == queryTable.Id || x.FromQueryTableId == queryTable.Id))
            {
                return new List<QueryTableJoin>();
            }

            var initialQueryTables = query.QueryTables.Where(x => x.SourceTableId != queryTable.SourceTableId).ToList();
            var dbScheme = _dbSchemaManager.GetTableDbSchema(queryTable.SourceTableId);
            var dijkstra = dbScheme.ToDijkstra(queryTable.SourceTableId);
            var closestVertex = dijkstra.GetClosestVertex(initialQueryTables.Select(x => x.SourceTableId));
            var result = new List<QueryTableJoin>();

            if (closestVertex == null) return result;

            var path = dijkstra.GetPathTo(closestVertex.Name);

            if (path?.Any() is false) return result;

            var firstPathEdge = path.First();

            QueryTable previousQueryTable = null;
            foreach (var initialQueryTable in initialQueryTables)
            {
                var bindingEdge = firstPathEdge.Start.Edges.FirstOrDefault(x => x.End.Name == initialQueryTable.SourceTableId);
                if (bindingEdge != null)
                {
                    var newJoin = await CreateJoinForQueryTable(
                        query,
                        initialQueryTable,
                        bindingEdge.Data.EndTableColumn.ColumnId,
                        bindingEdge.Data.StartTableColumn.TableId,
                        bindingEdge.Data.StartTableColumn.ColumnId,
                        bindingEdge.Data.IsRequired ? QueryJoinTypeEnum.Inner : QueryJoinTypeEnum.Left,
                        ct);

                    result.Add(newJoin);

                    previousQueryTable = newJoin.ToQueryTable;
                }
            }

            foreach (var pathItem in path.Skip(1))
            {
                var newJoin = await CreateJoinForQueryTable(
                        query,
                        previousQueryTable,
                        pathItem.Data.EndTableColumn.ColumnId,
                        pathItem.Data.StartTableColumn.TableId,
                        pathItem.Data.StartTableColumn.ColumnId,
                        pathItem.Data.IsRequired ? QueryJoinTypeEnum.Inner : QueryJoinTypeEnum.Left,
                        ct);

                result.Add(newJoin);

                previousQueryTable = newJoin.ToQueryTable;
            }

            return result;
        }

        private async Task CheckQueryFolder(Guid folderId, CancellationToken ct)
        {
            var folder = await _context.Set<Folder>().SingleOrDefaultAsync(x => x.Id == folderId, ct);

            if (folder is null)
                throw new ObjectNotExistsException("The folder with ID '{folderId}' doesn't exist.");

            if (!folder.Owners.Contains(ModuleLinkage.DbDocFolderOwnerName))
                throw new BusinessException("Reporting doesn't own this folder.");
        }

        private async Task<QueryTable> CreateDuplicatedQueryTable(Query query, string dbDocTableId, CancellationToken ct)
        {
            #region calculate table alias name
            var tableQueryBase = _context.Set<QueryTable>().Where(x =>
                x.QueryId == query.Id && x.Query.DbDocFolderId == query.DbDocFolderId);

            var dbDocTable = _dbSchemaManager.GetTable(dbDocTableId);
            var aliasFixedPart = dbDocTable.TableName;
            string alias;
            var aliasPostfix = 1;

            do
            {
                alias = aliasFixedPart + aliasPostfix++;
            }
            while (await tableQueryBase.AnyAsync(x => x.Alias == alias, ct));
            #endregion

            return new QueryTable
            {
                Query = query,
                SourceTableId = dbDocTableId,
                Alias = alias
            };
        }

        private async Task<QueryTableJoin> CreateJoinForQueryTable(
            Query query,
            QueryTable fromQueryTable,
            string fromDbSchemaColumnId,
            string toDbSchemaTableId,
            string toDbSchemaColumnId,
            QueryJoinTypeEnum joinType,
            CancellationToken ct)
        {
            var toQueryTable = query.QueryTables.SingleOrDefault(x => x.SourceTableId == toDbSchemaTableId);

            if (toQueryTable == null)
            {
                var joinOnlyToQueryTable = new QueryTable
                {
                    OnlyForJoin = true,
                    QueryId = query.Id,
                    SourceTableId = toDbSchemaTableId
                };

                toQueryTable = joinOnlyToQueryTable;
            }

            return await CreateJoinForQueryTable(query, fromQueryTable, fromDbSchemaColumnId, toQueryTable, toDbSchemaColumnId, joinType, ct);
        }

        private async Task<QueryTableJoin> CreateJoinForQueryTable(
            Query query,
            QueryTable fromQueryTable,
            string fromDbSchemaColumnId,
            QueryTable toQueryTable,
            string toDbSchemaColumnId,
            QueryJoinTypeEnum joinType,
            CancellationToken ct)
        {
            var queryTableJoin = new QueryTableJoin
            {
                QueryId = query.Id,
                JoinType = joinType,
                FromQueryTable = fromQueryTable,
                ToQueryTable = toQueryTable,
            };

            queryTableJoin.FromQueryTableColumn = fromQueryTable.Columns.SingleOrDefault(x => x.SourceColumnId == fromDbSchemaColumnId);
            if (queryTableJoin.FromQueryTableColumn == null)
            {
                var joinOnlyFromQueryTableColumn = new QueryTableColumn
                {
                    OnlyForJoin = true,
                    QueryTableId = queryTableJoin.FromQueryTable.Id,
                    SourceColumnId = fromDbSchemaColumnId
                };

                queryTableJoin.FromQueryTableColumn = joinOnlyFromQueryTableColumn;
            }

            queryTableJoin.ToQueryTableColumn = queryTableJoin.ToQueryTable.Columns.SingleOrDefault(x => x.SourceColumnId == toDbSchemaColumnId);
            if (queryTableJoin.ToQueryTableColumn == null)
            {
                var joinOnlyToQueryTableColumn = new QueryTableColumn
                {
                    OnlyForJoin = true,
                    QueryTableId = queryTableJoin.ToQueryTable.Id,
                    SourceColumnId = toDbSchemaColumnId
                };

                queryTableJoin.ToQueryTableColumn = joinOnlyToQueryTableColumn;
                toQueryTable.Columns.Add(joinOnlyToQueryTableColumn);
            }

            var alreadyExistingQueryTableJoin = query.QueryTableJoins.SingleOrDefault(x =>
                x.FromQueryTableId == queryTableJoin.FromQueryTable.Id && x.FromQueryTableColumnId == queryTableJoin.FromQueryTableColumn.Id &&
                x.ToQueryTableId == queryTableJoin.ToQueryTable.Id && x.ToQueryTableColumnId == queryTableJoin.ToQueryTableColumn.Id ||
                x.FromQueryTableId == queryTableJoin.ToQueryTable.Id && x.FromQueryTableColumnId == queryTableJoin.ToQueryTableColumn.Id &&
                x.ToQueryTableId == queryTableJoin.FromQueryTable.Id && x.ToQueryTableColumnId == queryTableJoin.FromQueryTableColumn.Id);

            if (alreadyExistingQueryTableJoin == null)
            {
                await _context.Set<QueryTableJoin>().AddAsync(queryTableJoin, ct);
                await _context.SaveChangesAsync(ct);
            }
            else
            {
                queryTableJoin = alreadyExistingQueryTableJoin;
            }

            return queryTableJoin;
        }

        private async Task<QueryRule> GetDefaultQueryRule(CancellationToken ct = default)
        {
            var defaultQueryRule = await _context.Set<QueryRule>()
                .Include(x => x.RuleTypes)
                .FirstOrDefaultAsync(x => x.Code == QueryRuleCode.Equals &&
                    x.RuleTypes.Any(y => y.Type == QueryRuleDataType.String) &&
                    x.RuleTypes.Any(y => y.Type == QueryRuleDataType.Numeric) &&
                    x.RuleTypes.Any(y => y.Type == QueryRuleDataType.Datetime) &&
                    x.RuleTypes.Any(y => y.Type == QueryRuleDataType.Boolean), ct);

            if (defaultQueryRule is null)
                throw new ConflictException("The DB doesn't contain the default universal 'Equals' QueryRule entity suitable for all data types.");

            return defaultQueryRule;
        }

        private GraphDijkstra GetDijkstra(string rootTableId) => _dbSchemaManager.GetTableDbSchema(rootTableId).ToDijkstra(rootTableId);

        private async Task<QueryTable> GetOrCreateQueryTable(
            Query query,
            string dbDocTableId,
            int? parentQueryTableId = null,
            CancellationToken ct = default)
        {
            var queryTable = await _context.Set<QueryTable>()
                .Include(x => x.Query)
                .Include(x => x.Columns)
                .SingleOrDefaultAsync(x => parentQueryTableId != null && x.Id == parentQueryTableId ||
                    parentQueryTableId == null && x.QueryId == query.Id && x.SourceTableId == dbDocTableId && x.Query.DbDocFolderId == query.DbDocFolderId, ct);

            if (queryTable is null)
            {
                queryTable = new QueryTable
                {
                    Query = query,
                    SourceTableId = dbDocTableId
                };
            }

            if (queryTable.OnlyForJoin)
            {
                queryTable.OnlyForJoin = false;
            }

            return queryTable;
        }

        private bool IsRuleCompatibleWithColumn(ClrTypeGroup clrTypeGroup, IEnumerable<QueryRuleType> ruleTypes) =>
            clrTypeGroup switch
            {
                ClrTypeGroup.Numeric => ruleTypes.Any(x => x.Type == QueryRuleDataType.Numeric),
                ClrTypeGroup.Date => ruleTypes.Any(x => x.Type == QueryRuleDataType.Datetime),
                ClrTypeGroup.Bool => ruleTypes.Any(x => x.Type == QueryRuleDataType.Boolean),
                _ => ruleTypes.Any(x => x.Type == QueryRuleDataType.String),
            };

        private async Task<QueryTableJoin> SaveQueryTableJoin(Query query, QueryTableJoinDTO dto, QueryTableJoin oldJoin = null, CancellationToken ct = default)
        {
            if (!string.IsNullOrEmpty(dto.FromDbDocTableId) && !string.IsNullOrEmpty(dto.ToDbDocTableId))
                throw new BusinessException("A saving join relation must contain at least one query table side be specified.");

            var savingJoin = oldJoin ?? new QueryTableJoin
            {
                QueryId = query.Id,
                JoinType = dto.JoinType
            };

            var oldJoinQueryTables = oldJoin != null ? new[] { oldJoin.FromQueryTable, oldJoin.ToQueryTable } : new QueryTable[0];

            QueryTable fromQueryTable;
            if (dto.FromDbDocTableId != null)
            {
                fromQueryTable = query.QueryTables.FirstOrDefault(x => x.SourceTableId == dto.FromDbDocTableId);

                if (fromQueryTable is null)
                {
                    fromQueryTable = new QueryTable
                    {
                        QueryId = query.Id,
                        SourceTableId = dto.FromDbDocTableId,
                        OnlyForJoin = true
                    };

                    await _context.Set<QueryTable>().AddAsync(fromQueryTable, ct);
                    await _context.SaveChangesAsync(ct);
                }
            }
            else
            {
                fromQueryTable = await _context.Set<QueryTable>().SingleAsync(x => x.Id == dto.FromQueryTableId);
            }

            savingJoin.FromQueryTableId = fromQueryTable.Id;

            var fromQueryTableIsForm = fromQueryTable.SourceCode == "form";

            if (!fromQueryTableIsForm)
            {
                var fromDbDocQueryTable = _dbSchemaManager.GetTable(fromQueryTable.SourceTableId);

                if (fromDbDocQueryTable is null)
                    throw new ObjectNotExistsException("The DBDoc table with the specified DBDoc table ID for the join 'From' side doesn't exist.");
            }

            QueryTableColumn fromQueryTableColumn;
            if (dto.FromDbDocColumnId != null)
            {
                fromQueryTableColumn = fromQueryTable.Columns.FirstOrDefault(x => x.SourceColumnId == dto.FromDbDocColumnId);

                fromQueryTableColumn = new QueryTableColumn
                {
                    QueryTableId = fromQueryTable.Id,
                    SourceColumnId = dto.FromDbDocColumnId,
                    OnlyForJoin = true
                };

                await _context.Set<QueryTableColumn>().AddAsync(fromQueryTableColumn, ct);
                await _context.SaveChangesAsync(ct);

                fromQueryTable.Columns.Add(fromQueryTableColumn);
            }
            else
            {
                fromQueryTableColumn = await _context.Set<QueryTableColumn>().SingleAsync(x => x.Id == dto.FromQueryTableColumnId);
            }

            savingJoin.FromQueryTableColumnId = fromQueryTableColumn.Id;

            if (!fromQueryTableIsForm)
            {
                var fromDbDocQueryTableColumn = _dbSchemaManager.GetColumn(fromQueryTableColumn.SourceColumnId);

                if (fromDbDocQueryTableColumn is null)
                    throw new ObjectNotExistsException("The DBDoc column with the specified DBDoc column ID for the join 'From' side doesn't exist.");

                if (fromDbDocQueryTableColumn.TableId != fromQueryTable.SourceTableId)
                    throw new BusinessException("The DBDoc column with the specified DBDoc column ID for the join 'From' side belongs to another DBDoc table.");
            }

            QueryTable toQueryTable;
            if (dto.ToDbDocTableId != null)
            {
                toQueryTable = query.QueryTables.FirstOrDefault(x => x.SourceTableId == dto.ToDbDocTableId);

                if (toQueryTable is null)
                {
                    toQueryTable = new QueryTable
                    {
                        QueryId = query.Id,
                        SourceTableId = dto.ToDbDocTableId,
                        OnlyForJoin = true
                    };

                    await _context.Set<QueryTable>().AddAsync(toQueryTable, ct);
                    await _context.SaveChangesAsync(ct);
                }
            }
            else
            {
                toQueryTable = await _context.Set<QueryTable>().SingleAsync(x => x.Id == dto.ToQueryTableId);
            }

            savingJoin.ToQueryTableId = toQueryTable.Id;

            var toQueryTableIsForm = toQueryTable.SourceCode == "form";

            if (!toQueryTableIsForm)
            {
                var toDbDocQueryTable = _dbSchemaManager.GetTable(toQueryTable.SourceTableId);

                if (toDbDocQueryTable is null)
                    throw new ObjectNotExistsException("The DBDoc table with the specified DBDoc table ID for the join 'To' side doesn't exist.");
            }

            QueryTableColumn toQueryTableColumn;
            if (dto.ToDbDocColumnId != null)
            {
                toQueryTableColumn = toQueryTable.Columns.FirstOrDefault(x => x.SourceColumnId == dto.ToDbDocColumnId);

                toQueryTableColumn = new QueryTableColumn
                {
                    QueryTableId = fromQueryTable.Id,
                    SourceColumnId = dto.ToDbDocColumnId,
                    OnlyForJoin = true
                };

                await _context.Set<QueryTableColumn>().AddAsync(toQueryTableColumn, ct);
                await _context.SaveChangesAsync(ct);

                toQueryTable.Columns.Add(toQueryTableColumn);
            }
            else
            {
                toQueryTableColumn = await _context.Set<QueryTableColumn>().SingleAsync(x => x.Id == dto.ToQueryTableColumnId);
            }

            savingJoin.ToQueryTableColumnId = toQueryTableColumn.Id;

            if (!toQueryTableIsForm)
            {
                var toDbDocQueryTableColumn = _dbSchemaManager.GetColumn(toQueryTableColumn.SourceColumnId);

                if (toDbDocQueryTableColumn is null)
                    throw new ObjectNotExistsException("The DBDoc column with the specified DBDoc column ID for the join 'To' side doesn't exist.");

                if (toDbDocQueryTableColumn.TableId != toQueryTable.SourceTableId)
                    throw new BusinessException("The DBDoc column with the specified DBDoc column ID for the join 'To' side belongs to another DBDoc table.");
            }

            if (query.QueryTableJoins.Any(x => x.Id != savingJoin.Id &&
                (x.FromQueryTableId == savingJoin.FromQueryTableId && x.ToQueryTableId == savingJoin.ToQueryTableId &&
                x.FromQueryTableColumnId == savingJoin.FromQueryTableColumnId && x.ToQueryTableColumnId == savingJoin.ToQueryTableColumnId) ||
                (x.FromQueryTableId == savingJoin.ToQueryTableId && x.ToQueryTableId == savingJoin.FromQueryTableId &&
                x.FromQueryTableColumnId == savingJoin.ToQueryTableColumnId && x.ToQueryTableColumnId == savingJoin.FromQueryTableColumnId)))
            {
                throw new BusinessException("The join with the same relations already exists.");
            }

            if (oldJoin == null)
            {
                await _context.Set<QueryTableJoin>().AddAsync(savingJoin, ct);
                await _context.SaveChangesAsync(ct);
            }
            else
            {
                foreach (var oldQueryTable in oldJoinQueryTables)
                {
                    if (oldQueryTable.OnlyForJoin && query.QueryTableJoins.All(x => x.FromQueryTableId != oldQueryTable.Id && x.ToQueryTableId != oldQueryTable.Id))
                    {
                        _context.Entry(oldQueryTable).State = EntityState.Deleted;
                        continue;
                    }

                    foreach (var oldQueryTableColumn in oldQueryTable.Columns)
                    {
                        if (oldQueryTableColumn.OnlyForJoin && query.QueryTableJoins.All(x => x.FromQueryTableColumnId != oldQueryTableColumn.Id && x.ToQueryTableColumnId != oldQueryTableColumn.Id))
                        {
                            _context.Entry(oldQueryTableColumn).State = EntityState.Deleted;
                        }
                    }
                }

                await _context.SaveChangesAsync(ct);
            }

            savingJoin.FromQueryTable = fromQueryTable;
            savingJoin.FromQueryTableColumn = fromQueryTableColumn;
            savingJoin.ToQueryTable = toQueryTable;
            savingJoin.ToQueryTableColumn = toQueryTableColumn;

            return savingJoin;
        }
    }
}
