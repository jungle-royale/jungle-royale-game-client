using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Message;
using System;

public class GameStateManager : Singleton<GameStateManager>
{

    private DateTime _sessionStartTime;

    private bool _gameStart = false;

    void Start()
    {
        _sessionStartTime = DateTime.Now;
        Debug.Log("게임 시작 : " + _sessionStartTime);

        // AudioManager를 통해 BackgroundBGM 재생
        AudioManager.Instance.PlayBGM("WaitingRoomBGM");
    }

    void Update()
    {

    }

    private void OnApplicationQuit()
    {
        var _sessionEndTime = DateTime.Now;
        Debug.Log("게임 종료 : " + DateTime.Now);
        Debug.Log("게임 플레이 시간 : " + _sessionEndTime.Subtract(_sessionStartTime));
    }


    public void HandleGameStart(GameStart gameStart)
    {
        // Debug.Log(gameStart.MapLength);
        _gameStart = true;
        AudioManager.Instance.PlaySfx(AudioManager.Sfx.GameStart);
        EventBus<InGameGUIEventType>.Publish(InGameGUIEventType.ActivateCanvas, "GameStart");
        AudioManager.Instance.PlayBGM("InGameBGM");
    }

    public void HandleGameState(GameState gameState)
    {
        if (gameState.PlayerState != null)
        {
            // 게임 시작 했는데 플레이어가 혼자면
            if (_gameStart && gameState.PlayerState.Count == 1)
            {
                foreach (var player in gameState.PlayerState)
                {
                    if (player.Id == ClientManager.Instance.ClientId)
                    {
                        // 승리
                        AudioManager.Instance.PlayOnceSfx(AudioManager.Sfx.Win, 1.0f);
                        EventBus<InGameGUIEventType>.Publish(InGameGUIEventType.ActivateCanvas, "GameEnd");
                    }
                    else
                    {
                        EventBus<InGameGUIEventType>.Publish(InGameGUIEventType.ActivateCanvas, "GameOver");
                    }
                }
            }
        }
    }

}

