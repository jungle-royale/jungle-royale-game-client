using System.Collections.Generic;
using Message;
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
        foreach (var state in HitBulletStateList)
        {
            if (state.IsPlayer())
            {
                HandlePlayerHitBulletState(state);
            }
            else
            {
                HandleObjectHitBulletState(state);
            }
        }

        if (GetItemStateList != null && GetItemStateList.Count > 0)
        {
            foreach (var state in GetItemStateList)
            {
                HandleGetItemState(state);
            }
        }
    }

    private void HandlePlayerHitBulletState(HitBulletState state)
    {
        // TODO: 여기서 카메라 range check

        // PlayerManager에서 플레이어 객체 가져오기
        GameObject player = playerManager.GetPlayerById(state.ObjectId);
        if (player == null)
        {
            Debug.LogWarning($"Player with ID {state.ObjectId} not found.");
            return;
        }

        AudioManager.Instance.PlayHitSfx(0.7f);

        // SnowHitEffect(부모 객체) 찾기
        Transform effectTransform = player.transform.Find("SnowHitEffect");
        if (effectTransform == null)
        {
            Debug.LogWarning($"SnowHitEffect not found on player {state.ObjectId}.");
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


    private void HandleObjectHitBulletState(HitBulletState state)
    {


    }

    private void HandleGetItemState(GetItemState state)
    {
        if (playerManager == null)
        {
            Debug.LogWarning("PlayerManager is not initialized.");
            return;
        }

        GameObject player = playerManager.GetPlayerById(state.playerId);
        if (player == null)
        {
            Debug.LogWarning($"Player with ID {state.playerId} not found.");
            return;
        }

        if (state.itemType == 0) // healpack
        {
            PlayHealEffectWithSfx(player);
        }
        else if (state.itemType == 1) // stone magic
        {

        }
        else if (state.itemType == 2) // fire magic
        {

        }
        else
        {
            Debug.Log($"itemType {state.itemType} 없음");
        }
    }

    private void PlayHealEffectWithSfx(GameObject player)
    {
        AudioManager.Instance.PlaySfx(AudioManager.Sfx.Heal, 0.7f);

        Transform healEffectTransform = player.transform.Find("HealEffect");
        if (healEffectTransform == null)
        {
            Debug.LogWarning("HealEffect not found.");
            return;
        }

        // 자식 파티클 시스템 모두 가져오기
        ParticleSystem[] childParticleSystems = healEffectTransform.GetComponentsInChildren<ParticleSystem>();

        // 각 파티클 시스템 재생
        foreach (var particleSystem in childParticleSystems)
        {
            particleSystem.Play();
        }
    }
}