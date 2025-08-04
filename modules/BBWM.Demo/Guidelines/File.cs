using AutoMapper;

using BBWM.Core.Data;

using System.ComponentModel.DataAnnotations.Schema;

namespace BBWM.Demo.Guidelines;

/// <summary>
/// Demo employee entity
/// </summary>
[Table("Files")]
public class File : IAuditableEntity
{
    public int Id { get; set; }

    /// <summary>
    /// Label
    /// </summary>
    public string Label { get; set; }

    /// <summary>
    /// Data
    /// </summary>
    public string Data { get; set; }

    /// <summary>
    /// Type
    /// </summary>
    public FileType Type { get; set; }

    /// <summary>
    /// Expanded
    /// </summary>
    public bool Expanded { get; set; }

    /// <summary>
    /// ParentId
    /// </summary>
    public int? ParentId { get; set; }

    /// <summary>
    /// Parent
    /// </summary>
    [ForeignKey("ParentId")]
    public virtual File Parent { get; set; }

    /// <summary>
    /// Children
    /// </summary>
    public virtual ICollection<File> Children { get; set; }
    public static void RegisterMap(IMapperConfigurationExpression c)
    {
        c.CreateMap<File, FileDTO>()
             .ForMember(p => p.Type, m => m.MapFrom(p => (int)p.Type))
             .ForMember(p => p.ExpandedIcon, m => m.MapFrom(p => p.Type == FileType.Folder ? "ui-icon-folder-open" : ""))
             .ForMember(p => p.CollapsedIcon, m => m.MapFrom(p => p.Type == FileType.Folder ? "ui-icon-folder" : ""))
            .ForMember(p => p.Icon, m => m.MapFrom(p => p.Type == FileType.File ? "ui-icon-insert-drive-file" : ""))
             .ForMember(p => p.Parent, m => m.MapFrom(p => p.Parent))
             .ReverseMap();
    }
}
