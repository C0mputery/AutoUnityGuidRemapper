# AutoUnityGuidRemapper
Simple little command line tool that is able to quickly remap thousands of referenced unity files, writen in C# utlizing multithreadng for the most performance.

Example:

AUTOREMAPPER "path/to/unityProjectDirectory" "path/to/unityProjectDirectory/Assets/!RippedAssets" "path/to/unityProjectDirectory/Assets/!ImportedAssets" true true

AUTOREMAPPER "path/to/unityProjectDirectory" "path/to/unityProjectDirectory/Assets/!RippedAssets" "path/to/unityProjectDirectory/Library/PackageCache" true true
