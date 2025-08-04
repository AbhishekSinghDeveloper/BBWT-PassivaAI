using BBWM.Core.Exceptions;
using BBWM.DbDoc.Model;
using BBWM.Reporting.Model;
using Xunit;
using Microsoft.EntityFrameworkCore;
using BBWM.Reporting.DTO;
using BBWM.Reporting.Services;
using BBWM.Reporting.Enums;

namespace BBWM.Reporting.Test
{
    public class QueryBuilderServiceTest
    {
        [Fact]
        public void MakeFiltersTreeTest()
        {
            var set1 = new QueryFilterSet { Id = 1, ParentQueryId = 1 };
            var set12 = new QueryFilterSet { Id = 2 };
            var set13 = new QueryFilterSet { Id = 3 };
            var set124 = new QueryFilterSet { Id = 4 };
            var collection = new List<QueryFilterSet> { set1, set12, set13, set124 };

            set12.ParentId = set1.Id;
            set13.ParentId = set1.Id;
            set124.ParentId = set12.Id;

            var result = QueryBuilderService.MakeFilterSetsTree(collection);

            Assert.Equal(set1.Id, result.Id);
            Assert.Equal(set12.Id, result.ChildSets[0].Id);
            Assert.Equal(set13.Id, result.ChildSets[1].Id);
            Assert.Equal(set124.Id, result.ChildSets[0].ChildSets[0].Id);

            Assert.Throws<InvalidOperationException>(() => QueryBuilderService.MakeFilterSetsTree(new List<QueryFilterSet>()));

            set1.Parent = null;
            set1.ChildSets.Clear();
            set12.Parent = null;
            set12.ChildSets.Clear();
            set13.Parent = null;
            set13.ChildSets.Clear();
            set124.Parent = null;
            set124.ChildSets.Clear();
            set124.ParentId = null;
            result = QueryBuilderService.MakeFilterSetsTree(collection);

            Assert.Equal(set1.Id, result.Id);
            Assert.Equal(set12.Id, result.ChildSets[0].Id);
            Assert.Equal(set13.Id, result.ChildSets[1].Id);
            Assert.Empty(result.ChildSets[0].ChildSets);

            Assert.Throws<InvalidOperationException>(() => QueryBuilderService.MakeFilterSetsTree(collection, true));
        }

        [Fact]
        public async Task AddColumnTest()
        {
            var servicesFactory = new ReportingTestServicesFactory();
            await servicesFactory.CreateInMemoryReportingServices();
            var context = servicesFactory.InMemoryContext;
            var service = servicesFactory.InMemoryQueryBuilderService;

            var folder1 = servicesFactory.CreateFullFolder();
            var folder2 = servicesFactory.CreateFullFolder();
            await context.Set<Folder>().AddAsync(folder1);
            await context.Set<Folder>().AddAsync(folder2);
            await context.SaveChangesAsync();

            var query = new Query { DbDocFolderId = folder1.Id };
            await context.Set<Query>().AddAsync(query);
            await context.SaveChangesAsync();

            await Assert.ThrowsAsync<ObjectNotExistsException>(() => service.AddQueryTableColumn(0, folder1.Tables.First().Columns.First().Id));
            await Assert.ThrowsAsync<ObjectNotExistsException>(() => service.AddQueryTableColumn(query.Id, 0));

            folder1.Owners = string.Empty;
            await context.SaveChangesAsync();

            await Assert.ThrowsAsync<BusinessException>(() => service.AddQueryTableColumn(query.Id, folder1.Tables.First().Columns.First().Id));

            folder1.Owners = ModuleLinkage.DbDocFolderOwnerName;
            await context.SaveChangesAsync();

            var queryTableColumn = await service.AddQueryTableColumn(query.Id, folder1.Tables.First().Columns.First().Id);

            Assert.NotNull(queryTableColumn);
            Assert.Equal(folder1.Id, query.DbDocFolderId);
            Assert.NotEmpty(query.QueryTables);
            Assert.Equal(folder1.Tables.First().Columns.First().ColumnId, query.QueryTables.First().Columns.First().DbDocColumnId);

            await Assert.ThrowsAsync<BusinessException>(() => service.AddQueryTableColumn(query.Id, folder2.Tables.First().Columns.First().Id));

            context.Set<QueryTable>().Remove(await context.Set<QueryTable>().SingleOrDefaultAsync(x => x.Id == query.QueryTables.First().Id));
            await context.SaveChangesAsync();

            folder2.Owners = string.Empty;
            await context.SaveChangesAsync();

            await Assert.ThrowsAsync<BusinessException>(() => service.AddQueryTableColumn(query.Id, folder2.Tables.First().Columns.First().Id));

            folder2.Owners = ModuleLinkage.DbDocFolderOwnerName;
            await context.SaveChangesAsync();

            queryTableColumn = await service.AddQueryTableColumn(query.Id, folder2.Tables.First().Columns.First().Id);

            Assert.NotNull(queryTableColumn);
            Assert.Equal(folder2.Id, query.DbDocFolderId);

            await Assert.ThrowsAsync<BusinessException>(() => service.AddQueryTableColumn(query.Id, folder2.Tables.First().Columns.First().Id));
        }

