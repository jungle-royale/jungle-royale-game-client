using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f; // 이동 속도

    private CharacterController cc;

    void Start()
    {
        // CharacterController 가져오기
        cc = GetComponent<CharacterController>();
        if (cc == null)
        {
            Debug.LogError("CharacterController 컴포넌트를 찾을 수 없습니다.");
        }
    }

    void Update()
    {
        // // WASD 입력 처리
        // float horizontal = Input.GetAxis("Horizontal"); // A/D 키
        // float vertical = Input.GetAxis("Vertical");     // W/S 키

        // // 이동 방향 계산
        // Vector3 movement = new Vector3(horizontal, 0, vertical) * moveSpeed * Time.deltaTime;

        // CharacterController를 통한 이동 처리
        // cc.MovePosition(cc.position + movement);
    }
}