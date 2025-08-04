using BBWM.Core;
using BBWM.Core.Exceptions;
using BBWM.Core.Membership.Model;
using BBWM.Reporting.DTO;
using BBWM.Reporting.Model;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BBWM.Reporting.Test;

public class ReportServiceTest
{
    [Fact]
    public async Task CancelDraftTest()
    {
        var servicesFactory = new ReportingTestServicesFactory();
        await servicesFactory.CreateInMemoryReportingServices();
        var context = servicesFactory.InMemoryContext;
        var service = servicesFactory.InMemoryReportService;

        var anotherUser = new User();
        await context.Set<User>().AddAsync(anotherUser, CancellationToken.None);
        var entity = servicesFactory.CreateFullReport();
        entity.CreatedBy = anotherUser.Id;
        entity.UpdatedBy = anotherUser.Id;
        entity.IsDraft = true;
        await context.Set<Report>().AddAsync(entity);
        await context.SaveChangesAsync();

        await Assert.ThrowsAsync<ObjectNotExistsException>(() => service.CancelDraft(Guid.Empty));
        await Assert.ThrowsAsync<ForbiddenException>(() => service.CancelDraft(entity.Id));

        entity.CreatedBy = servicesFactory.CurrentUser.Id;
        entity.UpdatedBy = servicesFactory.CurrentUser.Id;
        entity.IsDraft = false;
        await context.SaveChangesAsync();

        await Assert.ThrowsAsync<BusinessException>(() => service.CancelDraft(entity.Id));

        entity.IsDraft = true;
        await context.SaveChangesAsync();

        await service.CancelDraft(entity.Id);

        Assert.True(await context.Set<Report>().AllAsync(x => x.Id != entity.Id));
    }

    [Fact]
    public async Task CreateDraftOfExistingReportTest()
    {
        var servicesFactory = new ReportingTestServicesFactory();
        await servicesFactory.CreateInMemoryReportingServices();
        var context = servicesFactory.InMemoryContext;
        var service = servicesFactory.InMemoryReportService;

        var entity = servicesFactory.CreateFullReport();
        entity.CreatedBy = servicesFactory.CurrentUser.Id;
        entity.UpdatedBy = servicesFactory.CurrentUser.Id;
        entity.CreatedOn = DateTime.UtcNow;
        entity.UpdatedOn = DateTime.UtcNow;
        entity.IsDraft = true;
        entity.Access = AggregatedRoles.Authenticated;
        await context.Set<Report>().AddAsync(entity);
        await context.SaveChangesAsync();

        await Assert.ThrowsAsync<ObjectNotExistsException>(() => service.CreateDraftOfExistingReport(Guid.Empty));
        await Assert.ThrowsAsync<BusinessException>(() => service.CreateDraftOfExistingReport(entity.Id));

        entity.IsDraft = false;
        await context.SaveChangesAsync();

        var createdDraft = await service.CreateDraftOfExistingReport(entity.Id);

        Assert.NotNull(createdDraft);
        Assert.Equal(servicesFactory.CurrentUser.Id, entity.CreatedBy);
        Assert.Equal(servicesFactory.CurrentUser.Id, entity.UpdatedBy);
        Assert.True(entity.CreatedOn > default(DateTime));
        Assert.True(entity.UpdatedOn > default(DateTime));
        Assert.True(createdDraft.IsDraft);
        Assert.Equal(entity.Id, createdDraft.PublishedReportId);

        await Assert.ThrowsAsync<BusinessException>(() => service.CreateDraftOfExistingReport(entity.Id));
    }

