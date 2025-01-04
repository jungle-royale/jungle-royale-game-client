using UnityEditor;

public class WebGLBuildScript
{
    public static void BuildWebGL()
    {
        string buildPath = "Builds"; // WebGL 빌드 결과를 저장할 경로

        // 빌드 옵션 설정
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = new[] {
                "Assets/Scenes/InGameScene.unity",
            }, // 빌드할 씬 경로
            locationPathName = buildPath,
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        };

        // 빌드 실행
        BuildPipeline.BuildPlayer(buildPlayerOptions);

        // 빌드 완료 메시지
        UnityEngine.Debug.Log($"WebGL 빌드가 완료되었습니다: {buildPath}");
    }
}