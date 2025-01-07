using System.Runtime.InteropServices;
using UnityEngine;

public class LoadingScreenRemover
{
    [DllImport("__Internal")]
    private static extern void RemoveLoadingScreen();

    public void Remove()
    {
        // WebGL에서만 실행
        #if UNITY_WEBGL && !UNITY_EDITOR
        RemoveLoadingScreen();
        #else
        Debug.Log("웹 아님~ 리무버!");
        #endif
    }
}