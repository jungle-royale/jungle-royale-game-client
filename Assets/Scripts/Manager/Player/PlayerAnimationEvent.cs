using UnityEngine;

public class PlayerAnimationEvent : MonoBehaviour
{
    public CameraManager cameraManager;

    void Awake()
    {
        cameraManager = FindObjectOfType<CameraManager>();
        if (cameraManager == null)
        {
            Debug.LogError("CameraManager 없음");
        }
    }

    public void StartWalk()
    {
        GameObject player = this.gameObject;
        if (cameraManager != null && !cameraManager.IsInMainCameraView(player.transform.position)) return;

        if (player.name == ClientManager.Instance.CurrentPlayerName)
        {
            AudioManager.Instance.PlaySfx(AudioManager.Sfx.Walk);
        }
        else
        {
            AudioManager.Instance.PlaySfx(AudioManager.Sfx.Walk, 0.5f);
        }
    }

    public void EndWalk()
    {
        GameObject player = this.gameObject;
        var newPosition = player.transform.position;
        newPosition.y = 0;
        player.transform.position = newPosition;
    }

    public void AfterFalling()
    {
        HandleDead();
    }

    public void AfterDeadByAttack()
    {
        HandleDead();
    }

    private void HandleDead()
    {
        // 죽음 처리
        GameObject player = this.gameObject;

        if (player.name == ClientManager.Instance.CurrentPlayerName)
        {
            AudioManager.Instance.PlaySfx(AudioManager.Sfx.GameOver, 0.7f);

            if (ClientManager.Instance.gameEnd)
            {
                return; // 애니메이션 실행 중에 게임이 종료되면 gui 표시 X, 시체 없애기 X
            }

            EventBus<InGameGUIEventType>.Publish(InGameGUIEventType.ActivateCanvas, "GameOver");

            StateUIDTO stateData = new StateUIDTO
            {
                placement = ClientManager.Instance.placement,
                totalPlayer = ClientManager.Instance.totalPlayerNum,
                killCount = ClientManager.Instance.killCount,
                // point = 500
            };

            EventBus<InGameGUIEventType>.Publish(InGameGUIEventType.UpdateStateLabel, stateData);

        }

        Destroy(player);
    }

}