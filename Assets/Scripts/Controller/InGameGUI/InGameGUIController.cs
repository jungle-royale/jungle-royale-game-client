using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.Xml.Serialization;
using System;

public class InGameGUIController : MonoBehaviour
{
    private GameObject waitingRoomCanvas;
    private GameObject inGameCanvas;
    private GameObject gameOverCanvas;

    private TextMeshProUGUI pingLabel;
    private TextMeshProUGUI hpLabel;
    private TextMeshProUGUI playerCountLabel;

    void Start()
    {
        // 캔버스 생성 및 초기화
        CreateCanvases();

        // 이벤트 구독
        EventBus<InGameGUIEventType>.Subscribe<string>(InGameGUIEventType.ActivateCanvas, OnActivateCanvas);

        // UpdatePingLabel 이벤트 구독
        EventBus<InGameGUIEventType>.Subscribe<long>(InGameGUIEventType.UpdatePingLabel, UpdatePingUI);
        EventBus<InGameGUIEventType>.Subscribe<int>(InGameGUIEventType.UpdateHpLabel, UpdateHpUI);
        EventBus<InGameGUIEventType>.Subscribe<int>(InGameGUIEventType.UpdatePlayerCountLabel, UpdatePlayerCountUI);


        // 초기 상태 설정 (Waiting Room 활성화)
        OnActivateCanvas("GameWait");
    }

    private void CreateCanvases()
    {
        waitingRoomCanvas = InstantiateCanvas("Prefabs/UI/WaitingRoomCanvas");
        inGameCanvas = InstantiateCanvas("Prefabs/UI/InGameCanvas");
        gameOverCanvas = InstantiateCanvas("Prefabs/UI/GameOverCanvas");


        // 초기 상태에서 모든 캔버스 비활성화
        waitingRoomCanvas.SetActive(false);
        inGameCanvas.SetActive(false);
        gameOverCanvas.SetActive(false);
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

    private void OnActivateCanvas(string GameState)
    {

        // 모든 캔버스를 비활성화
        waitingRoomCanvas.SetActive(false);
        inGameCanvas.SetActive(false);
        gameOverCanvas.SetActive(false);

        if (GameState == "GameWait")
        {
            waitingRoomCanvas.SetActive(true);
        }
        else if (GameState == "GameStart")
        {
            inGameCanvas.SetActive(true);
        }
        else if (GameState == "GameOver")
        {
            gameOverCanvas.SetActive(true);
        }
        else
        {
            Debug.LogError("Null Canvas");
        }
    }

    private void CreateWaitingRoomCanvas()
    {
        GameObject canvasPrefab = Resources.Load<GameObject>("Prefabs/UI/InGameCanvas");
        if (canvasPrefab != null)
        {
            GameObject canvasObject = Instantiate(canvasPrefab);
        }
        else
        {
            Debug.LogError("Canvas Prefab could not be loaded.");
        }
    }

    private void CreateInGameCanvas()
    {
        GameObject canvasPrefab = Resources.Load<GameObject>("Prefabs/UI/InGameCanvas");
        if (canvasPrefab != null)
        {
            GameObject canvasObject = Instantiate(canvasPrefab);
        }
        else
        {
            Debug.LogError("Canvas Prefab could not be loaded.");
        }
    }

    private void CreateGameOverCanvas(Player player)
    {
        GameObject canvasPrefab = Resources.Load<GameObject>("Prefabs/UI/GameOverCanvas");
        if (canvasPrefab != null)
        {
            GameObject canvasObject = Instantiate(canvasPrefab);
        }
        else
        {
            Debug.LogError("Canvas Prefab could not be loaded.");
        }
    }

    private void UpdatePingUI(long ping)
    {
        if (pingLabel != null)
            pingLabel.text = $"Ping: {ping} ms";
        // Debug.Log($"Ping Updated in UI: {ping}");
    }

    private void UpdateHpUI(int hp)
    {
        if (hpLabel != null)
            hpLabel.text = $"HP: {hp}";
        // Debug.Log($"HP Updated in UI: {hp}");
    }

    private void UpdatePlayerCountUI(int playerCount)
    {
        if (playerCountLabel != null)
            playerCountLabel.text = $"Player Count : {playerCount}";
    }

    private void UpdateGameCountDownUI(int gameCountDown)
    {
        playerCountLabel = waitingRoomCanvas.GetComponentInChildren<TextMeshProUGUI>();
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        EventBus<InGameGUIEventType>.Unsubscribe<long>(InGameGUIEventType.UpdatePingLabel, UpdatePingUI);
        EventBus<InGameGUIEventType>.Unsubscribe<int>(InGameGUIEventType.UpdateHpLabel, UpdateHpUI);
        EventBus<InGameGUIEventType>.Unsubscribe<int>(InGameGUIEventType.UpdatePlayerCountLabel, UpdatePlayerCountUI);
    }
}