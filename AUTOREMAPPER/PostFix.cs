﻿namespace AUTOREMAPPER
{
    internal class PostFix
    {
        static readonly List<string> FilesToSave = new List<string> { "*DisableIntercept.cs", "*MicrophoneRelay.cs", "*NetworkStatistics.cs" }; // Regex
        static readonly List<string> ScriptDirectorysToKeep = new List<string> { "Assembly-CSharp", "pworld", "Zorro.Core.Runtime", "Zorro.PhotonUtility", "Zorro.Recorder", "Zorro.Settings.Runtime", "Zorro.UI.Runtime" }; // Directory Name
        static readonly List<string> AssemblyCSharpDirectorysToKeep = new List<string> { "DefaultNamespace" }; // Directory Name
        static readonly List<string> UnwantedFilesToRemove = new List<string> { 
            "*__JobReflectionRegistrationOutput*", "*UnitySourceGeneratedAssemblyMonoScriptTypes_v1*", "*bleedModeParameter.cs", "*BlendModeParameter.cs",
            "*FisheyeTypeParameter.cs", "*maskChannelModeParameter.cs", "*preLParameter.cs", "*resModeParameter.cs", "*Vector2IntParameter.cs", "*VignetteModeParameter.cs",
            "*WarpModeParameter.cs", "*BleedMode.cs", "*FisheyeTypeEnum.cs", 
        }; // Regex

        internal static void SaveFilesNeededFiles(string AssetsFolderDirecotry)
        {
            string neededFilesFolder = Path.Combine(AssetsFolderDirecotry, "NeededFiles");
            Directory.CreateDirectory(neededFilesFolder);
            IEnumerable<string> filesToSave = FilesToSave.AsParallel().SelectMany(regex => Directory.EnumerateFiles(AssetsFolderDirecotry, regex, SearchOption.AllDirectories));
            foreach (string file in filesToSave) { File.Move(file, Path.Combine(neededFilesFolder, Path.GetFileName(file))); }
        }

        internal static void ScriptDirectoryFix(string ScriptDirectory)
        {
            foreach (string DirectoryInScripts in Directory.GetDirectories(ScriptDirectory))
            {
                if (!Directory.Exists(DirectoryInScripts)) { continue; }

                string directoryName = Path.GetFileName(DirectoryInScripts)!;
                if (!ScriptDirectorysToKeep.Contains(directoryName))
                {
                    Directory.Delete(DirectoryInScripts, true);
                    string metaFile = DirectoryInScripts + ".meta";
                    if (File.Exists(metaFile)) { File.Delete(metaFile); }
                }
            }
        }

        internal static void AssemblyCSharpFix(string AssemblyCSharpDirectory)
        {
            foreach (string DirectoryInAssemblyCSharp in Directory.GetDirectories(AssemblyCSharpDirectory))
            {
                if (!Directory.Exists(DirectoryInAssemblyCSharp)) { continue; }

                string directoryName = Path.GetFileName(DirectoryInAssemblyCSharp)!;
                if (!AssemblyCSharpDirectorysToKeep.Contains(directoryName))
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

        internal static void PluginDirectoryFix(string PluginsDirectorys)
        {
            Directory.Delete(PluginsDirectorys, true);
        }

        internal static void RemoveUnwantedFiles(string AssetsFolderDirecotry)
        {
            IEnumerable<string> filesToNuke = UnwantedFilesToRemove.AsParallel().SelectMany(
                    regex => Directory.EnumerateFiles(AssetsFolderDirecotry, regex, SearchOption.AllDirectories));

            foreach (string file in filesToNuke)
            {
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