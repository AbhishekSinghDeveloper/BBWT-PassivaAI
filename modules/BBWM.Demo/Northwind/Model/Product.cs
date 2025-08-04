using BBWM.Core.Data;
using BBWM.Core.Data.Attributes;

using System.ComponentModel.DataAnnotations.Schema;

namespace BBWM.Demo.Northwind.Model;

/// <summary>
/// Product
/// </summary>
[Table("Products")]
[AllowDeleteAll]
public class Product : IAuditableEntity
{
    public int Id { get; set; }

    /// <summary>
    /// Title
    /// </summary>
    public string Title { get; set; }
}
