using BBWM.DataProcessing.Classes;

using Xunit;

namespace BBWM.DataProcessing.Test.Classes;

public class ImportEntryCellTest
{
    [Fact]
    public void Properties()
    {
        // Arrange
        ColumnDefinition columnDefinition = new()
        {
            TargetFieldName = "Name",
            OrderNumber = 1,
            Type = CellDataType.String,
        };

        // Act
        ImportEntryCell importEntryCell = new("John Doe", columnDefinition);

        // Assert
        Assert.Equal(columnDefinition.TargetFieldName, importEntryCell.TargetFieldName);
        Assert.Equal(columnDefinition.OrderNumber, importEntryCell.OrderNumber);
        Assert.Equal((int)columnDefinition.Type, importEntryCell.Type);
    }
}
