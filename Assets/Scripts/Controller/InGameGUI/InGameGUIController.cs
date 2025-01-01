using UnityEngine;
using TMPro;
using System;

public class InGameGUIController : MonoBehaviour
{
    private GameObject waitingRoomCanvas;
    private GameObject inGameCanvas;
    private GameObject gameOverCanvas;

    // WaitingRoomCanvas Child
    private TextMeshProUGUI gameCountDownLabel;

    // InGameCanvas Child
    private TextMeshProUGUI pingLabel;
    private TextMeshProUGUI hpLabel;
    private TextMeshProUGUI playerCountLabel;

    // GameOverCanvas Child
    private TextMeshProUGUI placementLabel;
    private TextMeshProUGUI rankLabel;

    void Start()
    {
        // 캔버스 생성 및 초기화
        CreateCanvases();

        // 이벤트 구독
        SubscribeToEvents();

        // 초기 상태 설정 (Waiting Room 활성화)
        OnActivateCanvas("GameWait");
    }

    private void CreateCanvases()
    {
        waitingRoomCanvas = InstantiateCanvas("Prefabs/UI/WaitingRoomCanvas");
        inGameCanvas = InstantiateCanvas("Prefabs/UI/InGameCanvas");
        gameOverCanvas = InstantiateCanvas("Prefabs/UI/GameOverCanvas");

        // 모든 캔버스 비활성화
        SetAllCanvasesInactive();

        // UI 요소 캐싱
        CacheCanvasComponents();
    }

    private GameObject InstantiateCanvas(string prefabPath)
    {
        GameObject canvasPrefab = Resources.Load<GameObject>(prefabPath);
        if (canvasPrefab != null)
        {
            GameObject canvasObject = Instantiate(canvasPrefab);
            canvasObject.SetActive(false); // 기본적으로 비활성화
            return canvasObject;
        }
        else
        {
            Debug.LogError($"Canvas Prefab at {prefabPath} could not be loaded.");
            return null;
        }
    }

    private void SetAllCanvasesInactive()
    {
        if (waitingRoomCanvas != null) waitingRoomCanvas.SetActive(false);
        if (inGameCanvas != null) inGameCanvas.SetActive(false);
        if (gameOverCanvas != null) gameOverCanvas.SetActive(false);
    }

    private void CacheCanvasComponents()
    {
        if (waitingRoomCanvas != null)
        {
            gameCountDownLabel = FindTextInCanvas(waitingRoomCanvas, "GameCountDownLabel");
        }

        if (inGameCanvas != null)
        {
            pingLabel = FindTextInCanvas(inGameCanvas, "PingLabel");
            hpLabel = FindTextInCanvas(inGameCanvas, "HpLabel");
            playerCountLabel = FindTextInCanvas(inGameCanvas, "PlayerCountLabel");
        }

        if (gameOverCanvas != null)
        {
            // placementLabel = FindTextInCanvas(gameOverCanvas, "PlacementLabel");
            // rankLabel = FindTextInCanvas(gameOverCanvas, "RankLabel");
        }
    }

    private TextMeshProUGUI FindTextInCanvas(GameObject canvas, string childName)
    {
        Transform childTransform = canvas.transform.Find(childName);
        if (childTransform == null)
        {
            Debug.LogError($"'{childName}' 이름을 가진 객체를 {canvas.name}에서 찾을 수 없습니다.");
            return null;
        }

        TextMeshProUGUI label = childTransform.GetComponent<TextMeshProUGUI>();
        if (label == null)
        {
            Debug.LogError($"'{childName}'에 TextMeshProUGUI 컴포넌트가 없습니다.");
        }

        return label;
    }

    private void SubscribeToEvents()
    {
        EventBus<InGameGUIEventType>.Subscribe<string>(InGameGUIEventType.ActivateCanvas, OnActivateCanvas);
        EventBus<InGameGUIEventType>.Subscribe<int>(InGameGUIEventType.UpdateGameCountDownLabel, UpdateGameCountDownUI);
        EventBus<InGameGUIEventType>.Subscribe<long>(InGameGUIEventType.UpdatePingLabel, UpdatePingUI);
        EventBus<InGameGUIEventType>.Subscribe<int>(InGameGUIEventType.UpdateHpLabel, UpdateHpUI);
        EventBus<InGameGUIEventType>.Subscribe<int>(InGameGUIEventType.UpdatePlayerCountLabel, UpdatePlayerCountUI);
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        EventBus<InGameGUIEventType>.Unsubscribe<string>(InGameGUIEventType.ActivateCanvas, OnActivateCanvas);
        EventBus<InGameGUIEventType>.Unsubscribe<int>(InGameGUIEventType.UpdateGameCountDownLabel, UpdateGameCountDownUI);
        EventBus<InGameGUIEventType>.Unsubscribe<long>(InGameGUIEventType.UpdatePingLabel, UpdatePingUI);
        EventBus<InGameGUIEventType>.Unsubscribe<int>(InGameGUIEventType.UpdateHpLabel, UpdateHpUI);
        EventBus<InGameGUIEventType>.Unsubscribe<int>(InGameGUIEventType.UpdatePlayerCountLabel, UpdatePlayerCountUI);
    }

    private void OnActivateCanvas(string gameState)
    {
        SetAllCanvasesInactive();

        switch (gameState)
        {
            case "GameWait":
                waitingRoomCanvas?.SetActive(true);
                break;
            case "GameStart":
                inGameCanvas?.SetActive(true);
                break;
            case "GameOver":
                gameOverCanvas?.SetActive(true);
                break;
            default:
                Debug.LogError($"Unknown game state: {gameState}");
                break;
        }
    }

    private void UpdateGameCountDownUI(int gameCountDown)
    {
        if (waitingRoomCanvas != null && waitingRoomCanvas.activeSelf && gameCountDownLabel != null)
        {
            gameCountDownLabel.text = $"Game Start In {gameCountDown}";
        }
        else
        {
            Debug.LogError("WaitingRoomCanvas 또는 GameCountDownLabel이 비활성화 상태입니다.");
        }
    }

    private void UpdatePingUI(long ping)
    {
        if (inGameCanvas != null && inGameCanvas.activeSelf && pingLabel != null)
        {
            pingLabel.text = $"Ping: {ping} ms";
        }
    }

    private void UpdateHpUI(int hp)
    {
        if (inGameCanvas != null && inGameCanvas.activeSelf && hpLabel != null)
        {
            hpLabel.text = $"HP: {hp}";
        }
    }

    private void UpdatePlayerCountUI(int playerCount)
    {
        if (inGameCanvas != null && inGameCanvas.activeSelf && playerCountLabel != null)
        {
            playerCountLabel.text = $"Player Count: {playerCount}";
        }
    }
}