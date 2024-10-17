using System.Diagnostics;

namespace AutoUnityGuidRemapper;

static class Program
{
    public static void WaitForUserInputAndQuit() { Console.WriteLine("Press any key to continue..."); Console.ReadKey(); Environment.Exit(0); }
    public static void WaitForUserInput() { Console.WriteLine("Press any key to continue..."); Console.ReadKey();}

    public static void FindFilesAndFixStucture() {
        string assetsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Assets");
        if (!Directory.Exists(assetsDirectory)) { Console.WriteLine("Failed to find the Asset folder!?"); WaitForUserInputAndQuit(); return; }

        string packageCacheDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Library\\PackageCache");
        if (!Directory.Exists(packageCacheDirectory)) { Console.WriteLine("Failed to find the PackageCache folder, have you opened the project in Unity yet?"); WaitForUserInputAndQuit(); return; }

        string importedFilesDirectory = Path.Combine(assetsDirectory, "!ImportedAssets");
        if (!Directory.Exists(importedFilesDirectory)) { Console.WriteLine("Failed to find the ImportedAssets folder make sure you've imported the 3rd party assets into the project."); WaitForUserInputAndQuit(); return; }

        string  exportedFilesDirectory = Path.Combine(assetsDirectory, "!ExportedAssets");
        if (!Directory.Exists(exportedFilesDirectory))
        {
            List<string> badFiles = Directory.GetFileSystemEntries(assetsDirectory).ToList();
            badFiles.Remove(importedFilesDirectory);
            badFiles.Remove(exportedFilesDirectory + ".meta");
            Directory.CreateDirectory(exportedFilesDirectory);
            foreach (string badFile in badFiles) { Directory.Move(badFile, Path.Combine(exportedFilesDirectory, Path.GetFileName(badFile))); }
        } 
    }

    public static void Main(string[] args)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        try
        {
            FindFilesAndFixStucture();
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