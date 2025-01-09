using UnityEngine;
using UnityEngine.UI;

public class QuitGameButton : MonoBehaviour
{
    private void OnEnable()
    {
        // 현재 활성화된 Canvas의 자식에서 QuitButton을 찾음
        Button[] quitButtons = GetComponentsInChildren<Button>(true);

        foreach (Button quitButton in quitButtons)
        {
            if (quitButton.name == "QuitButton")
            {
                // 기존 리스너를 제거하고 새로 추가
                quitButton.onClick.RemoveAllListeners();
                quitButton.onClick.AddListener(RedirectToURL);
            }
        }
    }

    private void RedirectToURL()
    {
        Debug.Log("Quit Button Clicked");
        new RedirectHandler().RedirectToHome();
    }
}