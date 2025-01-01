using UnityEngine;
using System;

public class InputManager : Singleton<InputManager>
{
    // 이동
    private Vector2 lastDirection = Vector2.zero; // 이전 프레임의 방향
    private bool wasMoved = false;                // 이전 프레임의 이동 상태

    private string ClientId;


    // 이벤트 정의
    public event Action<bool> Dash;
    public event Action<float, bool> Move;
    public event Action<float> Direction;
    
    public event Action<string, float, float, float> Bullet;


    private string previousMouseDirection = ""; // 이전 8방향 저장



    void Update()
    {
        HandleMove();
        HandleDash();
        HandleDirection();
        HandleBullet();
    }

    public void ConfigureClientId(string clientId)
    {
        ClientId = clientId;
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

        // 입력 상태 변화 감지
        if (inputDirection != lastDirection || isMoved != wasMoved)
        {
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
            // Debug.LogError("Player tag에 해당하는 객체 없음");
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

                // Debug.Log($"click: {clickPosition}, player: {playerPosition}");
                // Debug.Log($"direction: {direction}");
                // Debug.Log($"angle: {angle}");

                Bullet?.Invoke(ClientId, playerPosition.x, playerPosition.z, angle);
            }
        }
    }

    private void HandleDash()
    {
        bool dash = false;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            dash = true;
            Dash?.Invoke(dash);
        }
    }

    void HandleDirection()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            Debug.LogError("Player tag에 해당하는 객체 없음");
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

            // 8방향 문자열로 변환
            string currentMouseDirection = GetDirection(angle);

            // 방향이 변경되었을 때만 이벤트 호출
            if (currentMouseDirection != previousMouseDirection)
            {
                previousMouseDirection = currentMouseDirection;
                Direction?.Invoke(angle);
                Debug.Log("Mouse Direction Changed: " + currentMouseDirection);
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

     string GetDirection(float angle)
    {
        if (angle < 0)
            angle += 360;

        if (angle >= 337.5 || angle < 22.5)
            return "East";
        else if (angle >= 22.5 && angle < 67.5)
            return "Northeast";
        else if (angle >= 67.5 && angle < 112.5)
            return "North";
        else if (angle >= 112.5 && angle < 157.5)
            return "Northwest";
        else if (angle >= 157.5 && angle < 202.5)
            return "West";
        else if (angle >= 202.5 && angle < 247.5)
            return "Southwest";
        else if (angle >= 247.5 && angle < 292.5)
            return "South";
        else if (angle >= 292.5 && angle < 337.5)
            return "Southeast";

        return "";
    }

}