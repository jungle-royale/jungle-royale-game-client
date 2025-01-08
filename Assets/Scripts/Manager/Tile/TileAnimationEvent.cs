using UnityEngine;

public class TileAnimationEvent : MonoBehaviour
{
    public void AfterFalling()
    {
        // 죽음 처리
        GameObject tileObject = this.gameObject;

        Debug.Log($"🪵 {tileObject.name}");

        Destroy(tileObject);
    }

}