    [Fact]
    public async Task CreateDraftOfNewReportTest()
    {
        var servicesFactory = new ReportingTestServicesFactory();
        await servicesFactory.CreateInMemoryReportingServices();
        var context = servicesFactory.InMemoryContext;
        var service = servicesFactory.InMemoryReportService;

        var entity = servicesFactory.CreateFullReport();
        entity.Sections.Clear();
        var dto = servicesFactory.Mapper.Map<Report, ReportDTO>(entity);

        dto.Name = " ";
        await Assert.ThrowsAsync<System.ComponentModel.DataAnnotations.ValidationException>(() => service.CreateDraftOfNewReport(dto));

        dto.Name = "Name";
        dto.UrlSlug = "";
        await Assert.ThrowsAsync<System.ComponentModel.DataAnnotations.ValidationException>(() => service.CreateDraftOfNewReport(dto));

        dto.UrlSlug = "url-slug";

        dto = await service.CreateDraftOfNewReport(dto);

        Assert.NotNull(dto);
        Assert.NotEqual(Guid.Empty, dto.Id);

        entity = context.Set<Report>().SingleOrDefault(x => x.Id == dto.Id);

        Assert.NotNull(entity);
        Assert.True(entity.IsDraft);
        Assert.Equal(servicesFactory.CurrentUser.Id, entity.CreatedBy);
        Assert.Equal(servicesFactory.CurrentUser.Id, entity.UpdatedBy);
        Assert.True(entity.CreatedOn > default(DateTime));
        Assert.True(entity.UpdatedOn > default(DateTime));
        Assert.Null(entity.PublishedReportId);

        await Assert.ThrowsAsync<BusinessException>(() => service.CreateDraftOfNewReport(dto));
    }

    [Fact]
    public async Task SectionCreateTest()
    {
        var servicesFactory = new ReportingTestServicesFactory();
        await servicesFactory.CreateInMemoryReportingServices();
        var context = servicesFactory.InMemoryContext;
        var reportService = servicesFactory.InMemoryReportService;

        var report = servicesFactory.CreateReport();
        report.IsDraft = false;
        await context.Set<Report>().AddAsync(report);
        await context.SaveChangesAsync();

        var section = servicesFactory.CreateSection();
        var sectionDto = servicesFactory.Mapper.Map<SectionDTO>(section);

        Assert.ThrowsAsync<BusinessException>(() => reportService.CreateSection(report.Id, sectionDto));

        report.IsDraft = true;
        await context.SaveChangesAsync();

        Assert.ThrowsAsync<ObjectNotExistsException>(() => reportService.CreateSection(Guid.Empty, sectionDto));

        sectionDto.Title = " ";
        Assert.ThrowsAsync<System.ComponentModel.DataAnnotations.ValidationException>(() =>
            reportService.CreateSection(report.Id, sectionDto));

        sectionDto.Title = "Section Title";
        var reportChangeResult = await reportService.CreateSection(report.Id, sectionDto);

        Assert.NotNull(reportChangeResult);
        Assert.NotNull(reportChangeResult.RequestTargetPart);
        Assert.NotNull(reportChangeResult.RequestTargetPart.Id);

        var guid = (Guid)reportChangeResult.RequestTargetPart.Id;

        var view = await context.Set<View>().SingleOrDefaultAsync(x => x.SectionId == guid);

        Assert.NotNull(view);
        Assert.NotNull(await context.Set<GridView>().SingleOrDefaultAsync(x => x.ViewId == view.Id));
    }

    [Fact]
    public async Task DeleteTest()
    {
        var servicesFactory = new ReportingTestServicesFactory();
        await servicesFactory.CreateInMemoryReportingServices();
        var context = servicesFactory.InMemoryContext;
        var service = servicesFactory.InMemoryReportService;

        var report = servicesFactory.CreateFullReport();
        report.CreatedBy = servicesFactory.CurrentUser.Id;
        report.UpdatedBy = servicesFactory.CurrentUser.Id;
        report.CreatedOn = DateTime.UtcNow;
        report.UpdatedOn = DateTime.UtcNow;
        report.IsDraft = false;
        report.Access = AggregatedRoles.Authenticated;
        await context.Set<Report>().AddAsync(report);
        await context.SaveChangesAsync();

        var draft = servicesFactory.CreateFullReport();
        draft.PublishedReportId = report.Id;
        draft.CreatedBy = servicesFactory.CurrentUser.Id;
        draft.UpdatedBy = servicesFactory.CurrentUser.Id;
        draft.CreatedOn = DateTime.UtcNow;
        draft.UpdatedOn = DateTime.UtcNow;
        draft.IsDraft = true;
        draft.Access = AggregatedRoles.Authenticated;
        await context.Set<Report>().AddAsync(draft);
        await context.SaveChangesAsync();

        await service.Delete(report.Id);

        Assert.False(context.Set<Report>().Any());
        Assert.False(context.Set<Section>().Any());
        Assert.False(context.Set<Query>().Any());
        Assert.False(context.Set<QueryTable>().Any());
        Assert.False(context.Set<QueryTableColumn>().Any());
        Assert.False(context.Set<QueryFilter>().Any());
        Assert.False(context.Set<QueryFilterSet>().Any());
        Assert.False(context.Set<View>().Any());
        Assert.False(context.Set<GridView>().Any());
        Assert.False(context.Set<GridViewColumn>().Any());
        Assert.False(context.Set<FilterControl>().Any());
        Assert.False(context.Set<QueryFilterBinding>().Any());
    }

