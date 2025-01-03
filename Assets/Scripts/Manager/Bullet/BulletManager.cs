using System.Collections.Generic;
using UnityEngine;

public class BulletManager : MonoBehaviour
{
    private Dictionary<string, GameObject> bulletObjects = new Dictionary<string, GameObject>();
    private float BULLET_Y = 0.9f;


    public void UpdateBullets(List<Bullet> bullets)
    {
        HashSet<string> activeBulletIds = new HashSet<string>();

        foreach (var bullet in bullets)
        {
            activeBulletIds.Add(bullet.BulletId);

            if (!bulletObjects.TryGetValue(bullet.BulletId, out GameObject bulletObject))
            {
                GameObject bulletPrefab = Resources.Load<GameObject>("Prefabs/Bullet");
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

    private void RemoveInactiveBullets(HashSet<string> activeBulletIds)
    {
        List<string> bulletsToRemove = new List<string>();

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

}