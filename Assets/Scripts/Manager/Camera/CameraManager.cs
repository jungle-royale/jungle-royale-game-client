using System;
using System.Collections.Generic;
using System.Linq;
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

    // 흔들림 효과 관련 변수
    private bool isShaking = false;
    private float shakeDuration = 0f;
    private float shakeMagnitude = 0f;
    private Vector3 originalCameraPosition;

    void Start()
    {
        mainCamera = Camera.main;

        // MiniMapCamera 찾기
        miniMapCamera = GameObject.FindWithTag("MiniMapCamera")?.GetComponent<Camera>();

        if (miniMapCamera == null)
        {
            Debug.LogError("MiniMapCamera를 찾을 수 없습니다. 'MiniMapCamera' 태그를 확인하세요.");
        }

        if (mainCamera != null)
        {
            originalCameraPosition = mainCamera.transform.localPosition;
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
            return; 
        }

        UpdateMainCamera(player);
        UpdateMiniMapCamera(player);
    }

    private void UpdateMainCamera(Player player)
    {
        if (mainCamera == null) return;

        Vector3 targetPosition = new Vector3(player.x, CAMERA_OFFSET_Y, player.y - CAMERA_OFFSET_Z);

        // 카메라 흔들림이 활성화된 경우 흔들림 적용
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
            // 카메라 위치 및 회전 설정
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
        if (currentPlayers.Count == 0) return;

         // 현재 clientId의 플레이어 인덱스 찾기
        int currentIndex = currentPlayers.FindIndex(p => p.id == focusedClientId);

        // focus에 해당하는 유저가 없으면 current 중에 찾아서 세팅한다.
        if (currentIndex == -1) {
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

    // 카메라 흔들림 시작
    public void StartCameraShake(float duration, float magnitude)
    {
        if (mainCamera == null) return;
        if (isShaking == true) return;
        Debug.Log("지진 true");
        isShaking = true;
        shakeDuration = duration;
        shakeMagnitude = magnitude;
        StartCoroutine(CameraShakeCoroutine());
    }

    // 카메라 흔들림 코루틴
    private System.Collections.IEnumerator CameraShakeCoroutine()
    {
        Debug.Log("흔들림");

        float elapsedTime = 0f;

        while (elapsedTime < shakeDuration)
        {
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
        mainCamera.transform.localPosition = originalCameraPosition;
    }
}