    [Fact]
    public async Task DeleteSectionTest()
    {
        var servicesFactory = new ReportingTestServicesFactory();
        await servicesFactory.CreateInMemoryReportingServices();
        var context = servicesFactory.InMemoryContext;
        var service = servicesFactory.InMemoryReportService;

        var report = servicesFactory.CreateFullReport();
        report.CreatedBy = servicesFactory.CurrentUser.Id;
        report.UpdatedBy = servicesFactory.CurrentUser.Id;
        report.CreatedOn = DateTime.UtcNow;
        report.UpdatedOn = DateTime.UtcNow;
        report.IsDraft = false;
        report.Access = AggregatedRoles.Authenticated;
        await context.Set<Report>().AddAsync(report);
        await context.SaveChangesAsync();

        Assert.ThrowsAsync<ObjectNotExistsException>(() => service.DeleteSection(Guid.Empty, report.Sections[0].Id));
        Assert.ThrowsAsync<ObjectNotExistsException>(() => service.DeleteSection(report.Id, Guid.Empty));
        Assert.ThrowsAsync<BusinessException>(() => service.DeleteSection(report.Id, report.Sections[0].Id));

        report.IsDraft = true;
        await context.SaveChangesAsync();

        await service.DeleteSection(report.Id, report.Sections[0].Id);

        Assert.False(context.Set<Section>().Any());
        Assert.False(context.Set<Query>().Any());
        Assert.False(context.Set<QueryTable>().Any());
        Assert.False(context.Set<QueryTableColumn>().Any());
        Assert.False(context.Set<QueryFilter>().Any());
        Assert.False(context.Set<QueryFilterSet>().Any());
        Assert.False(context.Set<View>().Any());
        Assert.False(context.Set<GridView>().Any());
        Assert.False(context.Set<GridViewColumn>().Any());
        Assert.False(context.Set<FilterControl>().Any());
        Assert.False(context.Set<QueryFilterBinding>().Any());
    }

    [Fact]
    public async Task ExistsTest()
    {
        var servicesFactory = new ReportingTestServicesFactory();
        await servicesFactory.CreateInMemoryReportingServices();
        var context = servicesFactory.InMemoryContext;
        var service = servicesFactory.InMemoryReportService;

        var report = servicesFactory.CreateReport();
        await context.Set<Report>().AddAsync(report);
        await context.SaveChangesAsync();

        Assert.True(await service.Exists(report.Id));
        Assert.False(await service.Exists(Guid.Empty));
    }

    //[Fact]
    //public async Task GetCurrentUserDraftReportTest()
    //{
    //    var servicesFactory = new ReportingTestServicesFactory();
    //    await servicesFactory.CreateInMemoryReportingServices();
    //    var context = servicesFactory.InMemoryContext;
    //    var service = servicesFactory.InMemoryReportService;

    //    var fetchedDraft = await service.GetCurrentUserDraftReport();
    //    Assert.Null(fetchedDraft);

    //    var report = servicesFactory.CreateFullReport();
    //    report.CreatedBy = servicesFactory.CurrentUser.Id;
    //    report.UpdatedBy = servicesFactory.CurrentUser.Id;
    //    report.CreatedOn = DateTime.UtcNow;
    //    report.UpdatedOn = DateTime.UtcNow;
    //    report.IsDraft = false;
    //    report.Access = AggregatedRoles.Authenticated;
    //    await context.Set<Report>().AddAsync(report);
    //    await context.SaveChangesAsync();

