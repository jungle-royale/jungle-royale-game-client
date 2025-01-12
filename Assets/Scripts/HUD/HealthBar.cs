using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBar : MonoBehaviour
{
    public Slider slider;
    public Gradient gradient;
    public Image fill;
    public TextMeshProUGUI hpText; // HpText를 참조하기 위한 변수

    private void Awake()
    {
        // HealthBar 오브젝트의 자식 중 "HpText" 이름을 가진 게임 오브젝트에서 TextMeshProUGUI 컴포넌트를 찾음
        hpText = transform.Find("HpText")?.GetComponent<TextMeshProUGUI>();

        if (hpText == null)
        {
            Debug.LogError("HpText를 찾을 수 없습니다. HealthBar의 자식 오브젝트에 HpText가 있는지 확인하세요.");
        }
    }

    public void SetMaxHealth(int health)
    {
        slider.maxValue = health;
        slider.value = health;

        fill.color = gradient.Evaluate(1f);

        if (hpText != null)
        {
            hpText.text = $"{health:D3}";
        }
    }

    public void SetHealth(int health)
    {
        slider.value = health;

        fill.color = gradient.Evaluate(slider.normalizedValue);

        if (hpText != null)
        {
            hpText.text = $"{health:D3}";
        }
    }
}