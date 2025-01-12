using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class BulletBar : MonoBehaviour
{
    public Slider slider;
    public Gradient gradient;
    public Image fill;

    public void SetMaxBulletGage(int gage)
    {
        slider.maxValue = gage;
        slider.value = gage;

        fill.color = gradient.Evaluate(1f);
    }

    public void SetBulletGage(int gage)
    {
        slider.value = gage;

        fill.color = gradient.Evaluate(slider.normalizedValue);
    }

    public void UpdateGradientColor()
    {
        // 새로운 색상 결정
        Color newMaxColor = Color.white;

        switch (ClientManager.Instance.MagicType)
        {
            case MagicType.None:
                newMaxColor = new Color(0.53f, 0.81f, 0.92f); // 하늘색
                break;
            case MagicType.Stone:
                newMaxColor = Color.yellow; // 노란색
                break;
            case MagicType.Fire:
                newMaxColor = Color.red; // 빨간색
                break;
            default:
                Debug.LogWarning($"Invalid magicType: {ClientManager.Instance.MagicType}. Defaulting to white.");
                newMaxColor = Color.white;
                break;
        }

        // Gradient 업데이트
        GradientColorKey[] colorKeys = gradient.colorKeys;
        GradientAlphaKey[] alphaKeys = gradient.alphaKeys;

        // 100% 위치에 있는 색상을 새로운 색상으로 변경
        colorKeys[colorKeys.Length - 1].color = newMaxColor;

        // 새로운 Gradient를 생성하여 업데이트
        Gradient updatedGradient = new Gradient();
        updatedGradient.SetKeys(colorKeys, alphaKeys);
        gradient = updatedGradient;

        // 현재 슬라이더 값에 맞는 색상으로 업데이트
        fill.color = gradient.Evaluate(slider.normalizedValue);
    }
}