    //    fetchedDraft = await service.GetReportDraft(report.Id);
    //    Assert.Null(fetchedDraft);

    //    var draft = servicesFactory.CreateFullReport();
    //    draft.PublishedReportId = report.Id;
    //    draft.CreatedBy = servicesFactory.CurrentUser.Id;
    //    draft.UpdatedBy = servicesFactory.CurrentUser.Id;
    //    draft.CreatedOn = DateTime.UtcNow;
    //    draft.UpdatedOn = DateTime.UtcNow;
    //    draft.IsDraft = true;
    //    draft.Access = AggregatedRoles.Authenticated;
    //    await context.Set<Report>().AddAsync(draft);
    //    await context.SaveChangesAsync();

    //    fetchedDraft = await service.GetReportDraft(report.Id);
    //    Assert.NotNull(fetchedDraft);
    //    Assert.Equal(draft.Id, fetchedDraft.Id);

    //    draft = servicesFactory.CreateFullReport();
    //    draft.CreatedBy = servicesFactory.CurrentUser.Id;
    //    draft.UpdatedBy = servicesFactory.CurrentUser.Id;
    //    draft.CreatedOn = DateTime.UtcNow;
    //    draft.UpdatedOn = DateTime.UtcNow;
    //    draft.IsDraft = true;
    //    draft.Access = AggregatedRoles.Authenticated;
    //    await context.Set<Report>().AddAsync(draft);
    //    await context.SaveChangesAsync();

    //    fetchedDraft = await service.GetCurrentUserDraftReport();
    //    Assert.NotNull(fetchedDraft);
    //    Assert.Equal(draft.Id, fetchedDraft.Id);
    //}

    //[Fact]
    //public async Task PublishReportDraftTest()
    //{
    //    var servicesFactory = new ReportingTestServicesFactory();
    //    await servicesFactory.CreateInMemoryReportingServices();
    //    var context = servicesFactory.InMemoryContext;
    //    var service = servicesFactory.InMemoryReportService;

    //    await Assert.ThrowsAsync<ObjectNotExistsException>(() => service.PublishReportDraft(Guid.Empty));

    //    var reportName = "Report Name";
    //    var report = servicesFactory.CreateFullReport();
    //    report.Name = reportName;
    //    report.CreatedBy = servicesFactory.CurrentUser.Id;
    //    report.UpdatedBy = servicesFactory.CurrentUser.Id;
    //    report.CreatedOn = DateTime.UtcNow;
    //    report.UpdatedOn = DateTime.UtcNow;
    //    report.IsDraft = false;
    //    report.Access = AggregatedRoles.Authenticated;
    //    await context.Set<Report>().AddAsync(report);
    //    await context.SaveChangesAsync();
    //    var reportId = report.Id;

    //    await Assert.ThrowsAsync<BusinessException>(() => service.PublishReportDraft(report.Id));

    //    var draftName = "Draft Name";
    //    var draft = servicesFactory.CreateFullReport();
    //    draft.Name = draftName;
    //    draft.PublishedReportId = report.Id;
    //    draft.CreatedBy = servicesFactory.CurrentUser.Id;
    //    draft.UpdatedBy = servicesFactory.CurrentUser.Id;
    //    draft.CreatedOn = DateTime.UtcNow;
    //    draft.UpdatedOn = DateTime.UtcNow;
    //    draft.IsDraft = true;
    //    draft.Access = AggregatedRoles.Authenticated;
    //    await context.Set<Report>().AddAsync(draft);
    //    await context.SaveChangesAsync();

    //    await service.PublishReportDraft(draft.Id);

    //    Assert.Equal(reportId, draft.Id);
    //    Assert.Equal(draftName, draft.Name);
    //}

