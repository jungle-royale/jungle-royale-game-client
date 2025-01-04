using UnityEngine;
using System;

public class InputManager : MonoBehaviour
{
    public InputNetworkSender networkSender; // 서버로 입력 정보를 보내는 클래스 참조
    public CameraHandler cameraHandler;

    // 이동
    private Vector2 lastDirection = Vector2.zero; // 이전 프레임의 방향
    private bool wasMoved = false;                // 이전 프레임의 이동 상태

    private string ClientId
    {
        get
        {
            return ClientManager.Instance.ClientId;
        }
    }

    private int lastSendAngleTime = 0;
    private bool lastClickState;  // 눌려있으면 true

    private Debouncer DashDebouncer = new Debouncer();

    // 이펙트
    private GameObject snowSlashEffect; // SnowSlashEffect를 참조

    void Update()
    {
        HandleBullet();
        HandleMove();
        HandleDash();
        HandleDirection();
        HandleTab();
    }

    private void HandleTab()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            cameraHandler.SwitchToNextPlayer();
        }
    }

    private void HandleMove()
    {
        // WASD 입력 벡터 계산
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        Vector2 inputDirection = new Vector2(x, y);


        // 입력 벡터로 방향 및 이동 상태 계산
        float angle = CalculateAngle(inputDirection);
        bool isMoved = inputDirection != Vector2.zero;

        // Debug.Log($"anlge, move: {angle}, {isMoved}");

        // 입력 상태 변화 감지
        if (inputDirection != lastDirection || isMoved != wasMoved)
        {
            networkSender.SendChangeDirMessage(angle, isMoved);

            if (isMoved)
            {
                // start audio
                AudioManager.Instance.StartWalkingSound();
            }
            else
            {
                // stopch audio
                AudioManager.Instance.StopWalkingSound();
            }

            // 상태 업데이트
            lastDirection = inputDirection;
            wasMoved = isMoved;
        }
    }

    private void HandleBullet()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            Debug.LogError("Player tag에 해당하는 객체 없음");
            return; // Player가 없으면 함수 종료
        }
        else
        {
            snowSlashEffect = player.transform.Find("SnowSlashEffect").gameObject;
        }

        if (Input.GetMouseButton(0) && !lastClickState) // 마우스 왼쪽 버튼 클릭 눌려있는 동안
        {
            lastClickState = true;
            networkSender.SendChangeBulletStateMessage(ClientId, true);

            // SnowSlashEffect 활성화
            if (!snowSlashEffect.activeSelf)
            {
                snowSlashEffect.SetActive(true);
            }
        }
        else if (!Input.GetMouseButton(0) && lastClickState)
        {
            lastClickState = false;
            networkSender.SendChangeBulletStateMessage(ClientId, false);

            // SnowSlashEffect 비활성화
            if (snowSlashEffect.activeSelf)
            {
                snowSlashEffect.SetActive(false);
            }
        }
    }

    private void HandleDash()
    {
        bool dash = false;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (ClientManager.Instance.CanDoDash())
            {
                DashDebouncer.Debounce(300, () =>
                {
                    AudioManager.Instance.PlaySfx(AudioManager.Sfx.Dash);
                });
            }
            dash = true;
            networkSender.SendDoDash(dash);
        }
    }

    void HandleDirection()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            // Debug.LogError("Player tag에 해당하는 객체 없음");
            return;
        }

        Vector3 playerPosition = player.transform.position;

        // 현재 마우스 위치를 월드 좌표로 변환
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, playerPosition); // Y축 평면 사용
        if (plane.Raycast(ray, out float enter))
        {
            Vector3 mouseWorldPosition = ray.GetPoint(enter);

            // 방향 벡터 계산
            Vector3 direction = (mouseWorldPosition - playerPosition).normalized;

            // 방향 각도 계산
            float angle = Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;

            // Unity의 좌표계에서는 W = 90°, S = -90°이므로 보정
            angle += 90f;

            // 각도를 0~360 범위로 변환
            if (angle < 0)
            {
                angle += 360f;
            }

            // angle 전송
            if (lastSendAngleTime <= 0)
            {
                networkSender.SendChangeAngleMessage(angle);
                lastSendAngleTime = 6;  // 0.1초마다 angle 전송
            }
            lastSendAngleTime--;
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
}