        [Fact]
        public async Task AddQueryFilterTest()
        {
            var servicesFactory = new ReportingTestServicesFactory();
            await servicesFactory.CreateInMemoryReportingServices();
            var context = servicesFactory.InMemoryContext;
            var service = servicesFactory.InMemoryQueryBuilderService;

            var report = servicesFactory.CreateFullReport();
            var query = report.Sections[0].Query;
            query.RootFilterSet.QueryFilters.Clear();
            await context.Set<Report>().AddAsync(report);
            await context.SaveChangesAsync();

            await Assert.ThrowsAsync<ObjectNotExistsException>(() =>
                service.AddQueryFilter(0, query.QueryTables[0].Columns[0].Id));
            await Assert.ThrowsAsync<ObjectNotExistsException>(() =>
                service.AddQueryFilter(query.RootFilterSet.Id, 0));

            var queryFilter = await service.AddQueryFilter(query.RootFilterSet.Id, query.QueryTables[0].Columns[0].Id);

            Assert.NotNull(queryFilter);
            Assert.Equal(query.QueryTables[0].Columns[0].Id, queryFilter.QueryTableColumnId);
            Assert.Equal(query.RootFilterSet.Id, queryFilter.QueryFilterSetId);
        }

        [Fact]
        public async Task AddQueryFilterSetTest()
        {
            var servicesFactory = new ReportingTestServicesFactory();
            await servicesFactory.CreateInMemoryReportingServices();
            var context = servicesFactory.InMemoryContext;
            var service = servicesFactory.InMemoryQueryBuilderService;

            var report = servicesFactory.CreateFullReport();
            var query = report.Sections[0].Query;
            query.RootFilterSet.ChildSets.Clear();
            await context.Set<Report>().AddAsync(report);
            await context.SaveChangesAsync();

            await Assert.ThrowsAsync<ObjectNotExistsException>(() => service.AddQueryFilterSet(0));

            var queryFilterSet = await service.AddQueryFilterSet(query.RootFilterSet.Id);
            query.RootFilterSet = QueryBuilderService.MakeFilterSetsTree(query.QueryFilterSets);

            Assert.NotNull(queryFilterSet);
            Assert.NotEmpty(query.RootFilterSet.ChildSets);
            Assert.Null(query.RootFilterSet.ChildSets[0].ParentQueryId);
            Assert.Equal(query.Id, query.RootFilterSet.ChildSets[0].QueryId);
            Assert.Equal(query.RootFilterSet.Id, query.RootFilterSet.ChildSets[0].ParentId);
        }

