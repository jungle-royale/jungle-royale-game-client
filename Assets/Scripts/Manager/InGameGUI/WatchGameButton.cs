using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class WatchGameButton : MonoBehaviour
{
    void Start()
    {
        // QuitButton GameObject를 찾음 (버튼이 있는 객체)
        GameObject watchButtonObject = GameObject.Find("WatchButton");

        if (watchButtonObject != null)
        {
            // Button 컴포넌트를 가져옴
            Button watchButton = watchButtonObject.GetComponent<Button>();

            if (watchButton != null)
            {
                // OnClick 이벤트에 메서드 추가
                watchButton.onClick.AddListener(() => OnWatchGame());
            }
            else
            {
                Debug.LogError("Button component is missing on Watch Button GameObject.");
            }
        }
        else
        {
            Debug.LogError("Watch Button GameObject not found.");
        }
    }

    public void OnWatchGame()
    {
        Debug.Log("Watch Button");
        EventBus<InGameGUIEventType>.Publish(InGameGUIEventType.ActivateCanvas, "WatchMode");
        EventBus<InputButtonEventType>.Publish(InputButtonEventType.ActivateTabKey);
    }

}
