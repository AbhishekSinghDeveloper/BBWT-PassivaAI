using AutoMapper;

using BBWM.Core;
using BBWM.Core.Data;
using BBWM.Core.Membership.Model;
using BBWM.Core.ModuleLinker;
using BBWM.Core.Services;
using BBWM.Core.Test;
using BBWM.Core.Test.Utils;
using BBWM.DbDoc.Core;
using BBWM.DbDoc.DbSchema.SchemaReaders.ContextModels;
using BBWM.DbDoc.Interfaces;
using BBWM.DbDoc.Services;
using BBWM.GitLab;

using BBWT.Data;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Moq;

using System.Security.Claims;

namespace BBWM.DbDoc.Tests;

public class DbDocTestServicesFactory
{
    public IColumnTypeService ColumnTypeService { get; private set; }
    public IDbDocDataService DbDocDataService { get; private set; }

    public IDbDocService DbDocService { get; private set; }

    public IDbDocSyncService DbDocSyncService { get; private set; }

    // TODO: Rework to DatabaseSchemaManager
    public IDbDocStaticDataService DbDocStaticDataService { get; private set; }

    public IDbDocAnonymizeService DbDocAnonymizeService { get; private set; }

    public IDataContext InMemoryContext { get; private set; }

    public IDataContext SqlLiteContext { get; private set; }


    public async Task CreateDbDocServices()
    {
        SqlLiteContext = SutDataHelper.CreateEmptyContext<DataContext>(DbType.SqlLite);
        await SqlLiteContext.Database.EnsureCreatedAsync();

        var mockDbContextProvider = new Mock<IDbContextProvider>();
        mockDbContextProvider.Setup(x => x.GetDbContexts(It.IsAny<IServiceProvider>())).Returns(new IDbContext[] { SqlLiteContext });
        var dbDocContextProvider = mockDbContextProvider.Object;

        var serviceProvider = new Mock<IServiceProvider>().Object;

        // TODO: Rework to DatabaseSchemaManager
        DbDocStaticDataService = new DbDocStaticDataService(dbDocContextProvider, serviceProvider, new DbContextModelsScanner());

        var user = new User();
        await SqlLiteContext.Set<User>().AddAsync(user);
        await SqlLiteContext.SaveChangesAsync();
        var accessor = ServicesFactory.GetHttpContextAccessor(new List<Claim> { new Claim(System.Security.Claims.ClaimTypes.NameIdentifier, user.Id) });

        var mockWebHostEnvironment = new Mock<IWebHostEnvironment>();
        mockWebHostEnvironment.Setup(x => x.ContentRootPath).Returns(Directory.GetCurrentDirectory());

        var mockGitLabService = new Mock<IGitLabService>();
        mockGitLabService
            .Setup(x => x.Push(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(true));

        var options = new Mock<IOptions<DbDocSettings>>();
        options.Setup(p => p.Value).Returns(new DbDocSettings { FilePath = DbDocServiceTest.TmpJsonFilePath, ShowTableDump = true, ReadOnlyDump = false });

        IServiceCollection services = new ServiceCollection();
        services.AddSingleton(sp => new TableStaticDataResolver(DbDocStaticDataService));
        services.AddSingleton(sp => new ColumnStaticDataResolver(DbDocStaticDataService));
        services.AddAutoMapper(cfg =>
        {
            var bbAssemblies = ModuleLinker.GetBbAssemblies();
            cfg.AddMaps(bbAssemblies);
            ProfileBase.CollectAndRegisterMappings(cfg);
            ProfileBase.AutomapEntities(cfg, bbAssemblies);
        });
        serviceProvider = services.BuildServiceProvider();

        var mapper = serviceProvider.GetService<IMapper>();

        var gitLabMock = new Mock<IDbDocGitLabService>();
        gitLabMock.Setup(x => x.SendCurrentDbDocStateToGit(It.IsAny<CancellationToken>())).Verifiable();

        ColumnTypeService = new ColumnTypeService(SqlLiteContext, mapper, new DataService(SqlLiteContext, mapper), gitLabMock.Object);

        DbDocService = new DbDocService(
            SqlLiteContext,
            DbDocStaticDataService,
            gitLabMock.Object,
            mapper);

        DbDocDataService = new DbDocDataService(
            dbDocContextProvider,
            SqlLiteContext,
            serviceProvider,
            mapper,
            options.Object,
            DbDocService);

        DbDocAnonymizeService = new DbDocAnonymizeService(DbDocService);

        DbDocSyncService = new DbDocSyncService(
            mockWebHostEnvironment.Object,
            SqlLiteContext,
            DbDocService,
            DbDocStaticDataService,
            ColumnTypeService,
            gitLabMock.Object,
            options.Object,
            new Mock<ILogger<DbDocService>>().Object);
    }

    public void CreateColumnTypeService()
    {
        InMemoryContext = SutDataHelper.CreateEmptyContext<DataContext>();

        var gitLabMock = new Mock<IDbDocGitLabService>();
        gitLabMock.Setup(x => x.SendCurrentDbDocStateToGit(It.IsAny<CancellationToken>())).Verifiable();

        var mapper = AutoMapperConfig.CreateMapper();
        ColumnTypeService = new ColumnTypeService(InMemoryContext, mapper, new DataService(InMemoryContext, mapper), gitLabMock.Object);
    }

    public async Task DeleteServices()
    {
        if (SqlLiteContext != null)
        {
            await SqlLiteContext.Database.EnsureDeletedAsync();
            SqlLiteContext.Dispose();
            SqlLiteContext = null;
        }

        DbDocService = null;
        DbDocStaticDataService = null;
        InMemoryContext?.Dispose();
        InMemoryContext = null;
        ColumnTypeService = null;
    }
}
