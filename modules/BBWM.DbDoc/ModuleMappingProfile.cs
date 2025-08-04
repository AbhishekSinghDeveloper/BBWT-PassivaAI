using AutoMapper;
using BBWM.Core.Utils;
using BBWM.DbDoc.Core.Classes.ValidationRules;
using BBWM.DbDoc.DbSchemas.Interfaces;
using BBWM.DbDoc.DbSchemas.SchemaModels;
using BBWM.DbDoc.DTO;
using BBWM.DbDoc.Model;
using System.Text.Json;

namespace BBWM.DbDoc;

public class ModuleMappingProfile : Profile
{
    public ModuleMappingProfile()
    {
        CreateMap<Folder, FolderDTO>()
            .ForMember(x => x.Owners, y => y.MapFrom(z =>
                JsonSerializer.Deserialize<List<string>>(
                    z.Owners, JsonSerializerOptionsProvider.OptionsWithoutCustomConverters)))
            .ForMember(x => x.Protected, y => y.MapFrom<ProtectedFolderResolver>())
            .ReverseMap()
            .ForMember(x => x.DatabaseSource, y => y.Ignore())
            .ForMember(x => x.DatabaseSourceId, y => y.Ignore())
            .ForMember(x => x.Tables, y => y.Ignore())
            .ForMember(x => x.Owners, y => y.MapFrom(z =>
                JsonSerializer.Serialize(
                    z.Owners, JsonSerializerOptionsProvider.OptionsWithoutCustomConverters)));

        CreateMap<DatabaseSource, DatabaseSourceDetailsDTO>()
            .ForMember(x => x.DatabaseName, y => y.MapFrom<DatabaseSourceSchemaResolver>());

        CreateMap<TableMetadata, TableMetadataDTO>()
            .ForMember(x => x.StaticData, y => y.MapFrom<TableSchemaResolver>())
            .ReverseMap()
            .ForMember(x => x.Columns, y => y.Ignore());
        CreateMap<ColumnMetadata, ColumnMetadataDTO>()
            .ForMember(x => x.StaticData, y => y.MapFrom<ColumnSchemaResolver>())
            .ReverseMap()
            .ForMember(x => x.ColumnType, y => y.Ignore())
            .ForMember(x => x.ValidationMetadata, y => y.Ignore())
            .ForMember(x => x.ViewMetadata, y => y.Ignore());
        CreateMap<ColumnValidationMetadata, ColumnValidationMetadataDTO>()
            .ForMember(x => x.Rules, y => y.MapFrom(z =>
                JsonSerializer.Deserialize<ValidationRule[]>(
                    z.Rules, JsonSerializerOptionsProvider.OptionsWithoutCustomConverters)))
            .ReverseMap()
            .ForMember(x => x.Rules, y => y.MapFrom(z => JsonSerializer.Serialize(
                z.Rules, JsonSerializerOptionsProvider.OptionsWithoutCustomConverters)));
    }
}

public class ProtectedFolderResolver : IValueResolver<Folder, FolderDTO, bool>
{
    private readonly IDbSchemaManager _dbSchemaManager;

    public ProtectedFolderResolver()
    { }

    public ProtectedFolderResolver(IDbSchemaManager dbSchemaManager) => _dbSchemaManager = dbSchemaManager;

    // For now only the main DB is marked as protected constantly
    public bool Resolve(Folder source, FolderDTO destination, bool destMember, ResolutionContext context)
        => source.DatabaseSource?.ContextId == _dbSchemaManager.MainDbContextId;
}


public class DatabaseSourceSchemaResolver : IValueResolver<DatabaseSource, DatabaseSourceDetailsDTO, string>
{
    private readonly IDbSchemaManager _dbSchemaManager;

    public DatabaseSourceSchemaResolver()
    { }

    public DatabaseSourceSchemaResolver(IDbSchemaManager dbSchemaManager) => _dbSchemaManager = dbSchemaManager;

    public string Resolve(
        DatabaseSource source,
        DatabaseSourceDetailsDTO destination,
        string destMember,
        ResolutionContext context) =>
        _dbSchemaManager?.GetDbSchema(source.Id)?.DatabaseName;
}

public class TableSchemaResolver : IValueResolver<TableMetadata, TableMetadataDTO, DbSchemaTable>
{
    private readonly IDbSchemaManager _dbSchemaManager;

    public TableSchemaResolver()
    { }

    public TableSchemaResolver(IDbSchemaManager dbSchemaManager) => _dbSchemaManager = dbSchemaManager;

    public DbSchemaTable Resolve(
        TableMetadata source,
        TableMetadataDTO destination,
        DbSchemaTable destMember,
        ResolutionContext context) =>
        _dbSchemaManager?.GetTable(source.TableId);
}

public class ColumnSchemaResolver : IValueResolver<ColumnMetadata, ColumnMetadataDTO, DbSchemaColumn>
{
    private readonly IDbSchemaManager _dbSchemaManager;

    public ColumnSchemaResolver()
    { }

    public ColumnSchemaResolver(IDbSchemaManager dbSchemaManager) => _dbSchemaManager = dbSchemaManager;

    public DbSchemaColumn Resolve(
        ColumnMetadata source,
        ColumnMetadataDTO destination,
        DbSchemaColumn destMember,
        ResolutionContext context) =>
        _dbSchemaManager?.GetColumn(source.ColumnId);
}