using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public CameraHandler cameraHandler;

    public GameObject playerPrefab; // 플레이어 프리팹

    private bool currentPlayerDead = false;

    private GameObject currentPlayer; // 현재 플레이어 객체
    private Dictionary<string, GameObject> otherPlayers = new Dictionary<string, GameObject>();
    private string currentPlayerId
    {
        get
        {
            return ClientManager.Instance.ClientId;
        }
    }

    private float PLAYER_Y;

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
        currentPlayer = Instantiate(playerPrefab, new Vector3(data.x, PLAYER_Y, data.y), Quaternion.identity);
        currentPlayer.tag = "Player";
        currentPlayer.name = "MyPlayer";
    }

    private void CreateOtherPlayer(Player data)
    {
        GameObject newPlayer = Instantiate(playerPrefab, new Vector3(data.x, PLAYER_Y, data.y), Quaternion.identity);
        otherPlayers[data.id] = newPlayer;
    }

    private void ValidateCurrentPlayer(Player serverData)
    {
        // 서버 데이터와 현재 입력 상태 비교 및 조정
        Vector3 serverPosition = new Vector3(serverData.x, PLAYER_Y, serverData.y);
        // if (Vector3.Distance(currentPlayer.transform.position, serverPosition) > 0.1f)
        // {
        //     currentPlayer.transform.position = serverPosition;
        // }
        currentPlayer.transform.position = serverPosition;
        currentPlayer.transform.rotation = Quaternion.Euler(0, -(serverData.angle - 180), 0);
        EventBus<InGameGUIEventType>.Publish(InGameGUIEventType.UpdateHpLabel, serverData.health);
    }

    private void UpdatePlayer(GameObject player, Player data)
    {
        player.transform.position = new Vector3(data.x, PLAYER_Y, data.y);
        player.transform.rotation = Quaternion.Euler(0, -(data.angle - 180), 0);
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
            AudioManager.Instance.PlaySfx(AudioManager.Sfx.Dead, 1.0f);
            AudioManager.Instance.PlaySfx(AudioManager.Sfx.GameOver, 0.7f);
            EventBus<InGameGUIEventType>.Publish(InGameGUIEventType.ActivateCanvas, "GameOver");
            Destroy(currentPlayer);
            currentPlayerDead = true;
        }

        foreach (var key in keysToRemove)
        {
            Destroy(otherPlayers[key]);
            otherPlayers.Remove(key);
        }
    }

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

}
