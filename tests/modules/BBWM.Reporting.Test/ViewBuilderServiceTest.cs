using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BBWM.Core.Exceptions;
using BBWM.DbDoc.Model;
using BBWM.Reporting.DTO;
using BBWM.Reporting.Enums;
using BBWM.Reporting.Model;
using BBWM.Reporting.Test;
using Xunit;

namespace BBWM.Reporting.Test
{
    public class ViewBuilderServiceTest
    {
        //[Fact]
        //public async Task AddFilterControlTest()
        //{
        //    var servicesFactory = new ReportingTestServicesFactory();
        //    await servicesFactory.CreateInMemoryReportingServices();
        //    var context = servicesFactory.InMemoryContext;
        //    var service = servicesFactory.InMemoryViewBuilderService;

        //    var dbDocFolder = servicesFactory.CreateFullFolder();
        //    await context.Set<Folder>().AddAsync(dbDocFolder);
        //    await context.SaveChangesAsync();

        //    var report = servicesFactory.CreateFullReport();
        //    var view = report.Sections[0].View;
        //    report.Sections[0].Query.DbDocFolderId = dbDocFolder.Id;
        //    view.Filters.Clear();
        //    await context.Set<Report>().AddAsync(report);
        //    await context.SaveChangesAsync();

        //    await Assert.ThrowsAsync<ObjectNotExistsException>(() =>
        //        service.AddFilterControl(0, report.Sections[0].Query.RootFilterSet.QueryFilters[0].Id));
        //    await Assert.ThrowsAsync<ObjectNotExistsException>(() =>
        //        service.AddFilterControl(view.Id, 0));

        //    var filterControl = await service.AddFilterControl(
        //        report.Sections[0].View.Id,
        //        report.Sections[0].Query.RootFilterSet.QueryFilters[0].Id);

        //    Assert.NotNull(filterControl);
        //    Assert.NotNull(view.Filters);
        //    Assert.NotEmpty(view.Filters);
        //    Assert.Equal(InputType.Number, view.Filters[0].InputType);
        //}

        [Fact]
        public async Task AddGridViewColumnTest()
        {
            var servicesFactory = new ReportingTestServicesFactory();
            await servicesFactory.CreateInMemoryReportingServices();
            var context = servicesFactory.InMemoryContext;
            var service = servicesFactory.InMemoryViewBuilderService;

            var dbDocFolder = servicesFactory.CreateFullFolder();
            await context.Set<Folder>().AddAsync(dbDocFolder);
            await context.SaveChangesAsync();

            var report = servicesFactory.CreateFullReport();
            var view = report.Sections[0].View;
            report.Sections[0].Query.DbDocFolderId = dbDocFolder.Id;
            view.GridView.ViewColumns.Clear();
            await context.Set<Report>().AddAsync(report);
            await context.SaveChangesAsync();

            await Assert.ThrowsAsync<ObjectNotExistsException>(() =>
                service.AddGridViewColumn(0, report.Sections[0].Query.QueryTables[0].Columns[0].Id));
            await Assert.ThrowsAsync<ObjectNotExistsException>(() =>
                service.AddGridViewColumn(view.GridView.Id, 0));

            var gridViewColumn = await service.AddGridViewColumn(view.GridView.Id, report.Sections[0].Query.QueryTables[0].Columns[0].Id);

            Assert.NotNull(gridViewColumn);
            Assert.NotNull(view.GridView);
            Assert.NotEmpty(view.GridView.ViewColumns);
            Assert.NotNull(view.GridView.ViewColumns[0].ExtraSettings);
            Assert.Equal(report.Sections[0].Query.QueryTables[0].Columns[0].Id, view.GridView.ViewColumns[0].QueryTableColumnId);
            Assert.Equal(view.GridView.Id, view.GridView.ViewColumns[0].GridViewId);
            Assert.Equal(1, view.GridView.ViewColumns[0].SortOrder);
            Assert.True(view.GridView.ViewColumns[0].Sortable);
            Assert.False(view.GridView.ViewColumns[0].Visible);

            await Assert.ThrowsAsync<BusinessException>(() => service.AddGridViewColumn(
                report.Sections[0].View.GridView.Id,
                report.Sections[0].Query.QueryTables[0].Columns[0].Id));

            gridViewColumn = await service.AddGridViewColumn(view.GridView.Id,
                report.Sections[0].Query.QueryTables[0].Columns[1].Id);

            Assert.NotNull(gridViewColumn);
            Assert.NotNull(view.GridView.ViewColumns[1].ExtraSettings);
            Assert.Equal(report.Sections[0].Query.QueryTables[0].Columns[1].Id, view.GridView.ViewColumns[1].QueryTableColumnId);
            Assert.Equal(2, view.GridView.ViewColumns[1].SortOrder);
            Assert.True(view.GridView.ViewColumns[1].Sortable);
            Assert.True(view.GridView.ViewColumns[1].Visible);
        }

