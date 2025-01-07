using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Billboard : MonoBehaviour
{
    public Transform cam;
    const float HPBAR_Y = 1.7f; // 고정할 Y축 위치

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

            // 부모 객체의 위치 가져오기 (플레이어 위치)
            Vector3 parentPosition = transform.parent.position;

            if (parentPosition.y < 0)
            {
                return;
            }

            // 고정된 Y축 값과 부모의 X, Z 값을 사용해 위치 설정
            transform.position = new Vector3(parentPosition.x, HPBAR_Y, parentPosition.z);
        }
        else
        {
            Debug.LogError("[Billboard.cs] Camera 없음");
        }
    }
}