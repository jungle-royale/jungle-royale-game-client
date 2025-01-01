using UnityEngine;
using UnityEngine.UI;

public class QuitGameButton : MonoBehaviour
{
    public void RedirectToURL()
    {
        // 현재 WebGL 게임이 실행 중인 URL 가져오기
        new RedirectHandler().RedirectToHome();
    }

}