using BBWM.RuntimeEditor.interfaces;
using System.Text.Json;

namespace BBWM.RuntimeEditor.services;

public interface IEditionSenderLocalFolderService : IEditionSenderService
{ }

public class EditionSenderLocalFolderService : IEditionSenderLocalFolderService
{
    public async Task<bool> SendEditionUpdate(ApplyEditsRequest request, CancellationToken ct)
    {
        var baseDir = AppContext.BaseDirectory;
        File.WriteAllText(
            $"{baseDir.Substring(0, baseDir.IndexOf("project\\", StringComparison.InvariantCulture))}{request.EditJsonFilesPath}local_{Guid.NewGuid()}.json",
            JsonSerializer.Serialize(request.EditionUpdate, new JsonSerializerOptions { WriteIndented = true }));
        return await Task.FromResult(true);
    }
}