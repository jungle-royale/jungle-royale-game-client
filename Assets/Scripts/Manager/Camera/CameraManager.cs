using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public PlayerManager playerManager;
    private Camera mainCamera;
    private Camera miniMapCamera;
    private int focusedClientId = -1;

    private const float CAMERA_ROTATION_X = 40f;
    private const float CAMERA_OFFSET_Y = 10f;
    private const float CAMERA_OFFSET_Z = 10f;

    private const float MINI_MAP_OFFSET_Y = 20f; // MiniMapCamera의 Y축 오프셋

    private List<Player> currentPlayerList = new List<Player>(); // 현재 players 리스트 저장


    // 흔들림 효과 관련 변수
    private bool isShaking = false;
    private float shakeDuration = 0f;
    private float shakeMagnitude = 0f;

    private Debouncer shakeAudioDebouncer = new Debouncer();

    void Awake()
    {
        // PlayerManager를 찾거나 연결
        playerManager = FindObjectOfType<PlayerManager>();
        if (playerManager == null)
        {
            Debug.LogError("PlayerManager not found in the scene.");
        }
    }

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

    void Update()
    {
        if (isMoving)
        {
            return;
        }

        var player = currentPlayerList.FirstOrDefault(p => p.id == focusedClientId);
        if (player == null)
        {
            // player가 죽어서 current에 없는 경우, 업데이트를 멈추고 현재 위치로 카메라를 고정시킨다.
            return;
        }
        UpdateMainCamera(player);
        UpdateMiniMapCamera(player);
    }

    public void SetFocusedClient(int id)
    {
        focusedClientId = id;
    }

    public void UpdateCameraFromServer(List<Player> playerListFromServer)
    {
        if (focusedClientId == -1)
        {
            focusedClientId = ClientManager.Instance.ClientId;
        }

        currentPlayerList = playerListFromServer; // players 리스트를 저장

        var player = currentPlayerList.FirstOrDefault(p => p.id == focusedClientId);

        if (player == null)
        {
            // player가 죽어서 current에 없는 경우, 업데이트를 멈추고 현재 위치로 카메라를 고정시킨다.
            return; 
        }

        UpdateMainCamera(player);
        UpdateMiniMapCamera(player);
    }

    private void UpdateMainCamera(Player player)
    {
        if (mainCamera == null) return;

        if (isMoving)
        {
            return;
        }

        GameObject playerObject = playerManager.GetPlayerById(focusedClientId);
        if (playerObject == null)
        {
            return;
        }

        Vector3 targetPosition = playerObject.transform.position;
        targetPosition.y = CAMERA_OFFSET_Y;
        targetPosition.z -= CAMERA_OFFSET_Z;

        if (isShaking)
        {
            Vector3 shakeOffset = new Vector3(
                UnityEngine.Random.Range(-1f, 1f) * shakeMagnitude,
                UnityEngine.Random.Range(-1f, 1f) * shakeMagnitude,
                UnityEngine.Random.Range(-1f, 1f) * shakeMagnitude
            );
            Debug.Log($"{shakeOffset.x},{shakeOffset.y},{shakeOffset.z}");
            mainCamera.transform.position = targetPosition + shakeOffset;
        }
        else
        {
            mainCamera.transform.position = targetPosition;
        }

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
        if (currentPlayerList.Count == 0) return;

        StopUpdateCameraMovement();

         // 현재 clientId의 플레이어 인덱스 찾기
        int currentIndex = currentPlayerList.FindIndex(p => p.id == focusedClientId);

        // focus에 해당하는 유저가 없으면 current 중에 찾아서 세팅한다.
        if (currentIndex == -1) {
            if (currentPlayerList.Count > 0)
            {
                focusedClientId = currentPlayerList[0].id; // focus player id를 설정해준다.
            }
            else
            {
                return; // 플레이어가 없으면 종료
            }
        }

        if (currentIndex != -1)
        {
            // 다음 플레이어로 변경 (리스트를 순환)
            int nextIndex = (currentIndex + 1) % currentPlayerList.Count;
            focusedClientId = currentPlayerList[nextIndex].id;

            // 카메라 위치 변경
            var nextPlayer = currentPlayerList[nextIndex];

            UpdateMainCamera(nextPlayer);
            UpdateMiniMapCamera(nextPlayer);
        }
    }

    // 카메라 흔들림 시작
    public void StartCameraShake(float duration, float magnitude)
    {
        if (mainCamera == null) return;
        if (isShaking == true) return;
        isShaking = true;
        shakeDuration = duration;
        shakeMagnitude = magnitude;
        StartCoroutine(CameraShakeCoroutine());
    }

    // 카메라 흔들림 코루틴
    private System.Collections.IEnumerator CameraShakeCoroutine()
    {
        float elapsedTime = 0f;

        while (elapsedTime < shakeDuration)
        {
            shakeAudioDebouncer.Debounce(2500, () =>
            {
                AudioManager.Instance.PlaySfx(AudioManager.Sfx.DestroyGround);
            });
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        StopCameraShake();
    }

    // 카메라 흔들림 멈춤
    public void StopCameraShake()
    {
        if (mainCamera == null) return;
        if (isShaking == false) return;
        Debug.Log("지진 false");
        isShaking = false;
    }

    private bool isMoving = false;

    public void StopUpdateCameraMovement()
    {
        isMoving = false;
    }

    public void UpdateCameraMovement(float x, float y)
    {
        Debug.Log($"move camera {x}, {y}");
        isMoving = true;
        float speed = 0.1f;
        var diff = new Vector3(x, 0, y).normalized * speed;
        Vector3 targetPosition = mainCamera.transform.position + diff;
        mainCamera.transform.position = targetPosition;
        miniMapCamera.transform.position = targetPosition;
    }

    public bool IsInCameraView(Vector3 worldPosition)
    {
        // 미니맵 카메라 참조
        Vector3 viewportPoint = miniMapCamera.WorldToViewportPoint(worldPosition);

        // 뷰포트의 x와 y가 0~1 사이인지 확인
        bool isInView = viewportPoint.x >= 0 && viewportPoint.x <= 1 &&
                        viewportPoint.y >= 0 && viewportPoint.y <= 1;

        return isInView;
    }

    public Vector3 GetMinimapCameraPosition()
    {
        return miniMapCamera.transform.position;
    }
}