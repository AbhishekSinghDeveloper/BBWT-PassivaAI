using AutoMapper;
using BBWM.Core.Data;
using BBWM.FormIO.DTO;
using BBWM.FormIO.Enums;

namespace BBWM.FormIO.Models
{
    public class MultiUserFormStagePermissions : IEntity
    {
        public int Id { get; set; }
        public string TabKey { get; set; } = null!;
        public MultiUserFormStagePermissionAction Action { get; set; }

        public int MultiUserFormStageId { get; set; }
        public MultiUserFormStage MultiUserFormStage { get; set; } = null!;

        public static void RegisterMap(IMapperConfigurationExpression expression)
        {
            expression.CreateMap<MultiUserFormStagePermissions, MultiUserFormStagePermissionsDTO>()
                .ForMember(multiuserFormStagePermissionsDto => multiuserFormStagePermissionsDto.Action, member => member
                    .MapFrom(multiuserFormStagePermissions => (byte)multiuserFormStagePermissions.Action))
                .ReverseMap()
                .ForMember(multiuserFormStagePermissions => multiuserFormStagePermissions.Action, member => member
                    .MapFrom(multiuserFormStagePermissionsDto => (MultiUserFormStagePermissionAction)multiuserFormStagePermissionsDto.Action));
        }
    }
}