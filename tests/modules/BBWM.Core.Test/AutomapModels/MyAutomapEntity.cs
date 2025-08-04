using BBWM.Core.Data;

namespace BBWM.Core.Test.AutomapModels;

public class MyAutomapEntity : IEntity
{
    public int Id { get; set; }

    public string ShouldMap1 { get; set; }

    public int ShouldMap2 { get; set; }

    public string ShouldntMap1 { get; set; }

    public int ShouldntMap2 { get; set; }
}

public class MyAutomapEntityDTO
{
    public int Id { get; set; }

    public string ShouldMap1 { get; set; }

    public int ShouldMap2 { get; set; }
}
