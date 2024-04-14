using AUTOREMAPPER;
using System.Diagnostics;

void waitForUserInput()
{
    Console.WriteLine("Press any key to continue...");
    Console.ReadKey();
}

Stopwatch stopwatch = new Stopwatch();
stopwatch.Start();

string UnityAssetFolderDirectory = Directory.GetCurrentDirectory();

string FFMPEGPath = Path.Combine(UnityAssetFolderDirectory, "ffmpeg.exe");
if (!File.Exists(FFMPEGPath)) { Console.WriteLine("Failed to find the file: " + FFMPEGPath); waitForUserInput(); return; }

string RootUnityProjectDirectory = Directory.GetParent(UnityAssetFolderDirectory)!.ToString();
if (!Directory.Exists(UnityAssetFolderDirectory)) { Console.WriteLine("Failed to find the directory: " + UnityAssetFolderDirectory); waitForUserInput(); return; }

string PackageCacheDirectory = Path.Combine(Directory.GetParent(UnityAssetFolderDirectory)!.ToString(), "Library\\PackageCache");
if (!Directory.Exists(PackageCacheDirectory)) { Console.WriteLine("Failed to find the directory: " + PackageCacheDirectory); waitForUserInput(); return; }

string ImportedFilesDirectory = Path.Combine(UnityAssetFolderDirectory, "!ImportedFiles");
if (!Directory.Exists(ImportedFilesDirectory)) { Console.WriteLine("Failed to find the directory: " + ImportedFilesDirectory); waitForUserInput(); return; }

string ExportedFilesDirectory = Path.Combine(UnityAssetFolderDirectory, "!ExportedFiles");
if (!Directory.Exists(ExportedFilesDirectory))
{
    Directory.CreateDirectory(ExportedFilesDirectory);
    List<string> BadFileDirectorys = Directory.GetDirectories(UnityAssetFolderDirectory).ToList();
    BadFileDirectorys.Remove(ImportedFilesDirectory);
    foreach (string BadFileDirectory in BadFileDirectorys)
    {
        Directory.Move(BadFileDirectory, ExportedFilesDirectory);
    }
    foreach (string BadFile in Directory.GetFiles(UnityAssetFolderDirectory))
    {
        File.Move(BadFile, Path.Combine(ExportedFilesDirectory, Path.GetFileName(BadFile)));
    }
} // Move ripped files to the !ExportedFiles directory

string ExportedAudioClipDirectory = Path.Combine(ExportedFilesDirectory, "AudioClips");
if (!Directory.Exists(ExportedAudioClipDirectory)) { Console.WriteLine("Failed to find the directory: " + ExportedAudioClipDirectory); waitForUserInput(); return; }

Remapper.RemapGuids(RootUnityProjectDirectory, ExportedFilesDirectory, ImportedFilesDirectory, true, true);
Remapper.RemapGuids(RootUnityProjectDirectory, ExportedFilesDirectory, PackageCacheDirectory, true, true);

List<string> BadWavFiles = Directory.GetFiles(ExportedAudioClipDirectory, "*.wav").ToList();
Process.Start("CMD.exe", $"for /f \"tokens=1 delims=.\" %a in ('dir /B *.wav') do {FFMPEGPath} -i \"%a.wav\" \"!%a.wav\"").WaitForExit();
foreach (string BadWavFile in BadWavFiles) { File.Delete(BadWavFile); }
List<string> GoodWavFiles = Directory.GetFiles(ExportedAudioClipDirectory, "*.wav").ToList();
foreach (string GoodWavFile in GoodWavFiles) { File.Move(GoodWavFile, GoodWavFile.Replace("!", "")); }

stopwatch.Stop();
Console.WriteLine("Total time elapsed: " + stopwatch.Elapsed);