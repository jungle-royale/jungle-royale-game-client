using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Message;
using System;

public class GameStateManager : Singleton<GameStateManager>
{

    private DateTime _sessionStartTime;
    public CameraManager cameraManager;

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
        AudioManager.Instance.PlaySfx(AudioManager.Sfx.GameStart);
        EventBus<InGameGUIEventType>.Publish(InGameGUIEventType.ActivateCanvas, "GameStart");
        AudioManager.Instance.PlayBGM("InGameBGM");
    }

    public void HandleGameEndState(List<PlayerDeadState> playerDeadStateList)
    {

        foreach (var deadPlayerState in playerDeadStateList)
        {

            Debug.Log($"🍎 스테이트 {deadPlayerState.deadPlayerId}, {deadPlayerState.dyingStatus}, {deadPlayerState.placement}");

            if (deadPlayerState.IsEndGame()) // dead에 1등이 왔다는건, 게임이 종료되었다는 것
            {

                int playerId = deadPlayerState.deadPlayerId;

                Debug.Log($"🍎 게임 종료, 1등은 {playerId}");

                // 모든 키를 막는다. - 모든 키 막을 때, 쏘고 있거나, 대시하고 있는거 다 false로 서버에 보내버리기
                EventBus<InputButtonEventType>.Publish(InputButtonEventType.StopPlay);

                // camera를 승리자로 옮긴다.
                cameraManager.SetFocusedClient(playerId);

                // TODO: 승리 애니메이션, 파티클 추가

                if (playerId == ClientManager.Instance.ClientId)
                {

                    // 승리
                    Debug.Log("승리");
                    AudioManager.Instance.PlayOnceSfx(AudioManager.Sfx.Win, 1.0f);
                    EventBus<InGameGUIEventType>.Publish(InGameGUIEventType.ActivateCanvas, "GameWin");
                }
                else
                {
                    Debug.Log("패배");
                    // TODO: 다른 사람이 1등했을 때에는 다른 화면 보여줘야?
                    EventBus<InGameGUIEventType>.Publish(InGameGUIEventType.ActivateCanvas, "GameEnd");
                }

                StateUIDTO stateData = new StateUIDTO
                {
                    placement = ClientManager.Instance.placement,
                    totalPlayer = ClientManager.Instance.totalPlayerNum,
                    killCount = ClientManager.Instance.killCount,
                    // point = 500
                };
                EventBus<InGameGUIEventType>.Publish(InGameGUIEventType.UpdateStateLabel, stateData);
            }
        }
    }

}

