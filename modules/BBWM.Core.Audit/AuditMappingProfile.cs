using AutoMapper;

using Microsoft.EntityFrameworkCore;

namespace BBWM.Core.Audit;

public class AuditMappingProfile : Profile
{
    public AuditMappingProfile()
    {
        CreateMap<ChangeLog, ChangeLogDTO>()
            .ForMember(d => d.State, opt => opt.MapFrom(src => Enum.GetName(typeof(EntityState), src.State)));
    }
}
