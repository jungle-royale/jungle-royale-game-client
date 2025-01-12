using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Specialized;
using System.Web;
using ZXing;
using ZXing.QrCode;

public class QRCodeGenerator : MonoBehaviour
{
    public GameObject canvas; // RawImage를 생성할 부모 Canvas
    public Toggle qrToggle; // QR 이미지를 열고 닫을 토글
    private GameObject rawImageObject; // 동적으로 생성된 RawImage 오브젝트
    private string qrData;

    void Start()
    {
        canvas = GameObject.Find("MainCanvas");
        if (canvas == null)
        {
            Debug.LogError("QR을 생성할 Canvas를 찾을 수 없음");
        }

        // MainCanvas 내 QRToggle 찾기
        if (qrToggle == null)
        {
            GameObject qrToggleObject = GameObject.Find("QRToggle");
            if (qrToggleObject != null)
            {
                qrToggle = qrToggleObject.GetComponent<Toggle>();
            }
            else
            {
                Debug.LogError("QRToggle GameObject를 찾을 수 없습니다.");
                return;
            }
        }

        // Application URL 처리
#if UNITY_EDITOR
        var url = "http://game.eternalsnowman.com/room?roomId=test";
#else
        var url = Application.absoluteURL;
#endif
        try
        {
            Uri uri = new Uri(url);
            NameValueCollection queryParameters = HttpUtility.ParseQueryString(uri.Query);
            string roomId = queryParameters["roomId"];
            qrData = $"http://game.eternalsnowman.com/room/ready?roomId=${roomId}";
        }
        catch (UriFormatException e)
        {
            Debug.LogError($"Invalid URL format: {e.Message}");
            qrData = "";
        }

        // 토글 이벤트에 메서드 연결
        if (qrToggle != null)
        {
            qrToggle.onValueChanged.AddListener(OnToggleValueChanged);
        }
    }

    private void OnToggleValueChanged(bool isOn)
    {
        if (isOn)
        {
            // QR 이미지 생성 및 표시
            GenerateAndDisplayQRCode(qrData, 256, 256);
        }
        else
        {
            // QR 이미지 숨김
            if (rawImageObject != null)
            {
                Destroy(rawImageObject);
                rawImageObject = null;
            }
        }
    }

    public void GenerateAndDisplayQRCode(string text, int width, int height)
    {
        if (string.IsNullOrEmpty(text))
        {
            Debug.LogError("QR Data is empty. Cannot generate QR Code.");
            return;
        }

        // QR 코드 텍스처 생성
        Texture2D qrTexture = GenerateQRCode(text, width, height);

        // RawImage 동적 생성
        rawImageObject = new GameObject("QRCodeImage");
        rawImageObject.transform.SetParent(canvas.transform, false); // Canvas의 자식으로 설정
        RawImage rawImage = rawImageObject.AddComponent<RawImage>();
        rawImage.texture = qrTexture; // 생성한 QR 코드를 텍스처로 설정

        // RawImage RectTransform 설정
        RectTransform rectTransform = rawImage.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(width, height); // 크기 설정
        rectTransform.anchoredPosition = Vector2.zero; // 위치 설정 (Canvas의 중심)
    }

    private Texture2D GenerateQRCode(string text, int width, int height)
    {
        // QR 코드 생성기
        var qrWriter = new QRCodeWriter();
        var bitMatrix = qrWriter.encode(text, BarcodeFormat.QR_CODE, width, height);

        // QR 코드 텍스처 생성
        Texture2D texture = new Texture2D(width, height);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                bool isBlack = bitMatrix[x, y];
                texture.SetPixel(x, y, isBlack ? Color.black : Color.white);
            }
        }
        texture.Apply();
        return texture;
    }
}