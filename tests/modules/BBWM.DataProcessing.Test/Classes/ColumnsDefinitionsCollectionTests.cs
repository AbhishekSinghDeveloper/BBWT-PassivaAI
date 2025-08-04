using BBWM.DataProcessing.Classes;

using System.Collections;

using Xunit;

namespace BBWM.DataProcessing.Test.Classes;

public class ColumnsDefinitionsCollectionTests
{
    [Fact]
    public void AddColumn_Should_Throw_On_Null_Column()
    {
        // Arrange
        ColumnsDefinitionsCollection columnDefinitions = new();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => columnDefinitions.AddColumn(null));
    }

    [Fact]
    public void AddColumn_Should_Throw_On_Invalid_Order()
    {
        // Arrange
        ColumnsDefinitionsCollection columnDefinitions = new();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => columnDefinitions.AddColumn(new() { OrderNumber = -1 }));
    }

    [Fact]
    public void AddColumn_Should_Throw_On_Same_Order()
    {
        // Arrange
        ColumnsDefinitionsCollection columnDefinitions = new(new ColumnDefinition { OrderNumber = 1 });

        // Act & Assert
        Assert.Throws<ArgumentException>(() => columnDefinitions.AddColumn(new() { OrderNumber = 1 }));
    }

    [Fact]
    public void Count()
    {
        // Arrange
        ColumnsDefinitionsCollection columnDefinitions = new();

        // Act
        columnDefinitions.AddColumn(new() { OrderNumber = 1 });

        // Assert
        Assert.Equal(1, columnDefinitions.Count);
    }

    [Fact]
    public void Enumerator()
    {
        // Arrange
        ColumnsDefinitionsCollection columnDefinitions = new();

        // Act
        foreach (var order in new[] { 3, 1, 2 })
        {
            columnDefinitions.AddColumn(new() { OrderNumber = order });
        }

        // Assert
        IEnumerator enumerator = (columnDefinitions as IEnumerable).GetEnumerator();
        int currentOrder = 1;

        while (enumerator.MoveNext())
        {
            Assert.Equal(currentOrder++, (enumerator.Current as ColumnDefinition).OrderNumber);
        }

        Assert.Equal(4, currentOrder);
    }
}
