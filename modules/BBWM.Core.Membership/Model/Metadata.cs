using AutoMapper;
using BBWM.Metadata;

namespace BBWM.Core.Membership.Model;

public class Metadata : MetadataModel<User>
{
    public static void RegisterMap(IMapperConfigurationExpression c)
    {
        c.CreateMap<Metadata, MetadataDTO>()
            .ForMember(x => x.LockedByUserFullName, opt => opt.Ignore())
            .ReverseMap();
    }
}
