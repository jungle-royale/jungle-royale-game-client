using UnityEngine;
using TMPro;

public class InGameGUIController : MonoBehaviour
{
    private TextMeshProUGUI pingLabel;

    void Start()
    {
        // Label 생성
        CreatePingLabel();

        // PingUpdated 이벤트 구독
        EventBus<InGameGUIEventType>.Subscribe<long>(InGameGUIEventType.PingUpdated, UpdatePingUI);
    }

    private void CreatePingLabel()
    {
        // Canvas 생성
        GameObject canvasObject = new GameObject("GUICanvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        // Ping Label 생성
        GameObject labelObject = new GameObject("PingLabel");
        labelObject.transform.SetParent(canvasObject.transform);

        // TextMeshPro 설정
        pingLabel = labelObject.AddComponent<TextMeshProUGUI>();
        pingLabel.fontSize = 24;
        pingLabel.alignment = TextAlignmentOptions.TopRight;
        pingLabel.rectTransform.anchorMin = new Vector2(1, 1);
        pingLabel.rectTransform.anchorMax = new Vector2(1, 1);
        pingLabel.rectTransform.pivot = new Vector2(1, 1);
        pingLabel.rectTransform.anchoredPosition = new Vector2(-10, -10);
        pingLabel.text = "Ping: -- ms";
    }

    private void UpdatePingUI(long ping)
    {
        pingLabel.text = $"Ping: {ping} ms";
        Debug.Log($"Ping Updated in UI: {ping}");
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        EventBus<InGameGUIEventType>.Unsubscribe<long>(InGameGUIEventType.PingUpdated, UpdatePingUI);
    }
}