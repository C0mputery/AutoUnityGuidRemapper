using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace AutoUnityGuidRemapper;

public static class ScriptFixer {
    public static readonly string[] ScriptsToAddTheOneUsingLineToo = [
        "HeathenEngineering.SteamworksIntegrationSteamInputManager",
        "HeathenEngineering.SteamworksIntegrationInputAction",
        "HeathenEngineering.SteamworksIntegrationInputActionEvent",
        "HeathenEngineering.SteamworksIntegrationInputActionSet",
        "HeathenEngineering.SteamworksIntegrationUGUIInputActionName",
        "HeathenEngineering.SteamworksIntegrationInputActionSetLayer",
        "HeathenEngineering.SteamworksIntegrationSteamworksEventTriggers",
        "HeathenEngineering.SteamworksIntegrationSteamSettings",
        "HeathenEngineering.SteamworksIntegrationInputActionSetData",
        "HeathenEngineering.SteamworksIntegrationInputActionName",
        "HeathenEngineering.SteamworksIntegrationInputActionGlyph",
    ];
    
    public static void FixScripts(string assetsDirectory) {
        string[] scriptFilePathsStrings = Directory.GetFiles(assetsDirectory, "*.cs", SearchOption.AllDirectories);
        Parallel.ForEach(scriptFilePathsStrings, PreFixScript);
        Parallel.ForEach(scriptFilePathsStrings, FixScript);
    }

    private static readonly ConcurrentDictionary<string, string> ScriptToClassName = new ConcurrentDictionary<string, string>();
    private static readonly ConcurrentDictionary<string, string[]> ClassToInheritorMap = new ConcurrentDictionary<string, string[]>();
    private static readonly Regex InheritorRegex = new Regex(@"(class|struct)\s+(\w+)\s*:\s*([^{]+)", RegexOptions.Compiled);
    private static readonly Regex NameSpaceRegex =  new Regex(@"namespace\s+([\w.]+)", RegexOptions.Compiled);
    
    private static void PreFixScript(string script) {
        Match classInheritorMatch = Match.Empty;
        Match namespaceMatch = Match.Empty;

        string namespaceName = "";
        string classKey = "";
        
        string tempFilePath = Path.GetTempFileName();
        bool updated = false;
        using (StreamReader reader = new StreamReader(script))
        using (StreamWriter writer = new StreamWriter(tempFilePath)) {
            while (!reader.EndOfStream) {
                string line = reader.ReadLine() ?? "";
                if (line.Contains("WriterPool.GetWriter()")) {
                    line = line.Replace("WriterPool.GetWriter()", "WriterPool.Retrieve()");
                    updated = true;
                }
                
                writer.WriteLine(line);

                if (classInheritorMatch.Success) {
                    if (!updated) { return; }
                    continue;
                }
                
                if (!namespaceMatch.Success) { 
                    namespaceMatch = NameSpaceRegex.Match(line);
                    if (!namespaceMatch.Success) { continue; }
                    namespaceName = namespaceMatch.Groups[1].Value;
                }
                
                classInheritorMatch = InheritorRegex.Match(line);
                if (classInheritorMatch.Success) {
                    string className = classInheritorMatch.Groups[2].Value;
                    string inheritorNameString = classInheritorMatch.Groups[3].Value;
                    string[] inheritorNames = inheritorNameString.Split(", ");

                    classKey = $"{namespaceName}{className}";
                    ClassToInheritorMap.TryAdd(classKey, inheritorNames);
                    ScriptToClassName.TryAdd(script, classKey);
                }
            }
        }
        
        if (updated) {
            File.Delete(script);
            File.Move(tempFilePath, script);
        }
        else {
            File.Delete(tempFilePath);
        }
    }
    
    private static void FixScript(string script) {
        if (!ScriptToClassName.TryGetValue(script, out string? className)) { return; }
        
        if (ScriptsToAddTheOneUsingLineToo.Contains(className)) {
            string tempFilePath = Path.GetTempFileName();
            using (StreamReader reader = new StreamReader(script))
            using (StreamWriter writer = new StreamWriter(tempFilePath)) {
                string? firstLine = reader.ReadLine();
                if (firstLine != null && !firstLine.Contains("using Input = HeathenEngineering.SteamworksIntegration.API.Input;")) {
                    writer.WriteLine("using Input = HeathenEngineering.SteamworksIntegration.API.Input;");
                    Console.WriteLine($"Added using line to {script}");
                }
                writer.WriteLine(firstLine);
                while (!reader.EndOfStream) { writer.WriteLine(reader.ReadLine()); }
            }
            File.Delete(script);
            File.Move(tempFilePath, script);
        }
    }
    
    private static bool RecursivelyCheckInheritor(string className, string inheritorName) {
        if (!ClassToInheritorMap.TryGetValue(className, out string[]? inheritorNames)) { return false; }
        return inheritorNames.Contains(inheritorName) || inheritorNames.Any(inheritor => RecursivelyCheckInheritor(inheritor, inheritorName));
    }
}