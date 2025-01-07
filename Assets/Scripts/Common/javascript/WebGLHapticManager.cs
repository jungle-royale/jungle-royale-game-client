using System.Runtime.InteropServices;
using UnityEngine;

public class WebGLHapticManager
{
    // JavaScript 함수 연결
    [DllImport("__Internal")]
    private static extern void Vibrate(int duration); // ms

    public void TriggerHaptic(int duration)
    {
        // WebGL에서만 실행
        #if UNITY_WEBGL && !UNITY_EDITOR

        Vibrate(duration);

        #else

        Debug.Log("웹 아님~");

        #endif
    }
}