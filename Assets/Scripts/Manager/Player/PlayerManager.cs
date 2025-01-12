using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public GameObject playerPrefab; // 내 플레이어 프리팹
    public GameObject currentPlayerMarkPrefab; // 내 플레이어 프리팹
    public GameObject otherPlayerMarkPrefab; // 내 플레이어 프리팹

    private bool currentPlayerDead = false;

    // 이펙트
    private GameObject shootingEffect;
    private GameObject currentPlayer; // 현재 플레이어 객체
    private GameObject currentPlayerMark;
    private Dictionary<int, GameObject> otherPlayerGameObjectDictionary = new Dictionary<int, GameObject>();
    private Dictionary<int, GameObject> otherPlayerMarkDictionary = new Dictionary<int, GameObject>();

    private HashSet<int> movePlayerIdList = new HashSet<int>();
    private HashSet<int> dashPlayerIdList = new HashSet<int>();
    private HashSet<int> shootingPlayerIdList = new HashSet<int>();


    private List<Player> playerList = new List<Player>();


    const float DASH_ROTATION = 15f;

    private int currentPlayerId
    {
        get
        {
            return ClientManager.Instance.ClientId;
        }
    }

    private float PLAYER_Y;

    public GameObject GetPlayerById(int playerId)
    {
        if (playerId == currentPlayerId)
        {
            return currentPlayer;
        }

        if (otherPlayerGameObjectDictionary.ContainsKey(playerId))
        {
            return otherPlayerGameObjectDictionary[playerId];
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
                if (otherPlayerGameObjectDictionary.ContainsKey(data.id))
                {
                    // 기존 플레이어 업데이트
                    UpdatePlayer(otherPlayerGameObjectDictionary[data.id], data);
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
        EventBus<InGameGUIEventType>.Publish(InGameGUIEventType.UpdatePlayerCountLabel, playerDataList.Count);
    }

    private void CreateCurrentPlayer(Player data)
    {
        currentPlayer = Instantiate(playerPrefab, new Vector3(data.x, PLAYER_Y, data.y), Quaternion.identity);
        currentPlayerMark = Instantiate(currentPlayerMarkPrefab, new Vector3(data.x, PLAYER_Y, data.y), Quaternion.identity); // 내 플레이어 마크

        currentPlayer.tag = "Player";
        currentPlayer.name = ClientManager.Instance.CurrentPlayerName;

        // 플레이어의 HealthBar 초기화
        HealthBar healthBarComponent = currentPlayer.GetComponentInChildren<HealthBar>();
        if (healthBarComponent != null)
        {
            healthBarComponent.SetMaxHealth(data.health);
        }
        EventBus<InGameGUIEventType>.Publish<int>(InGameGUIEventType.SetBulletBarLabel, data.bulletGage);
    }

    private void CreateOtherPlayer(Player data)
    {
        GameObject newPlayer = Instantiate(playerPrefab, new Vector3(data.x, PLAYER_Y, data.y), Quaternion.identity);
        GameObject newPlayerMark = Instantiate(otherPlayerMarkPrefab, new Vector3(data.x, PLAYER_Y, data.y), Quaternion.identity); // 다른 플레이어 마크

        // 플레이어의 HealthBar 초기화
        HealthBar healthBarComponent = newPlayer.GetComponentInChildren<HealthBar>();
        if (healthBarComponent != null)
        {
            healthBarComponent.SetMaxHealth(data.health);
        }

<<<<<<< HEAD
        Debug.Log($"init BulletGage: {data.bulletGage}");

        otherPlayers[data.id] = newPlayer;
        otherPlayersMark[data.id] = newPlayerMark;
=======
        otherPlayerGameObjectDictionary[data.id] = newPlayer;
        otherPlayerMarkDictionary[data.id] = newPlayerMark;
>>>>>>> 4d258c6 (feat: dx, dy 메세지 프로토콜 반영)
    }

    private void ValidateCurrentPlayer(Player serverData)
    {
        // HealthBar 업데이트
        HealthBar healthBarComponent = currentPlayer.GetComponentInChildren<HealthBar>();
        if (healthBarComponent != null)
        {
            healthBarComponent.SetHealth(serverData.health);
        }

        EventBus<InGameGUIEventType>.Publish<int>(InGameGUIEventType.UpdateBulletBarLabel, serverData.bulletGage);

        UpdatePlayerMoveState(currentPlayer, serverData);
        UpdatePlayerShootState(currentPlayer, serverData);
        UpdatePlayerMark(serverData);
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
        UpdateOtherPlayersMark(serverData);
    }

    private void UpdatePlayerMark(Player serverData)
    {
        // Mark 위치 업데이트
        currentPlayerMark.transform.position = serverData.NewPosition(PLAYER_Y);
    }

    private void UpdateOtherPlayersMark(Player serverData)
    {
        var otherPlayerMark = otherPlayerMarkDictionary[serverData.id];

        if (otherPlayerMark != null)
        {
            otherPlayerMark.transform.position = serverData.NewPosition(PLAYER_Y);
        }
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
            if (!dashPlayerIdList.Contains(serverData.id))
            {
                dashPlayerIdList.Add(serverData.id);
                AudioManager.Instance.PlaySfx(AudioManager.Sfx.Dash);
            }
            if (movementDirection != Vector3.zero)
            {
                Quaternion tiltRotation = Quaternion.LookRotation(movementDirection.normalized); // 이동 방향을 기준으로 회전
                                                                                                 // Y축 기울이기 (Roll 추가)
                Quaternion tilt = Quaternion.Euler(
                    DASH_ROTATION,               // 상하 기울임 유지
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
            if (dashPlayerIdList.Contains(serverData.id))
            {
                dashPlayerIdList.Remove(serverData.id);
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
            if (!movePlayerIdList.Contains(serverData.id))
            {
                movePlayerIdList.Add(serverData.id);
                animator.SetBool("isMoving", true);
            }
        }
        else
        {
            if (movePlayerIdList.Contains(serverData.id))
            {
                movePlayerIdList.Remove(serverData.id);
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
            if (dashPlayerIdList.Contains(serverData.id))
            {
                shootingEffect.SetActive(false);
                shootingPlayerIdList.Remove(serverData.id);
            }
            else
            {
                if (!shootingPlayerIdList.Contains(serverData.id))
                {
                    shootingPlayerIdList.Add(serverData.id);
                    shootingEffect.SetActive(true);
                }
            }
        }
        else
        {
            if (shootingPlayerIdList.Contains(serverData.id))
            {
                shootingPlayerIdList.Remove(serverData.id);

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
        var existingIds = new HashSet<int>(playerDataList.ConvertAll(p => p.id));
        var keysToRemove = new List<int>();

        foreach (var key in otherPlayerGameObjectDictionary.Keys)
        {
            if (!existingIds.Contains(key))
            {
                keysToRemove.Add(key);
            }
        }

        foreach (var key in keysToRemove)
        {
            Destroy(otherPlayerMarkDictionary[key]);
            otherPlayerGameObjectDictionary.Remove(key);
            movePlayerIdList.Remove(key);
            dashPlayerIdList.Remove(key);
        }

        if (!currentPlayerDead && !existingIds.Contains(currentPlayerId))
        {
            Destroy(currentPlayerMark);
            currentPlayerDead = true;
            movePlayerIdList.Remove(currentPlayerId);
            dashPlayerIdList.Remove(currentPlayerId);
        }
    }

}