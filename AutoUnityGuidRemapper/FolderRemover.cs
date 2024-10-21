namespace AutoUnityGuidRemapper;

public class FolderRemover {
    private static readonly string[] foldersToRemove = [
    ];
    
    private static void RemoveFolders(string assetsDirectory) {
        foreach (string folder in foldersToRemove) {
            string path = Path.Combine(assetsDirectory, folder);
            if (Directory.Exists(path)) {
                Directory.Delete(path, true);
                string metaPath = path + ".meta";
                if (File.Exists(metaPath)) { File.Delete(metaPath); }
            }
        }
    }
}