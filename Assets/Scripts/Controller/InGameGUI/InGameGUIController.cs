using UnityEngine;
using TMPro;
using System.Xml.Serialization;

public class InGameGUIController : MonoBehaviour
{
    private TextMeshProUGUI pingLabel;
    private TextMeshProUGUI hpLabel;
    private TextMeshProUGUI playerCountLabel;
    GameObject canvasObject;

    void Start()
    {
        // Canvas 생성
        CreateCanvas();

        // Label 생성
        CreatePingLabel();
        CreateHpLabel();
        CreatePlayerCountLabel();

        // UpdatePingLabel 이벤트 구독
        EventBus<InGameGUIEventType>.Subscribe<long>(InGameGUIEventType.UpdatePingLabel, UpdatePingUI);
        EventBus<InGameGUIEventType>.Subscribe<int>(InGameGUIEventType.UpdateHpLabel, UpdateHpUI);
        EventBus<InGameGUIEventType>.Subscribe<int>(InGameGUIEventType.UpdatePlayerCountLabel, UpdatePlayerCountUI);
    }

    private void CreateCanvas()
    {
        canvasObject = new GameObject("GUICanvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
    }

    private void CreatePingLabel()
    {
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

    private void CreateHpLabel()
    {
        GameObject labelObject = new GameObject("HpLabel");
        labelObject.transform.SetParent(canvasObject.transform);

        // TextMeshPro 설정
        hpLabel = labelObject.AddComponent<TextMeshProUGUI>();
        hpLabel.fontSize = 24;
        hpLabel.alignment = TextAlignmentOptions.TopRight;
        hpLabel.rectTransform.anchorMin = new Vector2(1, 1);
        hpLabel.rectTransform.anchorMax = new Vector2(1, 1);
        hpLabel.rectTransform.pivot = new Vector2(1, 1);
        hpLabel.rectTransform.anchoredPosition = new Vector2(-10, -60);
        hpLabel.text = "HP: --";
    }

    private void CreatePlayerCountLabel()
    {
        GameObject labelObject = new GameObject("PlayerCountLabel");
        labelObject.transform.SetParent(canvasObject.transform);

        // TextMeshPro 설정
        playerCountLabel = labelObject.AddComponent<TextMeshProUGUI>();
        playerCountLabel.fontSize = 24;
        playerCountLabel.alignment = TextAlignmentOptions.TopRight;
        playerCountLabel.rectTransform.anchorMin = new Vector2(1, 1);
        playerCountLabel.rectTransform.anchorMax = new Vector2(1, 1);
        playerCountLabel.rectTransform.pivot = new Vector2(1, 1);
        playerCountLabel.rectTransform.anchoredPosition = new Vector2(-10, -120);
        playerCountLabel.text = "Player Count: --";
    }

    private void UpdatePingUI(long ping)
    {
        pingLabel.text = $"Ping: {ping} ms";
        // Debug.Log($"Ping Updated in UI: {ping}");
    }

    private void UpdateHpUI(int hp)
    {
        hpLabel.text = $"HP: {hp}";
        // Debug.Log($"HP Updated in UI: {hp}");
    }

    private void UpdatePlayerCountUI(int playerCount)
    {
        playerCountLabel.text = $"Player Count : {playerCount}";
    }

    private void UpdateGameCountDownUI(int gameCountDown)
    {

    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        EventBus<InGameGUIEventType>.Unsubscribe<long>(InGameGUIEventType.UpdatePingLabel, UpdatePingUI);
        EventBus<InGameGUIEventType>.Unsubscribe<int>(InGameGUIEventType.UpdateHpLabel, UpdateHpUI);
        EventBus<InGameGUIEventType>.Unsubscribe<int>(InGameGUIEventType.UpdatePlayerCountLabel, UpdatePlayerCountUI);
    }
}