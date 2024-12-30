using System.Collections.Generic;
using UnityEngine;

public class MainCameraController : MonoBehaviour
{
    private Camera mainCamera;
    private string clientId;
    
    const float CAMERA_ROTATION_X = 40f;
    const float CAMERA_OFFSET_Y = 10f;
    const float CAMERA_OFFSET_Z = 10f;
    float PLAYER_Y;

    void Start()
    {
        EventBus<MainCameraEventType>.Subscribe<MainCameraInit>(MainCameraEventType.MainCameraInit, InitializeClient);
        EventBus<MainCameraEventType>.Subscribe<IEnumerable<MainCamera>>(MainCameraEventType.MainCameraState, UpdateCamera);

        mainCamera = Camera.main;
    }

    private void InitializeClient(MainCameraInit init)
    {
        clientId = init.ClientId;
    }

    private void UpdateCamera(IEnumerable<MainCamera> players)
    {
        foreach (var player in players)
        {
            if (player.Id == clientId)
            {
                mainCamera.transform.position = new Vector3(player.X, 0 + CAMERA_OFFSET_Y, player.Y - CAMERA_OFFSET_Z);
                mainCamera.transform.rotation = Quaternion.Euler(CAMERA_ROTATION_X, 0, 0);
            }
        }
    }

    private void OnDestroy()
    {
        EventBus<MainCameraEventType>.Unsubscribe<MainCameraInit>(MainCameraEventType.MainCameraInit, InitializeClient);
        EventBus<MainCameraEventType>.Unsubscribe<IEnumerable<MainCamera>>(MainCameraEventType.MainCameraState, UpdateCamera);
    }
}