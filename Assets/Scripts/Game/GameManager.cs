using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Message;
using Google.Protobuf;
using Unity.VisualScripting;
using System.Net.WebSockets;
using System;

public class GameManager : MonoBehaviour
{
    private NetworkManager networkManager;

    // 카메라
    private Camera mainCamera;
    const float CAMERA_ROTATION_X = 40f;
    const float CAMERA_OFFSET_Y = 10f;
    const float CAMERA_OFFSET_Z = 10f;

    // 플레이어 데이터 관리
    private Dictionary<string, GameObject> playerObjectList = new Dictionary<string, GameObject>();
    private string clientId;
    float PLAYER_Y;
    float BULLET_Y;

    // 이동
    private Vector2 lastDirection = Vector2.zero; // 이전 프레임의 방향
    private bool wasMoved = false;                // 이전 프레임의 이동 상태

    // 총알
    private Dictionary<string, GameObject> bulletObjectList = new Dictionary<string, GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        // NetworkManager를 찾습니다.
        networkManager = FindObjectOfType<NetworkManager>();

        if (networkManager == null)
        {
            Debug.LogError("NetworkManager not found in the scene!");
            return;
        }

        // NetworkManager 초기화 및 핸들러 등록
        if (Debug.isDebugBuild)
        {
            var roomId = "test";
            networkManager.Initialize($"ws://localhost:8000/room?roomId={roomId}");
        }
        else
        {
            networkManager.Initialize("ws://eternal-snowman:8000/room");
        }

        networkManager.RegisterHandler(
            onOpen: HandleOnOpen,
            onError: HandleOnError,
            onClose: HandleOnClose,
            onMessage: HandleOnMessage
        );

        // WebSocket 연결 시도
        networkManager.Connect();

        GameObject mapPrefab = Resources.Load<GameObject>($"Prefabs/Map");
        GameObject playerPrefab = Resources.Load<GameObject>($"Prefabs/Player");

        PLAYER_Y = CalculateSurfaceY(mapPrefab);
        BULLET_Y = PLAYER_Y + 1.5f;

