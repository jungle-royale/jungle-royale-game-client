using UnityEngine;

public class PlatformDetector : MonoBehaviour
{
    public bool isMobile;

    void Start()
    {
        // 간단한 모바일/데스크톱 구분 (WebGL 환경)
        isMobile = DetectMobile();
        Debug.Log("Is Mobile: " + isMobile);
    }

    bool DetectMobile()
    {
        // 화면 크기 기준
        if (Screen.width < 800 || Screen.height < 600)
        {
            return true;
        }

        // User-Agent 확인 (WebGL 빌드에서만 작동)
        #if UNITY_WEBGL
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            return SystemInfo.deviceType == DeviceType.Handheld;
        }
        #endif

        return false; // 기본값: 데스크톱
    }
}