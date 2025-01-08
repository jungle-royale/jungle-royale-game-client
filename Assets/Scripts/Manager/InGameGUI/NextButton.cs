using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class NextButton : MonoBehaviour
{
    void Start()
    {
        // QuitButton GameObject를 찾음 (버튼이 있는 객체)
        GameObject quitButtonObject = GameObject.Find("NextButton");

        if (quitButtonObject != null)
        {
            // Button 컴포넌트를 가져옴
            Button quitButton = quitButtonObject.GetComponent<Button>();

            if (quitButton != null)
            {
                // OnClick 이벤트에 메서드 추가
                quitButton.onClick.AddListener(() => WatchNext());
            }
            else
            {
                Debug.LogError("Button component is missing on NextButton GameObject.");
            }
        }
        else
        {
            Debug.LogError("NextButton GameObject not found.");
        }
    }

    public void WatchNext()
    {
        Debug.Log("Next Button");
        CameraManager cameraManager = GameObject.FindFirstObjectByType<CameraManager>();
        if (cameraManager == null)
        {
            Debug.LogError("CameraManager 못 찾음");
            return;
        }

        cameraManager.SwitchToNextPlayer();
    }

}
