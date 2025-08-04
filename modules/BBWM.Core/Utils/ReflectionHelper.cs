using System.Reflection;

namespace BBWM.Core.Utils;

public static class ReflectionHelper
{
    /// <summary>
    /// Extracts all constants in the solution from classes which names match the specified value.
    /// </summary>
    /// <typeparam name="T">Type of constants.</typeparam>
    /// <param name="className">Name of classes where the searching performs.</param>
    public static IEnumerable<T> GetAllConstantsValuesFromClassesOfSolution<T>(string className) =>
        AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly => assembly.FullName.StartsWith("BBWT") || assembly.FullName.StartsWith("BBWM"))
            .SelectMany(assembly => assembly.GetTypes().Where(type => type.IsClass && type.Name == className))
            .SelectMany(classItem => classItem.GetFields(BindingFlags.Public | BindingFlags.Static))
            .Where(fieldInfo => fieldInfo.IsLiteral && !fieldInfo.IsInitOnly && fieldInfo.FieldType == typeof(T))
            .Select(fieldInfo => fieldInfo.GetValue(null))
            .Cast<T>();

    /// <summary>
    /// Extracts all constants in the specified assembly from classes which names match the specified value.
    /// </summary>
    /// <typeparam name="T">Type of constants.</typeparam>
    /// <param name="assembly">Assembly where the searching performs.</param>
    /// <param name="className">Name of classes where the searching performs.</param>
    public static IEnumerable<T> GetAllConstantsValuesFromClassesOfAssembly<T>(Assembly assembly, string className) =>
        assembly.GetTypes().Where(type => type.IsClass && type.Name == className)
            .SelectMany(classItem => classItem.GetFields(BindingFlags.Public | BindingFlags.Static))
            .Where(fieldInfo => fieldInfo.IsLiteral && !fieldInfo.IsInitOnly && fieldInfo.FieldType == typeof(T))
            .Select(fieldInfo => fieldInfo.GetValue(null))
            .Cast<T>();

    /// <summary>
    /// Extracts all constants from the specified class type.
    /// </summary>
    /// <typeparam name="T">Type of constants.</typeparam>
    /// <param name="classType">Class type where the searching performs.</param>
    public static IEnumerable<T> GetAllConstantsValuesOfClass<T>(Type classType) =>
        classType.GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(fieldInfo => fieldInfo.IsLiteral && !fieldInfo.IsInitOnly && fieldInfo.FieldType == typeof(T))
            .Select(fieldInfo => fieldInfo.GetValue(null))
            .Cast<T>();
}