        mainCamera = Camera.main;
    }

    private void OnApplicationQuit()
    {
        networkManager.Close();
    }


    // 연결 성공 이벤트 핸들러
    private void HandleOnOpen()
    {
    }

    // 오류 발생 이벤트 핸들러
    private void HandleOnError(string error)
    {
    }

    // 연결 종료 이벤트 핸들러
    private void HandleOnClose(string closeCode)
    {
    }

    // 메시지 수신 이벤트 핸들러
    private void HandleOnMessage(byte[] bytes)
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

                case Wrapper.MessageTypeOneofCase.GameState:
                    // Debug.Log($"State: {wrapper.State}");
                    HandleGameState(wrapper.GameState);
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
    }

    void Update()
    {
        HandlePlayerInput();
    }

    private void HandlePlayerInput()
    {
        HandleKeyboardInput();

        HandleMouseInput();
    }

    private void HandleKeyboardInput()
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

    private void HandleMouseInput()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            Debug.LogError("Player tag에 해당하는 객체 없음");
            return; // Player가 없으면 함수 종료
        }

        if (Input.GetMouseButtonDown(0)) // 마우스 왼쪽 버튼 클릭
        {
            // 클릭한 위치 계산
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                // hit된 오브젝트가 내 플레이어라면 return
                if (hit.collider.gameObject == player)
                {
                    Debug.Log("Hit on own player, ignoring.");
                    return;
                }

                Vector3 clickPosition = hit.point; // 클릭한 월드 좌표
                Vector3 playerPosition = player.transform.position; // 플레이어 위치

                // 클릭한 위치와 플레이어 위치 간 벡터 계산
                Vector3 direction = (clickPosition - playerPosition).normalized;
                direction.z = -direction.z; // Z축 반전 적용

                // Z축 기준 각도 계산 (Z축 중심으로 회전)
                float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

                // 각도를 0~360° 범위로 변환
                if (angle < 0)
                {
                    angle += 360f;
                }

                Debug.LogError($"click: {clickPosition}, player: {playerPosition}");
                Debug.LogError($"direction: {direction}");
                Debug.LogError($"angle: {angle}");

                // 서버로 데이터 전송
                SendBulletCreateMessage(player.name, playerPosition.x, playerPosition.z, angle);
            }
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

    private void HandleGameInit(GameInit init)
    {
        clientId = init.Id;
        Debug.Log($"Assigned Client ID: {clientId}");
    }

    private void HandleGameState(GameState gameState)
    {
        if (gameState.PlayerState != null)
        {
            UpdatePlayers(gameState.PlayerState);
        }

        if (gameState.BulletState != null)
        {
            UpdateAllBullets(gameState.BulletState);
        }
    }

    private void UpdateAllBullets(IEnumerable<BulletState> bulletStates)
    {
        // 서버에서 받은 총알 ID 저장
        HashSet<string> bulletStateIds = new HashSet<string>();

        // 서버에서 받은 총알 상태를 순회
        foreach (var bulletState in bulletStates)
        {
            bulletStateIds.Add(bulletState.BulletId);
            UpdateBulletPosition(bulletState);
        }

        // Dictionary에서 서버에 없는 총알 제거
        RemoveInactiveBullets(bulletStateIds);
    }

    // GameState를 기반으로 플레이어 관리
    private void UpdatePlayers(IEnumerable<PlayerState> players)
    {
        HashSet<string> playerStateIds = new HashSet<string>();

        // 서버에서 받은 players 리스트를 순회하며 업데이트
        foreach (var player in players)
        {
            playerStateIds.Add(player.Id);
            UpdatePlayerPosition(player);
        }

        // 서버에 없는 플레이어 제거
        RemoveInactivePlayers(playerStateIds);
    }

    private void UpdatePlayerPosition(PlayerState playerState)
    {
        GameObject playerObject;

        // 플레이어가 Dictionary에 없으면 새로 생성
        if (!playerObjectList.TryGetValue(playerState.Id, out playerObject))
        {
            GameObject playerPrefab = Resources.Load<GameObject>("Prefabs/Player");
            if (playerPrefab != null)
            {
                playerObject = Instantiate(playerPrefab, new Vector3(playerState.X, PLAYER_Y, playerState.Y), Quaternion.identity);

                if (playerState.Id == clientId) // 내 플레이어면 태그를 Player로 설정
                {
                    playerObject.tag = "Player";
                    Debug.Log($"Created client player for ID: {playerState.Id}");
                }

                // Dictionary에 추가
                playerObjectList[playerState.Id] = playerObject;
            }
            else
            {
                Debug.LogError("Player prefab could not be loaded.");
            }
        }

        // 위치 업데이트
        playerObject.transform.position = new Vector3(playerState.X, PLAYER_Y, playerState.Y);

        // 카메라 세팅 (내 플레이어일 경우에만)
        if (playerState.Id == clientId)
        {
            mainCamera.transform.position = new Vector3(playerState.X, PLAYER_Y + CAMERA_OFFSET_Y, playerState.Y - CAMERA_OFFSET_Z);
            mainCamera.transform.rotation = Quaternion.Euler(CAMERA_ROTATION_X, 0, 0);
        }
    }

    private void RemoveInactivePlayers(HashSet<string> playerStateIds)
    {
        List<string> playersToRemove = new List<string>();

        // Dictionary에서 서버에 없는 플레이어 찾기
        foreach (var playerId in playerObjectList.Keys)
        {
            if (!playerStateIds.Contains(playerId))
            {
                playersToRemove.Add(playerId);
            }
        }

        // Dictionary에서 제거 및 GameObject 파괴
        foreach (var playerId in playersToRemove)
        {
            if (playerObjectList.TryGetValue(playerId, out GameObject player))
            {
                Destroy(player);
                playerObjectList.Remove(playerId);
                Debug.Log($"Removed player with ID: {playerId}");
            }
        }
    }

    private void UpdateBulletPosition(BulletState bullet)
    {
        GameObject firedBullet;

        // 총알이 Dictionary에 없으면 새로 생성
        if (!bulletObjectList.TryGetValue(bullet.BulletId, out firedBullet))
        {
            GameObject bulletPrefab = Resources.Load<GameObject>("Prefabs/Bullet");
            if (bulletPrefab != null)
            {
                firedBullet = Instantiate(bulletPrefab, new Vector3(bullet.X, BULLET_Y, bullet.Y), Quaternion.identity);
                firedBullet.tag = "Bullet";

                // Dictionary에 추가
                bulletObjectList[bullet.BulletId] = firedBullet;
                Debug.Log($"발사: {bullet.BulletId}");
            }
            else
            {
                Debug.LogError("Bullet prefab could not be loaded.");
            }
        }

        firedBullet.transform.position = new Vector3(bullet.X, BULLET_Y, bullet.Y);
        Debug.LogError($"총알 위치: {firedBullet.transform.position}");
    }

    private void RemoveInactiveBullets(HashSet<string> bulletStateIds)
    {
        // 서버에서 제공되지 않은 총알 ID를 제거
        List<string> bulletsToRemove = new List<string>();

        foreach (var bulletId in bulletObjectList.Keys)
        {
            if (!bulletStateIds.Contains(bulletId))
            {
                bulletsToRemove.Add(bulletId);
            }
        }

        // Dictionary에서 제거하고 GameObject 파괴
        foreach (var bulletId in bulletsToRemove)
        {
            if (bulletObjectList.TryGetValue(bulletId, out GameObject bullet))
            {
                Destroy(bullet);
                bulletObjectList.Remove(bulletId);
                Debug.Log($"총알 제거: {bulletId}");
            }
        }
    }

    private float CalculateSurfaceY(GameObject mapPrefab)
    {
        if (mapPrefab == null)
        {
            Debug.LogError("Map or Player prefab is not assigned or could not be loaded.");
            return 0f;
        }

        // // Map의 Scale에서 높이 계산
        float mapHeight = mapPrefab.transform.localScale.y / 2;

        return mapHeight;
    }

    private void SendMovementMessage(float angle, bool isMoved)
    {
        if (networkManager == null || !networkManager.IsOpen())
        {
            Debug.LogError("WebSocket is not connected.");
            return;
        }

        // DirChange 메시지 생성
        var changeDir = new ChangeDir
        {
            Angle = angle,
            IsMoved = isMoved
        };

        // Wrapper 메시지 생성 및 DirChange 메시지 포함
        var wrapper = new Wrapper
        {
            ChangeDir = changeDir
        };

        // Protobuf 직렬화
        var data = wrapper.ToByteArray();

        try
        {
            // WebSocket으로 메시지 전송
            networkManager.Send(data);
            // Debug.Log($"Sent movement: angle={angle}, isMoved={isMoved}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to send movement: {ex.Message}");
        }
    }

    private void SendBulletCreateMessage(string playerId, float startX, float startY, float angle)
    {
        if (networkManager == null || !networkManager.IsOpen())
        {
            Debug.LogError("WebSocket is not connected.");
            return;
        }

        try
        {
            // BulletCreate 메시지 생성
            var createBullet = new CreateBullet
            {
                PlayerId = playerId,
                StartX = startX,
                StartY = startY,
                Angle = angle
            };

            // Wrapper 메시지 생성 및 CreateBullet 메시지 포함
            var wrapper = new Wrapper
            {
                CreateBullet = createBullet
            };

            // Protobuf 직렬화
            var data = wrapper.ToByteArray();

            // WebSocket으로 메시지 전송
            networkManager.Send(data);

            Debug.Log($"Sent CreateBullet: PlayerId={playerId}, StartX={startX}, StartY={startY}, Angle={angle}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to send CreateBullet message: {ex.Message}");
        }
    }
}

