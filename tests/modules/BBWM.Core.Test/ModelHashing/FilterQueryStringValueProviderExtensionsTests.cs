using BBWM.Core.ModelHashing;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

using Moq;

using System.Globalization;

using Xunit;

namespace BBWM.Core.Test.ModelHashing;

public class FilterQueryStringValueProviderExtensionsTests
{
    public FilterQueryStringValueProviderExtensionsTests()
    {
    }

    [Fact]
    public void AddOriginalFiltersFixingValueProvider_StateUnderTest_ExpectedBehavior()
    {
        var mvcOptions = new Mock<MvcOptions>();

        FilterQueryStringValueProviderExtensions.AddOriginalFiltersFixingValueProvider(mvcOptions.Object);

        var bindingSource = new BindingSource("1", It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>());
        var queryCollection = new Mock<IQueryCollection>();
        queryCollection.Setup(p => p.Keys).Returns(new List<string>() { "Filters", "PropertyName", "_origin" });

        var cultureInfo = new CultureInfo(1, true);

        var filterQueryStringValueProvider = new FilterQueryStringValueProvider(bindingSource, queryCollection.Object, cultureInfo);

        filterQueryStringValueProvider.GetValue("Filters");

        var filterQueryStringValueProviderFactory = new FilterQueryStringValueProviderFactory();

        var actionCtx = new ActionContext();
        var valueProviderFactoryContext = new ValueProviderFactoryContext(actionCtx);

        Action result = () => filterQueryStringValueProviderFactory.CreateValueProviderAsync(valueProviderFactoryContext);

        Assert.Throws<NullReferenceException>(result);
        Assert.NotNull(mvcOptions.Object);
        Assert.NotNull(filterQueryStringValueProvider);
        Assert.NotNull(cultureInfo);
    }
}