        [Fact]
        public async Task AddTableTest()
        {
            var servicesFactory = new ReportingTestServicesFactory();
            await servicesFactory.CreateInMemoryReportingServices();
            var context = servicesFactory.InMemoryContext;
            var service = servicesFactory.InMemoryQueryBuilderService;

            var folder1 = servicesFactory.CreateFullFolder();
            var folder2 = servicesFactory.CreateFullFolder();
            await context.Set<Folder>().AddAsync(folder1);
            await context.Set<Folder>().AddAsync(folder2);
            await context.SaveChangesAsync();

            var query = new Query { DbDocFolderId = folder1.Id };
            await context.Set<Query>().AddAsync(query);
            await context.SaveChangesAsync();

            await Assert.ThrowsAsync<ObjectNotExistsException>(() => service.AddQueryTable(0, folder1.Tables.First().Id));
            await Assert.ThrowsAsync<ObjectNotExistsException>(() => service.AddQueryTable(query.Id, 0));

            folder1.Owners = string.Empty;
            await context.SaveChangesAsync();

            await Assert.ThrowsAsync<BusinessException>(() => service.AddQueryTable(query.Id, folder1.Tables.First().Id));

            folder1.Owners = ModuleLinkage.DbDocFolderOwnerName;
            await context.SaveChangesAsync();

            var queryTable = await service.AddQueryTable(query.Id, folder1.Tables.First().Id);

            Assert.NotNull(queryTable);
            Assert.Equal(folder1.Id, query.DbDocFolderId);
            Assert.NotEmpty(query.QueryTables);
            Assert.Equal(folder1.Tables.First().TableId, query.QueryTables.First().DbDocTableId);
            Assert.True(query.QueryTables.First().Columns.Select(x => x.DbDocColumnId)
                .All(x => folder1.Tables.First().Columns.Select(y => y.ColumnId).Contains(x)));

            await Assert.ThrowsAsync<BusinessException>(() => service.AddQueryTable(query.Id, folder2.Tables.First().Id));

            context.Set<QueryTable>().Remove(await context.Set<QueryTable>().SingleOrDefaultAsync(x => x.Id == query.QueryTables.First().Id));
            await context.SaveChangesAsync();

            folder2.Owners = string.Empty;
            await context.SaveChangesAsync();

            await Assert.ThrowsAsync<BusinessException>(() => service.AddQueryTable(query.Id, folder2.Tables.First().Id));

            folder2.Owners = ModuleLinkage.DbDocFolderOwnerName;
            await context.SaveChangesAsync();

            queryTable = await service.AddQueryTable(query.Id, folder2.Tables.First().Id);

            Assert.NotNull(queryTable);
            Assert.Equal(folder2.Id, query.DbDocFolderId);
        }

        [Fact]
        public async Task CreateQueryTest()
        {
            var servicesFactory = new ReportingTestServicesFactory();
            await servicesFactory.CreateInMemoryReportingServices();
            var context = servicesFactory.InMemoryContext;
            var service = servicesFactory.InMemoryQueryBuilderService;

            var folder = servicesFactory.CreateFullFolder();
            await context.Set<Folder>().AddAsync(folder);
            await context.SaveChangesAsync();

            await Assert.ThrowsAsync<ObjectNotExistsException>(() => service.CreateQuery(Guid.Empty));

            folder.Owners = string.Empty;
            await context.SaveChangesAsync();

            await Assert.ThrowsAsync<BusinessException>(() => service.CreateQuery(folder.Id));

            folder.Owners = ModuleLinkage.DbDocFolderOwnerName;
            await context.SaveChangesAsync();

            var query = await service.CreateQuery(folder.Id);
            await context.Set<Query>().AddAsync(query);
            await context.SaveChangesAsync();

            Assert.NotNull(query);
            Assert.NotEqual(default, query.Id);
            Assert.Equal(folder.Id, query.DbDocFolderId);
            Assert.NotNull(query.RootFilterSet);
            Assert.Equal(query.Id, query.RootFilterSet.QueryId);
            Assert.Equal(query.Id, query.RootFilterSet.ParentQueryId);
            Assert.NotEqual(default, query.Id);
        }

        [Fact]
        public async Task DeleteQueryFilterTest()
        {
            var servicesFactory = new ReportingTestServicesFactory();
            await servicesFactory.CreateInMemoryReportingServices();
            var context = servicesFactory.InMemoryContext;
            var service = servicesFactory.InMemoryQueryBuilderService;

            var report = servicesFactory.CreateFullReport();
            await context.Set<Report>().AddAsync(report);
            await context.SaveChangesAsync();

            Assert.ThrowsAsync<ObjectNotExistsException>(() => service.DeleteQueryFilter(0));

            var firstQueryFilterId = report.Sections[0].Query.RootFilterSet.QueryFilters[0].Id;

            await service.DeleteQueryFilter(firstQueryFilterId);

            Assert.True(await context.Set<QueryFilter>().AllAsync(x => x.Id != firstQueryFilterId));
        }

