using BBWM.Core.Data;
using BBWM.Core.Membership.Model;
using BBWM.Core.Test;
using BBWM.Core.Test.Utils;

using BBWT.Tests.modules.BBWM.Core.Test.Models;

using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;

using Moq;

using System.Security.Claims;

using Xunit;

using AspNetClaimTypes = System.Security.Claims.ClaimTypes;

namespace BBWM.Core.Audit.Tests;

public class AuditWrapperTests
{
    [Theory]
    [MemberData(nameof(AfterSaveNoopTestData))]
    public async Task OnAfterSaveChanges_Should_Noop(IEnumerable<EntityEntry> entries)
    {
        // Arrange
        var auditDataContext = CreateAuditDataContext();
        var service = CreateAuditWrapper(auditDataContext);
        service.OnBeforeSaveChanges(entries);

        // Act
        await service.OnAfterSaveChanges();

        // Assert
        Assert.Empty(auditDataContext.ChangeLogs);
    }

    [Theory]
    [MemberData(nameof(AfterSaveShouldCreateLogsTestData))]
    public async Task OnAfterSaveChanges_Should_Create_Logs(
        string entityName,
        EntityEntry entry,
        EntityState stateShouldBe,
        HttpContextAccessorCreateMode httpContextAccessorCreateMode,
        Action<ChangeLog> asserts)
    {
        // Arrange
        IAuditContext auditDataContext = CreateAuditDataContext();
        IHttpContextAccessor httpContextAccessor = CreateContextAccessor(httpContextAccessorCreateMode);
        IAuditWrapper auditWrapper = CreateAuditWrapper(auditDataContext, httpContextAccessor);

        // Act
        auditWrapper.OnBeforeSaveChanges(new[] { entry });
        await auditWrapper.OnAfterSaveChanges();

        // Assert
        var changeLog = Assert.Single(auditDataContext.ChangeLogs);
        Assert.Equal(entityName, changeLog.EntityName);
        Assert.Equal(stateShouldBe, changeLog.State);
        asserts?.Invoke(changeLog);
    }

    private static IAuditContext CreateAuditDataContext(string dbName = default)
    {
        if (string.IsNullOrEmpty(dbName))
            dbName = Guid.NewGuid().ToString();

        var options = new DbContextOptionsBuilder<AuditContext>()
            .UseInMemoryDatabase(dbName, new InMemoryDatabaseRoot())
            .Options;

        return new AuditContext(options);
    }

    private static IAuditWrapper CreateAuditWrapper(
        IAuditContext auditDataContext = default, IHttpContextAccessor httpContextAccessor = default)
    {
        auditDataContext ??= SutDataHelper.CreateEmptyContext<IAuditContext>();
        return new AuditWrapper(auditDataContext, httpContextAccessor);
    }

    public static IEnumerable<object[]> AfterSaveNoopTestData => new[]
    {
            new object[] { Enumerable.Empty<EntityEntry>() },
            new object[] { null },
        };

    public static IEnumerable<object[]> AfterSaveShouldCreateLogsTestData => CreateAfterSaveShouldCreateLogsTestData();

