using BBWM.Core.Membership.Model;
using BBWM.RuntimeEditor.interfaces;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using System.Text.Json;

namespace BBWM.RuntimeEditor.services;

public class EditionStorageService : IEditionStorageService
{
    private readonly RuntimeEditorSettings runtimeEditorConfig;

    private readonly IEditionSendManagerService editionSendManagerService;
    private readonly IEditionDeltaCalcService editionDeltaCalcService;
    private readonly UserManager<User> userManager;
    private IWebHostEnvironment HostingEnvironment { get; }
    private string DictionaryJsonPath => $"{HostingEnvironment.ContentRootPath}{runtimeEditorConfig.DictionaryFilePath}";
    private string EditionJsonPath => $"{HostingEnvironment.ContentRootPath}{runtimeEditorConfig.EditionFilePath}";

    public EditionStorageService(
        IOptionsSnapshot<RuntimeEditorSettings> runtimeEditorConfig,
        IEditionSendManagerService editionSendManagerService,
        IEditionDeltaCalcService editionDeltaCalcService,
        IWebHostEnvironment hostingEnvironment,
        UserManager<User> userManager)
    {
        this.runtimeEditorConfig = runtimeEditorConfig.Value;
        this.editionDeltaCalcService = editionDeltaCalcService;
        this.editionSendManagerService = editionSendManagerService;
        this.userManager = userManager;
        HostingEnvironment = hostingEnvironment;
    }

    public async Task<RteDictionary> GetDictionary(CancellationToken ct)
        => await Task.FromResult(GetLocalDictionary());

    public async Task<RteEdition> GetEdition(CancellationToken ct)
        => await Task.FromResult(GetLocalEdition());

    public async Task<RteEditionUpdate> SaveEdition(RteEdition edition, string editorUserId, CancellationToken ct)
    {
        var editionUpdate = editionDeltaCalcService.GetEditionUpdate(
            await GetEdition(ct),
            edition,
            await GetDictionary(ct),
            await userManager.FindByIdAsync(editorUserId));

        if (editionUpdate.Edits.Any())
        {
            try
            {
                var sendProviderType = HostingEnvironment.IsDevelopment() ?
                    EditionSendProviderType.LocalFolder :
                    EditionSendProviderType.GitApi;

                await editionSendManagerService.SendEditionUpdateToRepository(editionUpdate, sendProviderType, ct);
            }
            catch (Exception ex)
            {
                throw new Exception("Error sending runtime editor changes to the repository. Details: " + ex.Message);
            }

            try
            {
                SaveEditionLocally(edition);
            }
            catch (Exception ex)
            {
                throw new Exception("Runtime editor changes has been sent to the repository but there has been an error saving your edition in a JSON file stored on the server. Details: " + ex.Message);
            }
        }

        return editionUpdate;
    }

    private RteDictionary GetLocalDictionary()
        => new()
        {
            Items = JsonSerializer.Deserialize<RteDictionaryItem[]>(File.ReadAllText(DictionaryJsonPath))
        };

    private RteEdition GetLocalEdition()
    {
        if (!File.Exists(EditionJsonPath))
            return new RteEdition();

        var content = File.ReadAllText(EditionJsonPath);
        return JsonSerializer.Deserialize<RteEdition>(content);
    }

    private void SaveEditionLocally(RteEdition edition)
    {
        var content = JsonSerializer.Serialize(edition, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(EditionJsonPath, content);
    }
}