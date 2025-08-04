using BBWM.Core.Utils;

using System.Reflection;

namespace BBWM.Core.Membership.Utils;

public static class RolesExtractor
{
    public static IEnumerable<string> GetAllRolesNamesOfSolution() =>
        ReflectionHelper.GetAllConstantsValuesFromClassesOfSolution<string>("Roles");

    public static IEnumerable<string> GetAllRolesNamesOfAssembly(Assembly assembly) =>
        ReflectionHelper.GetAllConstantsValuesFromClassesOfAssembly<string>(assembly, "Roles");

    public static IEnumerable<string> GetRolesNamesOfClass(Type classType) =>
        ReflectionHelper.GetAllConstantsValuesOfClass<string>(classType);
}
