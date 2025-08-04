using BBWM.DbDoc.Core;

using NPOI.SS.Formula.Functions;

using Xunit;

namespace BBWM.DbDoc.Tests;

public class DbDocStaticDataServiceTest
{
    // TODO: Rework to DatabaseSchemaManager
    //[Fact]
    //public async Task Test()
    //{
    //    var servicesFactory = new DbDocTestServicesFactory();
    //    await servicesFactory.CreateDbDocServices();
    //    var context = servicesFactory.SqlLiteContext;
    //    var dbDocStaticDataService = servicesFactory.DbDocStaticDataService;

    //    var entities = context.Model.GetEntityTypesWithPrimaryKey().ToList();

    //    Assert.Equal(entities.Count, dbDocStaticDataService.TablesStaticData.Count);

    //    var properties = entities.SelectMany(x => x.GetProperties()).ToList();

    //    Assert.Equal(properties.Count, dbDocStaticDataService.ColumnsStaticData.Count);

    //    await servicesFactory.DeleteServices();
    //}
}
