namespace RuntimeEditor.Classes;

public class TemplateFile
{
    public string Path { get; set; }
    public string Content { get; set; }
}

public class ProcessedTemplateFile : TemplateFile
{
    public int InsertsCount;

    public TemplateFile ToTemplateFile() =>
        new TemplateFile { Path = this.Path, Content = this.Content };
}