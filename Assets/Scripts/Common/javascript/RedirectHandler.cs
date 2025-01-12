using System.Runtime.InteropServices;
using UnityEngine;

public class RedirectHandler
{
    [DllImport("__Internal")]
    private static extern void Redirect(string url);

    public void RedirectTo(string url)
    {
        Debug.Log("Redirect in WebGL: " + url);

        // WebGL에서만 실행
#if UNITY_WEBGL && !UNITY_EDITOR
        Redirect(url);
#else
        Debug.Log("웹 아님~" + url);
#endif
    }

    public void RedirectToHome()
    {

        string url = "http://eternalsnowman.com";
        new RedirectHandler().RedirectTo(url);
    }

    // public void RedirectToFailure(int code)
    // {
    //     string url = $"http://eternalsnowman.com/failure?code={code}";
    //     new RedirectHandler().RedirectTo(url);
    // }
}