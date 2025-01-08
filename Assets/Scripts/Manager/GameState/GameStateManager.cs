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
        _gameStart = true;
        AudioManager.Instance.PlaySfx(AudioManager.Sfx.GameStart);
        EventBus<InGameGUIEventType>.Publish(InGameGUIEventType.ActivateCanvas, "GameStart");
        AudioManager.Instance.PlayBGM("InGameBGM");
    }

    public void HandleGameState(GameState gameState)
    {
        if (gameState.PlayerState != null)
        {
            // 게임 시작한 후에, player가 한 명 남았을 때 게임 종료 처리
            if (_gameStart && gameState.PlayerState.Count == 1)
            {
                foreach (var player in gameState.PlayerState)
                {

                    // 모든 키를 막는다.
                    EventBus<InputButtonEventType>.Publish(InputButtonEventType.StopPlay);

                    // camera를 승리자로 옮긴다. -> camera state 업데이트 할 때 자동으로 옮겨질 것

                    // TODO: 승리 애니메이션, 파티클 추가

                    if (player.Id == ClientManager.Instance.ClientId)
                    {
                        // 승리
                        AudioManager.Instance.PlayOnceSfx(AudioManager.Sfx.Win, 1.0f);
                        EventBus<InGameGUIEventType>.Publish(InGameGUIEventType.ActivateCanvas, "GameEnd");
                    }
                    else
                    {
                        // TODO: 다른 사람이 1등했을 때에는 다른 화면 보여줘야?
                        EventBus<InGameGUIEventType>.Publish(InGameGUIEventType.ActivateCanvas, "GameOver");
                    }
                }
            }
        }
    }

}

