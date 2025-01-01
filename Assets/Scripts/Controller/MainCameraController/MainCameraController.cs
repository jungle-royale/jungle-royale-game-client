using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class MainCameraController : MonoBehaviour
{
    private Camera mainCamera;
    private string focusedClientId;
    private string clientId;

    private const float CAMERA_ROTATION_X = 40f;
    private const float CAMERA_OFFSET_Y = 10f;
    private const float CAMERA_OFFSET_Z = 10f;

    private List<MainCamera> currentPlayers = new List<MainCamera>(); // 현재 players 리스트 저장


    void Start()
    {
        EventBus<MainCameraEventType>.Subscribe<MainCameraInit>(MainCameraEventType.MainCameraInit, InitializeClient);
        EventBus<MainCameraEventType>.Subscribe<IEnumerable<MainCamera>>(MainCameraEventType.MainCameraState, UpdateCamera);

        mainCamera = Camera.main;
    }

    void Update()
    {
        if (ClientIsDead())
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                SwitchToNextPlayer();
            }
        }
    }

    private void InitializeClient(MainCameraInit init)
    {
        focusedClientId = init.ClientId;
        clientId = focusedClientId; // 이걸로 관전모드 체크
    }

    private void UpdateCamera(IEnumerable<MainCamera> players)
    {

        currentPlayers = players.ToList(); // players 리스트를 저장

        var player = currentPlayers.FirstOrDefault(p => p.Id == focusedClientId);

        if (player == null)
        {
            // clientId에 해당하는 플레이어가 없으면 첫 번째 플레이어로 clientId 변경
            if (currentPlayers.Count > 0)
            {
                focusedClientId = currentPlayers[0].Id;
                player = currentPlayers[0];
            }
            else
            {
                return; // 플레이어가 없으면 종료
            }
        }

        // 카메라 위치 및 회전 설정
        mainCamera.transform.position = new Vector3(player.X, 0 + CAMERA_OFFSET_Y, player.Y - CAMERA_OFFSET_Z);
        mainCamera.transform.rotation = Quaternion.Euler(CAMERA_ROTATION_X, 0, 0);
    }

    private bool ClientIsDead()
    {
        return focusedClientId != clientId;
    }

    private void SwitchToNextPlayer()
    {
        if (currentPlayers.Count == 0) return;

        // 현재 clientId의 플레이어 인덱스 찾기
        int currentIndex = currentPlayers.FindIndex(p => p.Id == focusedClientId);

        if (currentIndex != -1)
        {
            // 다음 플레이어로 변경 (리스트를 순환)
            int nextIndex = (currentIndex + 1) % currentPlayers.Count;
            focusedClientId = currentPlayers[nextIndex].Id;

            // 카메라 위치 변경
            var nextPlayer = currentPlayers[nextIndex];
            mainCamera.transform.position = new Vector3(nextPlayer.X, 0 + CAMERA_OFFSET_Y, nextPlayer.Y - CAMERA_OFFSET_Z);
            mainCamera.transform.rotation = Quaternion.Euler(CAMERA_ROTATION_X, 0, 0);
        }
    }


    private void OnDestroy()
    {
        EventBus<MainCameraEventType>.Unsubscribe<MainCameraInit>(MainCameraEventType.MainCameraInit, InitializeClient);
        EventBus<MainCameraEventType>.Unsubscribe<IEnumerable<MainCamera>>(MainCameraEventType.MainCameraState, UpdateCamera);
    }
}