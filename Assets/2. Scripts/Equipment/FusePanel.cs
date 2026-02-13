using UnityEngine;
using UnityEngine.UI;

public class FusePanel : MonoBehaviour
{
    [SerializeField] EquipSlot mainSlot;                // 합성으로 등급 상승을 노릴, 사라지지 않는 메인 장비를 놓을 공간입니다.
    [SerializeField] EquipSlot subSlot1;                // 합성으로 인해 소모될, 재료 장비를 놓을 공간입니다.
    [SerializeField] EquipSlot subSlot2;                // 합성으로 인해 소모될, 재료 장비를 놓을 공간입니다.
    [SerializeField] Button fuseButton;                 // 합성 준비가 되었을 때 합성을 진행하도록 해줄 버튼입니다.
    [SerializeField] EquipmentInventory inventory;      // 실질적인 인벤토리입니다.
    [SerializeField] CanvasGroup _fusePanelGroup;       // 자기 자신을 넣어주면 되는, 캔버스 그룹 제어용.
    [SerializeField] InventoryPanel _inventoryPanel;    // 인벤토리를 열고 닫기 위한 패널입니다.

    private void Awake()
    {
        //합성 버튼에 합성 기능을 추가합니다.
        fuseButton.onClick.AddListener(OnClickFuse);   
    }

    private void OnClickFuse()
    {
        //합성 슬롯에 3개의 장비가 전부 채워지지 않으면 합성을 진행하지 않습니다.
        if(mainSlot.equipped == null || subSlot1.equipped == null || subSlot2.equipped == null)
        {
            Debug.Log("합성을 위한 슬롯에 장비가 전부 채워지지 않았습니다.");
            return;
        }

        //3개의 슬롯에 장비가 전부 채워지면 합성을 진행시킵니다.
        Fuse(mainSlot.equipped, subSlot1.equipped, subSlot2.equipped);
    }

    //합성을 진행합니다
    public void Fuse(Equipment mainEquipment, Equipment subEquipmentOne, Equipment subEquipmentTwo)
    {
        //세 장비의 장착 부위가 일치하지 않으면 합성이 불가능합니다.
        if (mainEquipment.equip_type != subEquipmentOne.equip_type ||
            mainEquipment.equip_type != subEquipmentTwo.equip_type)
        {
            Debug.Log("합성하기 위한 세 장비의 장착 부위가 일치하지 않습니다.");
            return;
        }

        //세 장비의 등급이 일치하지 않으면 합성이 불가능합니다.
        if(mainEquipment.equipment_Rarity != subEquipmentOne.equipment_Rarity ||
            mainEquipment.equipment_Rarity != subEquipmentTwo.equipment_Rarity)
        {
            Debug.Log("합성하기 위한 세 장비의 레어도가 일치하지 않습니다.");
            return;
        }

        //세 장비는 전부 다른 칸의 장비여야만 합니다.
        if(inventory.GetInventoryIndex(mainEquipment) == inventory.GetInventoryIndex(subEquipmentOne)||
            inventory.GetInventoryIndex(mainEquipment) == inventory.GetInventoryIndex(subEquipmentTwo)||
            inventory.GetInventoryIndex(subEquipmentOne) == inventory.GetInventoryIndex(subEquipmentTwo))
        {
            Debug.Log("동일한 장비를 중복으로 사용하실 수 없습니다.");
            return;
        }

        //1부터 100까지의 값 중 랜덤으로 값을 뽑습니다.
        int randomNumber = Random.Range(1, 101);

        //성공을 결정할 숫자는, 장비 등급에 따라 결정됩니다.
        int successNumber = CheckFuseSuccessRate(mainEquipment);

        //디버그를 위한, 확인용 출력입니다.
        Debug.Log($"{randomNumber} / {successNumber}");

        //랜덤으로 뽑은 숫자가 성공을 결정할 숫자보다 작거나 같은 경우, 합성에 성공합니다.
        if (randomNumber <= successNumber)
        {
            Debug.Log("합성에 성공하였습니다! 장비의 레어도가 상승합니다.");

            //장비의 레어도를 1 올려, 메인 장비의 레어도를 1 올립니다.
            int rarity = (int)mainEquipment.equipment_Rarity;
            rarity += 1;
            mainEquipment.equipment_Rarity = (Equip_Rank)rarity;
        }

        //랜덤으로 뽑은 숫자가 성공을 결정할 숫자를 넘어갔을 경우, 합성에 실패합니다.
        else
        {
            Debug.Log("합성에 실패하였습니다.");
        }

        //재료로 넣은 장비 두 개를 슬롯에서 삭제합니다.
        subSlot1.ClearSlot();
        subSlot2.ClearSlot();

        //재료로 넣었던 장비 두 개를 인벤토리에서 삭제합니다.
        inventory.RemoveEquipment(subEquipmentOne);
        inventory.RemoveEquipment(subEquipmentTwo);

        //메인 슬롯에 합성 성공 여부를 확인할 수 있게 색상을 변화시킵니다.
        mainSlot.UpdatePartUI();

        //인벤토리를 한 번 갱신합니다.
        _inventoryPanel.Refresh();
    }

    //합성 성공률을 확인합니다.
    public int CheckFuseSuccessRate(Equipment mainEquipment)
    {
        //확인용 숫자를 초기화합니다.
        int successRate = 0;

        //인자값으로 받은 장비의 레어도에 따라 성공률을 결정합니다. (하드코딩)
        switch (mainEquipment.equipment_Rarity)
        {
            case Equip_Rank.일반:
                successRate = 90;
                break;

            case Equip_Rank.희귀:
                successRate = 50;
                break;

            case Equip_Rank.레어:
                successRate = 25;
                break;

            case Equip_Rank.전설:
                successRate = 5;
                break;

            case Equip_Rank.신화:
                successRate = 0;
                break;
        }

        //성공률을 계산하기 위한 숫자를 반환합니다.
        return successRate;
    }

    /// <summary>
    /// 패널의 활성화 여부를 정합니다.
    /// </summary>
    /// <param name="value">활성화 여부</param>
    public void SetPanelActiveValue(bool value)
    {
        //참이면 1, 거짓이면 0으로 하여 참일 경우에만 보이게 합니다.
        _fusePanelGroup.alpha = value == true ? 1 : 0;

        //상호작용 여부와 뒤 오브젝트와의 상호작용 제한은 참일 경우에만 활성화되도록 합니다.
        _fusePanelGroup.interactable = value;
        _fusePanelGroup.blocksRaycasts = value;
    }
}
