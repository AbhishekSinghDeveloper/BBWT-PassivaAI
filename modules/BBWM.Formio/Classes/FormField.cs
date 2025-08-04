using Newtonsoft.Json.Linq;

namespace BBWM.FormIO.Classes;

public class FormField
{
    public string Path { get; set; } = null!;
    public string Key { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string Label { get; set; } = null!;
    public JToken Token { get; set; } = null!;

    public FormField? Parent { get; set; }
    public string? ChildrenPath { get; set; }
    public ICollection<FormField> Children { get; set; } = new List<FormField>();

    public string PathPrefix => string.Concat(Parent?.Path, Parent?.ChildrenPath);
    public string RelativePath => string.IsNullOrEmpty(PathPrefix) || !Path.StartsWith(PathPrefix) ? Path : Path[PathPrefix.Length..];
    public ICollection<FormField> Descendents => Children.SelectMany(child => child.Descendents.Prepend(child)).ToList();
    public ICollection<FormField> Fields => Descendents.Prepend(this).ToList();
}