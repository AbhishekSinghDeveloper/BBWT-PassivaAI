using AutoMapper;

using BBWM.Core.Exceptions;
using BBWM.Core.Test;
using BBWM.DbDoc.Core.Classes.ValidationRules;
using BBWM.DbDoc.DTO;
using BBWM.DbDoc.Model;

using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BBWM.DbDoc.Tests;

public class ColumnTypeServiceTest
{
    private readonly IMapper _mapper;


    public ColumnTypeServiceTest() => _mapper = AutoMapperConfig.CreateMapper();

    [Fact]
    public async Task Delete_Test()
    {
        var servicesFactory = new DbDocTestServicesFactory();
        servicesFactory.CreateColumnTypeService();
        var context = servicesFactory.InMemoryContext;
        var service = servicesFactory.ColumnTypeService;

        var columnType = await service.Create(new ColumnTypeDTO { Name = "Column Type Name" });
        var columnValidationMetadata = await service.SetValidationMetadata(columnType.Id, new ColumnValidationMetadataDTO
        {
            Rules = new ValidationRule[] { new RequiredValidationRule() }
        }, CancellationToken.None);
        var columnViewMetadata = await service.SetViewMetadata(columnType.Id, new ColumnViewMetadataDTO
        {
            GridColumnView = new GridColumnViewDTO
            {
                Mask = "__-__"
            }
        }, CancellationToken.None);

        await service.Delete(columnType.Id);

        Assert.True(context.Set<GridColumnView>().All(x => x.Id != columnViewMetadata.GridColumnView.Id));
        Assert.True(context.Set<ColumnViewMetadata>().All(x => x.Id != columnViewMetadata.Id));
        Assert.True(context.Set<ColumnValidationMetadata>().All(x => x.Id != columnValidationMetadata.Id));
        Assert.True(context.Set<ColumnType>().All(x => x.Id != columnType.Id));
    }

    [Fact]
    public async Task DeleteValidationMetadata_Test()
    {
        var servicesFactory = new DbDocTestServicesFactory();
        servicesFactory.CreateColumnTypeService();
        var context = servicesFactory.InMemoryContext;
        var service = servicesFactory.ColumnTypeService;

        await Assert.ThrowsAnyAsync<ObjectNotExistsException>(async () =>
            await service.DeleteValidationMetadata(Guid.Empty, CancellationToken.None));

        var columnType = await service.Create(new ColumnTypeDTO { Name = "Column Type Name" });
        var columnValidationMetadata = await service.SetValidationMetadata(columnType.Id, new ColumnValidationMetadataDTO
        {
            Rules = new ValidationRule[] { new RequiredValidationRule() }
        }, CancellationToken.None);
        await service.DeleteValidationMetadata(columnType.Id, CancellationToken.None);

        Assert.True(context.Set<ColumnValidationMetadata>().All(x => x.Id != columnValidationMetadata.Id));
    }

    [Fact]
    public async Task DeleteViewMetadata_Test()
    {
        var servicesFactory = new DbDocTestServicesFactory();
        servicesFactory.CreateColumnTypeService();
        var context = servicesFactory.InMemoryContext;
        var service = servicesFactory.ColumnTypeService;

        await Assert.ThrowsAnyAsync<ObjectNotExistsException>(async () =>
            await service.DeleteViewMetadata(Guid.Empty, CancellationToken.None));

        var columnType = await service.Create(new ColumnTypeDTO { Name = "Column Type Name" });
        var columnViewMetadata = await service.SetViewMetadata(columnType.Id, new ColumnViewMetadataDTO
        {
            GridColumnView = new GridColumnViewDTO
            {
                Mask = "__-__"
            }
        }, CancellationToken.None);
        await service.DeleteViewMetadata(columnType.Id, CancellationToken.None);

        Assert.True(context.Set<ColumnViewMetadata>().All(x => x.Id != columnViewMetadata.Id));
    }

