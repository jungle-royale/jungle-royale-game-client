using UnityEngine;
using TMPro;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;

public class InGameGUIManager : MonoBehaviour
{
    // FPS 계산
    public float updateInterval = 0.5f;
    private float timeSinceLastUpdate = 0.0f;
    private float deltaTime = 0.0f;

    private GameObject mainCanvas;
    private Transform watchModeTransform;

    private GameObject waitingRoomCanvas;
    private GameObject inGameCanvas;
    private GameObject gameOverCanvas;
    private GameObject gameEndCanvas;
    private GameObject gameWinCanvas;
    private GameObject errorCanvas;
    private GameObject descriptionCanvas;

    // MainCanvas
    [Header("MainCanvas Label")]
    public List<TextMeshProUGUI> pingLabel;
    public List<TextMeshProUGUI> fpsLabel;
    public List<TextMeshProUGUI> timerLabel;
    public List<TextMeshProUGUI> playerCountLabel;

    // WaitingRoomCanvas Child
    [Header("WaitingRoomCanvas Label")]
    public List<TextMeshProUGUI> gameCountDownLabel;
    public List<TextMeshProUGUI> minPlayerLabel;

    // InGameCanvas Child
    [Header("InGameCanvas Label")]
    public List<TextMeshProUGUI> hpLabel;
    public List<BulletBar> bulletBarLabel;

    // State Canvas
    [Header("State Canvas Label")]
    public List<TextMeshProUGUI> nickNameLabel;
    public List<TextMeshProUGUI> placementLabel;
    public List<TextMeshProUGUI> totalPlayerLabel;
    public List<TextMeshProUGUI> killCountLabel;
    public List<TextMeshProUGUI> pointLabel;