        [Fact]
        public async Task DeleteFilterControlTest()
        {
            var servicesFactory = new ReportingTestServicesFactory();
            await servicesFactory.CreateInMemoryReportingServices();
            var context = servicesFactory.InMemoryContext;
            var service = servicesFactory.InMemoryViewBuilderService;

            var report = servicesFactory.CreateFullReport();
            var view = report.Sections[0].View;
            await context.Set<Report>().AddAsync(report);
            await context.SaveChangesAsync();

            await Assert.ThrowsAsync<ObjectNotExistsException>(() => service.DeleteFilterControl(0, true));

            var deletingViewColumnId = view.Filters[0].Id;

            await service.DeleteFilterControl(deletingViewColumnId, true);

            Assert.DoesNotContain(view.Filters, x => x.Id == deletingViewColumnId);
        }

        [Fact]
        public async Task DeleteGridViewColumnTest()
        {
            var servicesFactory = new ReportingTestServicesFactory();
            await servicesFactory.CreateInMemoryReportingServices();
            var context = servicesFactory.InMemoryContext;
            var service = servicesFactory.InMemoryViewBuilderService;

            var report = servicesFactory.CreateFullReport();
            var view = report.Sections[0].View;
            await context.Set<Report>().AddAsync(report);
            await context.SaveChangesAsync();

            await Assert.ThrowsAsync<ObjectNotExistsException>(() => service.DeleteGridViewColumn(0));

            var deletingViewColumnId = report.Sections[0].View.GridView.ViewColumns[0].Id;

            await service.DeleteGridViewColumn(deletingViewColumnId);

            Assert.DoesNotContain(view.GridView.ViewColumns, x => x.Id == deletingViewColumnId);
        }

        //TODO: Fix DTO mapping
        [Fact]
        public async Task UpdateFilterControlTest()
        {
            var servicesFactory = new ReportingTestServicesFactory();
            await servicesFactory.CreateInMemoryReportingServices();
            var map = servicesFactory.Mapper;
            var context = servicesFactory.InMemoryContext;
            var service = servicesFactory.InMemoryViewBuilderService;

            var report = servicesFactory.CreateFullReport();
            var view = report.Sections[0].View;
            await context.Set<Report>().AddAsync(report);
            await context.SaveChangesAsync();

            await Assert.ThrowsAsync<ObjectNotExistsException>(() => service.UpdateFilterControl(0, null));

            var filterControl0 = view.Filters[0];
            var filterControl1 = view.Filters[1];
            var filterControl2 = view.Filters[2];

            var oldFilterControl1SortOrder = filterControl1.SortOrder;
            var oldFilterControl2SortOrder = filterControl2.SortOrder;
            var oldId = filterControl0.Id;
            var oldViewId = filterControl0.ViewId;
            var newHintText = "New hint text value";
            var newControlName = "New control name value";
            var newSortOrder = 2;
            var dto = map.Map<FilterControlDTO>(filterControl0);
            dto.Id = 0;
            dto.ViewId = 0;
            dto.HintText = newHintText;
            dto.Name = newControlName;
            dto.SortOrder = newSortOrder;

            var filterControl = await service.UpdateFilterControl(oldId, dto);

            Assert.NotNull(filterControl);
            Assert.NotEmpty(view.Filters);
            Assert.Equal(oldId, filterControl0.Id);
            Assert.Equal(oldViewId, filterControl0.ViewId);
            Assert.Equal(newHintText, filterControl0.HintText);
            Assert.Equal(oldFilterControl1SortOrder - 1, filterControl1.SortOrder);
            Assert.Equal(oldFilterControl2SortOrder - 1, filterControl2.SortOrder);

            dto = map.Map<FilterControlDTO>(filterControl0);
            dto.SortOrder = 0;
            await service.UpdateFilterControl(oldId, dto);

            Assert.Equal(0, filterControl0.SortOrder);
            Assert.Equal(1, filterControl1.SortOrder);
            Assert.Equal(2, filterControl2.SortOrder);
        }

