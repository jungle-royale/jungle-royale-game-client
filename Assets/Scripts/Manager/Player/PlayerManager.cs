using System;
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
    private GameObject currentPlayerGameObject; // 현재 플레이어 객체
    private GameObject currentPlayerMark;
    private Dictionary<int, GameObject> otherPlayerGameObjectDictionary = new Dictionary<int, GameObject>();
    private Dictionary<int, GameObject> otherPlayerMarkDictionary = new Dictionary<int, GameObject>();

    private HashSet<int> movePlayerIdList = new HashSet<int>();
    private HashSet<int> dashPlayerIdList = new HashSet<int>();
    private HashSet<int> shootingPlayerIdList = new HashSet<int>();

    private List<Player> playerDataList = new List<Player>();

    private float lastServerUpdateTime;

    const float DASH_ROTATION = 15f;

    // 60FPS 환경에서는 lerpSpeed가 10이라면, 한 프레임 동안 약 10 * 1/60 = 0.1667의 속도로 보간
    float LERP_SPEED = 10f; // 10f는 빠르게 따라가고, 2~5f는 더 느리고 부드럽게
    const int GAGE_PER_BULLET = 30;

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
            return currentPlayerGameObject;
        }

        if (otherPlayerGameObjectDictionary.ContainsKey(playerId))
        {
            return otherPlayerGameObjectDictionary[playerId];
        }

        return null;
    }

    void Update()
    {
        foreach (var data in playerDataList)
        {
            if (data.id == currentPlayerId)
            {
                if (currentPlayerGameObject)
                {
                    ValidateCurrentPlayerAtUpdate(data);
                }
                else
                {
                    CreateCurrentPlayer(data);
                }
            }
            else
            {
                if (otherPlayerGameObjectDictionary.ContainsKey(data.id))
                {
                    UpdateOtherPlayerAtUpdate(otherPlayerGameObjectDictionary[data.id], data);
                }
                else
                {
                    CreateOtherPlayer(data);
                }
            }
        }

        RemoveDisconnectedPlayers(playerDataList);
    }

    public void UpdatePlayersFromServer(List<Player> activePlayerDataListFromServer)
    {
        lastServerUpdateTime = Time.time;
        playerDataList = activePlayerDataListFromServer;
        int activePlayerNumber = activePlayerDataListFromServer.Count;
        EventBus<InGameGUIEventType>.Publish(InGameGUIEventType.UpdatePlayerCountLabel, activePlayerNumber);
    }

    private void CreateCurrentPlayer(Player data)
    {
        currentPlayerGameObject = Instantiate(playerPrefab, new Vector3(data.x, PLAYER_Y, data.y), Quaternion.identity);
        currentPlayerMark = Instantiate(currentPlayerMarkPrefab, new Vector3(data.x, PLAYER_Y, data.y), Quaternion.identity); // 내 플레이어 마크

        currentPlayerGameObject.tag = "Player";
        currentPlayerGameObject.name = ClientManager.Instance.CurrentPlayerName;

        // 플레이어의 HealthBar 초기화
        HealthBar healthBarComponent = currentPlayerGameObject.GetComponentInChildren<HealthBar>();
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

        otherPlayerGameObjectDictionary[data.id] = newPlayer;
        otherPlayerMarkDictionary[data.id] = newPlayerMark;
    }

    private void ValidateCurrentPlayerAtUpdate(Player serverData)
    {
        EventBus<InGameGUIEventType>.Publish<int>(InGameGUIEventType.UpdateBulletBarLabel, serverData.bulletGage);
        UpdateHealthBar(currentPlayerGameObject, serverData);
        UpdatePlayerMoveState(currentPlayerGameObject, serverData);
        UpdatePlayerShootState(currentPlayerGameObject, serverData);
        UpdateCurrentPlayerMark(serverData);
        UpdatePlayerPosition(currentPlayerGameObject, serverData);
    }

    private void UpdateOtherPlayerAtUpdate(GameObject player, Player serverData)
    {
        if (!serverData.isOutofView) // 범위 밖이면 비활성화
        {
            player.SetActive(false);
            return;
        }
        else
        {
            player.SetActive(true);

            UpdateHealthBar(player, serverData);
            UpdatePlayerMoveState(player, serverData);
            UpdatePlayerShootState(player, serverData);
            UpdateOtherPlayersMark(serverData);
            UpdatePlayerPosition(player, serverData);
        }
    }

    private void UpdateHealthBar(GameObject player, Player serverData)
    {
        HealthBar healthBarComponent = player.GetComponentInChildren<HealthBar>();
        if (healthBarComponent != null)
        {
            healthBarComponent.SetHealth(serverData.health);
        }
    }

    private void UpdateCurrentPlayerMark(Player serverData)
    {
        var targetPosition = CalculatePredicatedPosition(PLAYER_Y, serverData);
        var currentPosition = currentPlayerMark.transform.position;
        var newPosition = Vector3.Lerp(currentPosition, targetPosition, LERP_SPEED * Time.deltaTime);
        currentPlayerMark.transform.position = newPosition;
    }

    private void UpdateOtherPlayersMark(Player serverData)
    {
        var otherPlayerMark = otherPlayerMarkDictionary[serverData.id];

        if (otherPlayerMark != null)
        {
            var targetPosition = CalculatePredicatedPosition(PLAYER_Y, serverData);
            var currentPosition = otherPlayerMark.transform.position;
            var newPosition = Vector3.Lerp(currentPosition, targetPosition, LERP_SPEED * Time.deltaTime);
            otherPlayerMark.transform.position = newPosition;
        }
    }

    private void UpdatePlayerMoveState(GameObject player, Player serverData)
    {
        var currentPosition = CalculatePredicatedPosition(player.transform.position.y, serverData);
        var previousPosition = serverData.NewPosition(player.transform.position.y);

        Vector3 movementDirection = currentPosition - previousPosition;

        Animator animator = player.GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogWarning($"Animator not found on player: {player.name}");
            return;
        }

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
    }

    private void UpdatePlayerPosition(GameObject player, Player serverData)
    {
        var targetPosition = CalculatePredicatedPosition(player.transform.position.y, serverData);
        var currentPosition = player.transform.position;
        var distance = Vector3.Distance(currentPosition, targetPosition);
        // Debug.Log("#########################################");
        // Debug.Log("#########################################");
        // Debug.Log($"player: {serverData.x}, {serverData.y}");
        // Debug.Log($"position: {currentPosition}, {targetPosition}");
        // Debug.Log($"distance: {distance}");
        if (serverData.isDashing)
        {
            var newPosition = Vector3.Lerp(currentPosition, targetPosition, LERP_SPEED * Time.deltaTime * 2);
            player.transform.position = newPosition;
        }
        else
        {
            var newPosition = Vector3.Lerp(currentPosition, targetPosition, LERP_SPEED * Time.deltaTime);
            player.transform.position = newPosition;
        }
    }

    private Vector3 CalculatePredicatedPosition(float y, Player serverData)
    {
        if (!serverData.isMoved)
        {
            return serverData.NewPosition(y);
        }

        float currentTime = Time.time;
        float timeSinceLastUpdate = (currentTime - lastServerUpdateTime);
        float frameFactor = timeSinceLastUpdate * 1000 / 16; // 16ms 기준

        var newPosition = serverData.NewPosition(y);
        newPosition.x += serverData.dx * frameFactor;
        newPosition.z += serverData.dy * frameFactor;

        return newPosition;
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

        if (serverData.isShooting && serverData.bulletGage >= GAGE_PER_BULLET)
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