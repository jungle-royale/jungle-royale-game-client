public enum InGameGUIEventType
{
    CreateWaitingRoomCanvas,
    CreateInGameCanvas,
    CreateGameOverCanvas,

    // WaitingRoomCanvas Child


    // InGameCanvas Child
    UpdatePingLabel, // Ping 값 업데이트
    UpdateHpLabel,
    UpdatePlayerCountLabel,
}