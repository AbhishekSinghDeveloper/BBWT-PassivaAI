using AutoMapper;

using BBWM.Menu.DTO;

namespace BBWM.Menu.Db;

public class ModuleMappingProfile : Profile
{
    public ModuleMappingProfile()
    {
        CreateMap<MenuItem, MenuDTO>().ReverseMap();
        CreateMap<FooterMenuItem, FooterMenuItemDTO>().ReverseMap();
    }
}
