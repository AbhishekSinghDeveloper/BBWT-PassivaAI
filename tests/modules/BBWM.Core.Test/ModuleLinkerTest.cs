using BBWM.Core.ModuleLinker;
using Xunit;

namespace BBWT.Tests.modules.BBWM.Core.Test
{
    public class ModuleLinkerTest
    {
        [Fact]
        public void AddInvokeExceptionTest()
        {
            Assert.Empty(ModuleLinker.InvokeExceptions);

            ModuleLinker.AddInvokeException(new Exception());

            Assert.NotEmpty(ModuleLinker.InvokeExceptions);
        }

        [Fact]
        public void AddCommonExceptionTest()
        {
            Assert.Empty(ModuleLinker.CommonExceptions);

            ModuleLinker.AddCommonException(new Exception());

            Assert.NotEmpty(ModuleLinker.CommonExceptions);
        }

        [Fact]
        public void DoesAssemblyBelongsToTest()
        {
            var projectAssebmly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.FullName.Contains(ModuleLinker.ProjectAssemblyNamePrefix));
            var moduleAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.FullName.Contains(ModuleLinker.ModuleAssemblyNamePrefix));
            var coreAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.FullName.Contains(ModuleLinker.CoreAssemblyNamePrefix));
            var demoAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.FullName.Contains(ModuleLinker.DemoAssemblyNamePrefix));

            if (projectAssebmly is not null)
            {
                Assert.True(ModuleLinker.IsProjectAssembly(projectAssebmly));
                Assert.False(ModuleLinker.IsModuleAssembly(projectAssebmly));
            }

            if (moduleAssembly is not null)
            {
                Assert.False(ModuleLinker.IsProjectAssembly(moduleAssembly));
                Assert.True(ModuleLinker.IsModuleAssembly(moduleAssembly));
            }

            if (coreAssembly is not null)
            {
                Assert.False(ModuleLinker.IsProjectAssembly(coreAssembly));
                Assert.True(ModuleLinker.IsModuleAssembly(coreAssembly));
                Assert.False(ModuleLinker.IsDemoModuleAssembly(coreAssembly));
                Assert.True(ModuleLinker.IsCoreModuleAssembly(coreAssembly));
            }

            if (demoAssembly is not null)
            {
                Assert.False(ModuleLinker.IsProjectAssembly(demoAssembly));
                Assert.True(ModuleLinker.IsModuleAssembly(demoAssembly));
                Assert.True(ModuleLinker.IsDemoModuleAssembly(demoAssembly));
                Assert.False(ModuleLinker.IsCoreModuleAssembly(demoAssembly));
            }
        }

        [Fact]
        public void GetInstancesTest()
        {
            var allTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes());

            var authenticationModuleLinkages = allTypes
                .Where(p => p.IsClass && !p.ContainsGenericParameters && typeof(IAuthenticationModuleLinkage).IsAssignableFrom(p))
                .ToList();
            Assert.Equal(authenticationModuleLinkages.Count, ModuleLinker.GetInstances<IAuthenticationModuleLinkage>().Count);

            var configureModuleLinkages = allTypes
                .Where(p => p.IsClass && !p.ContainsGenericParameters && typeof(IConfigureModuleLinkage).IsAssignableFrom(p))
                .ToList();
            Assert.Equal(configureModuleLinkages.Count, ModuleLinker.GetInstances<IConfigureModuleLinkage>().Count);

            var dataContextModuleLinkages = allTypes
                .Where(p => p.IsClass && !p.ContainsGenericParameters && typeof(IDataContextModuleLinkage).IsAssignableFrom(p))
                .ToList();
            Assert.Equal(dataContextModuleLinkages.Count, ModuleLinker.GetInstances<IDataContextModuleLinkage>().Count);

            var dbCreateModuleLinkages = allTypes
                .Where(p => p.IsClass && !p.ContainsGenericParameters && typeof(IDbCreateModuleLinkage).IsAssignableFrom(p))
                .ToList();
            Assert.Equal(dbCreateModuleLinkages.Count, ModuleLinker.GetInstances<IDbCreateModuleLinkage>().Count);

            var dependenciesModuleLinkages = allTypes
                .Where(p => p.IsClass && !p.ContainsGenericParameters && typeof(IDependenciesModuleLinkage).IsAssignableFrom(p))
                .ToList();
            Assert.Equal(dependenciesModuleLinkages.Count, ModuleLinker.GetInstances<IDependenciesModuleLinkage>().Count);

            var initialModuleLinkages = allTypes
                .Where(p => p.IsClass && !p.ContainsGenericParameters && typeof(IInitialDataModuleLinkage).IsAssignableFrom(p))
                .ToList();
            Assert.Equal(initialModuleLinkages.Count, ModuleLinker.GetInstances<IInitialDataModuleLinkage>().Count);

            var servicesModuleLinkages = allTypes
                .Where(p => p.IsClass && !p.ContainsGenericParameters && typeof(IServicesModuleLinkage).IsAssignableFrom(p))
                .ToList();
            Assert.Equal(servicesModuleLinkages.Count, ModuleLinker.GetInstances<IServicesModuleLinkage>().Count);

            var signalRModuleLinkages = allTypes
                .Where(p => p.IsClass && !p.ContainsGenericParameters && typeof(ISignalRModuleLinkage).IsAssignableFrom(p))
                .ToList();
            Assert.Equal(signalRModuleLinkages.Count, ModuleLinker.GetInstances<ISignalRModuleLinkage>().Count);
        }
    }
}
