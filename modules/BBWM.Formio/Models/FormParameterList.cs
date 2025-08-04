using AutoMapper;
using BBWM.Core.Data;
using BBWM.FormIO.DTO;

namespace BBWM.FormIO.Models
{
    public class FormParameterList : IEntity
    {
        public int Id { get; set; }

        /// <summary>
        /// Unique Parameter ID, this is going to be used on the FormComponent -> API -> Tag
        /// to know which rules to apply to the property name value "dbo.table.field"
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// In which position of the parameter list is the value we are going to use to filter. Null means no filter
        /// </summary>
        public int? Position { get; set; } = null;

        /// <summary>
        /// Name of the table this parameter ID is related to, this is a security measure to guarantee
        /// that the property name is pointing to a valid table, to avoid misusage
        /// </summary>
        public string TableName { get; set; } = null!;

        /// <summary>
        /// Name of the field for the where clause of the query, using the value according ParameterList["Position"]. Null means no filter
        /// </summary>
        public string? KeyField { get; set; } = null;

        public static void RegisterMap(IMapperConfigurationExpression expression)
        {
            expression.CreateMap<FormParameterList, FormParameterListDTO>().ReverseMap();
        }
    }
}