    [Fact]
    public async Task ReportUrlSlugExistsTest()
    {
        var servicesFactory = new ReportingTestServicesFactory();
        await servicesFactory.CreateInMemoryReportingServices();
        var context = servicesFactory.InMemoryContext;
        var service = servicesFactory.InMemoryReportService;

        var entity = servicesFactory.CreateFullReport();

        entity.Sections.Clear();
        entity.IsDraft = true;
        await context.Set<Report>().AddAsync(entity);

        var draftReportUrlSlug = entity.UrlSlug;

        var publishedReportUrlSlug = "another-url-slug";
        entity.UrlSlug = publishedReportUrlSlug;
        entity.IsDraft = false;
        await context.Set<Report>().AddAsync(entity);

        await context.SaveChangesAsync();

        Assert.True(await service.ReportUrlSlugExists(publishedReportUrlSlug));
        Assert.False(await service.ReportUrlSlugExists("Non-existing report alias"));
        Assert.False(await service.ReportUrlSlugExists(draftReportUrlSlug));
    }

    [Fact]
    public async Task UpdateSectionTest()
    {
        var servicesFactory = new ReportingTestServicesFactory();
        await servicesFactory.CreateInMemoryReportingServices();
        var context = servicesFactory.InMemoryContext;
        var reportService = servicesFactory.InMemoryReportService;

        var report = servicesFactory.CreateReport();
        report.IsDraft = false;
        var section0 = servicesFactory.CreateSection();
        var section1 = servicesFactory.CreateSection();
        var section1OldSortOrder = 1;
        section1.SortOrder = section1OldSortOrder;
        var section2 = servicesFactory.CreateSection();
        var section2OldSortOrder = 2;
        section2.SortOrder = section2OldSortOrder;
        var oldSectionTitle = section0.Title;
        report.Sections.Add(section0);
        report.Sections.Add(section1);
        report.Sections.Add(section2);
        await context.Set<Report>().AddAsync(report);
        await context.SaveChangesAsync();

        var sectionDto = servicesFactory.Mapper.Map<SectionDTO>(context.Set<Section>().Single(x => x.Id == section0.Id));

        Assert.ThrowsAsync<ObjectNotExistsException>(() => reportService.UpdateSection(Guid.Empty, sectionDto.Id, sectionDto));
        Assert.ThrowsAsync<ObjectNotExistsException>(() => reportService.UpdateSection(report.Id, Guid.Empty, sectionDto));
        Assert.ThrowsAsync<BusinessException>(() => reportService.UpdateSection(report.Id, sectionDto.Id, sectionDto));

        report.IsDraft = true;
        await context.SaveChangesAsync();

        sectionDto.Title = " ";
        Assert.ThrowsAsync<System.ComponentModel.DataAnnotations.ValidationException>(() =>
            reportService.UpdateSection(report.Id, sectionDto.Id, sectionDto));

        sectionDto.Title = "Another Section Title";
        sectionDto.SortOrder = 2;
        sectionDto = (await reportService.UpdateSection(report.Id, sectionDto.Id, sectionDto))?.RequestTargetPart;

        Assert.NotNull(sectionDto);
        Assert.NotEqual(oldSectionTitle, sectionDto.Title);
        Assert.Equal(section1OldSortOrder - 1, section1.SortOrder);
        Assert.Equal(section2OldSortOrder - 1, section2.SortOrder);
        Assert.Equal(2, section0.SortOrder);

        sectionDto.SortOrder = 0;
        sectionDto = (await reportService.UpdateSection(report.Id, sectionDto.Id, sectionDto))?.RequestTargetPart;

        Assert.Equal(section1OldSortOrder, section1.SortOrder);
        Assert.Equal(section2OldSortOrder, section2.SortOrder);
        Assert.Equal(0, section0.SortOrder);
    }

