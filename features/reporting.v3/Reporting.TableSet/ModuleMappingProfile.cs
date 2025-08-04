using AutoMapper;
using BBF.Reporting.TableSet.DTO;

namespace BBF.Reporting.TableSet;

public class ModuleMappingProfile : Profile
{
    public ModuleMappingProfile()
    {
        CreateMap<DbModel.TableSet, TableSetDTO>();
    }
}