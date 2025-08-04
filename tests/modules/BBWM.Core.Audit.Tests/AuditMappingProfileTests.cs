using AutoMapper;

using Microsoft.EntityFrameworkCore;

using Xunit;

namespace BBWM.Core.Audit.Tests;

public class AuditMappingProfileTests
{
    public static IEnumerable<object[]> NullLogsTextTestData => new[]
    {
            new object[] { EntityState.Added, new List<ChangeLogItem>() },
            new object[] { EntityState.Added, null },
            new object[] { EntityState.Modified, new List<ChangeLogItem>() },
            new object[] { EntityState.Modified, null },
            new object[]
            {
                EntityState.Deleted,
                new List<ChangeLogItem>
                {
                    new () { PropertyName = "P", OldValue = "old-P", NewValue = "new-P" },
                    new () { PropertyName = "Q", OldValue = "old-Q", NewValue = "new-Q" },
                },
            },
        };

    private void AssertText(string property, string[] logTexts, EntityState changeLogState)
    {
        var expected = changeLogState == EntityState.Added
            ? $"\"{property}\" assigned a value of \"new-{property}\""
            : $"\"{property}\" changed from \"old-{property}\" to \"new-{property}\"";

        Assert.Contains(expected, logTexts);
    }

    private static (IMapper, ChangeLog) CreateMapper(EntityState changeLogState, List<ChangeLogItem> items)
    {
        ChangeLog changeLog = new()
        {
            ChangeLogItems = items,
            State = changeLogState,
        };
        IMapper mapper = new MapperConfiguration(c => c.AddProfile<AuditMappingProfile>()).CreateMapper();

        return (mapper, changeLog);
    }
}
