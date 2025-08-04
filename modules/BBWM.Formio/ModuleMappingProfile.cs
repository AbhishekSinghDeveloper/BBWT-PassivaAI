using AutoMapper;
using BBWM.FormIO.DTO.FormVersioningDTOs;
using BBWM.FormIO.DTO.FormViewDTOs;
using BBWM.FormIO.Models;
using BBWM.FormIO.Models.FormViewModels;
using Newtonsoft.Json.Linq;

namespace BBWM.FormIO;

public class ModuleMappingProfile : Profile
{
    public ModuleMappingProfile()
    {
        CreateMap<FormRevision, FormRevisionViewDTO>();

        CreateMap<FormDefinition, FormDefinitionViewDTO>();

        CreateMap<FormRevisionGrid, FormRevisionGridDTO>()
            .ReverseMap()
            .ForMember(grid => grid.FormDefinition, member => member.Ignore())
            .ForMember(grid => grid.ParentFormRevisionGrid, member => member.Ignore());

        CreateMap<FormData, FormDataVersioningDTO>()
            .ForMember(dataDto => dataDto.JsonObject, member => member
                .MapFrom(data => string.IsNullOrEmpty(data.Json) ? null : JObject.Parse(data.Json)));
    }
}