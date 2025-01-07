using UnityEngine;

public class MoveEvent: MonoBehaviour
{
    private void Dash()
    {
        AudioManager.Instance.PlaySfx(AudioManager.Sfx.Walk);
    }

    void Awake()
    {

    }

    void Start()
    {

    }
}