using UnityEngine;
using TMPro;
using System;

public class InGameGUIManager : MonoBehaviour
{
    // FPS 계산
    public float updateInterval = 0.5f;
    private float timeSinceLastUpdate = 0.0f;
    private float deltaTime = 0.0f;

    private GameObject waitingRoomCanvas;
    private GameObject inGameCanvas;
    private GameObject gameOverCanvas;
    private GameObject gameEndCanvas;
    private GameObject watchModeCanvas;

    // MainCanvas
    private TextMeshProUGUI pingLabel;
    private TextMeshProUGUI fpsLabel;


    // WaitingRoomCanvas Child
    private TextMeshProUGUI gameCountDownLabel;

    // InGameCanvas Child
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

    void Update()
    {
        UpdateFpsUI();
    }

    private void CreateCanvases()
    {
        waitingRoomCanvas = InstantiateCanvas("Prefabs/UIs/WaitingRoomCanvas");
        inGameCanvas = InstantiateCanvas("Prefabs/UIs/InGameCanvas");
        gameOverCanvas = InstantiateCanvas("Prefabs/UIs/GameOverCanvas");
        gameEndCanvas = InstantiateCanvas("Prefabs/UIs/GameEndCanvas");
        watchModeCanvas = InstantiateCanvas("Prefabs/UIs/WatchModeCanvas");

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
        if (gameEndCanvas != null) gameEndCanvas.SetActive(false);
        if (watchModeCanvas != null) watchModeCanvas.SetActive(false);
    }

    private void CacheCanvasComponents()
    {
        if (GameObject.FindWithTag("MainCanvas") != null)
        {
            pingLabel = FindLabelByTag("PingLabel");
            fpsLabel = FindLabelByTag("FpsLabel");
        }

        if (waitingRoomCanvas != null)
        {
            gameCountDownLabel = FindTextInCanvas(waitingRoomCanvas, "GameCountDownLabel");
        }

        if (inGameCanvas != null)
        {
            hpLabel = FindTextInCanvas(inGameCanvas, "HpLabel");
            playerCountLabel = FindTextInCanvas(inGameCanvas, "PlayerCountLabel");
        }

        if (gameOverCanvas != null)
        {
            // placementLabel = FindTextInCanvas(gameOverCanvas, "PlacementLabel");
            // rankLabel = FindTextInCanvas(gameOverCanvas, "RankLabel");
        }

        if (gameEndCanvas != null)
        {

        }

        if (watchModeCanvas != null)
        {

        }
    }

    private TextMeshProUGUI FindLabelByTag(string tag)
    {
        GameObject labelObject = GameObject.FindWithTag(tag);
        if (labelObject == null)
        {
            Debug.LogError($"'{tag}' 태그를 가진 객체를 찾을 수 없습니다.");
            return null;
        }

        TextMeshProUGUI label = labelObject.GetComponent<TextMeshProUGUI>();
        if (label == null)
        {
            Debug.LogError($"'{tag}' 태그를 가진 객체에 TextMeshProUGUI 컴포넌트가 없습니다.");
            return null;
        }

        return label;
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
            case "GameEnd":
                gameEndCanvas?.SetActive(true);
                break;
            case "WatchMode":
                watchModeCanvas?.SetActive(true);
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
        if (pingLabel != null)
        {
            pingLabel.text = $"Ping: {ping} ms";
        }
    }

    private void UpdateFpsUI()
    {
        // 경과 시간 누적
        timeSinceLastUpdate += Time.unscaledDeltaTime;
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;

        // 설정한 업데이트 간격이 지났을 때만 FPS 계산 및 표시
        if (timeSinceLastUpdate >= updateInterval)
        {
            int fps = Mathf.CeilToInt(1.0f / deltaTime);

            // FPS 값을 화면에 표시
            if (fpsLabel != null)
            {
                fpsLabel.text = $"FPS: {fps} ms";
            }

            // 경과 시간 초기화
            timeSinceLastUpdate = 0.0f;
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