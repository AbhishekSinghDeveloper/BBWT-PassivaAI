using BBWM.DataProcessing.Classes;

using Microsoft.AspNetCore.Http;

using Moq;

using Xunit;

namespace BBWM.DataProcessing.Test.Classes;

public class ImportDataModelTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("my-file.png")]
    public void Get_FileName(string formFileName)
    {
        // Arrange
        Mock<IFormFile> formFile = new();
        formFile.Setup(f => f.FileName).Returns(formFileName);

        ImportDataModel importDataModel = new() { File = string.IsNullOrEmpty(formFileName) ? null : formFile.Object };

        // Act
        string filename = importDataModel.FileName;

        // Assert
        Assert.Equal(formFileName, filename);
    }

    [Fact]
    public void Set_FileName()
    {
        // Arrange
        ImportDataModel importDataModel = new();
        const string FileName = "my-file.png";

        // Act
        importDataModel.FileName = FileName;

        // Assert
        Assert.Equal(FileName, importDataModel.FileName);
    }
}
