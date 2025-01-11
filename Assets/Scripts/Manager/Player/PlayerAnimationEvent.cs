using UnityEngine;

public class PlayerAnimationEvent : MonoBehaviour
{
    public void StartWalk()
    {
        AudioManager.Instance.PlaySfx(AudioManager.Sfx.Walk);
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

            if (ClientManager.Instance.gameEnd) {
                return; // 애니메이션 실행 중에 게임이 종료되면 gui 표시 X
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
    }

}