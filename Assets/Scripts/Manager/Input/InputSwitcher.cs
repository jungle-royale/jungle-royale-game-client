using UnityEngine;

public class InputSwitcher : MonoBehaviour
{
    public GameObject moveJoystickUI;
    public GameObject aimJoystickUI;

    public GameObject dashButton;
    public GameObject watchingButton;


    private bool isMobile;

    void Start()
    {
        isMobile = new DeviceCheck().IsMobile();

        // 모바일에서는 조이스틱 활성화
        moveJoystickUI.SetActive(isMobile);
        aimJoystickUI.SetActive(isMobile);
        dashButton.SetActive(isMobile);
        watchingButton.SetActive(isMobile);

        // 데스크톱에서는 키보드 + 마우스 활성화
        if (!isMobile)
        {
            Debug.Log("Desktop mode: Keyboard + Mouse");
        }
        else
        {
            Debug.Log("Mobile mode: Joysticks enabled");
        }
    }

    bool DetectMobile()
    {
        return Screen.width < 800 || Screen.height < 600;
    }
}