        [Fact]
        public async Task DeleteQueryFilterSetTest()
        {
            var servicesFactory = new ReportingTestServicesFactory();
            await servicesFactory.CreateInMemoryReportingServices();
            var context = servicesFactory.InMemoryContext;
            var service = servicesFactory.InMemoryQueryBuilderService;

            var report = servicesFactory.CreateFullReport();
            var query = report.Sections[0].Query;
            await context.Set<Report>().AddAsync(report);
            await context.SaveChangesAsync();

            await Assert.ThrowsAsync<ObjectNotExistsException>(() => service.DeleteQueryFilterSet(0));
            await Assert.ThrowsAsync<BusinessException>(() => service.DeleteQueryFilterSet(report.Sections[0].Query.RootFilterSet.Id));

            var deletedQueryFilterSetIds = new List<int> { query.RootFilterSet.ChildSets[0].Id };
            deletedQueryFilterSetIds.Add(query.RootFilterSet.ChildSets[0].ChildSets[0].Id);
            var deletedFilterIds = query.RootFilterSet.ChildSets[0].QueryFilters.Select(x => x.Id).ToList();
            deletedFilterIds.AddRange(query.RootFilterSet.ChildSets[0].ChildSets[0].QueryFilters.Select(x => x.Id));

            await service.DeleteQueryFilterSet(query.RootFilterSet.ChildSets[0].Id);

            Assert.True(await context.Set<QueryFilterSet>().AllAsync(x => !deletedQueryFilterSetIds.Contains(x.Id)));
            Assert.True(await context.Set<QueryFilter>().AllAsync(x => !deletedFilterIds.Contains(x.Id)));
        }

        [Fact]
        public async Task DeleteColumnTest()
        {
            var servicesFactory = new ReportingTestServicesFactory();
            await servicesFactory.CreateInMemoryReportingServices();
            var context = servicesFactory.InMemoryContext;
            var service = servicesFactory.InMemoryQueryBuilderService;

            var report = servicesFactory.CreateFullReport();
            await context.Set<Report>().AddAsync(report);
            await context.SaveChangesAsync();

            var deletingColumnId = report.Sections[0].Query.QueryTables[0].Columns[0].Id;

            await service.DeleteQueryTableColumn(deletingColumnId);

            Assert.True(context.Set<QueryTableColumn>().All(x => x.Id != deletingColumnId));
            Assert.True(context.Set<QueryFilter>().All(x => x.QueryTableColumnId != deletingColumnId));

            var deletingTableId = report.Sections[0].Query.QueryTables[0].Id;
            for (var index = report.Sections[0].Query.QueryTables[0].Columns.Count - 1; index >= 0; index--)
            {
                await service.DeleteQueryTableColumn(report.Sections[0].Query.QueryTables[0].Columns[index].Id);
            }

            Assert.True(context.Set<QueryTable>().All(x => x.Id != deletingTableId));
        }

        [Fact]
        public async Task DeleteTableTest()
        {
            var servicesFactory = new ReportingTestServicesFactory();
            await servicesFactory.CreateInMemoryReportingServices();
            var context = servicesFactory.InMemoryContext;
            var service = servicesFactory.InMemoryQueryBuilderService;

            var report = servicesFactory.CreateFullReport();
            await context.Set<Report>().AddAsync(report);
            await context.SaveChangesAsync();

            await service.DeleteQueryTable(report.Sections[0].Query.QueryTables[0].Id);

            Assert.False(context.Set<QueryTable>().Any());
            Assert.False(context.Set<QueryTableColumn>().Any());
            Assert.False(context.Set<QueryFilter>().Any());
            Assert.False(context.Set<GridViewColumn>().Any());
            Assert.False(context.Set<QueryFilterBinding>().Any());
        }

        //TODO: Fix DTO mapping
        //[Fact]
        //public async Task UpdateQueryFilterTest()
        //{
        //    var servicesFactory = new ReportingTestServicesFactory();
        //    await servicesFactory.CreateInMemoryReportingServices();
        //    var mapper = servicesFactory.Mapper;
        //    var context = servicesFactory.InMemoryContext;
        //    var service = servicesFactory.InMemoryQueryBuilderService;

