using UnityEngine;

public class FollowCam : MonoBehaviour
{
    [SerializeField] private Vector3 offset = new Vector3(0, 5, -10); // 플레이어와 카메라의 상대적 위치
    private Transform playerTransform;

    // 플레이어 객체를 설정하는 메서드
    public void SetPlayer(Transform player)
    {
        playerTransform = player;
    }

    private void LateUpdate()
    {
        if (playerTransform != null)
        {
            // 카메라의 위치를 플레이어를 기준으로 업데이트
            transform.position = playerTransform.position + offset;
        }
    }
}