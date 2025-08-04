using AutoMapper;
using BBF.Reporting.QueryBuilder.DbModel;
using BBF.Reporting.QueryBuilder.DTO;

namespace BBF.Reporting.QueryBuilder;

public class ModuleMappingProfile : Profile
{
    public ModuleMappingProfile()
    {
        CreateMap<SqlQuery, SqlQueryBuildDTO>()
            .ReverseMap()
            .ForMember(query => query.QuerySource, member => member.Ignore())
            .ForMember(query => query.TableSet, member => member.Ignore());
    }
}