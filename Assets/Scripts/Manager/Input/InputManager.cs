using UnityEngine;
using System;

public class InputManager : MonoBehaviour
{
    public InputNetworkSender networkSender; // 서버로 입력 정보를 보내는 클래스 참조
    public CameraManager cameraManager;

    public InputAdapter input;


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

    private bool IsConnected = false;
    private bool EndGame = false;
    private bool IsActivateTab = false;

    void Start()
    {
        EventBus<InputButtonEventType>.Subscribe(InputButtonEventType.CompleteConnect, CompleteConnection);
        EventBus<InputButtonEventType>.Subscribe(InputButtonEventType.StopPlay, StopPlay);
        EventBus<InputButtonEventType>.Subscribe(InputButtonEventType.ActivateTabKey, ActivateTabKey);
    }

    private void OnDestroy()
    {
        EventBus<InputButtonEventType>.Unsubscribe(InputButtonEventType.CompleteConnect, CompleteConnection);
        EventBus<InputButtonEventType>.Unsubscribe(InputButtonEventType.StopPlay, StopPlay);
        EventBus<InputButtonEventType>.Unsubscribe(InputButtonEventType.ActivateTabKey, ActivateTabKey);
    }

    void Update()
    {
        if (!IsConnected)
        {
            return;
        }
        if (IsActivateTab)
        {
            HandleTab();
        }
        if (!EndGame)
        {
            HandleBullet();
            HandleMove();
            HandleDash();
            HandleDirection();
        }
    }

    private void CompleteConnection()
    {
        IsConnected = true;
    }

    private void StopPlay()
    {
        EndGame = true;
        input.DeactivateButton();

        // 죽었을 때, 종료 시, 키 정리(쏘던 거 멈춤)를 위해 서버에 데이터 보냄
        networkSender.SendChangeBulletStateMessage(ClientId, false);
        networkSender.SendChangeDirMessage(0, false);
    }

    private void ActivateTabKey()
    {
        IsActivateTab = true;
    }


    private void HandleTab()
    {
        if (input.GetTab())
        {
            cameraManager.SwitchToNextPlayer(); // TODO: next button에도 추가
        }
    }

    private void HandleMove()
    {
        // WASD 입력 벡터 계산
        float x = input.GetAxisX();
        float y = input.GetAxisY();
        Vector2 inputDirection = new Vector2(x, y);


        // 입력 벡터로 방향 및 이동 상태 계산
        float angle = CalculateAngle(inputDirection);
        bool isMoved = inputDirection != Vector2.zero;

        // Debug.Log($"anlge, move: {angle}, {isMoved}");

        // 입력 상태 변화 감지
        if (inputDirection != lastDirection || isMoved != wasMoved)
        {
            networkSender.SendChangeDirMessage(angle, isMoved);

            // Debug.Log($"🍎 {isMoved}");

            // 상태 업데이트
            lastDirection = inputDirection;
            wasMoved = isMoved;
        }
    }

    private void HandleBullet()
    {
        if (input.GetMouseLeftButton() && !lastClickState) // 마우스 왼쪽 버튼 클릭 눌려있는동안
        {
            lastClickState = true;
            networkSender.SendChangeBulletStateMessage(ClientId, true);
        }
        else if (!input.GetMouseLeftButton() && lastClickState)
        {
            lastClickState = false;
            networkSender.SendChangeBulletStateMessage(ClientId, false);
        }
    }

    private void HandleDash()
    {
        bool dash = false;

        if (input.GetSpace())
        {
            dash = true;
            networkSender.SendDoDash(dash);
        }
    }

    void HandleDirection()
    {
        if (lastSendAngleTime <= 0)
        {
            float angle = input.GetCurrentPlayerAngle();
            networkSender.SendChangeAngleMessage(angle);
            lastSendAngleTime = 6; // 0.1초마다 angle 전송
        }
        lastSendAngleTime--;
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
