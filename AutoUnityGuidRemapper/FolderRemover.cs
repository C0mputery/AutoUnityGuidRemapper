namespace AutoUnityGuidRemapper;

public class FolderRemover {
    private static readonly string[] foldersToRemove = [
        @"Assets/!ExportedAssets/Scripts/ColorBoxGroup",
        @"Assets/!ExportedAssets/Scripts/FishNet.CodeAnalysisAssets/!ExportedAssets/Scripts/FishNet.CodeAnalysis",
        @"Assets/!ExportedAssets/Scripts/FishNet.Demos",
        @"Assets/!ExportedAssets/Scripts/FishNet.Runtime",
        @"Assets/!ExportedAssets/Scripts/GameKit.Dependencies",
        @"Assets/!ExportedAssets/Scripts/GameKit.Utilities",
        @"Assets/!ExportedAssets/Scripts/LambdaTheDev.NetworkAudioSync",
        @"Assets/!ExportedAssets/Scripts/LambdaTheDev.NetworkAudioSync.Demo.FishNet",
        @"Assets/!ExportedAssets/Scripts/LambdaTheDev.NetworkAudioSync.FishNet",
        @"Assets/!ExportedAssets/Scripts/Newtonsoft.Json",
        @"Assets/!ExportedAssets/Scripts/Sirenix.OdinInspector.Attributes",
        @"Assets/!ExportedAssets/Scripts/Sirenix.OdinInspector.CompatibilityLayer",
        @"Assets/!ExportedAssets/Scripts/Sirenix.Serialization",
        @"Assets/!ExportedAssets/Scripts/Sirenix.Serialization.Config",
        @"Assets/!ExportedAssets/Scripts/Sirenix.Utilities",
        @"Assets/!ExportedAssets/Scripts/Unity.Animation.Rigging",
        @"Assets/!ExportedAssets/Scripts/Unity.Burst",
        @"Assets/!ExportedAssets/Scripts/Unity.Burst.Cecil",
        @"Assets/!ExportedAssets/Scripts/Unity.Burst.Cecil.Rocks",
        @"Assets/!ExportedAssets/Scripts/Unity.Burst.Unsafe",
        @"Assets/!ExportedAssets/Scripts/Unity.InputSystem",
        @"Assets/!ExportedAssets/Scripts/Unity.InputSystem.RebindingUI",
        @"Assets/!ExportedAssets/Scripts/Unity.Mathematics",
        @"Assets/!ExportedAssets/Scripts/Unity.Postprocessing.Runtime",
        @"Assets/!ExportedAssets/Scripts/Unity.RenderPipelines.Core.Runtime",
        @"Assets/!ExportedAssets/Scripts/Unity.RenderPipelines.Core.ShaderLibrary",
        @"Assets/!ExportedAssets/Scripts/Unity.RenderPipelines.ShaderGraph.ShaderGraphLibrary",
        @"Assets/!ExportedAssets/Scripts/Unity.Services.Core",
        @"Assets/!ExportedAssets/Scripts/Unity.Services.Core.Analytics",
        @"Assets/!ExportedAssets/Scripts/Unity.Services.Core.Configuration",
        @"Assets/!ExportedAssets/Scripts/Unity.Services.Core.Device",
        @"Assets/!ExportedAssets/Scripts/Unity.Services.Core.Environments",
        @"Assets/!ExportedAssets/Scripts/Unity.Services.Core.Environments.Internal",
        @"Assets/!ExportedAssets/Scripts/Unity.Services.Core.Internal",
        @"Assets/!ExportedAssets/Scripts/Unity.Services.Core.Networking",
        @"Assets/!ExportedAssets/Scripts/Unity.Services.Core.Registration",
        @"Assets/!ExportedAssets/Scripts/Unity.Services.Core.Scheduler",
        @"Assets/!ExportedAssets/Scripts/Unity.Services.Core.Telemetry",
        @"Assets/!ExportedAssets/Scripts/Unity.Services.Core.Threading",
        @"Assets/!ExportedAssets/Scripts/Unity.TextMeshPro",
        @"Assets/!ExportedAssets/Scripts/Unity.Timeline",
        @"Assets/!ExportedAssets/Scripts/Unity.VisualScripting.Antlr3.Runtime",
        @"Assets/!ExportedAssets/Scripts/Unity.VisualScripting.Core",
        @"Assets/!ExportedAssets/Scripts/Unity.VisualScripting.Flow",
        @"Assets/!ExportedAssets/Scripts/Unity.VisualScripting.State",
        @"Assets/!ExportedAssets/Plugins/Assembly-CSharp-firstpass/FishySteamworks",
        @"Assets/!ExportedAssets/Plugins/Assembly-CSharp-firstpass/FishNet",
    ];

    public static void RemoveFolders(string rootDirectory) {
        foreach (string folder in foldersToRemove) {
            string path = Path.Combine(rootDirectory, folder);
            if (!Directory.Exists(path)) { continue; }
            Directory.Delete(path, true);
            string metaPath = path + ".meta";
            if (File.Exists(metaPath)) { File.Delete(metaPath); }
        }
    }
}