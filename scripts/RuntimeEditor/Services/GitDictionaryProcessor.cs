using Newtonsoft.Json;
using RuntimeEditor.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RuntimeEditor.Services;

public static class GitDictionaryProcessor
{
    public static void UpdateDictionary(List<TemplateFile> taskOutputFiles)
    {
        Console.WriteLine($">> Refreshing dictionary...");

        #region Get templates for dictionary
        List<TemplateFile> filesForDictionary;
        // Optimization: if processed files are taken from folder that matches the root source folder
        // which is scanned by dictionary then we should not rescan the folder and should take content
        // from the processed files
        if (TaskSettings.TaskSubFolders.Length == 1 && TaskSettings.TaskSubFolders[0] == TaskSettings.SourceRootFolder)
        {
            filesForDictionary = taskOutputFiles;
        }
        else
        {
            filesForDictionary = TemplatesProcessor.ScanFolderTemplates(new string[] { TaskSettings.SourceRootFolder });
        }
        #endregion

        var prevDictionary = ReadDictionary();

        var dictionarySettingsContent = File.ReadAllText(TaskSettings.LocalRoot + Constants.DictionarySettingsFilePath, Encoding.UTF8);
        var dictionarySettings = JsonConvert.DeserializeObject<DictionarySettings>(dictionarySettingsContent);

        var newDictionary = DictionaryService.Generate(filesForDictionary, dictionarySettings);
        newDictionary.Sort((a, b) => a.RteId.CompareTo(b.RteId));

        var prevDictionaryContent = DictionaryToContent(prevDictionary);
        var newDictionaryContent = DictionaryToContent(newDictionary);

        if (newDictionaryContent != prevDictionaryContent)
        {
            File.WriteAllText(TaskSettings.LocalRoot + Constants.DictionaryFilePath, newDictionaryContent);
            Console.WriteLine($"Dictionary file '{Constants.DictionaryFilePath}' refreshed. New dictionary size: {newDictionary.Count}. Previous dictionary size: {prevDictionary?.Count ?? 0}");
        }
        else
        {
            Console.WriteLine($"Dictionary not changed");
        }
    }

    static List<RteDictionaryItem> ReadDictionary()
    {
        try
        {
            var content = File.ReadAllText(TaskSettings.LocalRoot + Constants.DictionaryFilePath, Encoding.UTF8);
            var dic = JsonConvert.DeserializeObject<List<RteDictionaryItem>>(content);

            if (dic == null)
            {
                throw new Exception("Dictionary object is NULL");
            }

            return dic;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading dictionary from file '{Constants.DictionaryFilePath}'. Details: {ex.Message}");
            return new List<RteDictionaryItem>();
        }
    }

    static string DictionaryToContent(List<RteDictionaryItem> dictionary) =>
        JsonConvert.SerializeObject(dictionary, Formatting.Indented);

}