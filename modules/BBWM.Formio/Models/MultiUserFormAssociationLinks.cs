using AutoMapper;
using BBWM.Core.Data;
using BBWM.Core.Membership.Model;
using BBWM.FormIO.DTO;

namespace BBWM.FormIO.Models
{
    /// <summary>
    /// This entity is the one that links a MUF definition with its Form Data for each Stage and allow a way to track which stage are completed or not.
    /// </summary>
    public class MultiUserFormAssociationLinks : IEntity
    {
        public int Id { get; set; }
        public bool IsFilled { get; set; }
        public DateTime Completed { get; set; }
        public string? ExternalUserEmail { get; set; }
        public string SecurityCode { get; set; } = null!;

        // Foreign key and navigational properties.
        public string? UserId { get; set; }
        public User? User { get; set; }

        public int MultiUserFormStageId { get; set; }
        public MultiUserFormStage MultiUserFormStage { get; set; } = null!;

        public int MultiUserFormAssociationsId { get; set; }
        public MultiUserFormAssociations MultiUserFormAssociations { get; set; } = null!;

        public static void RegisterMap(IMapperConfigurationExpression expression)
        {
            expression.CreateMap<MultiUserFormAssociationLinks, MultiUserFormAssociationLinksDTO>().ReverseMap();
        }
    }
}