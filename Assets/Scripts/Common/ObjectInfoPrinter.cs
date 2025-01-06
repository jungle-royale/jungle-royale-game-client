using UnityEngine;

public class ObjectInfoPrinter : MonoBehaviour
{
    [SerializeField]
    private GameObject prefab;
    // Resources 폴더 내 프리팹 경로
    private string prefabPath = "Prefabs/Tile/Tile01";

    void Start()
    {
        // 프리팹 로드
        if (prefab == null)
            prefab = Resources.Load<GameObject>(prefabPath);

        if (prefab == null)
        {
            Debug.LogError($"프리팹을 로드할 수 없습니다. 경로: {prefabPath}");
            return;
        }

        // 모든 자식 오브젝트 탐색
        foreach (Transform child in prefab.GetComponentsInChildren<Transform>())
        {
            // Capsule Collider를 가진 경우 처리
            CapsuleCollider capsuleCollider = child.GetComponent<CapsuleCollider>();
            if (capsuleCollider != null)
            {
                Vector3 capsulePosition = child.localPosition; // 로컬 좌표
                bool isShorterThanBulletY = capsuleCollider.height < 0.8;

                Debug.Log($"[Capsule] 오브젝트 이름: {child.name}, X: {capsulePosition.x}, Z: {capsulePosition.z}, Radius: {capsuleCollider.radius}, Shorter Than BulletY: {isShorterThanBulletY}");
            }

            // Box Collider를 가진 경우 처리
            BoxCollider boxCollider = child.GetComponent<BoxCollider>();
            if (boxCollider != null)
            {
                Vector3 boxCenter = child.localPosition; // 로컬 좌표
                Vector3 size = boxCollider.size;
                Vector3 bottomLeftCorner = new Vector3(
                    boxCenter.x - size.x / 2,
                    boxCenter.y,
                    boxCenter.z - size.z / 2
                );

                bool isShorterThanBulletY = size.y < 0.8;

                Debug.Log($"[Box] 오브젝트 이름: {child.name}, X: {bottomLeftCorner.x}, Z: {bottomLeftCorner.z}), Width: {size.x}, Length: {size.z}, Shorter Than BulletY: {isShorterThanBulletY}");
            }
        }
    }
}