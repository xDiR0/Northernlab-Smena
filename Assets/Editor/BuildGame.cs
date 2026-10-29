using UnityEditor;
using UnityEditor.Build.Reporting;
namespace NorthernLab {
public static class BuildGame {
 [MenuItem("NorthernLab/Build Windows x64")] public static void Windows(){Build(BuildTarget.StandaloneWindows64,"Builds/Windows/NorthernLab.exe");}
 [MenuItem("NorthernLab/Build Linux x64")] public static void Linux(){Build(BuildTarget.StandaloneLinux64,"Builds/Linux/NorthernLab");}
 static void Build(BuildTarget target,string path){var report=BuildPipeline.BuildPlayer(new[]{"Assets/Scenes/Station.unity"},path,target,BuildOptions.None);if(report.summary.result!=BuildResult.Succeeded)throw new System.Exception("Build failed: "+report.summary.result);}
}
}
