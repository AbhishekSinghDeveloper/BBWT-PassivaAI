using BBWM.Core.Data;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BBWM.Menu.Db;

[Table("FooterMenuItems")]
public class FooterMenuItem : IAuditableEntity
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string Name { get; set; }

    [Required, MaxLength(100)]
    public string RouterLink { get; set; }

    public int OrderNo { get; set; }
}
