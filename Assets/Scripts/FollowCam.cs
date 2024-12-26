using UnityEngine;

public class FollowCam : MonoBehaviour
{
    [SerializeField]
    private Transform target; // 카메라가 따라갈 대상

    [SerializeField]
    private Vector3 offset = new Vector3(0, 5, -10); // 타겟과의 거리 오프셋

    [SerializeField]
    private float followSpeed = 5f; // 따라가는 속도

    private void FixedUpdate()
    {
        if (target == null)
        {
            return; // 타겟이 없으면 카메라를 움직이지 않음
        }

        // 목표 위치 계산 (타겟 위치 + 오프셋)
        Vector3 targetPosition = target.position + offset;

        // 부드럽게 타겟 위치로 이동
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);

        // Side View를 유지하기 위해 회전 고정
        transform.rotation = Quaternion.Euler(30, 0, 0); // 예시: 고정된 각도
    }

    // 타겟을 동적으로 설정하는 메서드
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}