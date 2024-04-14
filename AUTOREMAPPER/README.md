# AutoUnityGuidRemapper
Simple little command line tool that is able to quickly remap thousands of referenced unity files, writen in C# utlizing multithreadng for the most performance.

Example:

AUTOREMAPPER "C:/ContentWarning" "C:/ContentWarning/Assets/!RippedAssets" "C:/ContentWarning/Assets/!ImportedAssets" true true

AUTOREMAPPER "C:/ContentWarning" "C:/ContentWarning/Assets/!RippedAssets" "C:/ContentWarning/Library/PackageCache" true true
