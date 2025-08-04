using AutoMapper;
using BBWM.Core.Data;
using BBWM.Core.Membership.Model;
using BBWM.FormIO.DTO;
using System.ComponentModel.DataAnnotations.Schema;

namespace BBWM.FormIO.Models
{
    public class FormDataDraft : IEntity
    {
        public int Id { get; set; }
        public string Json { get; set; } = null!;
        public DateTimeOffset CreatedOn { get; set; }

        // Foreign key and navigational properties.
        public int FormDefinitionId { get; set; }
        public FormDefinition FormDefinition { get; set; } = null!;

        public string UserId { get; set; } = null!;
        [ForeignKey("UserId")] public User CreatedBy { get; set; } = null!;

        public static void RegisterMap(IMapperConfigurationExpression expression)
        {
            expression.CreateMap<FormDataDraft, FormDataDraftDTO>().ReverseMap();
        }
    }
}