    private static IEnumerable<object[]> CreateAfterSaveShouldCreateLogsTestData()
    {
        using var dbContext = SutDataHelper.CreateEmptyContext<DataContext>();

        User userModified = new() { Email = "user1@uts.dev" };
        User userDeleted = new() { Email = "user2@uts.dev" };
        AuditableIntPKEntity auditableIntPKEntityModified = new() { Name = "Auditable One" };
        AuditableIntPKEntity auditableIntPKEntityDeleted = new() { Name = "Auditable Two" };

        dbContext.Users.AddRange(userModified, userDeleted);
        dbContext.AuditableIntPKEntities.AddRange(auditableIntPKEntityModified, auditableIntPKEntityDeleted);
        dbContext.SaveChanges();

        User userAdded = new() { Email = "user3@uts.dev" };
        AuditableIntPKEntity auditableIntPKAdded = new() { Name = "Auditable Three" };

        EntityEntry userAddedEntry = dbContext.Users.Add(userAdded);
        EntityEntry auditableIntPKAddedEntry = dbContext.AuditableIntPKEntities.Add(auditableIntPKAdded);

        userModified.Email = "user1-updated@uts.dev";
        auditableIntPKEntityModified.Name = "Auditable One - updated";

        EntityEntry userModifiedEntry = dbContext.Users.Update(userModified);
        EntityEntry auditableIntPKEntityModifiedEntry = dbContext.AuditableIntPKEntities.Update(auditableIntPKEntityModified);

        EntityEntry userDeletedEntry = dbContext.Users.Remove(userDeleted);
        EntityEntry auditableIntPKEntityDeletedEntry = dbContext.AuditableIntPKEntities.Remove(auditableIntPKEntityDeleted);

        return new[]
        {
                new object[]
                {
                    nameof(User),
                    userAddedEntry,
                    EntityState.Added,
                    HttpContextAccessorCreateMode.WithHttpContext,
                    AssertUserEmail("user3@uts.dev", null),
                },
                new object[]
                {
                    nameof(AuditableIntPKEntity),
                    auditableIntPKAddedEntry,
                    EntityState.Added,
                    HttpContextAccessorCreateMode.WithHttpContext,
                    AssertAuditableIntPKName("Auditable Three", null),
                },
                new object[]
                {
                    nameof(User),
                    userModifiedEntry,
                    EntityState.Modified,
                    HttpContextAccessorCreateMode.None,
                    AssertUserEmail("user1-updated@uts.dev", "user1@uts.dev"),
                },
                new object[]
                {
                    nameof(AuditableIntPKEntity),
                    auditableIntPKEntityModifiedEntry,
                    EntityState.Modified,
                    HttpContextAccessorCreateMode.None,
                    AssertAuditableIntPKName("Auditable One - updated", "Auditable One"),
                },
                new object[]
                {
                    nameof(User),
                    userDeletedEntry,
                    EntityState.Deleted,
                    HttpContextAccessorCreateMode.WithNullHttpContext,
                    null,
                },
                new object[]
                {
                    nameof(AuditableIntPKEntity),
                    auditableIntPKEntityDeletedEntry,
                    EntityState.Deleted,
                    HttpContextAccessorCreateMode.WithNullHttpContext,
                    null,
                },
            };
    }

    private static Action<ChangeLog> AssertAuditableIntPKName(string expectedNewName, string expectedOldName)
        => changeLog =>
        {
            ChangeLogItem changeLogItem =
                changeLog.ChangeLogItems.FirstOrDefault(item => item.PropertyName == nameof(AuditableIntPKEntity.Name));

            Assert.NotNull(changeLogItem);
            Assert.Equal(expectedNewName, changeLogItem.NewValue);
            Assert.Equal(expectedOldName, changeLogItem.OldValue);
        };

    private static Action<ChangeLog> AssertUserEmail(string expectedNewEmail, string expectedOldEmail)
        => changeLog =>
        {
            ChangeLogItem changeLogItem =
                changeLog.ChangeLogItems.FirstOrDefault(item => item.PropertyName == nameof(User.Email));

            Assert.NotNull(changeLogItem);
            Assert.Equal(expectedOldEmail, changeLogItem.OldValue);
            Assert.Equal(expectedNewEmail, changeLogItem.NewValue);
        };

    private IHttpContextAccessor CreateContextAccessor(HttpContextAccessorCreateMode httpContextAccessorCreateMode)
        => httpContextAccessorCreateMode switch
        {
            HttpContextAccessorCreateMode.None => null,
            HttpContextAccessorCreateMode.WithHttpContext =>
                ServicesFactory.GetHttpContextAccessor(new List<Claim> { new Claim(AspNetClaimTypes.Name, "UT-User") }),
            HttpContextAccessorCreateMode.WithNullHttpContext => Mock.Of<IHttpContextAccessor>()
        };
}

public enum HttpContextAccessorCreateMode
{
    None = 1,
    WithNullHttpContext,
    WithHttpContext,
}
