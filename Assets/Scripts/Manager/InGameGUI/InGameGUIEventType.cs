public enum InGameGUIEventType
{
    // Canvas 활성화
    ActivateCanvas,


    // MainCanvas
    UpdateTimerLabel,
    UpdatePingLabel, // Ping 값 업데이트
    UpdateHpLabel,
    UpdatePlayerCountLabel,

    // WaitingRoomCanvas Child
    UpdateMinPlayerLabel,
    UpdateGameCountDownLabel,

    // InGameCanvas Child
    SetBulletBarLabel,
    UpdateBulletBarLabel,

    // State Canvas (GameOvaer, Win, End)
    UpdateStateLabel,
}