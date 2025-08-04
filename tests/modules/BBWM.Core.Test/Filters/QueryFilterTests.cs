using BBWM.Core.Filters;
using BBWM.Core.Filters.TypedFilters;
using Bogus;

using System.Diagnostics.CodeAnalysis;
using System.Globalization;

using Xunit;

namespace BBWM.Core.Test.Filters;

public class QueryFilterTests
{
    [Fact]
    public void Handle_Should_Apply_Filter()
    {
        // Arrange
        var (entities, qf, filter) = CreateQueryFilter();

        // Act
        qf.Handle(nameof(HelperEntity.Day), (q, f) => q.Where(e => e.Day == f.Value));

        // Assert
        var expected = entities.Where(e => e.Day == filter.Value).ToList();
        var actual = qf.Query.ToList();

        Assert.Empty(qf.Filters);
        AssertHandle(expected, actual);
    }

    [Fact]
    public void Handle_Should_Not_Apply_Filter()
    {
        // Arrange
        var (entities, qf, filter) = CreateQueryFilter();
        filter.PropertyName += "`";

        // Act
        qf.Handle(nameof(HelperEntity.Day), (q, f) => q.Where(e => e.Day == f.Value));

        // Assert
        var expected = entities.ToList();
        var actual = qf.Query.ToList();

        Assert.Single(qf.Filters);
        AssertHandle(expected, actual);
    }

    private static (HelperEntity[], QueryFilter<HelperEntity>, StringFilter) CreateQueryFilter()
    {
        var days = new[] { "Monday, Tuesday, Wednesday, Thursday, Friday, Saturday, Sunday" };
        var entities = new Faker<HelperEntity>()
            .RuleFor(e => e.Day, f => f.PickRandom(days))
            .Generate(20)
            .ToArray();

        var filter = new StringFilter()
        {
            MatchMode = StringFilterMatchMode.Equals,
            PropertyName = nameof(HelperEntity.Day),
            Value = "Tuesday",
        };

        var qf = new QueryFilter<HelperEntity>(new List<FilterInfoBase> { filter }, entities.AsQueryable());

        return (entities, qf, filter);
    }

    private static void AssertHandle(List<HelperEntity> expected, List<HelperEntity> actual)
    {
        Assert.Equal(expected.Count, actual.Count);
        Assert.Equal(expected, actual, new HelperEntity());
    }

    private class HelperEntity : IEqualityComparer<HelperEntity>
    {
        public string Day { get; set; }

        public bool Equals(HelperEntity x, HelperEntity y)
            => string.Compare(x.Day, y.Day, false, CultureInfo.InvariantCulture) == 0;

        public int GetHashCode([DisallowNull] HelperEntity obj) => Day.GetHashCode();
    }
}
