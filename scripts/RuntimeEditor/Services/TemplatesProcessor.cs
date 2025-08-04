using System;
using System.IO;
using System.Collections.Generic;
using RuntimeEditor.Classes;
using System.Linq;

namespace RuntimeEditor.Services;

public static class TemplatesProcessor
{
    public static List<TemplateFile> ScanFolderTemplates(string[] folders)
    {
        var files = new List<string>();

        for (var i = 0; i < folders.LongLength; i++)
        {
            var folderFiles = Directory.EnumerateFiles(folders[i], "*.html", SearchOption.AllDirectories);
            folderFiles = folderFiles.ToList().ConvertAll(o => o.Replace("\\", "/"));
            files.AddRange(folderFiles);
        }

        // Ensure that if folders intersect then we exclude duplicates
        files = files.Distinct().ToList();

        var templateFiles = new List<TemplateFile>();

        Console.WriteLine($">>>> Scanning folder files...");
        foreach (var file in files)
        {
            var originContent = File.ReadAllText(file, System.Text.Encoding.UTF8);

            templateFiles.Add(new TemplateFile
            {
                Path = file,
                Content = originContent
            });
        }
        Console.WriteLine($"<<<< Scanned {files.Count} files.");
        Console.WriteLine("--------------------------------");
        Console.WriteLine();
        Console.WriteLine();

        return templateFiles;
    }

    public static List<ProcessedTemplateFile> ProcessTemplateFiles(List<TemplateFile> templateFiles, IEmbedService embedService)
    {
        Console.WriteLine($">>>> Processing folder files...");

        var processedTemplateFiles = new List<ProcessedTemplateFile>();
        var totalInserts = 0;

        foreach (var tfile in templateFiles)
        {
            var embedResult = embedService.Embed(tfile.Content);

            if (!string.IsNullOrEmpty(embedResult.ErrorMessage))
            {
                Console.WriteLine($"    {tfile.Path}");
                Console.WriteLine($"        template parser error: {embedResult.ErrorMessage}");
            }

            if (embedResult.AmendmentsCount > 0)
            {
                Console.WriteLine($"    {tfile.Path}");
                var changeSize = embedResult.ResultContent.Length - embedResult.OriginalContent.Length;
                Console.WriteLine($"        read {embedResult.OriginalContent.Length} bytes, change: {changeSize} bytes");

                totalInserts += embedResult.AmendmentsCount;
            }

            processedTemplateFiles.Add(new ProcessedTemplateFile
            {
                Path = tfile.Path,
                Content = embedResult.ResultContent,
                InsertsCount = embedResult.AmendmentsCount
            });
        }

        Console.WriteLine($"TOTAL inserts/cleanups: " + totalInserts);
        Console.WriteLine($"<<<< Processed {templateFiles.Count} files.");
        Console.WriteLine("--------------------------------");
        Console.WriteLine();
        Console.WriteLine();

        return processedTemplateFiles;
    }

    public static void SaveSourceFiles(List<TemplateFile> files)
    {
        if (files.Count > 0)
        {
            Console.WriteLine($">>>> Saving processed files...");

            foreach (var a in files)
            {
                File.WriteAllText(a.Path, a.Content);
                Console.WriteLine($"    {a.Path}");
            }

            Console.WriteLine($"DONE");
            Console.WriteLine($"<<<< Saved files: {files.Count}");
            Console.WriteLine("--------------------------------");
            Console.WriteLine();
            Console.WriteLine();
        }
        else
        {
            Console.WriteLine($"Nothing to save. No file has been changed");
        }
    }
}