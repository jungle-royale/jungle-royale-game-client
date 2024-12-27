using UnityEngine;
using System;

public class InputManager : MonoBehaviour
{

    // 이동
    private Vector2 lastDirection = Vector2.zero; // 이전 프레임의 방향
    private bool wasMoved = false;                // 이전 프레임의 이동 상태


    // 이벤트 정의
    public static event Action<bool> Dash;
    public static event Action<float, bool> Move;
    public static event Action<float, float, float> Bullet;


    void Update()
    {
        HandleMove();
        HandleShift();
        HandleBullet();
    }

    private void HandleMove()
    {
        // WASD 입력 벡터 계산
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");
        Vector2 inputDirection = new Vector2(x, y);

        // 입력 벡터로 방향 및 이동 상태 계산
        float angle = CalculateAngle(inputDirection);
        bool isMoved = inputDirection != Vector2.zero;

        // 입력 상태 변화 감지
        if (inputDirection != lastDirection || isMoved != wasMoved)
        {
            // SendChangeDirMessage(angle, isMoved);
            Move?.Invoke(angle, isMoved);

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

        if (Input.GetMouseButtonDown(0)) // 마우스 왼쪽 버튼 클릭
        {
            // 클릭한 위치 계산
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
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

                Debug.Log($"click: {clickPosition}, player: {playerPosition}");
                Debug.Log($"direction: {direction}");
                Debug.Log($"angle: {angle}");

                Bullet?.Invoke(playerPosition.x, playerPosition.z, angle);
            }
        }
    }

    private void HandleShift()
    {
        bool dash = false;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            dash = true;
            Dash?.Invoke(dash);
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