using System.Diagnostics;

namespace AUTOREMAPPER;

internal class Program
{
    // File and directory paths.
    static string AssetsDirectory = null!;
    static string FFMPEGPath = null!;
    static string RootUnityProjectDirectory = null!;
    static string PackageCacheDirectory = null!;
    static string ImportedFilesDirectory = null!;
    static string ExportedFilesDirectory = null!;
    static string ExportedAudioClipDirectory = null!;
    static string ScriptsDirectory = null!;
    static string PluginsDirectory = null!;
    static string AssemblyCSharpDirectory = null!;

    internal static void WaitForUserInputAndQuit() { Console.WriteLine("Press any key to continue..."); Console.ReadKey(); Environment.Exit(0); }
    internal static void WaitForUserInput() { Console.WriteLine("Press any key to continue..."); Console.ReadKey();}

    internal static void FixGUIDs()
    {
        Remapper.RemapGuids(RootUnityProjectDirectory, ExportedFilesDirectory, ImportedFilesDirectory, true, true);
        Remapper.RemapGuids(RootUnityProjectDirectory, ExportedFilesDirectory, PackageCacheDirectory, true, true);
    }

    internal static void FixAudio()
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

    internal static void FindFilesAndFixStucture()
    {
        AssetsDirectory = Directory.GetCurrentDirectory();

        FFMPEGPath = Path.Combine(AssetsDirectory, "ffmpeg.exe");
        if (!File.Exists(FFMPEGPath)) { Console.WriteLine("Failed to find the file: " + FFMPEGPath); WaitForUserInputAndQuit(); return; }

        RootUnityProjectDirectory = Directory.GetParent(AssetsDirectory)!.ToString();
        if (!Directory.Exists(AssetsDirectory)) { Console.WriteLine("Failed to find the directory: " + AssetsDirectory); WaitForUserInputAndQuit(); return; }

        PackageCacheDirectory = Path.Combine(Directory.GetParent(AssetsDirectory)!.ToString(), "Library\\PackageCache");
        if (!Directory.Exists(PackageCacheDirectory)) { Console.WriteLine("Failed to find the directory: " + PackageCacheDirectory); WaitForUserInputAndQuit(); return; }

        ImportedFilesDirectory = Path.Combine(AssetsDirectory, "!ImportedAssets");
        if (!Directory.Exists(ImportedFilesDirectory)) { Console.WriteLine("Failed to find the directory: " + ImportedFilesDirectory); WaitForUserInputAndQuit(); return; }

        ExportedFilesDirectory = Path.Combine(AssetsDirectory, "!ExportedAssets");
        if (!Directory.Exists(ExportedFilesDirectory))
        {
            List<string> BadFileDirectorys = Directory.GetDirectories(AssetsDirectory).ToList();
            BadFileDirectorys.Remove(ImportedFilesDirectory);
            Directory.CreateDirectory(ExportedFilesDirectory);
            foreach (string BadFileDirectory in BadFileDirectorys)
            {
                Directory.Move(BadFileDirectory, Path.Combine(ExportedFilesDirectory, Path.GetFileName(BadFileDirectory)));
            }
            List<string> BadFiles = Directory.GetFiles(AssetsDirectory).ToList();

            // Remove files that would cause issues if moved
            BadFiles.Remove(Path.Combine(AssetsDirectory, "AUTOREMAPPER.exe"));
            BadFiles.Remove(Path.Combine(AssetsDirectory, "AUTOREMAPPER.exe.meta"));
            BadFiles.Remove(Path.Combine(AssetsDirectory, "AUTOREMAPPER.pdb"));
            BadFiles.Remove(Path.Combine(AssetsDirectory, "AUTOREMAPPER.pdb.meta"));
            BadFiles.Remove(Path.Combine(AssetsDirectory, "ffmpeg.exe"));
            BadFiles.Remove(Path.Combine(AssetsDirectory, "ffmpeg.exe.meta"));
            BadFiles.Remove(Path.Combine(AssetsDirectory, "!ImportedAssets.meta"));

            foreach (string BadFile in BadFiles)
            {
                File.Move(BadFile, Path.Combine(ExportedFilesDirectory, Path.GetFileName(BadFile)));
            }
        } // Move ripped files to the !ExportedFiles directory

        ExportedAudioClipDirectory = Path.Combine(ExportedFilesDirectory, "AudioClip");
        if (!Directory.Exists(ExportedAudioClipDirectory)) { Console.WriteLine("Failed to find the directory: " + ExportedAudioClipDirectory); WaitForUserInputAndQuit(); return; }

        ScriptsDirectory = Path.Combine(ExportedFilesDirectory, "Scripts");
        if (!Directory.Exists(ScriptsDirectory)) { Console.WriteLine("Failed to find the directory: " + ScriptsDirectory); WaitForUserInputAndQuit(); return; }

        AssemblyCSharpDirectory = Path.Combine(ScriptsDirectory, "Assembly-CSharp");
        if (!Directory.Exists(AssemblyCSharpDirectory)) { Console.WriteLine("Failed to find the directory: " + AssemblyCSharpDirectory); WaitForUserInputAndQuit(); return; }

        PluginsDirectory = Path.Combine(ExportedFilesDirectory, "Plugins");
        if (!Directory.Exists(PluginsDirectory)) { Console.WriteLine("Failed to find the directory: " + PluginsDirectory); WaitForUserInputAndQuit(); return; }
    }

    internal static void Main(string[] args)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        try
        {
            FindFilesAndFixStucture();
            FixGUIDs();
            FixAudio();
            PostFix.SaveFilesNeededFiles(ExportedFilesDirectory);
            PostFix.ScriptDirectoryFix(ScriptsDirectory);
            PostFix.AssemblyCSharpFix(AssemblyCSharpDirectory);
            PostFix.PluginDirectoryFix(PluginsDirectory);
            PostFix.RemoveUnwantedFiles(AssetsDirectory);
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