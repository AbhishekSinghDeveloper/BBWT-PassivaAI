using BBWM.Core;
using BBWM.Core.Exceptions;
using BBWM.DbDoc.Model;
using BBWM.Reporting.DTO;
using BBWM.Reporting.Model;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BBWM.Reporting.Test
{
    public class SectionServiceTest
    {
        /*[Fact]
        public async Task AddQueryTableColumnTest()
        {
            var servicesFactory = new ReportingTestServicesFactory();
            await servicesFactory.CreateInMemoryReportingServices();
            var context = servicesFactory.InMemoryContext;
            var service = servicesFactory.InMemorySectionService;

            var folder = servicesFactory.CreateFullFolder();
            await context.Set<Folder>().AddAsync(folder);
            await context.SaveChangesAsync();

            var report = servicesFactory.CreateFullReport();
            report.Sections[0].Query = null;
            report.Sections[0].View.Filters.Clear();
            report.Sections[0].View.GridView.ViewColumns.Clear();
            await context.Set<Report>().AddAsync(report);
            await context.SaveChangesAsync();

            var addedTable = folder.Tables.First();
            var addedColumn = addedTable.Columns.First();

            await Assert.ThrowsAsync<ObjectNotExistsException>(() => service.AddQueryTableColumn(Guid.Empty, addedColumn.Id));
            await Assert.ThrowsAsync<ObjectNotExistsException>(() => service.AddQueryTableColumn(report.Sections[0].Id, 0));

            var section = await service.AddQueryTableColumn(report.Sections[0].Id, addedColumn.Id);

            Assert.NotNull(section);
            Assert.NotNull(section.Query);
            Assert.Equal(folder.Id, section.Query.DbDocFolderId);
            Assert.True(section.Query.QueryFilterSets.Count == 1 &&
                section.Query.QueryFilterSets[0].ParentQueryId == section.Query.Id &&
                section.Query.QueryFilterSets[0].ParentId is null);
            Assert.NotEmpty(section.Query.QueryTables);
            Assert.Equal(addedTable.TableId, section.Query.QueryTables[0].DbDocTableId);
            Assert.NotEmpty(section.Query.QueryTables[0].Columns);
            Assert.Equal(addedColumn.ColumnId, section.Query.QueryTables[0].Columns[0].DbDocColumnId);
            Assert.NotNull(section.View);
            Assert.NotNull(section.View.GridView);
            Assert.NotNull(section.View.GridView.ViewColumns);
            Assert.NotEmpty(section.View.GridView.ViewColumns);
            Assert.Equal(section.View.GridView.ViewColumns[0].QueryTableColumnId, section.Query.QueryTables[0].Columns[0].Id);

            await Assert.ThrowsAsync<BusinessException>(() => service.AddQueryTableColumn(report.Sections[0].Id, addedColumn.Id));
        }

        //[Fact]
        //public async Task AddFilterControlTest()
        //{
        //    var servicesFactory = new ReportingTestServicesFactory();
        //    await servicesFactory.CreateInMemoryReportingServices();
        //    var service = servicesFactory.InMemorySectionService;

        //    await Assert.ThrowsAsync<ObjectNotExistsException>(() =>
        //        service.AddFilterControl(Guid.Empty, 0));

        //    // All other logic is inside the ViewBuilderService and is checked by its tests.
        //}

        [Fact]
        public async Task AddQueryFilterTest()
        {
            var servicesFactory = new ReportingTestServicesFactory();
            await servicesFactory.CreateInMemoryReportingServices();
            var service = servicesFactory.InMemorySectionService;

            await Assert.ThrowsAsync<ObjectNotExistsException>(() =>
                service.AddQueryFilter(Guid.Empty, 0, 0));

            // All other logic is inside the QeryBuilderService and is checked by its tests.
        }

        [Fact]
        public async Task AddQueryFilterSetTest()
        {
            var servicesFactory = new ReportingTestServicesFactory();
            await servicesFactory.CreateInMemoryReportingServices();
            var service = servicesFactory.InMemorySectionService;

            await Assert.ThrowsAsync<ObjectNotExistsException>(() =>
                service.AddQueryFilterSet(Guid.Empty, 0));

            // All other logic is inside the QeryBuilderService and is checked by its tests.
        }

        [Fact]
        public async Task AddQueryTableTest()
        {
            var servicesFactory = new ReportingTestServicesFactory();
            await servicesFactory.CreateInMemoryReportingServices();
            var context = servicesFactory.InMemoryContext;
            var service = servicesFactory.InMemorySectionService;

            var folder = servicesFactory.CreateFullFolder();
            await context.Set<Folder>().AddAsync(folder);
            await context.SaveChangesAsync();

            var report = servicesFactory.CreateFullReport();
            report.Sections[0].Query = null;
            report.Sections[0].View.Filters.Clear();
            report.Sections[0].View.GridView.ViewColumns.Clear();
            await context.Set<Report>().AddAsync(report);
            await context.SaveChangesAsync();

            var addedTable = folder.Tables.First();

            await Assert.ThrowsAsync<ObjectNotExistsException>(() => service.AddQueryTable(Guid.Empty, addedTable.Id));
            await Assert.ThrowsAsync<ObjectNotExistsException>(() => service.AddQueryTable(report.Sections[0].Id, 0));

            var section = await service.AddQueryTable(report.Sections[0].Id, addedTable.Id);

            Assert.NotNull(section);
            Assert.NotNull(section.Query);
            Assert.Equal(folder.Id, section.Query.DbDocFolderId);
            Assert.True(section.Query.QueryFilterSets.Count == 1 &&
                section.Query.QueryFilterSets[0].ParentQueryId == section.Query.Id &&
                section.Query.QueryFilterSets[0].ParentId is null);
            Assert.NotEmpty(section.Query.QueryTables);
            Assert.Equal(addedTable.TableId, section.Query.QueryTables[0].DbDocTableId);
            Assert.NotEmpty(section.Query.QueryTables[0].Columns);
            Assert.True(addedTable.Columns.Select(x => x.ColumnId)
                .All(x => section.Query.QueryTables[0].Columns.Select(y => y.DbDocColumnId)
                .Any(y => y == x)));
            Assert.NotNull(section.View);
            Assert.NotNull(section.View.GridView);
            Assert.NotNull(section.View.GridView.ViewColumns);
            Assert.NotEmpty(section.View.GridView.ViewColumns);
            Assert.All(section.Query.QueryTables[0].Columns.Select(x => x.Id),
                x => section.View.GridView.ViewColumns.Single(y => y.QueryTableColumnId == x));
            Assert.Equal(section.View.GridView.ViewColumns[0].QueryTableColumnId, section.Query.QueryTables[0].Columns[0].Id);
        }

        [Fact]
        public async Task DeleteQueryTableColumnTest()
        {
            var servicesFactory = new ReportingTestServicesFactory();
            await servicesFactory.CreateInMemoryReportingServices();
            var context = servicesFactory.InMemoryContext;
            var service = servicesFactory.InMemorySectionService;

            var report = servicesFactory.CreateFullReport();
            await context.Set<Report>().AddAsync(report);
            await context.SaveChangesAsync();

            await Assert.ThrowsAsync<ObjectNotExistsException>(() =>
                service.DeleteQueryTableColumn(Guid.Empty, report.Sections[0].Query.QueryTables[0].Columns[0].Id));
            await Assert.ThrowsAsync<ObjectNotExistsException>(() => service.DeleteQueryTableColumn(report.Sections[0].Id, 0));

            var firstQueryTableColumnId = report.Sections[0].Query.QueryTables[0].Columns[0].Id;
            var relatedQueryFilterIds = context.Set<QueryFilter>()
                .Where(x => x.QueryTableColumnId == firstQueryTableColumnId)
                .Select(x => x.Id);
            var relatedColumnViewIds = context.Set<GridViewColumn>()
                .Where(x => x.QueryTableColumnId == firstQueryTableColumnId)
                .Select(x => x.Id);
            var relatedQueryFilterBindingIds = context.Set<QueryFilterBinding>()
                .Where(x => relatedQueryFilterIds.Any(y => y == x.QueryFilterId))
                .Select(x => x.Id);
            var relatedFilterControlIds = context.Set<FilterControl>()
                .Include(x => x.QueryFilterBindings)
                .Where(x => x.QueryFilterBindings.Any(y => relatedQueryFilterBindingIds.Contains(y.Id)))
                .Select(x => x.Id);

            await service.DeleteQueryTableColumn(report.Sections[0].Id, report.Sections[0].Query.QueryTables[0].Columns[0].Id);

            Assert.True(await context.Set<QueryTableColumn>().AllAsync(x => x.Id != firstQueryTableColumnId));
            Assert.True(await context.Set<QueryFilter>().AllAsync(x => !relatedQueryFilterIds.Contains(x.Id)));
            Assert.True(await context.Set<GridViewColumn>().AllAsync(x => !relatedColumnViewIds.Contains(x.Id)));
            Assert.True(await context.Set<QueryFilterBinding>().AllAsync(x => !relatedQueryFilterBindingIds.Contains(x.Id)));
            Assert.True(await context.Set<FilterControl>().AllAsync(x => !relatedFilterControlIds.Contains(x.Id)));
        }

        [Fact]
        public async Task DeleteFilterControlTest()
        {
            var servicesFactory = new ReportingTestServicesFactory();
            await servicesFactory.CreateInMemoryReportingServices();
            var service = servicesFactory.InMemorySectionService;

            await Assert.ThrowsAsync<ObjectNotExistsException>(() =>
                service.DeleteFilterControl(Guid.Empty, 0, true));

            // All other logic is inside the ViewBuilderService and is checked by its tests.
        }

        [Fact]
        public async Task DeleteQueryFilterTest()
        {
            var servicesFactory = new ReportingTestServicesFactory();
            await servicesFactory.CreateInMemoryReportingServices();
            var service = servicesFactory.InMemorySectionService;

            await Assert.ThrowsAsync<ObjectNotExistsException>(() =>
                service.DeleteQueryFilter(Guid.Empty, 0));

            // All other logic is inside the QeryBuilderService and is checked by its tests.
        }

        [Fact]
        public async Task DeleteQueryFilterSetTest()
        {
            var servicesFactory = new ReportingTestServicesFactory();
            await servicesFactory.CreateInMemoryReportingServices();
            var context = servicesFactory.InMemoryContext;
            var service = servicesFactory.InMemorySectionService;

            var report = servicesFactory.CreateFullReport();
            await context.Set<Report>().AddAsync(report);
            await context.SaveChangesAsync();

            await Assert.ThrowsAsync<ObjectNotExistsException>(() =>
                service.DeleteQueryFilterSet(Guid.Empty, report.Sections[0].Query.RootFilterSet.ChildSets[0].Id));
            await Assert.ThrowsAsync<ObjectNotExistsException>(() =>
                service.DeleteQueryFilterSet(report.Sections[0].Id, 0));

            var deletedQueryFilterSetIds = new List<int> { report.Sections[0].Query.RootFilterSet.ChildSets[0].Id };
            deletedQueryFilterSetIds.Add(report.Sections[0].Query.RootFilterSet.ChildSets[0].ChildSets[0].Id);
            var deletedFilterIds = report.Sections[0].Query.RootFilterSet.ChildSets[0].QueryFilters.Select(x => x.Id).ToList();
            deletedFilterIds.AddRange(report.Sections[0].Query.RootFilterSet.ChildSets[0].ChildSets[0].QueryFilters.Select(x => x.Id));
            var deletedQueryFilterBindingIds = context.Set<QueryFilterBinding>()
                .Where(x => deletedFilterIds.Any(y => y == x.QueryFilterId))
                .Select(x => x.Id);
            var deletedFilterControlIds = context.Set<FilterControl>()
                .Include(x => x.QueryFilterBindings)
                .Where(x => x.QueryFilterBindings.Any(y => deletedQueryFilterBindingIds.Contains(y.Id)))
                .Select(x => x.Id);

            var query = await service.DeleteQueryFilterSet(report.Sections[0].Id, report.Sections[0].Query.RootFilterSet.ChildSets[0].Id);

            Assert.True(await context.Set<QueryFilterSet>().AllAsync(x => !deletedQueryFilterSetIds.Contains(x.Id)));
            Assert.True(await context.Set<QueryFilter>().AllAsync(x => !deletedFilterIds.Contains(x.Id)));
            Assert.True(await context.Set<QueryFilterBinding>().AllAsync(x => !deletedQueryFilterBindingIds.Contains(x.Id)));
            Assert.True(await context.Set<FilterControl>().AllAsync(x => !deletedFilterControlIds.Contains(x.Id)));
        }

        [Fact]
        public async Task DeleteQueryTableTest()
        {
            var servicesFactory = new ReportingTestServicesFactory();
            await servicesFactory.CreateInMemoryReportingServices();
            var context = servicesFactory.InMemoryContext;
            var service = servicesFactory.InMemorySectionService;

            var report = servicesFactory.CreateFullReport();
            var query = report.Sections[0].Query;
            await context.Set<Report>().AddAsync(report);
            await context.SaveChangesAsync();

            await Assert.ThrowsAsync<ObjectNotExistsException>(() => service.DeleteQueryTable(Guid.Empty, query.QueryTables[0].Id));
            await Assert.ThrowsAsync<ObjectNotExistsException>(() => service.DeleteQueryTable(report.Sections[0].Id, 0));

            var section = await service.DeleteQueryTable(report.Sections[0].Id, query.QueryTables[0].Id);

            Assert.False(context.Set<QueryTable>().Any());
            Assert.False(context.Set<QueryTableColumn>().Any());
        }

        [Fact]
        public async Task ExistsTest()
        {
            var servicesFactory = new ReportingTestServicesFactory();
            await servicesFactory.CreateInMemoryReportingServices();
            var context = servicesFactory.InMemoryContext;
            var service = servicesFactory.InMemorySectionService;

            var section = servicesFactory.CreateSection();
            await context.Set<Section>().AddAsync(section);
            await context.SaveChangesAsync();

            Assert.True(await service.Exists(section.Id));
            Assert.False(await service.Exists(Guid.Empty));
        }

        [Fact]
        public async Task UpdateFilterControlTest()
        {
            var servicesFactory = new ReportingTestServicesFactory();
            await servicesFactory.CreateInMemoryReportingServices();
            var service = servicesFactory.InMemorySectionService;

            await Assert.ThrowsAsync<ObjectNotExistsException>(() =>
                service.UpdateFilterControl(Guid.Empty, 0, null));

            // All other logic is inside the ViewBuilderService and is checked by its tests.
        }

        [Fact]
        public async Task UpdateGridViewTest()
        {
            var servicesFactory = new ReportingTestServicesFactory();
            await servicesFactory.CreateInMemoryReportingServices();
            var service = servicesFactory.InMemorySectionService;

            await Assert.ThrowsAsync<ObjectNotExistsException>(() =>
                service.UpdateGridView(Guid.Empty, 0, null));

            // All other logic is inside the ViewBuilderService and is checked by its tests.
        }

        [Fact]
        public async Task UpdateGridViewColumnTest()
        {
            var servicesFactory = new ReportingTestServicesFactory();
            await servicesFactory.CreateInMemoryReportingServices();
            var service = servicesFactory.InMemorySectionService;

            await Assert.ThrowsAsync<ObjectNotExistsException>(() =>
                service.UpdateGridViewColumn(Guid.Empty, 0, null));

            // All other logic is inside the ViewBuilderService and is checked by its tests.
        }

        [Fact]
        public async Task UpdateQueryFilterSetTest()
        {
            var servicesFactory = new ReportingTestServicesFactory();
            await servicesFactory.CreateInMemoryReportingServices();
            var service = servicesFactory.InMemorySectionService;

            await Assert.ThrowsAsync<ObjectNotExistsException>(() =>
                service.UpdateQueryFilterSet(Guid.Empty, 0, new QueryFilterSetDTO()));

            // All other logic is inside the QeryBuilderService and is checked by its tests.
        }*/
    }
}
