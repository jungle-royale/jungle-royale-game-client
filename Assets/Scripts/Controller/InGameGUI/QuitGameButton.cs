using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class QuitGameButton : MonoBehaviour
{
    void Start()
    {
        // QuitButton GameObject를 찾음 (버튼이 있는 객체)
        GameObject quitButtonObject = GameObject.Find("QuitButton");

        if (quitButtonObject != null)
        {
            // Button 컴포넌트를 가져옴
            Button quitButton = quitButtonObject.GetComponent<Button>();

            if (quitButton != null)
            {
                // OnClick 이벤트에 메서드 추가
                quitButton.onClick.AddListener(() => RedirectToURL());
            }
            else
            {
                Debug.LogError("Button component is missing on QuitButton GameObject.");
            }
        }
        else
        {
            Debug.LogError("QuitButton GameObject not found.");
        }
    }

    public void RedirectToURL()
    {
        Debug.Log("Quit Button");
        new RedirectHandler().RedirectToHome();
    }

}
