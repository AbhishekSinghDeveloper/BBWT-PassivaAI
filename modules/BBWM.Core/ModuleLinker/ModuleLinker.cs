using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Concurrent;
using System.Reflection;

namespace BBWM.Core.ModuleLinker;

public class ModuleLinker
{
    public const string CoreAssemblyNamePrefix = "BBWM.Core";
    public const string DemoAssemblyNamePrefix = "BBWM.Demo";
    public const string FeatureAssemblyNamePrefix = "BBF.";
    public const string ModuleAssemblyNamePrefix = "BBWM.";
    public const string ProjectAssemblyNamePrefix = "BBWT.";

    public static readonly List<KeyValuePair<Type, string>> LinkedClasses = new();
    public static readonly List<Exception> InvokeExceptions = new();
    public static readonly BlockingCollection<Exception> CommonExceptions = new();

    private static bool _assembliesPreloaded = false;

    public static void AddInvokeException(Exception ex) => InvokeExceptions.Add(ex);

    public static void AddCommonException(Exception ex) => CommonExceptions.Add(ex);

    public static bool IsDemoModuleAssembly(Assembly assembly) =>
        assembly.FullName.StartsWith(DemoAssemblyNamePrefix + ",")
            || assembly.FullName.StartsWith(DemoAssemblyNamePrefix + ".");

    public static bool IsCoreModuleAssembly(Assembly assembly) =>
        assembly.FullName.StartsWith(CoreAssemblyNamePrefix + ",")
            || assembly.FullName.StartsWith(CoreAssemblyNamePrefix + ".");

    public static bool IsFeatureAssembly(Assembly assembly) => assembly.FullName.StartsWith(FeatureAssemblyNamePrefix);

    public static bool IsModuleAssembly(Assembly assembly) => assembly.FullName.StartsWith(ModuleAssemblyNamePrefix);

    public static bool IsProjectAssembly(Assembly assembly) => assembly.FullName.StartsWith(ProjectAssemblyNamePrefix);

    public static IEnumerable<Assembly> GetBbAssemblies() =>
        AppDomain.CurrentDomain.GetAssemblies()
            .Where(o => o.FullName.StartsWith(FeatureAssemblyNamePrefix)
                || o.FullName.StartsWith(ModuleAssemblyNamePrefix)
                || o.FullName.StartsWith(ProjectAssemblyNamePrefix));

    public static void RunLinkers<TInterface>(Action<TInterface> handler)
    {
        var linkers = GetInstances<TInterface>();

        foreach (var linker in linkers)
        {
            try
            {
                handler.Invoke(linker);
            }
            catch (Exception ex)
            {
                AddInvokeException(ex);
            }
        }
    }

    public static List<TInterface> GetInstances<TInterface>()
    {
        if (!_assembliesPreloaded)
        {
            PreloadAssemblies();
            _assembliesPreloaded = true;
        }

        var linkers = new List<TInterface>();
        var linkerType = typeof(TInterface);

        foreach (var assembly in GetBbAssemblies().OrderBy(o => o.FullName, new AssembliesLinkageOrderComparer()))
        {
            var linkerClasses = assembly.GetTypes().Where(p => p.IsClass && linkerType.IsAssignableFrom(p) && !p.ContainsGenericParameters);

            foreach (var linkerClass in linkerClasses)
            {
                if (linkerClass is not null)
                {
                    var linkerInstance = (TInterface)assembly.CreateInstance(linkerClass.FullName);
                    linkers.Add(linkerInstance);

                    LinkedClasses.Add(new KeyValuePair<Type, string>(linkerClass, Environment.StackTrace));
                }
            }
        }

        return linkers;
    }

    private static void PreloadAssemblies()
    {
        foreach (var assembly in GetBbAssemblies())
            LoadReferencedAssembly(assembly);
    }

    private static void LoadReferencedAssembly(Assembly assembly)
    {
        var refAssembliesNames = assembly.GetReferencedAssemblies()
            .Where(o => o.FullName.StartsWith(ModuleAssemblyNamePrefix)
                || o.FullName.StartsWith(FeatureAssemblyNamePrefix));

        foreach (AssemblyName name in refAssembliesNames)
        {
            var childAssembly = GetBbAssemblies().FirstOrDefault(a => a.FullName == name.FullName)
                ?? Assembly.Load(name);
            LoadReferencedAssembly(childAssembly);
        }
    }


    // This is a basic solution on how to force the core modules to be initialized first and the demo module last.
    // It's done because non-core modules are supposed to use the core modules' functionality and the demo module
    // in theory may use all other modules for demonstation purposes. 
    // Ideally we should use a smarter approach where the modules are ordered according to their tree of references.
    // So if module B refers to module A then the module A is initialized first.
    private class AssembliesLinkageOrderComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            if (x.StartsWith(CoreAssemblyNamePrefix) && !y.StartsWith(CoreAssemblyNamePrefix))
                return -1;
            if (!x.StartsWith(CoreAssemblyNamePrefix) && y.StartsWith(CoreAssemblyNamePrefix))
                return 1;
            if (x.StartsWith(DemoAssemblyNamePrefix) && !y.StartsWith(DemoAssemblyNamePrefix))
                return 1;
            if (!x.StartsWith(DemoAssemblyNamePrefix) && y.StartsWith(DemoAssemblyNamePrefix))
                return -1;

            return string.CompareOrdinal(x, y);
        }
    }
}
