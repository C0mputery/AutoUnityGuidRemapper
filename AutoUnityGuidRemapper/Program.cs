using System.Diagnostics;

namespace AutoUnityGuidRemapper;

static class Program
{
    private static void WaitForUserInputAndQuit() { Console.WriteLine("Press any key to continue..."); Console.ReadKey(); Environment.Exit(0); }
    private static void WaitForUserInput() { Console.WriteLine("Press any key to continue..."); Console.ReadKey();}
    
    private static void FindFilesAndFixStructure() {
        Console.WriteLine("STRAFTAT Auto Ripper");
        Console.WriteLine("Automagically create a working Unity project.");
        Console.WriteLine("What is the root directory of the Unity project?");
        string? rootDirectory = Console.ReadLine();
        if (rootDirectory == null || !Directory.Exists(rootDirectory)) { Console.WriteLine("The directory does not exist!"); WaitForUserInputAndQuit(); return; }
        
        string assetsDirectory = Path.Combine(rootDirectory, "Assets");
        if (!Directory.Exists(assetsDirectory)) { Console.WriteLine("Failed to find the Asset folder!?"); WaitForUserInputAndQuit(); return; }
        
        string packageCacheDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Library\\PackageCache");
        if (!Directory.Exists(packageCacheDirectory)) { Console.WriteLine("Failed to find the PackageCache folder, have you opened the project in Unity yet?"); WaitForUserInputAndQuit(); return; }
        
        string exportedFilesDirectory = Path.Combine(assetsDirectory, "!ExportedAssets");
        if (!Directory.Exists(exportedFilesDirectory)) {
            List<string> exportedFiles = Directory.GetFileSystemEntries(assetsDirectory).ToList();
            Directory.CreateDirectory(exportedFilesDirectory);
            foreach (string exportedFile in exportedFiles) { Directory.Move(exportedFile, Path.Combine(exportedFilesDirectory, Path.GetFileName(exportedFile))); }
        }
        
        string importedFilesDirectory = Path.Combine(assetsDirectory, "!ImportedAssets");
        if (!Directory.Exists(importedFilesDirectory)) {
            Directory.CreateDirectory(importedFilesDirectory);
        }
    }

    public static void Main(string[] args)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        try
        {
            FindFilesAndFixStructure();
            Console.WriteLine("Import the 3rd party assets into the !ImportedAssets folder.");
            WaitForUserInput();
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