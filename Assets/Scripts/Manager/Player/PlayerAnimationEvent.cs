using UnityEngine;

public class PlayerAnimationEvent: MonoBehaviour
{
    public void Dash()
    {
        AudioManager.Instance.PlaySfx(AudioManager.Sfx.Walk);
    }

    public void AfterFalling()
    {
        // 죽음 처리
        GameObject player = this.gameObject;

        if (player.name == ClientManager.Instance.CurrentPlayerName)
        {
            AudioManager.Instance.PlaySfx(AudioManager.Sfx.GameOver, 0.7f);
            EventBus<InGameGUIEventType>.Publish(InGameGUIEventType.ActivateCanvas, "GameOver");
            EventBus<InputButtonEventType>.Publish(InputButtonEventType.ActivateTabKey);
        }
    }

}