using UnityEngine;
using UnityEngine.UI;

public class GuideToggleButton : MonoBehaviour
{
    private void OnEnable()
    {
        // 현재 활성화된 Canvas의 자식에서 GuideToggleButton 찾음
        Button[] closeButtons = GetComponentsInChildren<Button>(true);

        foreach (Button closeButton in closeButtons)
        {
            if (closeButton.name == "GuideToggleButton")
            {
                // 기존 리스너를 제거하고 새로 추가
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(OnToggleCanvas);
            }
        }
    }

    private void OnToggleCanvas()
    {
        // Debug.Log("닫아!!!!!!!!!!!");
        EventBus<InGameGUIEventType>.Publish(InGameGUIEventType.ToggleCanvas);
    }
}