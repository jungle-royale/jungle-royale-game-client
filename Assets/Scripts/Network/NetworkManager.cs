using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf;
using Message; // Protobuf으로 생성된 네임스페이스
using UnityEngine;
using NativeWebSocket;
using Unity.VisualScripting;

public class NetworkManager : MonoBehaviour
{
    private WebSocket websocket;

    // 카메라
    [SerializeField] private FollowCam cameraFollow;

    // 플레이어 데이터 관리
    private Dictionary<string, GameObject> players = new Dictionary<string, GameObject>();
    private GameObject mapPrefab;
    private GameObject playerPrefab;
    private string clientId;

    private float PLAYER_Y;

    // 이동
    private Vector2 lastDirection = Vector2.zero; // 이전 프레임의 방향
    private bool wasMoved = false;                // 이전 프레임의 이동 상태

    // Start is called before the first frame update
    async void Start()
    {
        GameObject mapPrefab = Resources.Load<GameObject>($"Prefabs/Map");
        GameObject playerPrefab = Resources.Load<GameObject>($"Prefabs/Player");

        PLAYER_Y = CalculateSurfaceY();

        // WebSocket 서버 주소
        websocket = new WebSocket("ws://localhost:8000/ws");

        websocket.OnOpen += () =>
        {
            Debug.Log("Connection open!");
        };

        websocket.OnError += (e) =>
        {
            Debug.Log($"Error! {e}");
        };

        websocket.OnClose += (e) =>
        {
            Debug.Log("Connection closed!");
        };

        // 메시지 수신
        websocket.OnMessage += (bytes) =>
        {
            try
            {
                // Protobuf 역직렬화
                var wrapper = Wrapper.Parser.ParseFrom(bytes);

                // 메시지 타입 확인
                // Debug.Log($"Message Type: {wrapper.MessageTypeCase}");

                // 메시지 타입 확인 및 처리
                switch (wrapper.MessageTypeCase)
                {
                    case Wrapper.MessageTypeOneofCase.GameInit:
                        // Debug.Log($"GameInit: {wrapper.GameInit}");
                        HandleGameInit(wrapper.GameInit);
                        break;

                    case Wrapper.MessageTypeOneofCase.State:
                        // Debug.Log($"State: {wrapper.State}");
                        HandleGameState(wrapper.State);
                        break;

                    case Wrapper.MessageTypeOneofCase.BulletCreate:
                        // Debug.Log($"BulletCreate: {wrapper.BulletCreate}");
                        break;

                    default:
                        Debug.Log($"Unknown message type received: {wrapper.MessageTypeCase}");
                        break;
                }
            }
            catch (InvalidProtocolBufferException ex)
            {
                Debug.LogError($"Protobuf parsing error: {ex.Message}");
                Debug.Log($"Raw bytes: {BitConverter.ToString(bytes)}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Unexpected error: {ex.Message}");
            }
        };

        // 서버 연결 시도
        await websocket.Connect();
    }

    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        websocket?.DispatchMessageQueue();
#endif
        HandleMovementInput();
    }

    private void HandleMovementInput()
    {
        // WASD 입력 벡터 계산
        Vector2 inputDirection = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        // 입력 벡터로 방향 및 이동 상태 계산
        float angle = CalculateAngle(inputDirection);
        bool isMoved = inputDirection != Vector2.zero;

        // 입력 상태 변화 감지
        if (inputDirection != lastDirection || isMoved != wasMoved)
        {
            // 서버로 메시지 전송
            SendMovementMessage(angle, isMoved);

            // 상태 업데이트
            lastDirection = inputDirection;
            wasMoved = isMoved;
        }
    }

    private float CalculateAngle(Vector2 inputDirection)
    {
        if (inputDirection == Vector2.zero)
        {
            return 0f; // 정지 상태일 경우 0도 반환
        }

        // 방향 벡터에서 각도를 계산 (Z축 기준, 시계 방향이 +)
        float angle = Mathf.Atan2(inputDirection.y, inputDirection.x) * Mathf.Rad2Deg;

        // Unity의 좌표계에서는 W = 90°, S = -90°이므로 보정
        angle += 90f;

        // 각도를 0~360 범위로 변환
        if (angle < 0)
        {
            angle += 360f;
        }

        return angle;
    }

    private async void SendMovementMessage(float angle, bool isMoved)
    {
        if (websocket == null || websocket.State != WebSocketState.Open)
        {
            Debug.LogError("WebSocket is not connected.");
            return;
        }

        // DirChange 메시지 생성
        var dirChange = new DirChange
        {
            Angle = angle,
            IsMoved = isMoved
        };

        // Wrapper 메시지 생성 및 DirChange 메시지 포함
        var wrapper = new Wrapper
        {
            DirChange = dirChange
        };

        // Protobuf 직렬화
        var data = wrapper.ToByteArray();

        try
        {
            // WebSocket으로 메시지 전송
            await websocket.Send(data);
            Debug.Log($"Sent movement: angle={angle}, isMoved={isMoved}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to send movement: {ex.Message}");
        }
    }

    private async void OnApplicationQuit()
    {
        if (websocket != null)
        {
            await websocket.Close();
        }
    }

    private void HandleGameInit(GameInit init)
    {
        clientId = init.Id;
        Debug.Log($"Assigned Client ID: {clientId}");
    }

    private void HandleGameState(GameState state)
    {
        if (state.Players != null)
        {
            foreach (var player in state.Players)
            {
                // 플레이어 ID와 클라이언트 ID가 일치하면 로컬 플레이어로 처리
                if (player.Id == clientId)
                {
                    // 로컬 플레이어의 위치를 업데이트
                    UpdateLocalPlayerPosition(player);

                    // 카메라를 로컬 플레이어에 고정
                    // LockCameraToPlayer(player);
                }
                else
                {
                    // 원격 플레이어의 위치를 업데이트
                    UpdateRemotePlayerPosition(player);
                }
            }
        }
    }

    private void UpdateLocalPlayerPosition(Player player)
    {
        GameObject localPlayer;

        // players 딕셔너리에서 ID에 해당하는 플레이어 찾기
        if (!players.TryGetValue(player.Id, out localPlayer))
        {

            GameObject playerPrefab = Resources.Load<GameObject>($"Prefabs/Player");
            // 플레이어가 없으면 Prefab으로 새로 생성
            if (playerPrefab != null)
            {
                localPlayer = Instantiate(playerPrefab, new Vector3(player.X, PLAYER_Y, player.Y), Quaternion.identity);
                players[player.Id] = localPlayer; // 딕셔너리에 추가
                Debug.Log($"Created new player for ID: {player.Id}");

                // cameraFollow.SetPlayer(localPlayer.transform);
            }
            else
            {
                Debug.LogError($"플레이어 프리팹 'Player'을(를) 찾을 수 없습니다.");
            }
        }
        // 플레이어 위치 업데이트

        localPlayer.transform.position = new Vector3(player.X, PLAYER_Y, player.Y);
    }

    private void UpdateRemotePlayerPosition(Player player)
    {
        GameObject remotePlayer;
        GameObject playerPrefab = Resources.Load<GameObject>($"Prefabs/Player");

        if (!players.TryGetValue(player.Id, out remotePlayer))
        {
            // 원격 플레이어 생성
            remotePlayer = Instantiate(playerPrefab, new Vector3(player.X, PLAYER_Y, player.Y), Quaternion.identity);
            players[player.Id] = remotePlayer;
            Debug.Log($"Created remote player for ID: {player.Id}");
        }

        // 위치 업데이트
        remotePlayer.transform.position = new Vector3(player.X, PLAYER_Y, player.Y);
    }



    private float CalculateSurfaceY()
    {
        // Resources에서 Prefabs 로드
        GameObject mapPrefab = Resources.Load<GameObject>($"Prefabs/Map");
        GameObject playerPrefab = Resources.Load<GameObject>($"Prefabs/Player");

        if (mapPrefab == null || playerPrefab == null)
        {
            Debug.LogError("Map or Player prefab is not assigned or could not be loaded.");
            return 0f;
        }

        // Map의 Scale에서 높이 계산
        float mapHeight = mapPrefab.transform.localScale.y / 2;

        // Player의 CapsuleCollider에서 높이 계산
        CapsuleCollider playerCollider = playerPrefab.GetComponent<CapsuleCollider>();
        if (playerCollider == null)
        {
            Debug.LogError("Player prefab does not have a CapsuleCollider.");
            return 0f;
        }
        float playerHeight = playerCollider.height / 2;

        // Map 표면에 정확히 위치하도록 Y값 계산
        float PLAYER_Y = mapHeight + playerHeight;
        Debug.Log(PLAYER_Y);

        return PLAYER_Y;
    }
}