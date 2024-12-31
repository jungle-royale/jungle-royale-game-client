using UnityEngine;
using UnityEngine.UI;

public class QuitGameButton : MonoBehaviour
{
    public void RedirectToURL()
    {
        // 현재 WebGL 게임이 실행 중인 URL 가져오기
        string url = Application.absoluteURL;

        // JavaScript를 호출해 URL 리다이렉트
        // Redirect(url);
        Debug.Log($"Redirect: {url}");
    }

    // [System.Runtime.InteropServices.DllImport("__Internal")]
    // private static extern void Redirect(string url);
}