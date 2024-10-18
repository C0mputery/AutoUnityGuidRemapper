using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.RegularExpressions;
// ReSharper disable MoveVariableDeclarationInsideLoopCondition

namespace AutoUnityGuidRemapper;

public static class AutoGuidRemapper
{
    private static readonly Regex ShaderPathRegex = new Regex(@"Shader\s+""([^""]+)""", RegexOptions.Compiled);
    private static readonly Regex NameSpaceRegex =  new Regex(@"namespace\s+([\w.]+)", RegexOptions.Compiled);
    
    public static void RemapGuids(string unityProjectRootDirectory, string rippedAssetsDirectory, string importedAssetsDirectory, bool removeReplacedFiles, bool makeAllFilesReadable)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        if (makeAllFilesReadable)
        {
            Console.WriteLine("Setting All Files As Readable");
            Parallel.ForEach(Directory.EnumerateFiles(unityProjectRootDirectory, "*", SearchOption.AllDirectories), file => { File.SetAttributes(file, File.GetAttributes(file) & ~FileAttributes.Normal); });
            Console.WriteLine("Done Setting All Files As Readable");
        }

        ConcurrentDictionary<string, string?> filesNameToImportedGuids = new ConcurrentDictionary<string, string?>();
        ConcurrentDictionary<string, string?> rippedGuidsToImportedGuids = new ConcurrentDictionary<string, string?>();

        Console.WriteLine("Started Good File Parsing");

        IEnumerable<string> importedMetaFilePaths = Directory.EnumerateFiles(importedAssetsDirectory, "*.meta", SearchOption.AllDirectories);

        Parallel.ForEach(importedMetaFilePaths, importedMetaFilePath => {
            string importedFilePath = importedMetaFilePath[..^5];
            if (!File.Exists(importedFilePath)) { return; }
            
            using StreamReader importedMetaFileStreamReader = new StreamReader(importedMetaFilePath);
            importedMetaFileStreamReader.ReadLine();
            string? possibleGuidLine = importedMetaFileStreamReader.ReadLine();
            if (possibleGuidLine == null || !possibleGuidLine.StartsWith("guid: ")) { return; }
            string importedGuid = possibleGuidLine[6..];

            using StreamReader importedFileStreamReader = new StreamReader(importedFilePath);
            switch (Path.GetExtension(importedFilePath)) {
                case ".cs": {
                    Match namespaceMatch = Match.Empty;
                    string? fileLine;
                    while ((fileLine = importedFileStreamReader.ReadLine()) != null) {
                        namespaceMatch = NameSpaceRegex.Match(fileLine);
                        if (!namespaceMatch.Success) { continue; }
                        filesNameToImportedGuids.TryAdd(Path.GetFileName(importedFilePath) + namespaceMatch.Groups[1].Value, importedGuid);
                        break;
                    }
                    if (!namespaceMatch.Success) { filesNameToImportedGuids.TryAdd(Path.GetFileName(importedFilePath), importedGuid); }
                    break;
                }
                case ".shader": {
                    Match shaderPathMatch = Match.Empty;
                    string? fileLine;
                    while ((fileLine = importedFileStreamReader.ReadLine()) != null) {
                        shaderPathMatch = ShaderPathRegex.Match(fileLine);
                        if (!shaderPathMatch.Success) { continue; }
                        filesNameToImportedGuids.TryAdd(shaderPathMatch.Groups[1].Value, importedGuid);
                        break;
                    }
                    if (!shaderPathMatch.Success) { filesNameToImportedGuids.TryAdd(Path.GetFileName(importedFilePath), importedGuid); }
                    break;
                }
                default: {
                    filesNameToImportedGuids.TryAdd(Path.GetFileName(importedFilePath), importedGuid);
                    break;
                }
            }
        });

        Console.WriteLine("Started Bad File Parsing");

        IEnumerable<string> rippedFilePaths = Directory.EnumerateFiles(rippedAssetsDirectory, "*.meta", SearchOption.AllDirectories).Where(file => !file.Contains(importedAssetsDirectory));

