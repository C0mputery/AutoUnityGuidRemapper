using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace AUTOREMAPPER
{
    internal static class Remapper
    {
        internal static long totalBadGUIDsRefrenced;
        internal static long totalBadReferencesFound;

        internal static void RemapGuids(string unityProjectDirectory, string badGuidDirectory, string goodGuidsDirectory, bool shouldRemoveOldFiles, bool setAllFilesAsReadable)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            if (setAllFilesAsReadable)
            {
                Console.WriteLine("Setting All Files As Readable");
                Parallel.ForEach(Directory.EnumerateFiles(unityProjectDirectory, "*", SearchOption.AllDirectories),
                    file => { File.SetAttributes(file, File.GetAttributes(file) & ~FileAttributes.Normal); });
                Console.WriteLine("Done Setting All Files As Readable");
            }

            ConcurrentDictionary<string, string> filesNameToGoodGuids = new ConcurrentDictionary<string, string>();
            ConcurrentDictionary<string, string> badGuidsToGoodGuids = new ConcurrentDictionary<string, string>();

            Console.WriteLine("Started Good File Parsing");

            IEnumerable<string> goodMetaFilePaths =
                Directory.EnumerateFiles(goodGuidsDirectory, "*.meta", SearchOption.AllDirectories);

            Parallel.ForEach(goodMetaFilePaths, goodMetaFilePath =>
            {
                using (StreamReader goodMetaFileStreamReader = new StreamReader(goodMetaFilePath))
                {
                    goodMetaFileStreamReader.ReadLine();
                    string possibleGuidLine;
                    if ((possibleGuidLine = goodMetaFileStreamReader.ReadLine()!) != null && possibleGuidLine.StartsWith("guid: "))
                    {
                        string goodGuid = possibleGuidLine[6..];
                        string goodFilePath = goodMetaFilePath[..^5];
                        if (File.Exists(goodFilePath))
                        {
                            switch (Path.GetExtension(goodFilePath))
                            {
                                case ".cs":
                                    using (StreamReader goodFileStreamReader = new StreamReader(goodFilePath))
                                    {
                                        Match namespaceMatch = null!;
                                        while (goodFileStreamReader.ReadLine() is { } goodFileLine)
                                        {
                                            namespaceMatch = Regex.Match(goodFileLine, @"namespace\s+([\w.]+)");
                                            if (namespaceMatch.Success)
                                            {
                                                filesNameToGoodGuids.TryAdd(Path.GetFileName(goodFilePath) + namespaceMatch.Groups[1].Value, goodGuid);
                                                break;
                                            }
                                        }
                                        if (namespaceMatch is { Success: false })
                                        {
                                            filesNameToGoodGuids.TryAdd(Path.GetFileName(goodFilePath), goodGuid);
                                        }
                                    }
                                    break;
                                case ".shader":
                                    using (StreamReader goodFileStreamReader = new StreamReader(goodFilePath))
                                    {
                                        Match shaderPathMatch = null!;
                                        while (goodFileStreamReader.ReadLine() is { } goodFileLine)
                                        {
                                            shaderPathMatch = Regex.Match(goodFileLine, @"Shader\s+""([^""]+)""");
                                            if (shaderPathMatch.Success)
                                            {
                                                filesNameToGoodGuids.TryAdd(shaderPathMatch.Groups[1].Value, goodGuid);
                                                break;
                                            }
                                        }
                                        if (shaderPathMatch is { Success: false })
                                        {
                                            filesNameToGoodGuids.TryAdd(Path.GetFileName(goodFilePath), goodGuid);
                                        }
                                    }
                                    break;
                                default:
                                    filesNameToGoodGuids.TryAdd(Path.GetFileName(goodFilePath), goodGuid);
                                    break;
                            }
                        }
                    }
                }
            });

            Console.WriteLine("Started Bad File Parsing");

            IEnumerable<string> badMetaFilePaths = Directory
                .EnumerateFiles(badGuidDirectory, "*.meta", SearchOption.AllDirectories)
                .Where(file => !file.Contains(goodGuidsDirectory));

            Parallel.ForEach(badMetaFilePaths, badMetaFilePath =>
            {
                using (StreamReader badMetaFileStreamReader = new StreamReader(badMetaFilePath))
                {
                    badMetaFileStreamReader.ReadLine();
                    string possibleGuidLine;
                    if ((possibleGuidLine = badMetaFileStreamReader.ReadLine()!) != null && possibleGuidLine.StartsWith("guid: "))
                    {
                        string badGuid = possibleGuidLine[6..];
                        string badFilePath = badMetaFilePath[..^5];
                        if (File.Exists(badFilePath))
                        {
                            switch (Path.GetExtension(badFilePath))
                            {
                                case ".cs":
                                    Match namespaceMatch = null!;
                                    using (StreamReader badFileStreamReader = new StreamReader(badFilePath))
                                    {
                                        while (badFileStreamReader.ReadLine() is { } line)
                                        {
                                            namespaceMatch = Regex.Match(line, @"namespace\s+([\w.]+)");
                                            if (namespaceMatch.Success)
                                            {
                                                break;
                                            }
                                        }
                                    }
                                    if (namespaceMatch != null && namespaceMatch.Success)
                                    {
                                        if (filesNameToGoodGuids.TryGetValue(
                                                Path.GetFileName(badFilePath) + namespaceMatch.Groups[1].Value, out string goodGuid))
                                        {
                                            badGuidsToGoodGuids.TryAdd(badGuid, goodGuid);
                                            if (shouldRemoveOldFiles) { File.Delete(badFilePath); badMetaFileStreamReader.Close(); File.Delete(badMetaFilePath); }
                                        }
                                    }
                                    else
                                    {
                                        if (filesNameToGoodGuids.TryGetValue(Path.GetFileName(badFilePath), out string goodGuid))
                                        {
                                            badGuidsToGoodGuids.TryAdd(badGuid, goodGuid);
                                            if (shouldRemoveOldFiles) { File.Delete(badFilePath); badMetaFileStreamReader.Close(); File.Delete(badMetaFilePath); }
                                        }
                                    }
                                    break;
                                case ".shader":
                                    Match shaderPathMatch = null!;
                                    using (StreamReader goodFileStreamReader = new StreamReader(badFilePath))
                                    {
                                        while (goodFileStreamReader.ReadLine() is { } goodFileLine)
                                        {
                                            shaderPathMatch = Regex.Match(goodFileLine, @"Shader\s+""([^""]+)""");
                                            if (shaderPathMatch.Success)
                                            {
                                                break;
                                            }
                                        }
                                    }
                                    if (shaderPathMatch != null && shaderPathMatch.Success)
                                    {
                                        if (filesNameToGoodGuids.TryGetValue(shaderPathMatch.Groups[1].Value, out string goodGuid))
                                        {
                                            badGuidsToGoodGuids.TryAdd(badGuid, goodGuid);
                                            if (shouldRemoveOldFiles)
                                            {
                                                File.Delete(badFilePath);
                                                badMetaFileStreamReader.Close();
                                                File.Delete(badMetaFilePath);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (filesNameToGoodGuids.TryGetValue(Path.GetFileName(badFilePath), out string goodGuid))
                                        {
                                            badGuidsToGoodGuids.TryAdd(badGuid, goodGuid);
                                            if (shouldRemoveOldFiles)
                                            {
                                                File.Delete(badFilePath);
                                                badMetaFileStreamReader.Close();
                                                File.Delete(badMetaFilePath);
                                            }
                                        }
                                    }
                                    break;
                                case ".asmdef":
                                    if (filesNameToGoodGuids.TryGetValue(Path.GetFileName(badFilePath), out string goodGuid2))
                                    {
                                        badGuidsToGoodGuids.TryAdd(badGuid, goodGuid2);
                                        if (shouldRemoveOldFiles) { File.Delete(badFilePath); badMetaFileStreamReader.Close(); File.Delete(badMetaFilePath); }
                                    }
                                    break;
                                default:
                                    if (filesNameToGoodGuids.TryGetValue(Path.GetFileName(badFilePath), out string GoodGuid))
                                    {
                                        badGuidsToGoodGuids.TryAdd(badGuid, GoodGuid);
                                    }
                                    break;
                            }
                        }
                    }
                }
            });

            Console.WriteLine("Finding Files To Remap");

            string[] badGuidsArray = badGuidsToGoodGuids.Keys.ToArray();
            string[] goodGuidsArray = badGuidsToGoodGuids.Values.ToArray();

            string[] extensions = { "*.prefab", "*.unity", "*.mat", "*.asmdef" }; // Add more??
            IEnumerable<string> filesToRemap = extensions.AsParallel().SelectMany(
                    extension => Directory.EnumerateFiles(unityProjectDirectory, extension, SearchOption.AllDirectories).Where(file => !file.Contains(goodGuidsDirectory)));

            filesToRemap = filesToRemap.Concat(Directory.EnumerateFiles(unityProjectDirectory, "*.asset", SearchOption.AllDirectories).AsParallel().Where(file => !file.Contains(goodGuidsDirectory)).Where(filePath =>
            {
                string[] lines = File.ReadAllLines(filePath);
                if (lines.Length >= 4 && lines[3].Contains("Mesh:"))
                {
                    return false;
                }
                return true;
            }));

            Console.WriteLine("Starting GUID Remapping");

            long badGUIDsRefrenced = 0;
            long badReferencesFound = 0;
            Parallel.ForEach(filesToRemap, fileToRemap =>
            {
                if (File.Exists(fileToRemap))
                {
                    bool guidsRemapped = false;
                    string tempFile = Path.GetTempFileName();
                    using (StreamReader fileToRemapStreamReader = new StreamReader(fileToRemap))
                    using (StreamWriter tempFileStreamWriter = new StreamWriter(tempFile))
                    {
                        string line;
                        while ((line = fileToRemapStreamReader.ReadLine()!) != null)
                        {
                            for (int i = 0; i < badGuidsArray.Length; i++)
                            {
                                if (line.Contains(badGuidsArray[i]))
                                {
                                    line = line.Replace(badGuidsArray[i], goodGuidsArray[i]);
                                    Interlocked.Add(ref badReferencesFound, 1);
                                    guidsRemapped = true;
                                    break;
                                }
                            }
                            tempFileStreamWriter.WriteLine(line);
                        }
                    }
                    if (guidsRemapped)
                    {
                        File.Move(tempFile, fileToRemap, true);
                        Console.WriteLine($"{Path.GetFileName(fileToRemap)} Successful Remapped GUIDs");
                        Interlocked.Add(ref badGUIDsRefrenced, 1);
                    }
                    else
                    {
                        File.Delete(tempFile);
                        Console.WriteLine($"{Path.GetFileName(fileToRemap)} Found No Bad GUIDS");
                    }
                }
                else
                {
                    Console.WriteLine($"{fileToRemap} Does Not Exist??");
                }
            });

            Console.WriteLine($"{badGUIDsRefrenced} bad GUID referenced");
            Console.WriteLine($"{badReferencesFound} bad references found");

            stopwatch.Stop();
            Console.WriteLine($"Elapsed Time: {stopwatch.Elapsed}");
            Console.WriteLine($"Settigns Used: unityProjectDirectory {unityProjectDirectory}, badGuidDirectory {badGuidDirectory}, goodGuidsDirectory {goodGuidsDirectory}");
            totalBadGUIDsRefrenced += badGUIDsRefrenced;
            totalBadReferencesFound += badReferencesFound;
        }
    }
}
