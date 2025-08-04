using System.Diagnostics;

namespace BBWM.SystemData;

public class ProductVersionService : IProductVersionService
{
    public string GetVersion()
        => FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).ProductVersion;
}
