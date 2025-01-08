using UnityEngine;

public class PlayerAnimationEvent : MonoBehaviour
{
    public void Dash()
    {
        AudioManager.Instance.PlaySfx(AudioManager.Sfx.Walk);
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