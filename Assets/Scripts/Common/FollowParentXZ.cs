using UnityEngine;

public class FollowParentXZ : MonoBehaviour
{
    private Transform parent; // 부모 객체의 Transform
    public float fixedY = 1.5f;

    void Start()
    {
        // 부모 Transform 자동 할당
        parent = transform.parent;
        fixedY = transform.position.y;

        if (parent == null)
        {
            Debug.LogError("부모 객체가 없습니다. 부모 객체를 설정해주세요.");
        }
    }

    void Update()
    {
        if (parent != null)
        {
            // 부모의 X와 Z를 따라가고, Y는 자식 자신의 Y 값을 유지
            Vector3 newPosition = new Vector3(parent.position.x, fixedY, parent.position.z);
            transform.position = newPosition;
        }
    }
}