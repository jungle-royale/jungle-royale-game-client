using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public CameraHandler cameraHandler;

    public GameObject currentPlayerPrefab; // 내 플레이어 프리팹
    public GameObject otherPlayerPrefab;   // 다른 플레이어 프리팹

    private bool currentPlayerDead = false;

    private GameObject currentPlayer; // 현재 플레이어 객체
    private Dictionary<string, GameObject> otherPlayers = new Dictionary<string, GameObject>();

    private HashSet<string> movePlayers = new HashSet<string>();
    private HashSet<string> dashPlayers = new HashSet<string>();

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
        cameraHandler.UpdateCamera(playerDataList);

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
        currentPlayer = Instantiate(currentPlayerPrefab, new Vector3(data.x, PLAYER_Y, data.y), Quaternion.identity);
        currentPlayer.tag = "Player";
        currentPlayer.name = "MyPlayer";

        // 플레이어의 HealthBar 초기화
        HealthBar healthBarComponent = currentPlayer.GetComponentInChildren<HealthBar>();
        if (healthBarComponent != null)
        {
            healthBarComponent.SetMaxHealth(data.health);
        }
    }

    private void CreateOtherPlayer(Player data)
    {
        GameObject newPlayer = Instantiate(otherPlayerPrefab, new Vector3(data.x, PLAYER_Y, data.y), Quaternion.identity);

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
     
        if (serverData.isDashing)
        {
            if (movementDirection != Vector3.zero)
            {
                Quaternion tiltRotation = Quaternion.LookRotation(movementDirection.normalized); // 이동 방향을 기준으로 회전
                                                                                                    // Y축 기울이기 (Roll 추가)
                Quaternion tilt = Quaternion.Euler(
                    tiltRotation.eulerAngles.x + 10,               // 상하 기울임 유지
                    tiltRotation.eulerAngles.y,
                    tiltRotation.eulerAngles.z
                );

                player.transform.rotation = tilt;
            }
        }
        else
        {
            dashPlayers.Remove(serverData.id);
            Quaternion uprightRotation = Quaternion.Euler(0, -(serverData.angle - 180), 0);
            player.transform.rotation = uprightRotation;
        }

        if (serverData.isMoved)
        {
            if (!movePlayers.Contains(serverData.id))
            {
                movePlayers.Add(serverData.id);
                animator.SetBool("isMoving", true);
                Debug.Log("moving true");
            }
        }
        else
        {
            if (movePlayers.Contains(serverData.id))
            {
                movePlayers.Remove(serverData.id);
                animator.SetBool("isMoving", false);
                Debug.Log("moving false");
            }
        }

        // 현재 위치 조정
        var newPosition = player.transform.position;
        newPosition.x = serverData.x;
        newPosition.z = serverData.y;
        player.transform.position = newPosition;
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

        if (!currentPlayerDead && !existingIds.Contains(currentPlayerId))
        {
            currentPlayerDead = true;
            Destroy(currentPlayer);
            movePlayers.Remove(currentPlayerId);
            dashPlayers.Remove(currentPlayerId);

            AudioManager.Instance.PlaySfx(AudioManager.Sfx.Dead, 1.0f);
            AudioManager.Instance.PlaySfx(AudioManager.Sfx.GameOver, 0.7f);
            EventBus<InGameGUIEventType>.Publish(InGameGUIEventType.ActivateCanvas, "GameOver");
            EventBus<InputButtonEventType>.Publish(InputButtonEventType.PlayerDead);
        }

        foreach (var key in keysToRemove)
        {
            Destroy(otherPlayers[key]);
            otherPlayers.Remove(key);
            movePlayers.Remove(key);
            dashPlayers.Remove(key);
        }
    }
}