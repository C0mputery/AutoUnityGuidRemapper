using System.Diagnostics;

namespace AutoUnityGuidRemapper;

static class Program
{
    private static void WaitForUserInputAndQuit() { Console.WriteLine("Press any key to continue..."); Console.ReadKey(); Environment.Exit(0); }
    private static void WaitForUserInput() { Console.WriteLine("Press any key to continue..."); Console.ReadKey();}

    private static string ExportedFilesDirectory = "";
    private static string ImportedFilesDirectory = "";
    private static string PackageCacheDirectory = "";
    private static string AssetsDirectory = "";
    private static string RootDirectory = "";
    
    private static void FindFilesAndFixStructure() {
        Console.WriteLine("What is the root directory of the Unity project?");
        RootDirectory = Console.ReadLine() ?? "";
        if (string.IsNullOrEmpty(RootDirectory)) { RootDirectory = @"C:\Users\Computery\Desktop\STRAFTAT"; }


        if (!Directory.Exists(RootDirectory)) { Console.WriteLine("The directory does not exist!"); WaitForUserInputAndQuit(); return; }
        
        AssetsDirectory = Path.Combine(RootDirectory, "Assets");
        if (!Directory.Exists(AssetsDirectory)) { Console.WriteLine("Failed to find the Asset folder!?"); WaitForUserInputAndQuit(); return; }
        
        PackageCacheDirectory = Path.Combine(RootDirectory, "Library\\PackageCache");
        if (!Directory.Exists(PackageCacheDirectory)) { Console.WriteLine("Failed to find the PackageCache folder, have you opened the project in Unity yet?"); WaitForUserInputAndQuit(); return; }
        
        ExportedFilesDirectory = Path.Combine(AssetsDirectory, "!ExportedAssets");
        if (!Directory.Exists(ExportedFilesDirectory)) {
            List<string> exportedFiles = Directory.GetFileSystemEntries(AssetsDirectory).ToList();
            Directory.CreateDirectory(ExportedFilesDirectory);
            foreach (string exportedFile in exportedFiles) { Directory.Move(exportedFile, Path.Combine(ExportedFilesDirectory, Path.GetFileName(exportedFile))); }
        }
        
        ImportedFilesDirectory = Path.Combine(AssetsDirectory, "!ImportedAssets");
        if (!Directory.Exists(ImportedFilesDirectory)) {
            Directory.CreateDirectory(ImportedFilesDirectory);
        }
        else {
            List<string> importedFiles = Directory.GetFileSystemEntries(AssetsDirectory).ToList();
            importedFiles.Remove(ExportedFilesDirectory);
            importedFiles.Remove(ExportedFilesDirectory + ".meta");
            importedFiles.Remove(ImportedFilesDirectory);
            importedFiles.Remove(ImportedFilesDirectory + ".meta");
            foreach (string importedFile in importedFiles) { Directory.Move(importedFile, Path.Combine(ImportedFilesDirectory, Path.GetFileName(importedFile))); }
        }
    }

    public static void Main(string[] args)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        try
        {
            Console.WriteLine("STRAFTAT Auto Ripper");
            Console.WriteLine("Automagically create a working Unity project.");
            
            FindFilesAndFixStructure();
            Console.WriteLine("Import the 3rd party assets into the !ImportedAssets folder.");
            
            Console.WriteLine("Do you want to skip the PackageCache remapping? (y/n)");
            string input = Console.ReadLine() ?? "";
            if (input.Equals("y", StringComparison.CurrentCultureIgnoreCase)) { goto skipPackageCache; }
            
            Console.WriteLine("Starting Package Cache Remapping...");
            AutoGuidRemapper.RemapGuids(RootDirectory, ExportedFilesDirectory, PackageCacheDirectory, true, true);
            skipPackageCache:
            Console.WriteLine("Starting Asset Remapping...");
            AutoGuidRemapper.RemapGuids(RootDirectory, ExportedFilesDirectory, ImportedFilesDirectory, true, false);
            
            FolderRemover.RemoveFolders(RootDirectory);
            ScriptFixer.FixScripts(AssetsDirectory);
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