using RuntimeEditor.Classes;

namespace RuntimeEditor.Services;

public interface IEmbedService
{
    ProcessedTemplate Embed(string content);
}