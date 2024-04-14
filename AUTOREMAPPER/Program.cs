using System.Diagnostics;

namespace AUTOREMAPPER;

internal class Program
{
    // File and directory paths.
    static string UnityAssetFolderDirectory = null!;
    static string FFMPEGPath = null!;
    static string RootUnityProjectDirectory = null!;
    static string PackageCacheDirectory = null!;
    static string ImportedFilesDirectory = null!;
    static string ExportedFilesDirectory = null!;
    static string ExportedAudioClipDirectory = null!;
    static string ScriptsDirectory = null!;
    static string PluginsDirectory = null!;

    static void WaitForUserInputAndQuit() { Console.WriteLine("Press any key to continue..."); Console.ReadKey(); Environment.Exit(0); }

    static void FixGUIDs()
    {
        Remapper.RemapGuids(RootUnityProjectDirectory, ExportedFilesDirectory, ImportedFilesDirectory, true, true);
        Remapper.RemapGuids(RootUnityProjectDirectory, ExportedFilesDirectory, PackageCacheDirectory, true, true);
    }

    static void FixAudio()
    {
        List<string> BadWavFiles = Directory.GetFiles(ExportedAudioClipDirectory, "*.wav").ToList();
        foreach (string BadWavFile in BadWavFiles)
        {
            string name = Path.GetFileNameWithoutExtension(BadWavFile);
            string NewFileName = BadWavFile.Replace(name, $"{name}!");
            Process.Start(FFMPEGPath, $"-i \"{BadWavFile}\" \"{NewFileName}\"").WaitForExit();
            File.Delete(BadWavFile);
        }
        List<string> GoodWavFiles = Directory.GetFiles(ExportedAudioClipDirectory, "*.wav").ToList();
        foreach (string GoodWavFile in GoodWavFiles)
        {
            string fileName = Path.GetFileName(GoodWavFile);
            File.Move(Path.Combine(ExportedAudioClipDirectory, fileName), Path.Combine(ExportedAudioClipDirectory, fileName.Replace("!", "")));
        }
    }

    static void FindFilesAndFixStucture()
    {
        UnityAssetFolderDirectory = Directory.GetCurrentDirectory();

        FFMPEGPath = Path.Combine(UnityAssetFolderDirectory, "ffmpeg.exe");
        if (!File.Exists(FFMPEGPath)) { Console.WriteLine("Failed to find the file: " + FFMPEGPath); WaitForUserInputAndQuit(); return; }

        RootUnityProjectDirectory = Directory.GetParent(UnityAssetFolderDirectory)!.ToString();
        if (!Directory.Exists(UnityAssetFolderDirectory)) { Console.WriteLine("Failed to find the directory: " + UnityAssetFolderDirectory); WaitForUserInputAndQuit(); return; }

        PackageCacheDirectory = Path.Combine(Directory.GetParent(UnityAssetFolderDirectory)!.ToString(), "Library\\PackageCache");
        if (!Directory.Exists(PackageCacheDirectory)) { Console.WriteLine("Failed to find the directory: " + PackageCacheDirectory); WaitForUserInputAndQuit(); return; }

        ImportedFilesDirectory = Path.Combine(UnityAssetFolderDirectory, "!ImportedAssets");
        if (!Directory.Exists(ImportedFilesDirectory)) { Console.WriteLine("Failed to find the directory: " + ImportedFilesDirectory); WaitForUserInputAndQuit(); return; }

        ExportedFilesDirectory = Path.Combine(UnityAssetFolderDirectory, "!ExportedAssets");
        if (!Directory.Exists(ExportedFilesDirectory))
        {
            List<string> BadFileDirectorys = Directory.GetDirectories(UnityAssetFolderDirectory).ToList();
            BadFileDirectorys.Remove(ImportedFilesDirectory);
            Directory.CreateDirectory(ExportedFilesDirectory);
            foreach (string BadFileDirectory in BadFileDirectorys)
            {
                Directory.Move(BadFileDirectory, Path.Combine(ExportedFilesDirectory, Path.GetFileName(BadFileDirectory)));
            }
            List<string> BadFiles = Directory.GetFiles(UnityAssetFolderDirectory).ToList();

            // Remove files that would cause issues if moved
            BadFiles.Remove(Path.Combine(UnityAssetFolderDirectory, "AUTOREMAPPER.exe"));
            BadFiles.Remove(Path.Combine(UnityAssetFolderDirectory, "ffmpeg.exe"));
            BadFiles.Remove(Path.Combine(UnityAssetFolderDirectory, "!ImportedAssets.meta"));

            foreach (string BadFile in BadFiles)
            {
                File.Move(BadFile, Path.Combine(ExportedFilesDirectory, Path.GetFileName(BadFile)));
            }
        } // Move ripped files to the !ExportedFiles directory

        ExportedAudioClipDirectory = Path.Combine(ExportedFilesDirectory, "AudioClip");
        if (!Directory.Exists(ExportedAudioClipDirectory)) { Console.WriteLine("Failed to find the directory: " + ExportedAudioClipDirectory); WaitForUserInputAndQuit(); return; }

        ScriptsDirectory = Path.Combine(ExportedFilesDirectory, "Scripts");
        if (!Directory.Exists(ScriptsDirectory)) { Console.WriteLine("Failed to find the directory: " + ScriptsDirectory); WaitForUserInputAndQuit(); return; }

        PluginsDirectory = Path.Combine(ExportedFilesDirectory, "Plugins");
        if (!Directory.Exists(PluginsDirectory)) { Console.WriteLine("Failed to find the directory: " + PluginsDirectory); WaitForUserInputAndQuit(); return; }
    }

    static void Main(string[] args)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        try
        {
            FindFilesAndFixStucture();
            FixGUIDs();
            FixAudio();
            PostFix.SaveFilesFromBeingNuked(ExportedFilesDirectory);
            PostFix.RemoveUnwantedScriptDirectorysAndFiles(ScriptsDirectory);
            PostFix.NukePluginsDirectorys(PluginsDirectory);
            PostFix.NukeFilesWithRegex(UnityAssetFolderDirectory);
        }
        catch (Exception e)
        {
            Console.WriteLine("An error occurred: ");
            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);
            stopwatch.Stop();
            Console.WriteLine("Total time elapsed: " + stopwatch.Elapsed);
            WaitForUserInputAndQuit();
        }

        stopwatch.Stop();
        Console.WriteLine("Total time elapsed: " + stopwatch.Elapsed);
        WaitForUserInputAndQuit();
    }
}