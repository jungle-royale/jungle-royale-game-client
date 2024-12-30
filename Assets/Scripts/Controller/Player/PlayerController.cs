using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Dictionary<string, GameObject> playerObjects = new Dictionary<string, GameObject>();
    private string clientId;
    private float PLAYER_Y;

    void Start()
    {
        EventBus<PlayerEventType>.Subscribe<PlayerInit>(PlayerEventType.InitPlayer, InitializeClient);
        EventBus<PlayerEventType>.Subscribe<IEnumerable<Player>>(PlayerEventType.UpdatePlayerStates, UpdatePlayers);

        GameObject mapPrefab = Resources.Load<GameObject>("Prefabs/Map");
        // PLAYER_Y = mapPrefab.transform.localScale.y / 2;
        PLAYER_Y = 0;
    }

    private void InitializeClient(PlayerInit init)
    {
        clientId = init.ClientId;
        Debug.Log($"Client initialized with ID: {clientId}");
    }

    private void UpdatePlayers(IEnumerable<Player> players)
    {
        HashSet<string> activePlayerIds = new HashSet<string>();

        foreach (var player in players)
        {
            activePlayerIds.Add(player.Id);

            // Debug.Log($"{player.X}{player.Y}");

            if (!playerObjects.TryGetValue(player.Id, out GameObject playerObject))
            {
                GameObject playerPrefab = Resources.Load<GameObject>("Prefabs/Player");
                if (playerPrefab != null)
                {
                    playerObject = Instantiate(playerPrefab, new Vector3(player.X, PLAYER_Y, player.Y), Quaternion.identity);

                    // 내 플레이어면 태그를 Player로 설정
                    if (player.Id == clientId)
                    {
                        playerObject.tag = "Player";
                        playerObject.name = "MyPlayer";
                        Debug.Log($"Created client player for ID: {player.Id}");
                    }

                    playerObjects[player.Id] = playerObject;
                }
                else
                {
                    Debug.LogError("Player prefab could not be loaded.");
                    return;
                }
            }

            playerObject.transform.position = new Vector3(player.X, PLAYER_Y, player.Y);

            if (player.Id == clientId)
                // EventBus를 통해 레이턴시 전달
                EventBus<InGameGUIEventType>.Publish(InGameGUIEventType.HpUpdated, player.health);
        }

        RemoveInactivePlayers(activePlayerIds);
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
                Destroy(player);
                playerObjects.Remove(playerId);
            }
        }
    }

    private void OnDestroy()
    {
        EventBus<PlayerEventType>.Unsubscribe<IEnumerable<Player>>(PlayerEventType.UpdatePlayerStates, UpdatePlayers);
        EventBus<PlayerEventType>.Unsubscribe<PlayerInit>(PlayerEventType.InitPlayer, InitializeClient);
    }
}