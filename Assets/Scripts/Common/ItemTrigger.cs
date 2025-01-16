using UnityEngine;

public class ItemTrigger : MonoBehaviour
{
    public ItemUIManager uiManager; // 동적으로 할당할 UI Manager
    public string itemName; // 자동 설정될 아이템 이름
    public string itemDescription; // 자동 설정될 아이템 설명

    public GameObject waitingRoomCanvas; // WaitingRoomCanvas 참조
    public GameObject headCountDown; // Head_CountDown 참조

    private void Start()
    {
        // ItemUIManager를 동적으로 찾아서 할당
        uiManager = FindObjectOfType<ItemUIManager>();
        if (uiManager == null)
        {
            Debug.LogError("ItemUIManager를 찾을 수 없습니다. 씬에 추가되었는지 확인하세요.");
            return;
        }

        // 객체 이름에 따라 itemName과 itemDescription 설정
        switch (gameObject.name)
        {
            case "Item_HealPack":
                itemName = "힐팩";
                itemDescription = "플레이어의 HP 30 회복";
                break;

            case "Item_StoneMagic":
                itemName = "전기 마법";
                itemDescription = "눈덩이 데미지 8";
                break;

            case "Item_FireMagic":
                itemName = "불 마법";
                itemDescription = "5초 동안 화상 데미지 1";
                break;
            case "Santa":
                itemName = "플레이어";
                itemDescription = "눈덩이 데미지 5\n한 게이지 당 20발";
                break;

            default:
                itemName = "알 수 없는 아이템";
                itemDescription = "설명이 없습니다.";
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && uiManager != null)
        {
            uiManager.ShowItemInfo(itemName, itemDescription, transform);

            // WaitingRoomCanvas 및 Head_CountDown 동적 참조
            waitingRoomCanvas = GameObject.Find("WaitingRoomCanvas(Clone)");
            if (waitingRoomCanvas == null)
            {
                Debug.LogError("WaitingRoomCanvas를 찾을 수 없습니다.");
                return;
            }

            headCountDown = waitingRoomCanvas.transform.Find("Head_CountDown")?.gameObject;
            if (headCountDown == null)
            {
                Debug.LogError("Head_CountDown 객체를 찾을 수 없습니다.");
                return;
            }

            // Head_CountDown 비활성화
            if (headCountDown != null)
            {
                headCountDown.SetActive(false);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && uiManager != null)
        {
            uiManager.HideItemInfo();

            // Head_CountDown 활성화
            if (headCountDown != null)
            {
                headCountDown.SetActive(true);
            }
        }
    }
}