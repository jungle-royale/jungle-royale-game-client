using UnityEngine;
using UnityEngine.UI;

public class FullScreenButton : MonoBehaviour
{

    public Image uiImage; // UI Image 컴포넌트 참조
    public Sprite expand; // 새로운 스프라이트 1
    public Sprite compress; // 새로운 스프라이트 2

    private Debouncer debouncer = new Debouncer();



    private void OnEnable()
    {
        // 현재 활성화된 Canvas의 자식에서 GuideToggleButton 찾음
        Button[] closeButtons = GetComponentsInChildren<Button>(true);

        foreach (Button closeButton in closeButtons)
        {
            if (closeButton.name == "FullScreenButton")
            {
                // 기존 리스너를 제거하고 새로 추가
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(OnToggleCanvas);
            }
        }
    }

    private void OnToggleCanvas()
    {
        debouncer.Debounce(3000, () => {
            Debug.Log("닫아!!!!!!!!!!!");
            Screen.fullScreen = !Screen.fullScreen;
            ChangeImage(Screen.fullScreen);
        });
    }

    public void ChangeImage(bool isFull)
    {
        if (isFull)
        {
            uiImage.sprite = compress; // sprite2로 변경
        }
        else
        {
            uiImage.sprite = expand; // sprite1로 변경
        }
    }
}