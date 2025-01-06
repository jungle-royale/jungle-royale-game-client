using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Billboard : MonoBehaviour
{
    public Transform cam;

    void Start()
    {
        // MainCamera를 cam에 동적으로 할당
        if (Camera.main != null)
        {
            cam = Camera.main.transform;
        }
        else
        {
            Debug.LogError("[Billboard.cs] MainCamera를 찾을 수 없습니다. 씬에 MainCamera 태그가 있는 카메라가 있는지 확인하세요.");
        }
    }

    void LateUpdate()
    {
        if (cam != null)
        {
            // 카메라를 바라보도록 회전
            transform.LookAt(transform.position + cam.forward);
        }
        else
        {
            Debug.LogError("[Billboard.cs] Camera 없음");
        }
    }
}