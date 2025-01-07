public enum InGameGUIEventType
{
    // Canvas 활성화
    ActivateCanvas,

    // MainCanvas
    UpdateTimerLabel,

    // WaitingRoomCanvas Child
    UpdateGameCountDownLabel,

    // InGameCanvas Child
    UpdatePingLabel, // Ping 값 업데이트
    UpdateHpLabel,
    UpdatePlayerCountLabel,
}