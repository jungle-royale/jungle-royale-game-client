using UnityEngine;

public class PlayerSelectEffectRotation : MonoBehaviour
{
    public Transform playerTransform; // 플레이어 Transform (Inspector에서 수동 설정 가능)
    public ParticleSystem thisParticleSystem; // 파티클 시스템 (Inspector에서 수동 설정 가능)

    private ParticleSystem.MainModule mainModule;

    private void Start()
    {
        // 파티클 시스템 확인
        if (thisParticleSystem != null)
        {
            mainModule = thisParticleSystem.main;
        }
        else
        {
            Debug.LogError("ParticleSystem is not assigned!");
        }

        // 부모를 기준으로 플레이어 Transform 자동 탐색
        if (playerTransform == null)
        {
            if (transform.parent != null)
            {
                playerTransform = transform.parent; // 자신의 부모 Transform을 가져옴
            }
            else
            {
                Debug.LogError("Parent Transform not found for the particle system!");
            }
        }
    }

    private void Update()
    {
        if (playerTransform != null && thisParticleSystem != null)
        {
            // 부모(플레이어)의 Y축 회전을 가져와 파티클에 반영
            Vector3 playerRotation = playerTransform.eulerAngles;
            mainModule.startRotationY = Mathf.Deg2Rad * playerRotation.y; // Rotation 값을 라디안으로 변환
        }
    }
}