    // PlayerCanvas
    public TextMeshProUGUI userNameLabel;

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
        mainCanvas = GameObject.FindGameObjectWithTag("MainCanvas");
        watchModeTransform = mainCanvas.transform.Find("WatchMode");
        waitingRoomCanvas = InstantiateCanvas("Prefabs/UIs/WaitingRoomCanvas");
        inGameCanvas = InstantiateCanvas("Prefabs/UIs/InGameCanvas");
        gameOverCanvas = InstantiateCanvas("Prefabs/UIs/GameOverCanvas");
        gameEndCanvas = InstantiateCanvas("Prefabs/UIs/GameEndCanvas");
        gameWinCanvas = InstantiateCanvas("Prefabs/UIs/GameWinCanvas");
        errorCanvas = InstantiateCanvas("Prefabs/UIs/ErrorCanvas");
        descriptionCanvas = InstantiateCanvas("Prefabs/UIs/DescriptionCanvas");

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
        if (watchModeTransform != null) watchModeTransform.gameObject.SetActive(false);
        if (gameWinCanvas != null) gameWinCanvas.SetActive(false);
        if (errorCanvas != null) errorCanvas.SetActive(false);
        if (descriptionCanvas != null) descriptionCanvas.SetActive(false);
    }

    private void CacheCanvasComponents()
    {
        pingLabel = FindLabelsByTag("PingLabel");
        fpsLabel = FindLabelsByTag("FpsLabel");
        timerLabel = FindLabelsByTag("TimerLabel");
        playerCountLabel = FindLabelsByTag("PlayerCountLabel");

        gameCountDownLabel = FindLabelsByTag("GameCountDownLabel");
        minPlayerLabel = FindLabelsByTag("MinPlayerLabel");

        hpLabel = FindLabelsByTag("HpLabel");
        bulletBarLabel = FindBulletBarsByTag("BulletBar");

        nickNameLabel = FindLabelsByTag("NicknameLabel");
        placementLabel = FindLabelsByTag("PlacementLabel");
        totalPlayerLabel = FindLabelsByTag("TotalPlayerLabel");
        killCountLabel = FindLabelsByTag("KillCountLabel");
    }

    private List<BulletBar> FindBulletBarsByTag(string tag)
    {
        GameObject[] bulletBarObjects = GameObject.FindGameObjectsWithTag(tag);
        if (bulletBarObjects == null || bulletBarObjects.Length == 0)
        {
#if UNITY_EDITOR
            // Debug.Log($"'{tag}' 태그를 가진 객체를 찾을 수 없습니다.");
#endif
            return new List<BulletBar>(); // 빈 리스트 반환
        }

        List<BulletBar> bars = new List<BulletBar>();
        foreach (GameObject obj in bulletBarObjects)
        {
            BulletBar bar = obj.GetComponent<BulletBar>();
            if (bar != null)
            {
                bars.Add(bar);
            }
            else
            {
                Debug.Log($"'{obj.name}' 객체에 BulletBar 컴포넌트가 없습니다.");
            }
        }

        return bars;
    }

    private List<TextMeshProUGUI> FindLabelsByTag(string tag)
    {
        // 태그를 가진 모든 객체를 찾습니다.
        GameObject[] labelObjects = GameObject.FindGameObjectsWithTag(tag);
        if (labelObjects == null || labelObjects.Length == 0)
        {
#if UNITY_EDITOR
            // Debug.Log($"'{tag}' 태그를 가진 객체를 찾을 수 없습니다.");
#endif
            return new List<TextMeshProUGUI>(); // 빈 리스트 반환
        }

        // TextMeshProUGUI 컴포넌트를 가진 객체를 리스트로 저장
        List<TextMeshProUGUI> labels = new List<TextMeshProUGUI>();
        foreach (GameObject obj in labelObjects)
        {
            TextMeshProUGUI label = obj.GetComponent<TextMeshProUGUI>();
            if (label != null)
            {
                labels.Add(label);
            }
            else
            {
                Debug.Log($"'{obj.name}' 객체에 TextMeshProUGUI 컴포넌트가 없습니다.");
            }
        }

        return labels;
    }

    private void SubscribeToEvents()
    {
        EventBus<InGameGUIEventType>.Subscribe<string>(InGameGUIEventType.ActivateCanvas, OnActivateCanvas);
        EventBus<InGameGUIEventType>.Subscribe<int>(InGameGUIEventType.UpdateMinPlayerLabel, UpdateMinPlayerUI);
        EventBus<InGameGUIEventType>.Subscribe<int>(InGameGUIEventType.UpdateGameCountDownLabel, UpdateGameCountDownUI);
        EventBus<InGameGUIEventType>.Subscribe<long>(InGameGUIEventType.UpdatePingLabel, UpdatePingUI);
        EventBus<InGameGUIEventType>.Subscribe<int>(InGameGUIEventType.UpdateHpLabel, UpdateHpUI);
        EventBus<InGameGUIEventType>.Subscribe<int>(InGameGUIEventType.UpdatePlayerCountLabel, UpdatePlayerCountUI);
        EventBus<InGameGUIEventType>.Subscribe<int>(InGameGUIEventType.UpdateTimerLabel, UpdateTimerUI);
        EventBus<InGameGUIEventType>.Subscribe<StateUIDTO>(InGameGUIEventType.UpdateStateLabel, UpdateStateUI);
        EventBus<InGameGUIEventType>.Subscribe<int>(InGameGUIEventType.SetBulletBarLabel, SetBulletBarUI);
        EventBus<InGameGUIEventType>.Subscribe<int>(InGameGUIEventType.UpdateBulletBarLabel, UpdateBulletBarUI);
        EventBus<InGameGUIEventType>.Subscribe<PlayerUIDTO>(InGameGUIEventType.SetUserNameLabel, UpdateUserNameLabel);
        EventBus<InGameGUIEventType>.Subscribe(InGameGUIEventType.ToggleCanvas, OnToggleCanvas);
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        EventBus<InGameGUIEventType>.Unsubscribe<string>(InGameGUIEventType.ActivateCanvas, OnActivateCanvas);
        EventBus<InGameGUIEventType>.Unsubscribe<int>(InGameGUIEventType.UpdateGameCountDownLabel, UpdateGameCountDownUI);
        EventBus<InGameGUIEventType>.Unsubscribe<long>(InGameGUIEventType.UpdatePingLabel, UpdatePingUI);
        EventBus<InGameGUIEventType>.Unsubscribe<int>(InGameGUIEventType.UpdateHpLabel, UpdateHpUI);
        EventBus<InGameGUIEventType>.Unsubscribe<int>(InGameGUIEventType.UpdatePlayerCountLabel, UpdatePlayerCountUI);
        EventBus<InGameGUIEventType>.Unsubscribe<int>(InGameGUIEventType.UpdateTimerLabel, UpdateTimerUI);
        EventBus<InGameGUIEventType>.Unsubscribe<StateUIDTO>(InGameGUIEventType.UpdateStateLabel, UpdateStateUI);
        EventBus<InGameGUIEventType>.Unsubscribe<int>(InGameGUIEventType.SetBulletBarLabel, SetBulletBarUI);
        EventBus<InGameGUIEventType>.Unsubscribe<int>(InGameGUIEventType.UpdateBulletBarLabel, UpdateBulletBarUI);
        EventBus<InGameGUIEventType>.Unsubscribe<PlayerUIDTO>(InGameGUIEventType.SetUserNameLabel, UpdateUserNameLabel);
        EventBus<InGameGUIEventType>.Unsubscribe(InGameGUIEventType.ToggleCanvas, OnToggleCanvas);
    }

    private void OnActivateCanvas(string gameState)
    {

        if (IsAlreadyGameEnd())
        {
            return;
        }

        SetAllCanvasesInactive();

        switch (gameState)
        {
            case "GameWait":
                waitingRoomCanvas?.SetActive(true);

                if (waitingRoomCanvas != null)
                {
                    // 자식 객체 찾기
                    Transform mobileDesc = waitingRoomCanvas.transform.Find("Description_Mobile");
                    Transform pcDesc = waitingRoomCanvas.transform.Find("Description_PC");

                    if (new DeviceCheck().IsMobile())
                    {
                        if (mobileDesc != null) mobileDesc.gameObject.SetActive(true);
                        if (pcDesc != null) pcDesc.gameObject.SetActive(false);
                    }
                    else
                    {
                        if (mobileDesc != null) mobileDesc.gameObject.SetActive(false);
                        if (pcDesc != null) pcDesc.gameObject.SetActive(true);
                    }
                }
                break;
            case "GameStart":
                inGameCanvas?.SetActive(true);
                break;
            case "GameOver":
                mainCanvas?.SetActive(false);
                gameOverCanvas?.SetActive(true);
                break;
            case "WatchMode":
                mainCanvas?.SetActive(true);
                watchModeTransform?.gameObject.SetActive(true);
                break;
            case "GameWin":
                mainCanvas?.SetActive(false);
                gameOverCanvas?.SetActive(false);
                gameWinCanvas?.SetActive(true);
                break;
            case "GameEnd":
                mainCanvas?.SetActive(false);
                gameOverCanvas?.SetActive(false);
                gameEndCanvas?.SetActive(true);
                break;
            case "ErrorCanvas":
                mainCanvas?.SetActive(false);
                errorCanvas.SetActive(true);
                break;
            default:
                Debug.LogError($"Unknown game state: {gameState}");
                break;
        }

        CacheCanvasComponents();
    }

    private void OnToggleCanvas()
    {
        mainCanvas?.SetActive(!mainCanvas.activeSelf);
        descriptionCanvas?.SetActive(!descriptionCanvas.activeSelf);
    }

    private bool IsAlreadyGameEnd()
    {
        return gameWinCanvas?.activeSelf == true || gameEndCanvas?.activeSelf == true;
    }

    private void UpdateMinPlayerUI(int minPlayerNum)
    {
        if (minPlayerLabel != null)
        {
            foreach (var label in minPlayerLabel) // minPlayerLabel 리스트 순회
            {
                label.text = $"/{minPlayerNum:D3}";
            }
        }
    }

    private void UpdateGameCountDownUI(int gameCountDown)
    {
        if (gameCountDown <= 0)
        {
            return;
        }

        if (gameCountDownLabel != null)
        {
            foreach (var label in gameCountDownLabel) // gameCountDownLabel 리스트 순회
            {
                gameCountDown -= 1;
                label.text = $"{gameCountDown:D2}";
            }
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
            foreach (var label in pingLabel) // pingLabel 리스트 순회
            {
                label.text = $"Ping: {ping} ms";
            }
        }
    }

    private void UpdateTimerUI(int sec)
    {
        if (timerLabel != null)
        {
            foreach (var label in timerLabel) // timerLabel 리스트 순회
            {
                int min = sec / 60;
                int remainingSec = sec % 60;
                label.text = $"{min:D2}:{remainingSec:D2}";
            }
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

            if (fpsLabel != null)
            {
                foreach (var label in fpsLabel) // fpsLabel 리스트 순회
                {
                    label.text = $"FPS : {fps}";
                }
            }

            // 경과 시간 초기화
            timeSinceLastUpdate = 0.0f;
        }
    }

    private void UpdateHpUI(int hp)
    {
        if (hpLabel != null)
        {
            foreach (var label in hpLabel) // hpLabel 리스트 순회
            {
                label.text = $"{hp:D3}";
            }
        }
    }

    private void SetBulletBarUI(int bulletGage)
    {
        if (bulletBarLabel != null)
        {
            foreach (var bulletBar in bulletBarLabel)
            {
                bulletBar.SetMaxBulletGage(bulletGage); // 각 BulletBar에 대해 호출
            }
        }
    }

    private void UpdateBulletBarUI(int bulletGage)
    {
        if (bulletBarLabel != null)
        {
            foreach (var bulletBar in bulletBarLabel)
            {
                if (bulletBar.slider.maxValue != ClientManager.Instance.maxBulletGage)
                {
                    bulletBar.SetMaxBulletGage(ClientManager.Instance.maxBulletGage);
                }
                bulletBar.SetBulletGage(bulletGage); // 각 BulletBar에 대해 호출
                bulletBar.UpdateGradientColor();
            }
        }
    }

    private void UpdatePlayerCountUI(int playerCount)
    {
        if (playerCountLabel != null)
        {
            foreach (var label in playerCountLabel) // playerCountLabel 리스트 순회
            {
                label.text = $"{playerCount:D2}";
            }
        }
    }

    private void UpdateStateUI(StateUIDTO stateData)
    {
        if (nickNameLabel != null)
        {
            foreach (var label in nickNameLabel) // placementLabel 리스트 순회
            {
                label.text = $"{stateData.nickName}";
            }
        }
        if (placementLabel != null)
        {
            foreach (var label in placementLabel) // placementLabel 리스트 순회
            {
                label.text = $"#{stateData.placement}";
            }
        }

        if (totalPlayerLabel != null)
        {
            foreach (var label in totalPlayerLabel) // totalPlayerLabel 리스트 순회
            {
                label.text = $"/{stateData.totalPlayer}";
            }
        }

        if (killCountLabel != null)
        {
            foreach (var label in killCountLabel) // killCountLabel 리스트 순회
            {
                label.text = $"{stateData.killCount}";
            }
        }

        // if (pointLabel != null)
        // {
        //     foreach (var label in pointLabel) // pointLabel 리스트 순회
        //     {
        //         // label.text = stateData.point.ToString("D3");
        //     }
        // }
    }

    private void UpdateUserNameLabel(PlayerUIDTO playerUIdata)
    {
        // PlayerCanvas 찾기
        Transform playerCanvasTransform = playerUIdata.playerObj.transform.Find("PlayerCanvas");
        if (playerCanvasTransform == null)
        {
            Debug.LogError("PlayerCanvas를 찾을 수 없습니다.");
            return;
        }

        // UserNameLabel 찾기
        Transform userNameLabelTransform = playerCanvasTransform.Find("UserNameLabel");
        if (userNameLabelTransform == null)
        {
            Debug.LogError("UserNameLabel을 찾을 수 없습니다.");
            return;
        }

        // TextMeshProUGUI 컴포넌트 가져오기
        TextMeshProUGUI userNameLabel = userNameLabelTransform.GetComponent<TextMeshProUGUI>();
        if (userNameLabel == null)
        {
            Debug.LogError("UserNameLabel에 TextMeshProUGUI 컴포넌트가 없습니다.");
            return;
        }

        userNameLabel.text = playerUIdata.userName;
    }
}