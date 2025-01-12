using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingBallon : MonoBehaviour
{
    public float floatingY = 0.3f; // 최대 Y 이동 거리
    public float speed = 2f;      // 이동 속도

    private Vector3 initialPosition;

    void Start()
    {
        // 초기 위치를 저장
        initialPosition = transform.position;
    }

    void Update()
    {
        // 위아래로 부드럽게 움직이는 계산
        float offset = Mathf.Sin(Time.time * speed) * floatingY;
        transform.position = new Vector3(
            initialPosition.x,
            initialPosition.y + offset,
            initialPosition.z
        );
    }
}