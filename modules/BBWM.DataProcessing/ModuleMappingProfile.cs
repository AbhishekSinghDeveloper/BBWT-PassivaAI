using AutoMapper;

using BBWM.DataProcessing.Classes;
using BBWM.DataProcessing.DTO;

namespace BBWM.DataProcessing;

public class ModuleMappingProfile : Profile
{
    public ModuleMappingProfile()
    {
        CreateMap<ColumnDefinitionDTO, ColumnDefinition>()
                .ForMember(d => d.TypeInfo, r => r.Ignore())
            .ReverseMap()
                .ForMember(dto => dto.TypeInfo, conf => conf.Ignore());

        CreateMap<ImportEntryCell, ImportEntryCellDTO>();

        CreateMap<ImportEntry, ImportEntryDTO>();

        CreateMap<DataImportResult, DataImportResultDTO>();

        CreateMap<DataImportConfig, DataImportConfigDTO>()
                .ForMember(
                    dto => dto.ColumnDefinitions,
                    conf => conf.MapFrom(
                        (m, dto, obj, c) => c.Mapper.Map<IEnumerable<ColumnDefinitionDTO>>(m.ColumnDefinitions)))
            .ReverseMap()
                .ForMember(d => d.ColumnDefinitions, r => r.Ignore());
    }
}
