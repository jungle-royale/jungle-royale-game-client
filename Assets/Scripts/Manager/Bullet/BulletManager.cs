using System.Collections.Generic;
using UnityEngine;

public class BulletManager : MonoBehaviour
{
    private Dictionary<int, GameObject> bulletObjects = new Dictionary<int, GameObject>();
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
                    AudioManager.Instance.PlaySfx(AudioManager.Sfx.ShootNormal);
                    bulletObject = Instantiate(bulletPrefab, new Vector3(bullet.X, BULLET_Y, bullet.Y), Quaternion.identity);
                    bulletObjects[bullet.BulletId] = bulletObject;
                }
                else
                {
                    Debug.LogError("Bullet prefab could not be loaded.");
                    return;
                }
            }

            bulletObject.transform.position = new Vector3(bullet.X, BULLET_Y, bullet.Y);

        }
        RemoveInactiveBullets(activeBulletIds);
    }

    private void RemoveInactiveBullets(HashSet<int> activeBulletIds)
    {
        List<int> bulletsToRemove = new List<int>();

        foreach (var bulletId in bulletObjects.Keys)
        {
            if (!activeBulletIds.Contains(bulletId))
            {
                bulletsToRemove.Add(bulletId);
            }
        }

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