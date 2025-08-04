using BBWM.Core.ModelHashing;
using BBWT.Tests.modules.BBWM.Core.Test.ModelHashing.Contexts;
using Xunit;

namespace BBWM.Core.Test.ModelHashing;

public class ModelHashingServiceTests
{
    private readonly IModelHashingService sut;

    public ModelHashingServiceTests()
    {
        sut = new ModelHashingService();
    }

    [Fact]
    public void GetMaps_ShouldEmptyKeysMaps_When_ModelIsNotRegistered()
    {
        var result = sut.GetMaps(typeof(MasterDTO));
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void GetMaps_ShouldCreateCorrectKeysMaps_When_RegisterWasCalled()
    {
        var mapper = TwoItemsTestDbContext.CreateMapper();
        var context = TwoItemsTestDbContext.CreateForInMemory();

        sut.Register(mapper, context);

        Assert.NotNull(sut.GetMaps(typeof(MasterDTO)));
        Assert.NotEmpty(sut.GetMaps(typeof(MasterDTO)));
        Assert.Contains(sut.GetMaps(typeof(MasterDTO)), x => x.ModelType == typeof(MasterDTO) && x.Property.Equals(nameof(MasterDTO.Id), StringComparison.InvariantCultureIgnoreCase));

        Assert.NotNull(sut.GetMaps(typeof(DetailDTO)));
        Assert.NotEmpty(sut.GetMaps(typeof(DetailDTO)));
        Assert.Contains(sut.GetMaps(typeof(DetailDTO)), x => x.ModelType == typeof(DetailDTO) && x.Property.Equals(nameof(DetailDTO.Id), StringComparison.InvariantCultureIgnoreCase));
        Assert.Contains(sut.GetMaps(typeof(DetailDTO)), x => x.ModelType == typeof(DetailDTO) && x.Property.Equals(nameof(DetailDTO.MasterId), StringComparison.InvariantCultureIgnoreCase));
    }

    [Fact]
    public void GetMaps_ShouldCreateCorrectKeysMaps_When_IgnoreModelHashingWasCalled()
    {
        var mapper = TwoItemsTestDbContext.CreateMapper();
        var context = TwoItemsTestDbContext.CreateForInMemory();

        sut.Register(mapper, context);
        sut.IgnoreModelHashing<DetailDTO>();

        Assert.NotNull(sut.GetMaps(typeof(MasterDTO)));
        Assert.NotEmpty(sut.GetMaps(typeof(MasterDTO)));
        Assert.Contains(sut.GetMaps(typeof(MasterDTO)), x => x.ModelType == typeof(MasterDTO) && x.Property.Equals(nameof(MasterDTO.Id), StringComparison.InvariantCultureIgnoreCase));

        Assert.NotNull(sut.GetMaps(typeof(DetailDTO)));
        Assert.Empty(sut.GetMaps(typeof(DetailDTO)));
    }

    [Fact]
    public void GetMaps_ShouldCreateCorrectKeysMaps_When_IgnorePropertyHashingWasCalled()
    {
        var mapper = TwoItemsTestDbContext.CreateMapper();
        var context = TwoItemsTestDbContext.CreateForInMemory();

        sut.Register(mapper, context);
        sut.IgnorePropertiesHashing<DetailDTO>(a => a.MasterId);

        Assert.NotNull(sut.GetMaps(typeof(MasterDTO)));
        Assert.NotEmpty(sut.GetMaps(typeof(MasterDTO)));
        Assert.Contains(sut.GetMaps(typeof(MasterDTO)), x => x.ModelType == typeof(MasterDTO) && x.Property.Equals(nameof(MasterDTO.Id), StringComparison.InvariantCultureIgnoreCase));

        Assert.NotNull(sut.GetMaps(typeof(MasterDTO)));
        Assert.NotEmpty(sut.GetMaps(typeof(MasterDTO)));
        Assert.Contains(sut.GetMaps(typeof(MasterDTO)), x => x.ModelType == typeof(MasterDTO) && x.Property.Equals(nameof(MasterDTO.Id), StringComparison.InvariantCultureIgnoreCase));

        Assert.NotNull(sut.GetMaps(typeof(DetailDTO)));
        Assert.NotEmpty(sut.GetMaps(typeof(DetailDTO)));
        Assert.Contains(sut.GetMaps(typeof(DetailDTO)), x => x.ModelType == typeof(DetailDTO) && x.Property.Equals(nameof(DetailDTO.Id), StringComparison.InvariantCultureIgnoreCase));
        Assert.DoesNotContain(sut.GetMaps(typeof(DetailDTO)), x => x.ModelType == typeof(DetailDTO) && x.Property.Equals(nameof(DetailDTO.MasterId), StringComparison.InvariantCultureIgnoreCase));
    }

    [Fact]
    public void GetMaps_ShouldCreateCorrectKeysMaps_When_ManualPropertyHashingWasCalled()
    {
        sut.ManualPropertyHashing<MasterDTO, Master>(e => e.Id);
        sut.ManualPropertyHashing<DetailDTO, Master>(e => e.MasterId);

        Assert.NotNull(sut.GetMaps(typeof(MasterDTO)));
        Assert.NotEmpty(sut.GetMaps(typeof(MasterDTO)));
        Assert.Contains(sut.GetMaps(typeof(MasterDTO)), x => x.ModelType == typeof(MasterDTO) && x.Property.Equals(nameof(MasterDTO.Id), StringComparison.InvariantCultureIgnoreCase));

        Assert.NotNull(sut.GetMaps(typeof(DetailDTO)));
        Assert.NotEmpty(sut.GetMaps(typeof(DetailDTO)));
        Assert.DoesNotContain(sut.GetMaps(typeof(DetailDTO)), x => x.ModelType == typeof(DetailDTO) && x.Property.Equals(nameof(DetailDTO.Id), StringComparison.InvariantCultureIgnoreCase));
        Assert.Contains(sut.GetMaps(typeof(DetailDTO)), x => x.ModelType == typeof(DetailDTO) && x.Property.Equals(nameof(DetailDTO.MasterId), StringComparison.InvariantCultureIgnoreCase));
    }
}
