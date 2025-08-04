using Newtonsoft.Json;
using RuntimeEditor.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RuntimeEditor.Services;

public static class GitEditionsProcessor
{
    public static void EmbedFolderEditions()
    {
        Console.WriteLine($">> Embedding edition(s) for GIT folder: {TaskSettings.TaskSubFolders}");
        var editionsFolder = TaskSettings.LocalRoot + Constants.EditJsonFilesPath;

        //
        if (!Directory.Exists(editionsFolder))
        {
            Console.WriteLine($"{editionsFolder} folder not found. Job skipped!");
            return;
        }

        var editionFiles = Directory.EnumerateFiles(editionsFolder, "*.json");

        var editions = GetEditionsToProcess(editionFiles);

        #region Process Editions
        if (editions.Count > 0)
        {
            ProcessEditions(editions);
        }
        else
        {
            Console.WriteLine($"No edition JSON-files found in folder {editionsFolder}. Job skipped!");
        }
        #endregion

        CleanupEditionsFolder(editionFiles);
    }

    private static List<RteEditionUpdate> GetEditionsToProcess(IEnumerable<string> editionFiles)
    {
        var orderedEditionFiles = editionFiles
            .Select(o => new Tuple<string, DateTime>(o, File.GetCreationTime(o)))
            .OrderBy(o => o.Item2);

        var editionUpdates = new List<RteEditionUpdate>();

        // * Read edits from JSONs
        foreach (var fileInfo in orderedEditionFiles)
        {
            Console.WriteLine($"    {fileInfo.Item2}: {fileInfo.Item1}");

            var content = File.ReadAllText(fileInfo.Item1, Encoding.UTF8);
            Console.WriteLine($"         {content}");

            editionUpdates.Add(JsonConvert.DeserializeObject<RteEditionUpdate>(content));
        }

        return editionUpdates;
    }

    private static void ProcessEditions(List<RteEditionUpdate> editions)
    {
        // * Scan HTMLs and create a map of existend IDs and their HTML files
        var originalFiles = TemplatesProcessor.ScanFolderTemplates(TaskSettings.TaskSubFolders);

        var embedLinksService = GitLinksProcessor.GetEmbedLinksService(originalFiles);

        Console.WriteLine($"Embedding edits from edition(s)...");
        var filesToProcess = originalFiles;
        var pathsOfChangedFiles = new HashSet<string>();

        foreach (var edition in editions)
        {
            // Processing a single edition
            var embedEditsService = new EmbedEditsService(embedLinksService, edition);
            var processedFiles = TemplatesProcessor.ProcessTemplateFiles(filesToProcess, embedEditsService);

            // Collect all files with changed content
            processedFiles.ForEach(o =>
            {
                if (o.InsertsCount > 0)
                    pathsOfChangedFiles.Add(o.Path);
            });

            // Use processed files as a source for the next processing of this cycle
            filesToProcess = processedFiles.ConvertAll(o => o.ToTemplateFile());
        }

        if (pathsOfChangedFiles.Any())
        {
            var taskOutputFiles = filesToProcess;

            // Save changed source files
            var filesToSave = taskOutputFiles.FindAll(o => pathsOfChangedFiles.Contains(o.Path));
            TemplatesProcessor.SaveSourceFiles(filesToSave);

            // Update the dictionary
            GitDictionaryProcessor.UpdateDictionary(taskOutputFiles);

            // New IDs may be added when edits embedding adds new nodes into HTML (like <bb-tooltip>)
            GitLinksProcessor.UpdateRteIdCounter(embedLinksService.idSet);
        }
    }

    private static void CleanupEditionsFolder(IEnumerable<string> editionFiles)
    {
        Console.WriteLine($">> Deleting JSON edition files");
        foreach (var file in editionFiles)
        {
            try
            {
                File.Delete(file);
                Console.WriteLine($"    {file} deleted");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"    {file} deletion error: {ex.Message}");
            }
        }
    }
}