        Parallel.ForEach(rippedFilePaths, badMetaFilePath => {
            string badFilePath = badMetaFilePath[..^5];
            if (!File.Exists(badFilePath)) { return; }

            using StreamReader rippedFileStreamReader = new StreamReader(badMetaFilePath);
            rippedFileStreamReader.ReadLine();
            string? possibleGuidLine = rippedFileStreamReader.ReadLine();
            if (possibleGuidLine == null || !possibleGuidLine.StartsWith("guid: ")) { return; }
            string rippedGuid = possibleGuidLine[6..];
            
            switch (Path.GetExtension(badFilePath)) {
                case ".cs": {
                    Match namespaceMatch = Match.Empty;
                    using (StreamReader badFileStreamReader = new StreamReader(badFilePath)) {
                        string? line;
                        while ((line = badFileStreamReader.ReadLine()) != null) {
                            namespaceMatch = NameSpaceRegex.Match(line);
                            if (namespaceMatch.Success) { break; }
                        }
                    }

                    string lookupKey = Path.GetFileName(badFilePath) + (namespaceMatch.Success ? namespaceMatch.Groups[1].Value : "");
                    if (filesNameToImportedGuids.TryGetValue(lookupKey, out string? goodGuid)) {
                        rippedGuidsToImportedGuids.TryAdd(rippedGuid, goodGuid);
                        if (removeReplacedFiles) { File.Delete(badFilePath); rippedFileStreamReader.Close(); File.Delete(badMetaFilePath); }
                    }
                    
                    break;
                }
                case ".shader": {
                    Match shaderPathMatch = Match.Empty;
                    using (StreamReader goodFileStreamReader = new StreamReader(badFilePath)) {
                        string? goodFileLine;
                        while ((goodFileLine = goodFileStreamReader.ReadLine()) != null) {
                            shaderPathMatch = ShaderPathRegex.Match(goodFileLine);
                            if (shaderPathMatch.Success) { break; }
                        }
                    }

                    string lookupKey = shaderPathMatch.Success ? shaderPathMatch.Groups[1].Value : Path.GetFileName(badFilePath);
                    if (filesNameToImportedGuids.TryGetValue(lookupKey, out string? goodGuid)) {
                        rippedGuidsToImportedGuids.TryAdd(rippedGuid, goodGuid);
                        if (removeReplacedFiles) { File.Delete(badFilePath); rippedFileStreamReader.Close(); File.Delete(badMetaFilePath); }
                    }
                    
                    break;
                }
                case ".asmdef": {
                    if (filesNameToImportedGuids.TryGetValue(Path.GetFileName(badFilePath), out string? goodGuid)) {
                        rippedGuidsToImportedGuids.TryAdd(rippedGuid, goodGuid);
                        if (removeReplacedFiles) { File.Delete(badFilePath); rippedFileStreamReader.Close(); File.Delete(badMetaFilePath); }
                    }
                    break;
                }
                default: {
                    if (filesNameToImportedGuids.TryGetValue(Path.GetFileName(badFilePath), out string? goodGuid)) {
                        rippedGuidsToImportedGuids.TryAdd(rippedGuid, goodGuid);
                    }
                    break;
                }
            }
        });

        Console.WriteLine("Finding Files To Remap");

        string[] rippedGuidsArray = rippedGuidsToImportedGuids.Keys.ToArray();
        string?[] importedGuidsArray = rippedGuidsToImportedGuids.Values.ToArray();

        string[] extensions = ["*.prefab", "*.unity", "*.mat", "*.asmdef"]; // Add more??
        IEnumerable<string> filesToRemap = extensions.AsParallel().SelectMany(extension => Directory.EnumerateFiles(unityProjectRootDirectory, extension, SearchOption.AllDirectories).Where(file => !file.Contains(importedAssetsDirectory)));

        filesToRemap = filesToRemap.Concat(Directory.EnumerateFiles(unityProjectRootDirectory, "*.asset", SearchOption.AllDirectories).Where(file => !file.Contains(importedAssetsDirectory)).Where(filePath =>
        {
            using StreamReader assetFileStreamReader = new StreamReader(filePath);
            assetFileStreamReader.ReadLine();
            assetFileStreamReader.ReadLine();
            assetFileStreamReader.ReadLine();
            string? line = assetFileStreamReader.ReadLine();
            return line == null || !line.StartsWith("Mesh:");
        }).AsParallel());

        Console.WriteLine("Starting GUID Remapping");

        long rippedGuidsReferenced = 0;
        long rippedReferencesFound = 0;
        Parallel.ForEach(filesToRemap, fileToRemap => {
            if (!File.Exists(fileToRemap)) {
                Console.WriteLine($"{fileToRemap} Does Not Exist??");
                return;
            }

            bool guidsRemapped = false;
            string tempFile = Path.GetTempFileName();
            using (StreamReader fileToRemapStreamReader = new StreamReader(fileToRemap))
            using (StreamWriter tempFileStreamWriter = new StreamWriter(tempFile)) {
                string line;
                while ((line = fileToRemapStreamReader.ReadLine()!) != null) {
                    for (int i = 0; i < rippedGuidsArray.Length; i++) {
                        if (!line.Contains(rippedGuidsArray[i])) { continue; }
                        line = line.Replace(rippedGuidsArray[i], importedGuidsArray[i]);
                        Interlocked.Add(ref rippedReferencesFound, 1);
                        guidsRemapped = true;
                        break;
                    }

                    tempFileStreamWriter.WriteLine(line);
                }
            }

            if (guidsRemapped) {
                File.Move(tempFile, fileToRemap, true);
                Console.WriteLine($"{Path.GetFileName(fileToRemap)} Successful Remapped GUIDs");
                Interlocked.Add(ref rippedGuidsReferenced, 1);
            }
            else {
                File.Delete(tempFile);
            }
        });

        Console.WriteLine($"{rippedGuidsReferenced} ripped GUID referenced");
        Console.WriteLine($"{rippedReferencesFound} ripped references found");

        stopwatch.Stop();
        Console.WriteLine($"Elapsed Time: {stopwatch.Elapsed}");
        Console.WriteLine($"Settings Used: unityProjectDirectory {unityProjectRootDirectory}, badGuidDirectory {rippedAssetsDirectory}, goodGuidsDirectory {importedAssetsDirectory}");
    }
}