        //    var report = servicesFactory.CreateFullReport();
        //    var query = report.Sections[0].Query;
        //    var numericQueryRule = new QueryRule
        //    {
        //        Name = "NumericLess",
        //        Code = QueryRuleCode.Less,
        //        RuleTypes = new List<QueryRuleType>
        //        {
        //            new QueryRuleType { Type = QueryRuleDataType.Numeric }
        //        }
        //    };
        //    query.QueryTables = new List<QueryTable>
        //    {
        //        new QueryTable
        //        {
        //            DbDocTableId = ReportingTestServicesFactory.InMemoryTableId,
        //            Columns = new List<QueryTableColumn>
        //            {
        //                new QueryTableColumn { DbDocColumnId = ReportingTestServicesFactory.InMemoryColumnIdNumber },
        //                new QueryTableColumn { DbDocColumnId = ReportingTestServicesFactory.InMemoryColumnIdDate }
        //            }
        //        }
        //    };
        //    report.Sections[0].Query.RootFilterSet = new QueryFilterSet
        //    {
        //        Query = query,
        //        QueryFilters = new List<QueryFilter>
        //        {
        //            new QueryFilter
        //            {
        //                QueryTableColumn = report.Sections[0].Query.QueryTables[0].Columns[0],
        //                Value = 10,
        //                QueryRule = numericQueryRule
        //            }
        //        }
        //    };
        //    await context.Set<Report>().AddAsync(report);

        //    var dateQueryRule = new QueryRule
        //    {
        //        Name = "DateMore",
        //        Code = QueryRuleCode.More,
        //        RuleTypes = new List<QueryRuleType>
        //        {
        //            new QueryRuleType { Type = QueryRuleDataType.Datetime }
        //        }
        //    };
        //    await context.Set<QueryRule>().AddAsync(dateQueryRule);

        //    await context.SaveChangesAsync();

        //    var firstIdNumericColumn = query.QueryTables[0].Columns[0];
        //    var secondRequiredDateDatetimeColumn = query.QueryTables[0].Columns[1];
        //    var queryFilterDto = mapper.Map<QueryFilterDTO>(query.RootFilterSet.QueryFilters[0]);

        //    Assert.ThrowsAsync<ObjectNotExistsException>(() => service.UpdateQueryFilter(0, queryFilterDto));

        //    queryFilterDto.QueryTableColumnId = secondRequiredDateDatetimeColumn.Id;
        //    queryFilterDto.QueryRuleId = dateQueryRule.Id;

        //    var queryFilter = await service.UpdateQueryFilter(queryFilterDto.Id, queryFilterDto);

        //    Assert.NotNull(queryFilter);
        //    Assert.NotEmpty(query.RootFilterSet.QueryFilters);
        //    Assert.Equal(secondRequiredDateDatetimeColumn.Id, query.RootFilterSet.QueryFilters[0].QueryTableColumnId);
        //    Assert.Equal(dateQueryRule.Id, query.RootFilterSet.QueryFilters[0].QueryRuleId);

        //    queryFilterDto.QueryTableColumnId = 0;

        //    await Assert.ThrowsAsync<ObjectNotExistsException>(() => service.UpdateQueryFilter(queryFilterDto.Id, queryFilterDto));

        //    queryFilterDto.QueryTableColumnId = secondRequiredDateDatetimeColumn.Id;
        //    queryFilterDto.QueryRuleId = 0;

        //    await Assert.ThrowsAsync<ObjectNotExistsException>(() => service.UpdateQueryFilter(queryFilterDto.Id, queryFilterDto));

        //    queryFilterDto.QueryRuleId = numericQueryRule.Id;

        //    await Assert.ThrowsAsync<BusinessException>(() => service.UpdateQueryFilter(queryFilterDto.Id, queryFilterDto));
        //}

        [Fact]
        public async Task UpdateQueryFilterSetTest()
        {
            var servicesFactory = new ReportingTestServicesFactory();
            await servicesFactory.CreateInMemoryReportingServices();
            var context = servicesFactory.InMemoryContext;
            var service = servicesFactory.InMemoryQueryBuilderService;

            var report = servicesFactory.CreateFullReport();
            var query = report.Sections[0].Query;
            query.RootFilterSet.ConditionalOperator = QueryConditionalOperator.And;
            await context.Set<Report>().AddAsync(report);
            await context.SaveChangesAsync();

            var dto = new QueryFilterSetDTO { ConditionalOperator = QueryConditionalOperator.Or };
            await Assert.ThrowsAsync<ObjectNotExistsException>(() => service.UpdateQueryFilterSet(0, dto));

            var queryFilterSet = await service.UpdateQueryFilterSet(query.RootFilterSet.Id, dto);

            Assert.NotNull(queryFilterSet);
            Assert.Equal(QueryConditionalOperator.Or, query.RootFilterSet.ConditionalOperator);
        }
    }
}
