using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    private Camera mainCamera;
    private Camera miniMapCamera;
    private int focusedClientId = -1;

    private const float CAMERA_ROTATION_X = 40f;
    private const float CAMERA_OFFSET_Y = 10f;
    private const float CAMERA_OFFSET_Z = 10f;

    private const float MINI_MAP_OFFSET_Y = 20f; // MiniMapCamera의 Y축 오프셋

    private List<Player> currentPlayers = new List<Player>(); // 현재 players 리스트 저장

    private GameObject FocusPlayer;


    void Start()
    {
        mainCamera = Camera.main;

        // MiniMapCamera 찾기
        miniMapCamera = GameObject.FindWithTag("MiniMapCamera")?.GetComponent<Camera>();

        if (miniMapCamera == null)
        {
            Debug.LogError("MiniMapCamera를 찾을 수 없습니다. 'MiniMapCamera' 태그를 확인하세요.");
        }
    }

    public void SetFocusedClient(int id)
    {
        focusedClientId = id;
    }

    public void UpdateCamera(List<Player> players)
    {
        if (focusedClientId == -1)
        {
            focusedClientId = ClientManager.Instance.ClientId;
        }

        currentPlayers = players; // players 리스트를 저장

        var player = currentPlayers.FirstOrDefault(p => p.id == focusedClientId);

        if (player == null)
        {
            // player가 죽어서 current에 없는 경우, 업데이트를 멈추고 현재 위치로 카메라를 고정시킨다.
            // 죽은 유저가 tab 키를 눌러서 SwitchToNextPlayer를 호출 할 때 focusedClientId를 변경해서 업데이트를 시켜주도록 한다.
            return; 
        }

        UpdateMainCamera(player);
        UpdateMiniMapCamera(player);
    }

    private void UpdateMainCamera(Player player)
    {
        if (mainCamera == null) return;
        // 카메라 위치 및 회전 설정
        mainCamera.transform.position = new Vector3(player.x, 0 + CAMERA_OFFSET_Y, player.y - CAMERA_OFFSET_Z);
        mainCamera.transform.rotation = Quaternion.Euler(CAMERA_ROTATION_X, 0, 0);
    }

    private void UpdateMiniMapCamera(Player player)
    {
        if (miniMapCamera == null) return;
        // MiniMapCamera는 플레이어 위치 위로 고정된 높이에서 따라다님
        miniMapCamera.transform.position = new Vector3(player.x, MINI_MAP_OFFSET_Y, player.y);
    }

    public void SwitchToNextPlayer()
    {
        if (currentPlayers.Count == 0) return;

         // 현재 clientId의 플레이어 인덱스 찾기
        int currentIndex = currentPlayers.FindIndex(p => p.id == focusedClientId);

        // focus에 해당하는 유저가 없으면 current 중에 찾아서 세팅한다.
        if (currentIndex == -1) {
            // clientId에 해당하는 플레이어가 없으면 첫 번째 플레이어로 clientId 변경
            if (currentPlayers.Count > 0)
            {
                focusedClientId = currentPlayers[0].id; // focus player id를 설정해준다.
            }
            else
            {
                return; // 플레이어가 없으면 종료
            }
        }

        if (currentIndex != -1)
        {
            // 다음 플레이어로 변경 (리스트를 순환)
            int nextIndex = (currentIndex + 1) % currentPlayers.Count;
            focusedClientId = currentPlayers[nextIndex].id;

            // 카메라 위치 변경
            var nextPlayer = currentPlayers[nextIndex];

            UpdateMainCamera(nextPlayer);
            UpdateMiniMapCamera(nextPlayer);
        }
    }
}