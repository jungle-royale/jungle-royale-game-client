using UnityEngine;
using UnityEngine.UI;

public class DamageEffect : MonoBehaviour
{
    [SerializeField] private Image redOverlay; // 붉은 이미지
    [SerializeField] private float flashDuration = 0.2f; // 깜빡이는 지속 시간

    private Color _transparentRed = new Color(1f, 0f, 0f, 0f); // 투명한 붉은색
    private Color _opaqueRed = new Color(1f, 0f, 0f, 0.2f); // 알파값 20%의 붉은색
    private bool _isFlashing = false;

    private void Start()
    {
        // 시작할 때 이미지를 투명하게 설정
        if (redOverlay != null)
        {
            redOverlay.color = _transparentRed;
        }
    }

    public void TriggerDamageEffect()
    {
        if (!_isFlashing)
        {
            StartCoroutine(FlashRed());
        }
    }

    private System.Collections.IEnumerator FlashRed()
    {
        _isFlashing = true;

        // 알파값 20%의 붉은색으로 변경
        if (redOverlay != null)
        {
            redOverlay.color = _opaqueRed;
        }

        // 지정된 시간 동안 대기
        yield return new WaitForSeconds(flashDuration);

        // 다시 투명하게 변경
        if (redOverlay != null)
        {
            redOverlay.color = _transparentRed;
        }

        _isFlashing = false;
    }
}