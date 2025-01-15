using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class BulletManager : MonoBehaviour
{
    private Dictionary<int, GameObject> bulletObjects = new Dictionary<int, GameObject>();
    private int bulletMaxTick;
    private float bulletSpeed;
    private Vector3 bulletDirection;
    private float BULLET_Y = 0.9f;

    public void UpdateBullets(List<Bullet> bullets)
    {
        // camera 비교해서 update할 아이들만

        // 애초에 state 받는곳에서 없애버려도..? - 구조상 서버에서 안주면 없다 처리 하니까!
        // 죽었다는 charging state에서 처리하면 되고
        // 어차피 current player는 포함될 거고

        HashSet<int> activeBulletIds = new HashSet<int>();

        foreach (var bullet in bullets)
        {
            activeBulletIds.Add(bullet.BulletId);

            if (!bulletObjects.TryGetValue(bullet.BulletId, out GameObject bulletObject))
            {
                GameObject bulletPrefab = Resources.Load<GameObject>("Prefabs/Bullets/Bullet");
                if (bulletPrefab != null)
                {
                    switch (bullet.BulletType)
                    {
                        case 0:
                            AudioManager.Instance.PlaySfx(AudioManager.Sfx.ShootNormal);
                            break;
                        case 1:
                            AudioManager.Instance.PlaySfx(AudioManager.Sfx.ShootStone);
                            break;
                        case 2:
                            AudioManager.Instance.PlaySfx(AudioManager.Sfx.ShootFire);
                            break;
                        default:
                            AudioManager.Instance.PlaySfx(AudioManager.Sfx.ShootNormal);
                            break;
                    }
                    bulletObject = Instantiate(bulletPrefab, new Vector3(bullet.X, BULLET_Y, bullet.Y), Quaternion.identity);
                    bulletObjects[bullet.BulletId] = bulletObject;
                }
                else
                {
                    Debug.LogError("Bullet prefab could not be loaded.");
                    return;
                }
            }
            ActivateBulletEffect(bulletObject, bullet.BulletType);

            bulletObject.transform.position = new Vector3(bullet.X, BULLET_Y, bullet.Y);
        }
        RemoveInactiveBullets(activeBulletIds);
    }

    // 총알 타입에 따른 이펙트를 활성화하는 메서드
    private void ActivateBulletEffect(GameObject bulletObject, int bulletType)
    {
        // Debug.Log($"Active Effect!!! BulletType:{bulletType}");
        // 자식 이펙트 비활성화
        Transform normalEffect = bulletObject.transform.Find("Bullet_NormalEffect");
        Transform stoneEffect = bulletObject.transform.Find("Bullet_StoneEffect");
        Transform fireEffect = bulletObject.transform.Find("Bullet_FireEffect");

        if (normalEffect != null) normalEffect.gameObject.SetActive(false);
        if (stoneEffect != null) stoneEffect.gameObject.SetActive(false);
        if (fireEffect != null) fireEffect.gameObject.SetActive(false);

        // 선택된 이펙트 활성화
        switch (bulletType)
        {
            case 0: // Normal
                if (normalEffect != null) normalEffect.gameObject.SetActive(true);
                break;
            case 1: // Stone
                if (stoneEffect != null) stoneEffect.gameObject.SetActive(true);
                break;
            case 2: // Fire
                if (fireEffect != null) fireEffect.gameObject.SetActive(true);
                break;
            default:
                Debug.LogWarning("Unknown bullet type: " + bulletType);
                break;
        }
    }

    private void RemoveInactiveBullets(HashSet<int> activeBulletIds)
    {
        List<int> bulletsToRemove = new List<int>();

        Debug.Log($"Client Bullet {bulletObjects}");

        foreach (var bulletId in bulletObjects.Keys)
        {
            if (!activeBulletIds.Contains(bulletId))
            {
                bulletsToRemove.Add(bulletId);
            }
        }

        Debug.Log($"지움? {activeBulletIds}");
        foreach (var bulletId in bulletsToRemove)
        {
            if (bulletObjects.TryGetValue(bulletId, out GameObject bullet))
            {
                Destroy(bullet);
                bulletObjects.Remove(bulletId);
            }
        }
    }

    public GameObject GetBulletById(int bulletId)
    {
        if (bulletObjects.ContainsKey(bulletId))
        {
            return bulletObjects[bulletId];
        }
        else
        {
            Debug.LogError($"{bulletId}에 해당하는 Bullet 없음");
            return null;
        }
    }
}