using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public GameObject playerPrefab; // 플레이어 프리팹
    public GameObject currentPlayer; // 현재 플레이어 객체
    private Dictionary<string, GameObject> otherPlayers = new Dictionary<string, GameObject>();
    private string currentPlayerId {
        get {
            return ClientManager.Instance.ClientId;
        }
    }

    private float PLAYER_Y;

    public void UpdatePlayers(List<Player> playerDataList)
    {
        Debug.Log("🍎" + playerDataList.Count);

        foreach (var data in playerDataList)
        {
            if (otherPlayers.ContainsKey(data.id))
            {
                if (data.id == currentPlayerId)
                {
                    // 현재 플레이어 데이터 업데이트 및 검증
                    ValidateCurrentPlayer(data);
                }
                else 
                {
                    // 기존 플레이어 업데이트
                    UpdatePlayer(otherPlayers[data.id], data);
                }
            }
            else
            {
                // 새로운 플레이어 생성
                CreateOtherPlayer(data);
            }
        }

        // 제거할 플레이어 처리
        RemoveDisconnectedPlayers(playerDataList);
    }

    private void ValidateCurrentPlayer(Player serverData)
    {
        // 서버 데이터와 현재 입력 상태 비교 및 조정
        Vector3 serverPosition = new Vector3(serverData.x, PLAYER_Y, serverData.y);
        if (Vector3.Distance(currentPlayer.transform.position, serverPosition) > 0.1f)
        {
            currentPlayer.transform.position = serverPosition;
        }

        currentPlayer.transform.rotation = Quaternion.Euler(0, serverData.angle, 0);
    }

    private void CreateOtherPlayer(Player data)
    {
        GameObject newPlayer = Instantiate(playerPrefab, new Vector3(data.x, PLAYER_Y, data.y), Quaternion.identity);
        if (data.id != currentPlayerId)
        {
            otherPlayers[data.id] = newPlayer;
        }
        else 
        {
            currentPlayer = newPlayer;
        }
    }

    private void UpdatePlayer(GameObject player, Player data)
    {
        player.transform.position = new Vector3(data.x, PLAYER_Y, data.y);
        player.transform.rotation = Quaternion.Euler(0, data.angle, 0);
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
            if (key == currentPlayerId)
            {
                AudioManager.Instance.PlaySfx(AudioManager.Sfx.Dead, 1.0f);
                AudioManager.Instance.PlaySfx(AudioManager.Sfx.GameOver, 0.7f);
                EventBus<InGameGUIEventType>.Publish(InGameGUIEventType.ActivateCanvas, "GameOver");
                Destroy(currentPlayer);
            }
            else
            {
                Destroy(otherPlayers[key]);
                otherPlayers.Remove(key);
            }
        }
    }

}
