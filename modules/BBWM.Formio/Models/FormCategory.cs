using AutoMapper;
using BBWM.Core.Data;
using BBWM.FormIO.DTO;

namespace BBWM.FormIO.Models
{
    public class FormCategory : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public static void RegisterMap(IMapperConfigurationExpression expression)
        {
            expression.CreateMap<FormCategory, FormCategoryDTO>().ReverseMap();
        }
    }
}