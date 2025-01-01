using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Dictionary<string, GameObject> playerObjects = new Dictionary<string, GameObject>();
    private float PLAYER_Y;

    void Start()
    {
        EventBus<PlayerEventType>.Subscribe<IEnumerable<Player>>(PlayerEventType.UpdatePlayerStates, UpdatePlayers);
        EventBus<PlayerEventType>.Subscribe<IEnumerable<PlayerDead>>(PlayerEventType.UpdatePlayeDeadrStates, UpdatePlayerDead);

        GameObject mapPrefab = Resources.Load<GameObject>("Prefabs/Map");
        // PLAYER_Y = mapPrefab.transform.localScale.y / 2;
        PLAYER_Y = 0;
    }

    private void UpdatePlayers(IEnumerable<Player> players)
    {
        HashSet<string> activePlayerIds = new HashSet<string>();
        List<Player> otherActivePlayers = new List<Player>();

        foreach (var player in players)
        {
            activePlayerIds.Add(player.id);

            // Debug.Log($"{player.X}{player.Y}");

            if (player.id != ClientManager.Instance.ClientId)
            {
                otherActivePlayers.Add(player);
            }

            if (!playerObjects.TryGetValue(player.id, out GameObject playerObject))
            {
                GameObject playerPrefab = Resources.Load<GameObject>("Prefabs/Player");
                if (playerPrefab != null)
                {
                    playerObject = Instantiate(playerPrefab, new Vector3(player.x, PLAYER_Y, player.y), Quaternion.identity);

                    // 내 플레이어면 태그를 Player로 설정
                    if (player.id == ClientManager.Instance.ClientId)
                    {
                        playerObject.tag = "Player";
                        playerObject.name = "MyPlayer";
                        Debug.Log($"Created client player for ID: {player.id}");
                    }

                    playerObjects[player.id] = playerObject;
                }
                else
                {
                    Debug.LogError("Player prefab could not be loaded.");
                    return;
                }
            }

            playerObject.transform.position = new Vector3(player.x, PLAYER_Y, player.y);
            playerObject.transform.rotation = Quaternion.Euler(0, -(player.angle - 180), 0);

            EventBus<InGameGUIEventType>.Publish(InGameGUIEventType.UpdatePlayerCountLabel, activePlayerIds.Count);
            if (player.id == ClientManager.Instance.ClientId)
                EventBus<InGameGUIEventType>.Publish(InGameGUIEventType.UpdateHpLabel, player.health);
        }


        HandleActivePlayers(otherActivePlayers);
        RemoveInactivePlayers(activePlayerIds);
    }

    // 죽은애들 처리 추가 필요

    private void HandleActivePlayers(List<Player> otherActivePlayers)
    {
        // audio 처리

        // activePlayerIds;

        // 대시

        // 총알 나갈 때 

        // 이동

        // 죽음

        // 힐

        // get item

        // 
    }

    private void RemoveInactivePlayers(HashSet<string> activePlayerIds)
    {
        List<string> playersToRemove = new List<string>();

        foreach (var playerId in playerObjects.Keys)
        {
            if (!activePlayerIds.Contains(playerId))
            {
                playersToRemove.Add(playerId);
            }
        }

        foreach (var playerId in playersToRemove)
        {
            if (playerObjects.TryGetValue(playerId, out GameObject player))
            {
                // 내가 죽으면 GameOver 화면 띄우기
                if (playerId == ClientManager.Instance.ClientId)
                {
                    AudioManager.Instance.PlaySfx(AudioManager.Sfx.Dead, 1.0f);
                    AudioManager.Instance.PlaySfx(AudioManager.Sfx.GameOver, 0.7f);
                    EventBus<InGameGUIEventType>.Publish(InGameGUIEventType.ActivateCanvas, "GameOver");
                }

                Destroy(player);
                playerObjects.Remove(playerId);
            }
        }
    }

    private void UpdatePlayerDead(IEnumerable<PlayerDead> playerDeads)
    {
        foreach (var playerDead in playerDeads)
        {
            if (playerDead.deadPlayerId == ClientManager.Instance.ClientId)
            {
                Debug.Log($"Game Over: {playerDead.deadPlayerId}");
            }
        }
    }

    private void OnDestroy()
    {
        EventBus<PlayerEventType>.Unsubscribe<IEnumerable<Player>>(PlayerEventType.UpdatePlayerStates, UpdatePlayers);
        EventBus<PlayerEventType>.Unsubscribe<IEnumerable<PlayerDead>>(PlayerEventType.UpdatePlayeDeadrStates, UpdatePlayerDead);
    }
}