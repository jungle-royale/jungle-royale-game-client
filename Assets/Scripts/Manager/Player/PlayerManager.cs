using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public GameObject playerPrefab; // 내 플레이어 프리팹
    // public GameObject otherPlayerPrefab;   // 다른 플레이어 프리팹
    private bool currentPlayerDead = false;

    // 이펙트
    private GameObject shootingEffect;

    private GameObject currentPlayer; // 현재 플레이어 객체
    private Dictionary<string, GameObject> otherPlayers = new Dictionary<string, GameObject>();

    private HashSet<string> movePlayers = new HashSet<string>();
    private HashSet<string> dashPlayers = new HashSet<string>();
    private HashSet<string> shootingPlayers = new HashSet<string>();

    private string currentPlayerId
    {
        get
        {
            return ClientManager.Instance.ClientId;
        }
    }

    private float PLAYER_Y;

    public GameObject GetPlayerById(string playerId)
    {
        if (playerId == currentPlayerId)
        {
            return currentPlayer;
        }

        if (otherPlayers.ContainsKey(playerId))
        {
            return otherPlayers[playerId];
        }

        return null;
    }

    public void UpdatePlayers(List<Player> playerDataList)
    {

        // 카메라 비교해서 update할 아이들만 update하기

        int activePlayerNumber = 0;

        foreach (var data in playerDataList)
        {
            if (data.id == currentPlayerId)
            {
                if (currentPlayer == null)
                {
                    CreateCurrentPlayer(data);
                }
                else
                {
                    ValidateCurrentPlayer(data);
                }
            }
            else
            {
                activePlayerNumber += 1;
                if (otherPlayers.ContainsKey(data.id))
                {
                    // 기존 플레이어 업데이트
                    UpdatePlayer(otherPlayers[data.id], data);
                }
                else
                {
                    // 새로운 플레이어 생성
                    CreateOtherPlayer(data);
                }
            }
        }

        // 제거할 플레이어 처리
        RemoveDisconnectedPlayers(playerDataList);
        EventBus<InGameGUIEventType>.Publish(InGameGUIEventType.UpdatePlayerCountLabel, activePlayerNumber + 1);
    }

    private void CreateCurrentPlayer(Player data)
    {
        currentPlayer = Instantiate(playerPrefab, new Vector3(data.x, PLAYER_Y, data.y), Quaternion.identity);
        currentPlayer.tag = "Player";
        currentPlayer.name = ClientManager.Instance.CurrentPlayerName;

        // 플레이어의 HealthBar 초기화
        HealthBar healthBarComponent = currentPlayer.GetComponentInChildren<HealthBar>();
        if (healthBarComponent != null)
        {
            healthBarComponent.SetMaxHealth(data.health);
        }
    }

    private void CreateOtherPlayer(Player data)
    {
        GameObject newPlayer = Instantiate(playerPrefab, new Vector3(data.x, PLAYER_Y, data.y), Quaternion.identity);

        // 플레이어의 HealthBar 초기화
        HealthBar healthBarComponent = newPlayer.GetComponentInChildren<HealthBar>();
        if (healthBarComponent != null)
        {
            healthBarComponent.SetMaxHealth(data.health);
        }

        otherPlayers[data.id] = newPlayer;
    }

    private void ValidateCurrentPlayer(Player serverData)
    {
        // HealthBar 업데이트
        HealthBar healthBarComponent = currentPlayer.GetComponentInChildren<HealthBar>();
        if (healthBarComponent != null)
        {
            healthBarComponent.SetHealth(serverData.health);
        }

        UpdatePlayerMoveState(currentPlayer, serverData);
        UpdatePlayerShootState(currentPlayer, serverData);
        EventBus<InGameGUIEventType>.Publish(InGameGUIEventType.UpdateHpLabel, serverData.health);
    }

    private void UpdatePlayer(GameObject player, Player serverData)
    {
        // HealthBar 업데이트
        HealthBar healthBarComponent = player.GetComponentInChildren<HealthBar>();
        if (healthBarComponent != null)
        {
            healthBarComponent.SetHealth(serverData.health);
        }
        UpdatePlayerMoveState(player, serverData);
        UpdatePlayerShootState(player, serverData);
    }

    private void UpdatePlayerMoveState(GameObject player, Player serverData)
    {
        var currentPosition = serverData.NewPosition(PLAYER_Y);
        var previousPosition = player.transform.position;
        Vector3 movementDirection = currentPosition - previousPosition;

        Animator animator = player.GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogWarning($"Animator not found on player: {player.name}");
            return;
        }

        // DustTrail 파티클 시스템 가져오기
        var dustTrail = player.transform.Find("DustTrail").gameObject;

        if (dustTrail == null)
        {
            Debug.LogWarning($"DustTrail ParticleSystem not found on player: {player.name}");
        }

        if (serverData.isDashing)
        {
            if (!dashPlayers.Contains(serverData.id))
            {
                dashPlayers.Add(serverData.id);
                AudioManager.Instance.PlaySfx(AudioManager.Sfx.Dash);
            }
            if (movementDirection != Vector3.zero)
            {
                Quaternion tiltRotation = Quaternion.LookRotation(movementDirection.normalized); // 이동 방향을 기준으로 회전
                                                                                                 // Y축 기울이기 (Roll 추가)
                Quaternion tilt = Quaternion.Euler(
                    10,               // 상하 기울임 유지
                    tiltRotation.eulerAngles.y,
                    0
                );

                player.transform.rotation = tilt;
            }

            // DustTrail 파티클 활성화
            if (dustTrail != null)
            {
                dustTrail.SetActive(true);
            }
        }
        else
        {
            if (dashPlayers.Contains(serverData.id))
            {
                dashPlayers.Remove(serverData.id);
            }
            Quaternion uprightRotation = Quaternion.Euler(0, -(serverData.angle - 180), 0);
            player.transform.rotation = uprightRotation;

            // DustTrail 파티클 비활성화
            if (dustTrail != null)
            {
                dustTrail.SetActive(false);
            }
        }

        if (serverData.isMoved)
        {
            if (!movePlayers.Contains(serverData.id))
            {
                movePlayers.Add(serverData.id);
                animator.SetBool("isMoving", true);
            }
        }
        else
        {
            if (movePlayers.Contains(serverData.id))
            {
                movePlayers.Remove(serverData.id);
                animator.SetBool("isMoving", false);
            }
        }

        // 현재 위치 조정
        var newPosition = player.transform.position;
        newPosition.x = serverData.x;
        newPosition.z = serverData.y;
        player.transform.position = newPosition;
    }

    private void UpdatePlayerShootState(GameObject player, Player serverData)
    {
        switch (serverData.magicType)
        {
            case 0:
                shootingEffect = player.transform.Find("ShootingIce").gameObject;
                break;
            case 1:
                shootingEffect = player.transform.Find("ShootingStone").gameObject;
                break;
            case 2:
                shootingEffect = player.transform.Find("ShootingFire").gameObject;
                break;
            default:
                Debug.Log($"{serverData.magicType}에 해당하는 Player의 MagicType 없음");
                break;
        }

        if (shootingEffect == null)
        {
            Debug.LogError("Shooting 이펙트 없음");
        }

        if (serverData.isShooting)
        {
            if (dashPlayers.Contains(serverData.id))
            {
                shootingEffect.SetActive(false);
                shootingPlayers.Remove(serverData.id);
            }
            else
            {
                if (!shootingPlayers.Contains(serverData.id))
                {
                    Debug.Log("isShooting True");
                    shootingPlayers.Add(serverData.id);
                    shootingEffect.SetActive(true);
                }
            }
        }
        else
        {
            if (shootingPlayers.Contains(serverData.id))
            {
                Debug.Log("isShooting False");
                shootingPlayers.Remove(serverData.id);

                // Shooting Effect 비활성화
                if (shootingEffect.activeSelf)
                {
                    shootingEffect.SetActive(false);
                }
            }
        }
    }

    private void RemoveDisconnectedPlayers(List<Player> playerDataList)
    {
        var existingIds = new HashSet<string>(playerDataList.ConvertAll(p => p.id));
        var keysToRemove = new List<string>();

        foreach (var key in otherPlayers.Keys)
        {
            if (!existingIds.Contains(key))
            {
                keysToRemove.Add(key);
            }
        }

        foreach (var key in keysToRemove)
        {
            // Destroy(otherPlayers[key]);
            otherPlayers.Remove(key);
            movePlayers.Remove(key);
            dashPlayers.Remove(key);
        }

        if (!currentPlayerDead && !existingIds.Contains(currentPlayerId))
        {
            currentPlayerDead = true;
            // Destroy(currentPlayer);
            movePlayers.Remove(currentPlayerId);
            dashPlayers.Remove(currentPlayerId);
        }
    }

    // private 
}