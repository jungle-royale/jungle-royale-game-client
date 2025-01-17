using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class NextButton : MonoBehaviour
{
    // public GameObject quitButtonObject;

    private void OnEnable()
    {
        // 현재 활성화된 Canvas의 자식에서 GuideToggleButton 찾음
        Button[] nextButtons = GetComponentsInChildren<Button>(true);

        foreach (Button nextButton in nextButtons)
        {
            if (nextButton.name == "NextButton")
            {
                // 기존 리스너를 제거하고 새로 추가
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(WatchNext);
            }
        }
    }

    // void Start()
    // {
    //     // QuitButton GameObject를 찾음 (버튼이 있는 객체)
    //     if (quitButtonObject == null)
    //         quitButtonObject = GameObject.Find("NextButton");

    //     if (quitButtonObject != null)
    //     {
    //         // Button 컴포넌트를 가져옴
    //         Button quitButton = quitButtonObject.GetComponent<Button>();

    //         if (quitButton != null)
    //         {
    //             // OnClick 이벤트에 메서드 추가
    //             quitButton.onClick.AddListener(() => WatchNext());
    //         }
    //         else
    //         {
    //             Debug.LogError("Button component is missing on NextButton GameObject.");
    //         }
    //     }
    //     else
    //     {
    //         Debug.LogError("NextButton GameObject not found.");
    //     }
    // }

    public void WatchNext()
    {
        // Debug.Log("Next Button 클릭");
        CameraManager cameraManager = GameObject.FindFirstObjectByType<CameraManager>();
        if (cameraManager == null)
        {
            Debug.LogError("CameraManager 못 찾음");
            return;
        }

        cameraManager.SwitchToNextPlayer();
    }

}