    [Fact]
    public async Task SetValidationMetadata_Test()
    {
        var servicesFactory = new DbDocTestServicesFactory();
        servicesFactory.CreateColumnTypeService();
        var context = servicesFactory.InMemoryContext;
        var service = servicesFactory.ColumnTypeService;

        await Assert.ThrowsAnyAsync<ObjectNotExistsException>(async () =>
            await service.SetValidationMetadata(Guid.Empty, null, CancellationToken.None));

        var columnType = await service.Create(new ColumnTypeDTO { Name = "Column Type Name" });

        await service.SetValidationMetadata(columnType.Id,
            new ColumnValidationMetadataDTO
            {
                Rules = new ValidationRule[] { new RequiredValidationRule() }
            }, CancellationToken.None);
        columnType = _mapper.Map<ColumnType, ColumnTypeDTO>(
        await context.Set<ColumnType>()
                .Include(x => x.ValidationMetadata)
                .SingleOrDefaultAsync(x => x.Id == columnType.Id));

        Assert.NotNull(columnType.ValidationMetadata);
        Assert.NotEmpty(columnType.ValidationMetadata.Rules);
        Assert.True(columnType.ValidationMetadata.Rules.First() is RequiredValidationRule);

        await service.SetValidationMetadata(columnType.Id,
            new ColumnValidationMetadataDTO
            {
                Rules = new ValidationRule[] { new InputFormatValidationRule() }
            }, CancellationToken.None);
        columnType = _mapper.Map<ColumnType, ColumnTypeDTO>(
            await context.Set<ColumnType>()
                .Include(x => x.ValidationMetadata)
                .SingleOrDefaultAsync(x => x.Id == columnType.Id));

        Assert.True(columnType.ValidationMetadata.Rules.First() is InputFormatValidationRule);
    }

    [Fact]
    public async Task SetViewMetadata_Test()
    {
        var servicesFactory = new DbDocTestServicesFactory();
        servicesFactory.CreateColumnTypeService();
        var context = servicesFactory.InMemoryContext;
        var service = servicesFactory.ColumnTypeService;

        await Assert.ThrowsAnyAsync<ObjectNotExistsException>(async () =>
            await service.SetViewMetadata(Guid.Empty, null, CancellationToken.None));

        var columnType = await service.Create(new ColumnTypeDTO { Name = "Column Type Name" });
        var gridColumnViewFirstMask = "__-__";
        var viewMetadata = await service.SetViewMetadata(columnType.Id,
            new ColumnViewMetadataDTO
            {
                GridColumnView = new GridColumnViewDTO
                {
                    Mask = gridColumnViewFirstMask
                }
            }, CancellationToken.None);
        columnType = _mapper.Map<ColumnType, ColumnTypeDTO>(
            await context.Set<ColumnType>()
                .Include(x => x.ViewMetadata)
                .ThenInclude(x => x.GridColumnView)
                .SingleOrDefaultAsync(x => x.Id == columnType.Id));

        Assert.NotNull(columnType.ViewMetadata);
        Assert.NotNull(columnType.ViewMetadata.GridColumnView);
        Assert.Equal(gridColumnViewFirstMask, columnType.ViewMetadata.GridColumnView.Mask);

        var gridColumnViewSecondMask = "__-__-__";
        viewMetadata.GridColumnView.Mask = gridColumnViewSecondMask;
        await service.SetViewMetadata(columnType.Id, viewMetadata, CancellationToken.None);
        columnType = _mapper.Map<ColumnType, ColumnTypeDTO>(
            await context.Set<ColumnType>()
                .Include(x => x.ViewMetadata)
                .ThenInclude(x => x.GridColumnView)
                .SingleOrDefaultAsync(x => x.Id == columnType.Id));

        Assert.Equal(gridColumnViewSecondMask, columnType.ViewMetadata.GridColumnView.Mask);

        await service.SetViewMetadata(columnType.Id,
            new ColumnViewMetadataDTO(), CancellationToken.None);
        columnType = _mapper.Map<ColumnType, ColumnTypeDTO>(
            await context.Set<ColumnType>()
                .Include(x => x.ViewMetadata)
                .ThenInclude(x => x.GridColumnView)
                .SingleOrDefaultAsync(x => x.Id == columnType.Id));

        Assert.Null(columnType.ViewMetadata.GridColumnView);
    }
}
