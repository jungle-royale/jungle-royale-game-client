using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ChangingStateManager : MonoBehaviour
{
    public PlayerManager playerManager;

    private void Awake()
    {
        // PlayerManager를 찾거나 연결
        playerManager = FindObjectOfType<PlayerManager>();
        if (playerManager == null)
        {
            Debug.LogError("PlayerManager not found in the scene.");
        }
    }

    public void UpdateState
    (
        List<HitBulletState> HitBulletStateList,
        List<GetItemState> GetItemStateList,
        List<PlayerDeadState> PlayerDeadStateList
    )
    {
        if (HitBulletStateList != null && HitBulletStateList.Count > 0)
        {
            foreach (var state in HitBulletStateList)
            {
                HandleHitBulletState(state);
            }
        }

        // 다른 상태 (GetItemState, PlayerDeadState 등) 처리 로직 추가 가능
    }

    private void HandleHitBulletState(HitBulletState state)
    {
        if (playerManager == null)
        {
            Debug.LogWarning("PlayerManager is not initialized.");
            return;
        }

        // PlayerManager에서 플레이어 객체 가져오기
        GameObject player = playerManager.GetPlayerById(state.PlayerId);
        if (player == null)
        {
            Debug.LogWarning($"Player with ID {state.PlayerId} not found.");
            return;
        }

        AudioManager.Instance.PlayHitSfx(0.7f);

        // SnowHitEffect(부모 객체) 찾기
        Transform effectTransform = player.transform.Find("SnowHitEffect");
        if (effectTransform == null)
        {
            Debug.LogWarning($"SnowHitEffect not found on player {state.PlayerId}.");
            return;
        }

        // 자식 파티클 시스템 모두 가져오기
        ParticleSystem[] childParticleSystems = effectTransform.GetComponentsInChildren<ParticleSystem>();

        // 각 파티클 시스템 재생
        foreach (var particleSystem in childParticleSystems)
        {
            particleSystem.Play();
        }
    }
}