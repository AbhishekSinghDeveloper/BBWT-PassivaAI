using RuntimeEditor.Classes;
using RuntimeEditor.Services;
using System;

namespace RuntimeEditor;

static class Program
{
    private static class ApplicationTask
    {
        public const string None = "";
        public const string EmbedLinks = "embed_links";
        public const string EmbedEdits = "embed_edits";
        public const string CleanupLinks = "cleanup_links";
    }

    static void Main(string[] args)
    {
        ParseInputParams(args);

        switch (AppTask)
        {
            case ApplicationTask.EmbedLinks:
                GitLinksProcessor.EmbedFolderLinks();
                break;

            case ApplicationTask.EmbedEdits:
                GitEditionsProcessor.EmbedFolderEditions();
                break;

            case ApplicationTask.CleanupLinks:
                GitLinksProcessor.CleanupFolderLinks(Constants.RteAttrName);
                break;
        }
    }

    const string paramNamePrefix = "--";
    const string localRootParam = "local";

    private static string FormatPath(string s) => s.Replace("\\", "/").TrimEnd('/') + "/";
    private static string AppTask = ApplicationTask.None;

    private static void ParseInputParams(string[] args)
    {
        var argIndex = 0;

        if (args.Length > argIndex)
        {
            TaskSettings.SourceRootFolder = FormatPath(args[argIndex]);
            Console.WriteLine($"Parsed agrument {argIndex}: {TaskSettings.SourceRootFolder}");
            argIndex++;
        }

        if (args.Length > argIndex)
        {
            // Get param name
            AppTask = args[argIndex].StartsWith(paramNamePrefix) ?
                args[argIndex].Remove(0, paramNamePrefix.Length) : args[argIndex];

            Console.WriteLine($"Parsed agrument {argIndex}: {AppTask}");
            argIndex++;

            // Get param value
            if (args.Length > argIndex && !args[argIndex].StartsWith(paramNamePrefix))
            {
                var foldersStr = args[argIndex];
                TaskSettings.TaskSubFolders = foldersStr.Trim(';').Split(";");

                for (var i = 0; i < TaskSettings.TaskSubFolders.Length; i++)
                    TaskSettings.TaskSubFolders[i] = FormatPath(TaskSettings.TaskSubFolders[i]);

                Console.WriteLine($"Parsed agrument {argIndex}: {foldersStr}");
                argIndex++;
            }
            else
            {
                TaskSettings.TaskSubFolders = new string[] { "" };
            }
        }

        if (args.Length > argIndex)
        {
            if (args[argIndex] == paramNamePrefix + localRootParam)
            {
                argIndex++;

                TaskSettings.LocalRoot = FormatPath(args[argIndex]);
            }
        }

        // ---
        if (!string.IsNullOrWhiteSpace(TaskSettings.LocalRoot))
        {
            TaskSettings.SourceRootFolder = TaskSettings.LocalRoot + TaskSettings.SourceRootFolder;
        }

        for (var i = 0; i < TaskSettings.TaskSubFolders.Length; i++)
            TaskSettings.TaskSubFolders[i] = TaskSettings.SourceRootFolder + TaskSettings.TaskSubFolders[i];
    }
}