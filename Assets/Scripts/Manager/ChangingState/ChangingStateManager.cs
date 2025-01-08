using System.Collections.Generic;
using System.Data.Common;
using System.Resources;
using Message;
using Unity.VisualScripting;
using UnityEngine;

public class ChangingStateManager : MonoBehaviour
{
    public PlayerManager playerManager;
    public BulletManager bulletManager;
    private WebGLHapticManager HaptickManager = new WebGLHapticManager();

    private float BULLET_Y = 0.9f;

    private void Awake()
    {
        // PlayerManager를 찾거나 연결
        playerManager = FindObjectOfType<PlayerManager>();
        if (playerManager == null)
        {
            Debug.LogError("PlayerManager 없음");
        }

        bulletManager = FindObjectOfType<BulletManager>();
        if (bulletManager == null)
        {
            Debug.LogError("BulletManager 없음");
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

        foreach (var state in GetItemStateList)
        {
            HandleGetItemState(state);
        }

        foreach (var deadState in PlayerDeadStateList)
        {
            HandlePlayerDeadState(deadState);
        }
    }

    private void HandlePlayerDeadState(PlayerDeadState state)
    {
        // TODO: 여기서 카메라 range check

        // falling인 경우 애니메이션과 다른 이유로 죽었을 때 애니메이션 분리
        // 애니메이션에서 

        GameObject player = playerManager.GetPlayerById(state.deadPlayerId);
        if (player == null)
        {
            return;
        }
        Animator animator = player.GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogWarning($"Animator not found on player: {player.name}");
            return;
        }

        // 1. 나 처리

        // - PlayerDead 이벤트를 발행하여 키를 막음
        if (state.deadPlayerId == ClientManager.Instance.ClientId)
        {
            EventBus<InputButtonEventType>.Publish(InputButtonEventType.StopPlay);
        }
        // 다른 사람인 경우
        
        // 2. 공통 처리

        // dead sound가 falling이랑 육지에서 죽을 때랑 다르거나, 공통된 소리를 쓸 수 있게 해야 함
        if (state.IsFall())
        {
            AudioManager.Instance.PlaySfx(AudioManager.Sfx.Dead, 1.0f);
            animator.SetTrigger("byeSnowman");
        } 
        else 
        {
            AudioManager.Instance.PlaySfx(AudioManager.Sfx.Dead, 1.0f);
            animator.SetTrigger("byeSnowman");
        }

    }

    private void UpdateFallDead(string deadPlayerId)
    {
        GameObject player = playerManager.GetPlayerById(deadPlayerId);
        
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

        if (state.ObjectId == ClientManager.Instance.ClientId)
        {
            HaptickManager.TriggerHaptic(100);
        }

        AudioManager.Instance.PlayHitSfx(0.7f);

        // 타입별 이펙트 재생
        Transform playerEffect = null;

        switch (state.BulletType)
        {
            case 0:
                playerEffect = player.transform.Find("HitSnow");
                break;
            case 1:
                playerEffect = player.transform.Find("HitStone");
                break;
            case 2:
                playerEffect = player.transform.Find("HitFire");
                break;
            default:
                break;
        }

        if (playerEffect == null)
        {
            Debug.LogWarning($"Effect not found on player {state.ObjectId}.");
            return;
        }

        // 자식 파티클 시스템 모두 가져오기
        ParticleSystem[] childParticleSystems = playerEffect.GetComponentsInChildren<ParticleSystem>();

        // 각 파티클 시스템 재생
        foreach (var particleSystem in childParticleSystems)
        {
            particleSystem.Play();
        }
    }


    private void HandleObjectHitBulletState(HitBulletState state)
    {

        AudioManager.Instance.PlayHitSfx(0.7f);

        GameObject objectEffectPrefab = Resources.Load<GameObject>("Prefabs/Effects/HitNormal");
        GameObject objectEffect = Instantiate(objectEffectPrefab, new Vector3(state.X, BULLET_Y, state.Y), Quaternion.identity);

        if (objectEffect == null)
        {
            Debug.LogWarning($"Effect not found on player {state.ObjectId}.");
            return;
        }

        ParticleSystem[] childParticleSystems = objectEffect.GetComponentsInChildren<ParticleSystem>();

        // 각 파티클 시스템 재생
        foreach (var particleSystem in childParticleSystems)
        {
            particleSystem.Play();
        }
    }

    private void HandleGetItemState(GetItemState state)
    {
        // TODO: 여기서 카메라 range check

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