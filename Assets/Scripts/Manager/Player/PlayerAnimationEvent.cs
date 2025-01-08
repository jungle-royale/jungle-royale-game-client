using UnityEngine;

public class PlayerAnimationEvent: MonoBehaviour
{
    private void Dash()
    {
        AudioManager.Instance.PlaySfx(AudioManager.Sfx.Walk);
    }

    private void AfterFalling()
    {

        Debug.Log("hello~~~~~");
        // 죽음 처리
        GameObject player = this.gameObject;

        if (player.name == ClientManager.Instance.CurrentPlayerName)
        {
            AudioManager.Instance.PlaySfx(AudioManager.Sfx.GameOver, 0.7f);
            EventBus<InGameGUIEventType>.Publish(InGameGUIEventType.ActivateCanvas, "GameOver");
        }
    }

}