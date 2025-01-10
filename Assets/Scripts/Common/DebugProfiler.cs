#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.Profiling;

public class DebugProfiler : MonoBehaviour
{
    private float deltaTime = 0.0f;
    private GUIStyle textStyle;
    private GUIStyle backgroundStyle;
    private long lastCollectedMemory; // 마지막으로 측정한 GC 메모리
    private int gcCount; // 가비지 컬렉션 횟수 추적

    private void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;

        // 현재 Mono 메모리 사용량
        long currentMemory = Profiler.GetMonoUsedSizeLong();

        // GC가 발생했는지 확인
        if (currentMemory < lastCollectedMemory)
        {
            gcCount++;
        }

        lastCollectedMemory = currentMemory;
    }

    private void OnGUI()
    {
        // 스타일 초기화 확인
        if (textStyle == null || backgroundStyle == null)
        {
            InitializeStyles();
        }

        // FPS 계산
        float fps = 1.0f / deltaTime;

        // Draw Call 및 Batch 정보
        int drawCalls = UnityEditor.UnityStats.drawCalls; // Draw Calls
        int batches = UnityEditor.UnityStats.batches;     // Batch Calls
        int triangles = UnityEditor.UnityStats.triangles; // Triangles
        int vertices = UnityEditor.UnityStats.vertices;   // Vertices

        // 메모리 사용량
        float totalAllocatedMemory = Profiler.GetTotalAllocatedMemoryLong() / (1024f * 1024f); // MB
        float totalReservedMemory = Profiler.GetTotalReservedMemoryLong() / (1024f * 1024f);   // MB
        float totalUnusedReservedMemory = Profiler.GetTotalUnusedReservedMemoryLong() / (1024f * 1024f); // MB
        float monoMemory = Profiler.GetMonoUsedSizeLong() / (1024f * 1024f); // MB

        // CPU 정보
        int processorCount = SystemInfo.processorCount;      // CPU 코어 수
        int processorFrequency = SystemInfo.processorFrequency; // CPU 주파수 (MHz)

        // 화면 크기 계산 (오른쪽 하단 배치)
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;
        float boxWidth = 350f;
        float boxHeight = 280f;
        float x = screenWidth - boxWidth - 10f; // 오른쪽에서 10픽셀 간격
        float y = screenHeight - boxHeight - 10f; // 아래에서 10픽셀 간격

        // 배경 박스
        GUI.Box(new Rect(x, y, boxWidth, boxHeight), GUIContent.none, backgroundStyle);

        // 텍스트 출력
        GUI.BeginGroup(new Rect(x, y, boxWidth, boxHeight));
        GUI.Label(new Rect(10, 10, boxWidth - 20, 20), $"FPS: {fps:0.0}", textStyle);
        GUI.Label(new Rect(10, 30, boxWidth - 20, 20), $"Draw Calls: {drawCalls}", textStyle);
        GUI.Label(new Rect(10, 50, boxWidth - 20, 20), $"Batches: {batches}", textStyle);
        GUI.Label(new Rect(10, 70, boxWidth - 20, 20), $"Triangles: {triangles}", textStyle);
        GUI.Label(new Rect(10, 90, boxWidth - 20, 20), $"Vertices: {vertices}", textStyle);
        GUI.Label(new Rect(10, 110, boxWidth - 20, 20), $"Allocated Memory: {totalAllocatedMemory:0.0} MB", textStyle);
        GUI.Label(new Rect(10, 130, boxWidth - 20, 20), $"Reserved Memory: {totalReservedMemory:0.0} MB", textStyle);
        GUI.Label(new Rect(10, 150, boxWidth - 20, 20), $"Unused Reserved: {totalUnusedReservedMemory:0.0} MB", textStyle);
        GUI.Label(new Rect(10, 170, boxWidth - 20, 20), $"Mono Memory: {monoMemory:0.0} MB", textStyle);
        GUI.Label(new Rect(10, 190, boxWidth - 20, 20), $"GC Count: {gcCount}", textStyle);
        GUI.Label(new Rect(10, 210, boxWidth - 20, 20), $"CPU Cores: {processorCount}", textStyle);
        GUI.Label(new Rect(10, 230, boxWidth - 20, 20), $"CPU Frequency: {processorFrequency} MHz", textStyle);
        GUI.EndGroup();
    }

    private void InitializeStyles()
    {
        // 텍스트 스타일 초기화
        textStyle = new GUIStyle
        {
            fontSize = 20, // 글자 크기 설정
            normal = { textColor = Color.white } // 텍스트 색상
        };

        // 배경 스타일 초기화
        backgroundStyle = new GUIStyle(GUI.skin.box)
        {
            normal =
            {
                background = MakeTexture(2, 2, new Color(0, 0, 0, 0.5f)) // 반투명 검정색 배경
            }
        };
    }

    // 반투명 배경 텍스처 생성
    private Texture2D MakeTexture(int width, int height, Color color)
    {
        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
        }

        Texture2D texture = new Texture2D(width, height);
        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }
}
#endif