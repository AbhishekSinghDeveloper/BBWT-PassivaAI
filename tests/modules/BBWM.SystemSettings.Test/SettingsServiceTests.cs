using BBWM.Core.Test;

using Bogus;

using Xunit;

namespace BBWM.SystemSettings.Test;

public class SettingsServiceTests
{
    private readonly DataContext context;

    public SettingsServiceTests()
    {
        this.context = InMemoryDataContext.GetContext(Guid.NewGuid().ToString());
    }

    private ISettingsService GetService(Action<ISettingsSectionService> registerSections = default)
    {
        var sectionsSettings = new SettingsSectionService();
        registerSections?.Invoke(sectionsSettings);

        return new SettingsService(this.context, sectionsSettings, null);
    }

    private static SettingsDTO GetEntity()
    {
        var fakerSection = new Faker<TestSettingsSection>().
            RuleFor(e => e.TestProperty, s => s.Random.AlphaNumeric(20));
        var fakerSettings = new Faker<SettingsDTO>().
            RuleFor(e => e.SectionName, s => s.Random.AlphaNumeric(10)).
            RuleFor(e => e.Value, fakerSection.Generate());
        var result = fakerSettings.Generate();
        return result;
    }

    /// <summary>
    /// Covers DecryptSettingsValue() and EncryptSettingsValue();
    /// </summary>
    /// <returns></returns>


    [Fact]
    public async Task SaveLoad_Encription_Test()
    {
        // Arrange
        await context.Set<AppSettings>().AddAsync(
            new AppSettings()
            {
                Section = "TestSectionSettings",
                Value = "{\"Test\": \"test\"}",
                EncryptedFields = "string",      //I'm not sure what should be here
            });
        await context.SaveChangesAsync();

        var entity = new SettingsDTO { SectionName = "TestSectionSettings", Value = "{\"Test\": \"test\"}", };
        var service = this.GetService(sectionsService => sectionsService.RegisterSection<string>(entity.SectionName));

        // Act
        await service.Save(new SettingsDTO[] { entity });
        var result = await service.Load();

        // Assert
        Assert.Equal(entity.SectionName, result.FirstOrDefault()?.SectionName);
        Assert.Equal(entity.Value, result.FirstOrDefault()?.Value);
    }

    [Fact]
    public void SaveGetSettingsSectionTest()
    {
        // Arrange
        var entity = GetEntity();
        var service = this.GetService(sectionsService => sectionsService.RegisterSection<TestSettingsSection>(entity.SectionName));

        // Act
        service.SaveSettingsSection(entity.Value as TestSettingsSection);
        var result = service.GetSettingsSection<TestSettingsSection>();

        // Assert
        Assert.Equal((entity.Value as TestSettingsSection).TestProperty, result.TestProperty);
    }
}
