using System.Runtime.InteropServices;
using UnityEngine;

public class DeviceCheck
{
    [DllImport("__Internal")]
    private static extern int IsMobileDevice();

    void Start()
    {
    }

    public bool IsMobile()
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
        bool isMobile = IsMobileDevice() == 1;
        Debug.Log("Is Mobile Device ? : " + isMobile);
        return isMobile;
        #else
        Debug.Log("This check works only in WebGL build.");
        return false;
        #endif
    }
}