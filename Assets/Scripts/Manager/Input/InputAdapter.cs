using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class InputAdapter : MonoBehaviour
{
    public VariableJoystick moveJoystick;
    public VariableJoystick aimJoystick;

    public Button dashButton;
    public Button watchingButton;

    private bool isDashButtonPressed; // 대시 버튼 눌림 상태
    private bool isWatchingButtonPressed; // 대시 버튼 눌림 상태

    public bool isMobile;

    void Start()
    {
        isMobile = new DeviceCheck().IsMobile();

        // 대시 버튼의 클릭 이벤트 등록
        if (dashButton != null)
        {
            dashButton.onClick.AddListener(() =>
            {
                isDashButtonPressed = true; // 버튼 눌림 상태 설정
            });
        }
        if (watchingButton != null)
        {
            watchingButton.onClick.AddListener(() =>
            {
                isWatchingButtonPressed = true; // 버튼 눌림 상태 설정
            });
        }
    }

    public float GetAxisX()
    {
        if (isMobile)
        {
            return moveJoystick.Horizontal;
        }
        return Input.GetAxisRaw("Horizontal");
    }

    public float GetAxisY()
    {
        if (isMobile)
        {
            return moveJoystick.Vertical;
        }
        return Input.GetAxisRaw("Vertical");
    }

    public bool GetMouseLeftButton()
    {
        if (isMobile)
        {
            return aimJoystick.Direction.magnitude > 0.1f; // 입력값이 존재하면 true
        }
        return Input.GetMouseButton(0);
    }
    public bool GetTab()
    {
        if (isMobile)
        {
            if (isWatchingButtonPressed)
            {
                isWatchingButtonPressed = false;
                return true;
            }
            return false;
        }
        return Input.GetKeyDown(KeyCode.Tab);
    }

    public bool GetSpace()
    {
        if (isMobile)
        {
            if (dashButton)
            {
                if (isDashButtonPressed)
                {
                    isDashButtonPressed = false;
                }
                return true;
            }
            return false;
        }
        return Input.GetKeyDown(KeyCode.Space);
    }

    public float GetCurrentPlayerAngle()
    {
        if (isMobile)
        {
            return GetJoystickPlayerAngle();
        }
        else
        {
            return GetDesktopPlayerAngle();
        }
    }

    private float GetJoystickPlayerAngle()
    {
        // 조이스틱의 방향값 가져오기
        Vector2 direction = aimJoystick.Direction;

        // 조이스틱 입력이 없을 경우 (0, 0), 기본 각도 반환
        if (direction == Vector2.zero)
        {
            return 0f;
        }

        // 방향 각도 계산
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Unity 좌표계에 맞게 보정 (위쪽이 0도, 오른쪽이 90도)
        angle += 90f;

        // 각도를 0~360 범위로 변환
        if (angle < 0)
        {
            angle += 360f;
        }

        return angle;
    }

    private float GetDesktopPlayerAngle()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            return 0;
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

            return angle;
        }
        return 0;
    }

  

}