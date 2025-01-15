using System.Collections;
using UnityEngine;
using TMPro; // TextMeshPro 네임스페이스 추가

public class ItemUIManager : MonoBehaviour
{
    [SerializeField] private CanvasGroup itemCanvasGroup; // CanvasGroup 참조
    [SerializeField] private TextMeshProUGUI itemNameText; // TextMeshProUGUI로 변경
    [SerializeField] private TextMeshProUGUI itemDescriptionText; // TextMeshProUGUI로 변경
    [SerializeField] private RectTransform canvasRectTransform; // Canvas RectTransform
    [SerializeField] private float fadeDuration = 0.5f; // Fade 효과 시간
    const float CANVAS_Y = 2.1f;

    private Coroutine fadeCoroutine;

    public void ShowItemInfo(string name, string description, Transform itemTransform)
    {
        // 텍스트 설정
        itemNameText.text = name;
        itemDescriptionText.text = description;

        // Canvas를 Trigger가 발생한 아이템 위치로 이동
        UpdateCanvasPosition(itemTransform);

        // Fade-in 시작
        StartFade(true);
    }

    public void HideItemInfo()
    {
        StartFade(false);
    }

    private void StartFade(bool fadeIn)
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        fadeCoroutine = StartCoroutine(FadeCanvasGroup(fadeIn));
    }

    private IEnumerator FadeCanvasGroup(bool fadeIn)
    {
        float startAlpha = itemCanvasGroup.alpha;
        float endAlpha = fadeIn ? 1 : 0;
        float elapsed = 0;

        itemCanvasGroup.interactable = fadeIn;
        itemCanvasGroup.blocksRaycasts = fadeIn;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            itemCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / fadeDuration);
            yield return null;
        }

        itemCanvasGroup.alpha = endAlpha;
    }

    private void UpdateCanvasPosition(Transform itemTransform)
    {
        // 아이템의 월드 좌표를 Canvas의 위치로 설정 (World Space Canvas용)
        canvasRectTransform.position = itemTransform.position + new Vector3(0, CANVAS_Y, 0); // 아이템 위로 약간 이동
        canvasRectTransform.LookAt(transform.position + Camera.main.transform.forward); // 카메라를 바라보도록 회전
    }
}