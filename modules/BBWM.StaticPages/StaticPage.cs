using AutoMapper;

using BBWM.Core.Data;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web;

namespace BBWM.StaticPages;

[Table("StaticPages")]
public class StaticPage : IAuditableEntity
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Alias { get; set; }

    [Required, MaxLength(100)]
    public string Heading { get; set; }

    [MaxLength]
    public string Contents { get; set; }

    public string ContentPreview { get; set; }

    public DateTime LastUpdated { get; set; }

    public static void RegisterMap(IMapperConfigurationExpression c)
    {
        c.CreateMap<StaticPage, StaticPageDTO>()
            .AfterMap((src, dest) => dest.Contents = HttpUtility.HtmlDecode(src.Contents))
            .ReverseMap()
            .AfterMap((src, dest) => dest.Contents = HttpUtility.HtmlEncode(src.Contents))
            .ForMember(dest => dest.Id, opt => opt.Condition(src => src.Id != 0));
    }
}