        // TODO: Fix DTO mapping
        [Fact]
        public async Task UpdateGridViewTest()
        {
            var servicesFactory = new ReportingTestServicesFactory();
            await servicesFactory.CreateInMemoryReportingServices();
            var mapper = servicesFactory.Mapper;
            var context = servicesFactory.InMemoryContext;
            var service = servicesFactory.InMemoryViewBuilderService;

            var report = servicesFactory.CreateFullReport();
            await context.Set<Report>().AddAsync(report);
            await context.SaveChangesAsync();

            await Assert.ThrowsAsync<ObjectNotExistsException>(() => service.UpdateGridView(0, null));

            var gridView = report.Sections[0].View.GridView;
            var id = gridView.Id;
            var defaultSortColumnId = gridView.DefaultSortColumnId;
            var defaultSortOrder = gridView.DefaultSortOrder;
            var showVisibleColumnsSelector = gridView.ShowVisibleColumnsSelector;
            var summaryFooterVisible = gridView.SummaryFooterVisible;
            var viewId = gridView.ViewId;

            var dto = mapper.Map<GridViewDTO>(gridView);
            dto.Id = 0;
            dto.DefaultSortColumnId = report.Sections[0].Query.QueryTables[0].Columns[0].Id;
            dto.ViewId = 0;
            dto.DefaultSortOrder = defaultSortOrder == SortOrder.Asc ? SortOrder.Desc : SortOrder.Asc;
            dto.ShowVisibleColumnsSelector = !showVisibleColumnsSelector;
            dto.SummaryFooterVisible = !summaryFooterVisible;

            var updatedGridView = await service.UpdateGridView(gridView.Id, dto);

            Assert.NotNull(updatedGridView);
            Assert.Equal(id, updatedGridView.Id);
            Assert.Equal(viewId, updatedGridView.ViewId);
            Assert.NotEqual(defaultSortColumnId, updatedGridView.DefaultSortColumnId);
            Assert.NotEqual(defaultSortOrder, updatedGridView.DefaultSortOrder);
            Assert.NotEqual(showVisibleColumnsSelector, updatedGridView.ShowVisibleColumnsSelector);
            Assert.NotEqual(summaryFooterVisible, updatedGridView.SummaryFooterVisible);
        }

        // TODO: Fix DTO mapping
        [Fact]
        public async Task UpdateGridViewColumnTest()
        {
            var servicesFactory = new ReportingTestServicesFactory();
            await servicesFactory.CreateInMemoryReportingServices();
            var map = servicesFactory.Mapper;
            var context = servicesFactory.InMemoryContext;
            var service = servicesFactory.InMemoryViewBuilderService;

            var report = servicesFactory.CreateFullReport();
            await context.Set<Report>().AddAsync(report);
            await context.SaveChangesAsync();

            await Assert.ThrowsAsync<ObjectNotExistsException>(() => service.UpdateGridViewColumn(0, null));

            var viewColumn0 = report.Sections[0].View.GridView.ViewColumns[0];
            var viewColumn1 = report.Sections[0].View.GridView.ViewColumns[1];
            var viewColumn2 = report.Sections[0].View.GridView.ViewColumns[2];

            var oldId = viewColumn0.Id;
            var oldGridViewId = viewColumn0.GridViewId;
            var oldQueryTableColumnId = viewColumn0.QueryTableColumnId;
            var oldVisible = viewColumn0.Visible;
            var oldSortable = viewColumn0.Sortable;
            var oldViewColumn1SortOrder = viewColumn1.SortOrder;
            var oldViewColumn2SortOrder = viewColumn2.SortOrder;
            var newHeaderValue = "New header value";
            var newSortOrder = 2;
            var dto = map.Map<GridViewColumnDTO>(viewColumn0);
            dto.Id = 0;
            dto.GridViewId = 0;
            dto.QueryTableColumnId = 0;
            dto.SortOrder = newSortOrder;
            dto.Header = newHeaderValue;
            dto.Visible = !dto.Visible;
            dto.Sortable = !dto.Sortable;

            var view = await service.UpdateGridViewColumn(oldId, dto);

            Assert.NotNull(view);
            Assert.NotNull(view.GridView);
            Assert.NotEmpty(view.GridView.ViewColumns);
            Assert.Equal(oldId, viewColumn0.Id);
            Assert.Equal(oldGridViewId, viewColumn0.GridViewId);
            Assert.Equal(oldQueryTableColumnId, viewColumn0.QueryTableColumnId);
            Assert.Equal(newHeaderValue, viewColumn0.Header);
            Assert.Equal(newSortOrder, viewColumn0.SortOrder);
            Assert.NotEqual(oldVisible, viewColumn0.Visible);
            Assert.NotEqual(oldSortable, viewColumn0.Sortable);
            Assert.Equal(oldViewColumn1SortOrder - 1, viewColumn1.SortOrder);
            Assert.Equal(oldViewColumn2SortOrder - 1, viewColumn2.SortOrder);

            dto = map.Map<GridViewColumnDTO>(viewColumn0);
            dto.SortOrder = 0;
            await service.UpdateGridViewColumn(oldId, dto);

            Assert.Equal(0, viewColumn0.SortOrder);
            Assert.Equal(1, viewColumn1.SortOrder);
            Assert.Equal(2, viewColumn2.SortOrder);
        }
    }
}
