using BBWM.Core.Web.OData;

using Xunit;

namespace BBWM.Core.Test.Web.OData;

public class ODataQueryStringTests
{
    public ODataQueryStringTests()
    {
    }

    private static ODataQueryString GetService()
    {
        return new ODataQueryString("http://host/service/Products?$filter=Name eq 'Milk");
    }

    [Fact]
    public void Contains_Filter_Test()
    {
        // Arrange
        var service = GetService();
        var result = service.ContainsFilter("http://host/service/Products?$filter=Name eq 'Milk'");
        Assert.NotNull(result);
    }

    [Fact]
    public void Contains_Expand_Test()
    {
        // Arrange
        var service = GetService();
        var result = service.ContainsExpand("http://host/service/Products?$expand=Category");
        Assert.NotNull(result);
    }

    [Fact]
    public void Get_Order_Field_Name_Test()
    {
        // Arrange
        var service = GetService();
        var result = service.GetOrderFieldName();
        Assert.Null(result);
    }

    [Fact]
    public void Get_Statement_Test()
    {
        var service = GetService();
        var result = service.GetStatement("statementName");
        Assert.Null(result);
    }

    [Fact]
    public void Get_Filter_Values_Test()
    {
        // Arrange
        var service = GetService();
        var result = service.GetFilterValues<FakeClass>("http://host/service/Products?$filter=Name eq 'Milk'");
        Assert.NotNull(result);
    }

    [Fact]
    public void Remove_Filter_Test()
    {
        // Arrange
        var service = GetService();
        var service2 = new ODataQueryString("GET http://services.odata.org/v4/TripPinServiceRW/People?$filter=(FirstName ne 'Mary' and LastName ne 'White') and UserName ne 'marywhite'");

        Action result = () => service2.RemoveFilter("GET http://services.odata.org/v4/TripPinServiceRW/People?$filter=(FirstName ne 'Mary' and LastName ne 'White') and UserName ne 'marywhite'");

        service.RemoveFilter("http://host/service/Products?$filter=Name eq 'Milk'");
        service.RemoveFilter("");

        Assert.NotNull(result);
    }

    [Fact]
    public void Remove_Ordering_Test()
    {
        // Arrange
        var service = new ODataQueryString("http://host/service/Categories? $expand=Products($orderby=ReleaseDate asc, Rating desc)");
        service.RemoveOrdering();
    }

    private class FakeClass
    {
        public string Name { get; set; } = "Milk";
    }
}
