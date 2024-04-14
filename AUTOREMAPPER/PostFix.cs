namespace AUTOREMAPPER
{
    internal class PostFix
    {
        static readonly List<string> FilesToSaveRegex = new List<string> { "*DisableIntercept.cs", "*MicrophoneRelay.cs" };
        public static void SaveFilesFromBeingNuked(string AssetsFolderDirecotry)
        {
            IEnumerable<string> filesToNotNuke = FilesToSaveRegex.AsParallel().SelectMany(
                regex => Directory.EnumerateFiles(AssetsFolderDirecotry, regex, SearchOption.AllDirectories));
            foreach (string file in filesToNotNuke)
            {
                File.Move(file, Path.Combine(AssetsFolderDirecotry, Path.GetFileName(file)));
            }
        }

        static readonly List<string> scriptFolderToKeep = new List<string> { "Assembly-CSharp", "pworld", "Zorro.Core.Runtime", "Zorro.PhotonUtility", "Zorro.Recorder", "Zorro.Settings.Runtime", "Zorro.UI.Runtime" };
        static readonly List<string> FoldersToKeepInAssmblyCSharp = new List<string> { "DefaultNamespace" };
        public static void RemoveUnwantedScriptDirectorysAndFiles(string ScriptDirectory)
        {
            List<string> ScriptFiles = Directory.GetFiles(ScriptDirectory).ToList();
            foreach (string FileInScripts in ScriptFiles)
            {
                if (!scriptFolderToKeep.Contains(Path.GetFileNameWithoutExtension(FileInScripts)))
                {
                    File.Delete(FileInScripts);
                }
            }

            foreach (string DirectoryInAssemblyCSharp in Directory.GetDirectories(Path.Combine(ScriptDirectory, "Assembly-CSharp")))
            {
                if (Directory.Exists(DirectoryInAssemblyCSharp))
                {
                    string name = Path.GetDirectoryName(DirectoryInAssemblyCSharp)!;
                    if (!FoldersToKeepInAssmblyCSharp.Contains(name))
                    {
                        Directory.Delete(DirectoryInAssemblyCSharp, true);
                        string metaFile = DirectoryInAssemblyCSharp + ".meta";
                        if (File.Exists(metaFile))
                        {
                            File.Delete(metaFile);
                        }
                    }
                }
            }

        }

        public static void NukePluginsDirectorys(string PluginsDirectorys)
        {
            Directory.Delete(PluginsDirectorys, true);
        }

        static readonly List<string> FilesToRemoveRegex = new List<string> { 
            "*__JobReflectionRegistrationOutput*", "*UnitySourceGeneratedAssemblyMonoScriptTypes_v1.cs"
        };
        public static void NukeFilesWithRegex(string AssetsFolderDirecotry)
        {
            IEnumerable<string> filesToNuke = FilesToRemoveRegex.AsParallel().SelectMany(
                    regex => Directory.EnumerateFiles(AssetsFolderDirecotry, regex, SearchOption.AllDirectories));

            foreach (string file in filesToNuke)
            {
                Console.WriteLine("Nuking file: " + file);
                File.Delete(file);
            }
        }
    }
}


// Running list of files I fixed so that we can just replace the file with the fixed version
// BidirectionalNativeDictionary.cs 
// NativeHashedBookkeeper.cs
// CustomCommands.cs
// CustomCommandListener.cs
// PhotonNetworkPart.cs
// MainMenuHandler.cs
// Outlinable.cs

// Remove all the times, that this shows up
// GameObject IBudgetCost.get_gameObject()
// {
//     return base.gameObject;
// }