    /*[Fact]
    public async Task UpdateTest()
    {
        var servicesFactory = new ReportingTestServicesFactory();
        servicesFactory.CreateInMemoryReportServices();
        var service = servicesFactory.InMemoryReportService;

        var dto1 = GetDto();

        dto1.Name = " ";
        await Assert.ThrowsAsync<System.ComponentModel.DataAnnotations.ValidationException>(() => service.Update(dto1));

        dto1.Name = "_ _";
        await Assert.ThrowsAsync<System.ComponentModel.DataAnnotations.ValidationException>(() => service.Update(dto1));

        dto1.Name = "Name";
        dto1.UrlSlug = " ";
        await Assert.ThrowsAsync<System.ComponentModel.DataAnnotations.ValidationException>(() => service.Update(dto1));

        dto1.UrlSlug = "UrlSlug";

        var dto2 = GetDto();

        var dto1ShortName = dto1.UrlSlug;

        dto1 = await service.Update(dto1, CancellationToken.None);
        dto2 = await service.Update(dto2, CancellationToken.None);

        dto1.UrlSlug = dto2.UrlSlug;

        await Assert.ThrowsAsync<BusinessException>(() => service.Update(dto1));

        dto1.UrlSlug = dto1ShortName;
        dto1.Name += "some change";

        Assert.NotNull(await service.Update(dto1));
    }

    [Fact]
    public async Task DeleteTest()
    {
        var servicesFactory = new ReportingTestServicesFactory();
        servicesFactory.CreateInMemoryReportServices();
        var service = servicesFactory.InMemoryReportService;
        var context = servicesFactory.InMemoryContext;

        var dto = GetDto();
        await service.CreateDraftOfNewReport(dto);
        await service.Delete(dto.Id);

        Assert.True(context.Set<Report>().All(x => x.Id != dto.Id));
    }

    [Fact]
    public async Task UpdateRolesTest()
    {
        var servicesFactory = new ReportingTestServicesFactory();
        servicesFactory.CreateInMemoryReportServices();
        var context = servicesFactory.InMemoryContext;
        var service = servicesFactory.InMemoryReportService;

        var firstlyAddedRole1 = new Role("FirstlyAddedRole1");
        var firstlyAddedRole2 = new Role("FirstlyAddedRole2");
        var secondlyAddedRole = new Role("SecondlyAddedRole");
        await context.Set<Role>()
            .AddRangeAsync(firstlyAddedRole1, firstlyAddedRole2, secondlyAddedRole);
        await context.SaveChangesAsync(CancellationToken.None);

        var dto = GetDto();
        dto.Roles = new List<RoleDTO>
            {
                servicesFactory.Mapper.Map<Role, RoleDTO>(firstlyAddedRole1),
                servicesFactory.Mapper.Map<Role, RoleDTO>(firstlyAddedRole2),
            };
        dto = await service.CreateDraftOfNewReport(dto, CancellationToken.None);

        Assert.True(dto.Roles.Any(x => x.Name == firstlyAddedRole1.Name) &&
                    dto.Roles.Any(x => x.Name == firstlyAddedRole2.Name));

        dto.Roles = new List<RoleDTO>
            {
                servicesFactory.Mapper.Map<Role, RoleDTO>(firstlyAddedRole2),
                servicesFactory.Mapper.Map<Role, RoleDTO>(secondlyAddedRole),
            };
        dto = await service.Update(dto, CancellationToken.None);

        Assert.True(dto.Roles.All(x => x.Name != firstlyAddedRole1.Name) &&
                    dto.Roles.Any(x => x.Name == firstlyAddedRole2.Name) &&
                    dto.Roles.Any(x => x.Name == secondlyAddedRole.Name));
    }

    [Fact]
    public async Task UpdatePermissionsTest()
    {
        var servicesFactory = new ReportingTestServicesFactory();
        servicesFactory.CreateInMemoryReportServices();
        var context = servicesFactory.InMemoryContext;
        var service = servicesFactory.InMemoryReportService;

        var firstlyAddedPermission1 = new Permission("FirstlyAddedPermission1");
        var firstlyAddedPermission2 = new Permission("FirstlyAddedPermission2");
        var secondlyAddedPermission = new Permission("SecondlyAddedPermission");
        await context.Set<Permission>()
            .AddRangeAsync(firstlyAddedPermission1, firstlyAddedPermission2, secondlyAddedPermission);
        await context.SaveChangesAsync();

        var dto = GetDto();
        dto.Permissions = new List<PermissionDTO>
            {
                servicesFactory.Mapper.Map<Permission, PermissionDTO>(firstlyAddedPermission1),
                servicesFactory.Mapper.Map<Permission, PermissionDTO>(firstlyAddedPermission2),
            };
        await service.CreateDraftOfNewReport(dto);

        Assert.True(dto.Permissions.Any(x => x.Name == firstlyAddedPermission1.Name) &&
                    dto.Permissions.Any(x => x.Name == firstlyAddedPermission2.Name));

        dto.Permissions = new List<PermissionDTO>
            {
                servicesFactory.Mapper.Map<Permission, PermissionDTO>(firstlyAddedPermission2),
                servicesFactory.Mapper.Map<Permission, PermissionDTO>(secondlyAddedPermission),
            };
        dto = await service.Update(dto);

        Assert.True(dto.Permissions.All(x => x.Name != firstlyAddedPermission1.Name) &&
                    dto.Permissions.Any(x => x.Name == firstlyAddedPermission2.Name) &&
                    dto.Permissions.Any(x => x.Name == secondlyAddedPermission.Name));
    }*/

