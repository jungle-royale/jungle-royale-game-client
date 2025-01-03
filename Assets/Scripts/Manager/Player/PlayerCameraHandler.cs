using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerCameraHandler : MonoBehaviour
{
    private Camera mainCamera;
    private String focusedClientId = null;

    private const float CAMERA_ROTATION_X = 40f;
    private const float CAMERA_OFFSET_Y = 10f;
    private const float CAMERA_OFFSET_Z = 10f;

    private List<Player> currentPlayers = new List<Player>(); // 현재 players 리스트 저장


    void Start()
    {
        mainCamera = Camera.main;
    }

    public void UpdateCamera(List<Player> players)
    {
        if (focusedClientId == null)
        {
            focusedClientId = ClientManager.Instance.ClientId;
        }

        currentPlayers = players; // players 리스트를 저장

        var player = currentPlayers.FirstOrDefault(p => p.id == focusedClientId);

        if (player == null)
        {
            // clientId에 해당하는 플레이어가 없으면 첫 번째 플레이어로 clientId 변경
            if (currentPlayers.Count > 0)
            {
                focusedClientId = currentPlayers[0].id;
                player = currentPlayers[0];
            }
            else
            {
                return; // 플레이어가 없으면 종료
            }
        }

        // 카메라 위치 및 회전 설정
        mainCamera.transform.position = new Vector3(player.x, 0 + CAMERA_OFFSET_Y, player.y - CAMERA_OFFSET_Z);
        mainCamera.transform.rotation = Quaternion.Euler(CAMERA_ROTATION_X, 0, 0);
    }

    private bool ClientIsDead()
    {
        return focusedClientId != ClientManager.Instance.ClientId;
    }

    public void SwitchToNextPlayer()
    {
        if (!ClientIsDead()) {
            return;
        }
        if (currentPlayers.Count == 0) return;

        // 현재 clientId의 플레이어 인덱스 찾기
        int currentIndex = currentPlayers.FindIndex(p => p.id == focusedClientId);

        if (currentIndex != -1)
        {
            // 다음 플레이어로 변경 (리스트를 순환)
            int nextIndex = (currentIndex + 1) % currentPlayers.Count;
            focusedClientId = currentPlayers[nextIndex].id;

            // 카메라 위치 변경
            var nextPlayer = currentPlayers[nextIndex];
            mainCamera.transform.position = new Vector3(nextPlayer.x, 0 + CAMERA_OFFSET_Y, nextPlayer.y - CAMERA_OFFSET_Z);
            mainCamera.transform.rotation = Quaternion.Euler(CAMERA_ROTATION_X, 0, 0);
        }
    }


}