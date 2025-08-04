using Newtonsoft.Json;
using RuntimeEditor.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RuntimeEditor.Services;

public static class GitLinksProcessor
{
    static public void EmbedFolderLinks()
    {
        Console.WriteLine($">> Embedding link for GIT folder: {TaskSettings.TaskSubFolders}");

        var templateFiles = TemplatesProcessor.ScanFolderTemplates(TaskSettings.TaskSubFolders);
        var embedLinksService = GetEmbedLinksService(templateFiles);

        // * Parses templates & embeds IDs
        var processedFiles = TemplatesProcessor.ProcessTemplateFiles(templateFiles, embedLinksService);

        var filesToSave = processedFiles.FindAll(o => o.InsertsCount > 0).ConvertAll(o => o.ToTemplateFile());
        TemplatesProcessor.SaveSourceFiles(filesToSave);

        GitDictionaryProcessor.UpdateDictionary(processedFiles.ConvertAll(o => o.ToTemplateFile()));

        UpdateRteIdCounter(embedLinksService.idSet);
    }

    public static void CleanupFolderLinks(string removeAttr)
    {
        Console.WriteLine($">> Cleaning up links for GIT folder: {TaskSettings.TaskSubFolders}");

        var cleanupLinksService = new CleanupLinksService(removeAttr);

        var templateFiles = TemplatesProcessor.ScanFolderTemplates(TaskSettings.TaskSubFolders);

        // * Parses templates & clean up links
        var processedFiles = TemplatesProcessor.ProcessTemplateFiles(templateFiles, cleanupLinksService);

        var filesToSave = processedFiles.FindAll(o => o.InsertsCount > 0).ConvertAll(o => o.ToTemplateFile());
        TemplatesProcessor.SaveSourceFiles(filesToSave);

        GitDictionaryProcessor.UpdateDictionary(processedFiles.ConvertAll(o => o.ToTemplateFile()));
    }

    public static EmbedLinksService GetEmbedLinksService(List<TemplateFile> templateFiles)
    {
        var nodeLocations = ScanRteNodesLocations(templateFiles);
        return new EmbedLinksService(nodeLocations, GetRteIdCounter(), GetLinksEmbedSettings());
    }

    private static List<RteNodeLocation> ScanRteNodesLocations(List<TemplateFile> templateFiles) =>
        templateFiles.Aggregate(new List<RteNodeLocation>(), (l, file) =>
        {
            var sourceLocations = HtmlProcessor.GetDocumentNodes(file.Content, Constants.RteAttrName)
                .Select(o => new RteNodeLocation
                {
                    RteId = o.AttrValueTrim(Constants.RteAttrName),
                    SourceFilePath = file.Path,
                    NodePath = o.XPath
                });

            l.AddRange(sourceLocations);
            return l;
        });

    private static LinksEmbedSettings GetLinksEmbedSettings()
    {
        var linkesEmbedSettingsContent = File.ReadAllText(TaskSettings.LocalRoot + Constants.LinksEmbedSettingsFilePath, Encoding.UTF8);
        return JsonConvert.DeserializeObject<LinksEmbedSettings>(linkesEmbedSettingsContent);
    }

    private static string GetRteIdCounter()
    {
        try
        {
            var content = File.ReadAllText(TaskSettings.LocalRoot + Constants.LinksCounterFilePath, Encoding.UTF8);
            var maxIdSettings = JsonConvert.DeserializeObject<MaxIdSettings>(content);

            if (maxIdSettings == null)
            {
                throw new Exception("Max ID settings object is NULL");
            }

            return maxIdSettings.MaxId;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading max ID value from file '{Constants.LinksCounterFilePath}'. Details: {ex.Message}");
            return null;
        }
    }

    public static void UpdateRteIdCounter(RteIdSet idSetManager)
    {
        try
        {
            var maxIdSettings = new MaxIdSettings { MaxId = idSetManager.GetNewMaxId() };
            var content = JsonConvert.SerializeObject(maxIdSettings);

            File.WriteAllText(TaskSettings.LocalRoot + Constants.LinksCounterFilePath, content);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving max ID value from file '{Constants.LinksCounterFilePath}'. Details: {ex.Message}");
        }
    }
}