    /*[Fact]
    public async Task GetReportAccessTest()
    {
        var servicesFactory = new ReportingTestServicesFactory();
        await servicesFactory.CreateSqlLiteReportServices();
        var context = servicesFactory.SqlLiteContext;
        var service = servicesFactory.SqlLiteReportService;
        var currentUser = servicesFactory.CurrentUser;

        var userRole = new Role("UserRole");
        var notUserRole = new Role("NotUserRole");
        await context.Set<Role>().AddRangeAsync(userRole, notUserRole);
        var userPermission = new Permission("UserPermission");
        var notUserPermission = new Permission("NotUserPermission");
        await context.Set<Permission>().AddRangeAsync(userPermission, notUserPermission);
        await context.SaveChangesAsync(CancellationToken.None);

        await context.Set<UserRole>().AddAsync(new UserRole { UserId = currentUser.Id, RoleId = userRole.Id }, CancellationToken.None);
        await context.Set<UserPermission>().AddAsync(new UserPermission { UserId = currentUser.Id, PermissionId = userPermission.Id }, CancellationToken.None);
        await context.SaveChangesAsync(CancellationToken.None);

        var report = GetDto();
        report.Access = AggregatedRole.Authenticated;
        report = await service.Create(report, CancellationToken.None);

        Assert.NotNull(await service.GetReportView(report.UrlSlug, CancellationToken.None));
        // Assert.NotNull(service.GetReportData(report.ShortName, null, CancellationToken.None));
        // Assert.NotNull(service.GetReportTotal(report.ShortName, null, CancellationToken.None));

        report.Access = null;
        report.Roles = new List<RoleDTO>
            {
                servicesFactory.Mapper.Map<Role, RoleDTO>(userRole)
            };
        report = await service.Update(report, CancellationToken.None);

        Assert.NotNull(await service.GetReportView(report.UrlSlug, CancellationToken.None));
        // Assert.NotNull(service.GetReportData(report.ShortName, null, CancellationToken.None));
        // Assert.NotNull(service.GetReportTotal(report.ShortName, null, CancellationToken.None));

        report.Roles = new List<RoleDTO>
            {
                servicesFactory.Mapper.Map<Role, RoleDTO>(notUserRole)
            };
        report = await service.Update(report, CancellationToken.None);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            service.GetReportView(report.UrlSlug, CancellationToken.None));
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            service.GetReportData(report.UrlSlug, null, CancellationToken.None));
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            service.GetReportTotal(report.UrlSlug, null, CancellationToken.None));

        report.Roles = new List<RoleDTO>();
        report.Permissions = new List<PermissionDTO>
            {
                servicesFactory.Mapper.Map<Permission, PermissionDTO>(userPermission)
            };
        report = await service.Update(report, CancellationToken.None);

        Assert.NotNull(await service.GetReportView(report.UrlSlug, CancellationToken.None));
        // Assert.NotNull(service.GetReportData(report.ShortName, null, CancellationToken.None));
        // Assert.NotNull(service.GetReportTotal(report.ShortName, null, CancellationToken.None));

        report.Permissions = new List<PermissionDTO>
            {
                servicesFactory.Mapper.Map<Permission, PermissionDTO>(notUserPermission)
            };
        await service.Update(report, CancellationToken.None);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            service.GetReportView(report.UrlSlug, CancellationToken.None));
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            service.GetReportData(report.UrlSlug, null, CancellationToken.None));
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            service.GetReportTotal(report.UrlSlug, null, CancellationToken.None));
    }*/
}
