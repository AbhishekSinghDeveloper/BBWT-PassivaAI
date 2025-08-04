using AutoMapper;

using BBWM.Core.Autofac;
using BBWM.Core.DTO;
using BBWM.DataProcessing.Classes;
using BBWM.DataProcessing.DTO;

using Microsoft.AspNetCore.SignalR;

namespace BBWM.DataProcessing.Services;

public abstract class BaseImportService<TEntityDTO, TKey> : IImportService
    where TEntityDTO : class, IDTO<TKey>, new()
    where TKey : IEquatable<TKey>
{
    private readonly IDataImportHelper _dataImportHelper;

    protected readonly IHubContext<DataImportHub> HubContext;
    protected readonly IMapper Mapper;

    protected BaseImportService(
        IDataImportHelper dataImportHelper,
        IHubContext<DataImportHub> hubContext,
        IMapper mapper)
    {
        _dataImportHelper = dataImportHelper;
        HubContext = hubContext;

        Mapper = mapper;
    }

    protected abstract Task SaveImportedEntities(IEnumerable<TEntityDTO> list, CancellationToken ct);

    protected abstract Task OnEntityImport(TEntityDTO entity, CancellationToken ct);

    [IgnoreLogging]
    public DataImportConfig CreateSettings(ImportDataModel importDataModel, MemoryStream memoryStream, string userId)
    {
        if (importDataModel is null)
            throw new ArgumentNullException(nameof(importDataModel));

        var columnDefinitions = new ColumnsDefinitionsCollection();

        importDataModel.Config ??= GetDefaultConfig(importDataModel);

        if (importDataModel.Config is null)
            throw new NullReferenceException($"{nameof(importDataModel)}.{nameof(importDataModel.Config)} cannot be null");

        foreach (var colDef in importDataModel.Config.ColumnDefinitions)
        {
            if (colDef is null)
                throw new NullReferenceException("Column definition cannot be null");

            var column = Mapper.Map<ColumnDefinition>(colDef);

            InitCustomTypeInfo(column, colDef.TypeInfo);
            columnDefinitions.AddColumn(column);
        }

        var dataImportConfig = new DataImportConfig();
        Mapper.Map(importDataModel.Config, dataImportConfig);
        dataImportConfig.FileName = importDataModel.FileName;
        dataImportConfig.FileStream = memoryStream;
        dataImportConfig.ColumnDefinitions = columnDefinitions;
        dataImportConfig.UserId = userId;

        return dataImportConfig;
    }

    [IgnoreLogging]
    public async Task<DataImportResultDTO> Import(DataImportConfig dataImportConfig, CancellationToken cancellationToken)
    {
        var list = new List<TEntityDTO>();
        var importResult = _dataImportHelper.ProcessDataImport(dataImportConfig,
            args =>
            {
                if (args.Entry.IsValid)
                {
                    var dto = CreateDTOByEntry(args.Entry);
                    if (dto is not null)
                    {
                        list.Add(dto);
                    }
                }
            });

        var res = Mapper.Map<DataImportResultDTO>(importResult);
        double percent = 0;
        int updatePercent = 0;

        if (!res.InvalidEntries.Any())
        {
            list = (await ParsedListPostprocessingAsync(list, dataImportConfig, res, cancellationToken)).ToList();

            for (int i = 0; i < list.Count; i++)
            {
                var item = list[i];

                if (cancellationToken.IsCancellationRequested)
                {
                    await HubContext.Clients.User(dataImportConfig.UserId).SendAsync("Stopped");
                    return res;
                }

                await OnEntityImport(item, cancellationToken);

                percent = (i + 1D) / list.Count * 100;

                var intPercent = (int)percent;
                if (intPercent > updatePercent)
                {
                    updatePercent = intPercent;
                    await HubContext.Clients.User(dataImportConfig.UserId).SendAsync("Update", updatePercent);
                }

                res.ImportedCount = i + 1;
            }

            await SaveImportedEntities(list, cancellationToken);
        }

        await HubContext.Clients.User(dataImportConfig.UserId).SendAsync("Result", res);

        return res;
    }

    protected virtual TEntityDTO CreateDTOByEntry(ImportEntry entry)
    {
        var dto = new TEntityDTO();
        try
        {
            foreach (var cell in entry.Cells)
            {
                var property = typeof(TEntityDTO).GetProperties()
                    .SingleOrDefault(x => x.Name == cell.ColumnDefinition.TargetFieldName);

                if (property is not null)
                {
                    var t = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

                    var safeValue = cell.Value is IConvertible ? Convert.ChangeType(cell.Value, t) : cell.Value;

                    property.SetValue(dto, safeValue, null);
                }
            }

            return dto;
        }
        catch (Exception ex)
        {
            entry.ErrorMessage = ex.ToString();
            return null;
        }
    }

    protected virtual Task<IEnumerable<TEntityDTO>> ParsedListPostprocessingAsync(
        IEnumerable<TEntityDTO> list,
        DataImportConfig config,
        DataImportResultDTO result,
        CancellationToken cancellationToken = default)
        => Task.Run(() => list);

    protected virtual DataImportConfigDTO GetDefaultConfig(ImportDataModel importDataModel) => default;

    protected virtual CustomValidationHandler GetCustomValidator(CellDataTypeInfoDTO typeInfo) => null;

    private void InitCustomTypeInfo(ColumnDefinition columnDefinition, CellDataTypeInfoDTO typeInfo)
    {
        switch (columnDefinition.Type)
        {
            case CellDataType.Number:
                columnDefinition.TypeInfo = new NumberCellDataTypeInfo(typeInfo.Min, typeInfo.Max);
                break;
            case CellDataType.Date:
            case CellDataType.DateTimeOffset:
                columnDefinition.TypeInfo = new DateTimeCellDataTypeInfo(typeInfo.DateFormats);
                break;
            case CellDataType.Custom:
                CustomValidationHandler validationHandler = GetCustomValidator(typeInfo);
                if (validationHandler is null)
                    throw new NullReferenceException($"Custom validation handler cannot be null if cell data type is {nameof(CellDataType.Custom)}");

                columnDefinition.TypeInfo = new CustomCellDataTypeInfo(validationHandler);
                break;
            default:
                break;
        }
    }
}

public abstract class BaseImportService<TEntityDTO> : BaseImportService<TEntityDTO, int>, IImportService
    where TEntityDTO : class, IDTO, new()
{
    protected BaseImportService(IDataImportHelper dataImportHelper,
        IHubContext<DataImportHub> hubContext,
        IMapper mapper) : base(dataImportHelper, hubContext, mapper)
    {
    }
}

