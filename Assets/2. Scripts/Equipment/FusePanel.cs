using UnityEngine;
using UnityEngine.UI;

public class FusePanel : MonoBehaviour
{
    [SerializeField] EquipSlot mainSlot;
    [SerializeField] EquipSlot subSlot1;
    [SerializeField] EquipSlot subSlot2;
    [SerializeField] Button fuseButton;
    [SerializeField] EquipmentInventory inventory;
    [SerializeField] CanvasGroup _fusePanelGroup; // 자기 자신을 넣어주면 되는, 캔버스 그룹 제어용.
    [SerializeField] InventoryPanel _inventoryPanel;

    private void Awake()
    {
        fuseButton.onClick.AddListener(OnClickFuse);   
    }

    private void OnClickFuse()
    {
        if(mainSlot.equipped == null || subSlot1.equipped == null || subSlot2.equipped == null)
        {
            Debug.Log("합성을 위한 슬롯에 장비가 전부 채워지지 않았습니다.");
            return;
        }

        Fuse(mainSlot.equipped, subSlot1.equipped, subSlot2.equipped);
    }

    //합성을 진행합니다
    public void Fuse(Equipment mainEquipment, Equipment subEquipmentOne, Equipment subEquipmentTwo)
    {
        if (mainEquipment.equipPart != subEquipmentOne.equipPart ||
            mainEquipment.equipPart != subEquipmentTwo.equipPart ||
            subEquipmentOne.equipPart != subEquipmentTwo.equipPart)
        {
            Debug.Log("합성하기 위한 세 장비의 장착 부위가 일치하지 않습니다.");
            return;
        }
        if(mainEquipment.equipmentRarity != subEquipmentOne.equipmentRarity ||
            mainEquipment.equipmentRarity != subEquipmentTwo.equipmentRarity)
        {
            Debug.Log("합성하기 위한 세 장비의 레어도가 일치하지 않습니다.");
            return;
        }

        if(inventory.GetInventoryIndex(mainEquipment) == inventory.GetInventoryIndex(subEquipmentOne)||
            inventory.GetInventoryIndex(mainEquipment) == inventory.GetInventoryIndex(subEquipmentTwo)||
            inventory.GetInventoryIndex(subEquipmentOne) == inventory.GetInventoryIndex(subEquipmentTwo))
        {
            Debug.Log("동일한 장비를 중복으로 사용하실 수 없습니다.");
            return;
        }

        int randomNumber = Random.Range(1, 101);
        int successNumber = CheckFuseSuccessRate(mainEquipment);
        Debug.Log($"{randomNumber} / {successNumber}");

        if (randomNumber <= successNumber)
        {
            Debug.Log("합성에 성공하였습니다! 장비의 레어도가 상승합니다.");
            int rarity = (int)mainEquipment.equipmentRarity;
            rarity += 1;
            mainEquipment.equipmentRarity = (EquipmentRarity)rarity;
        }
        else
        {
            Debug.Log("합성에 실패하였습니다.");
        }

        subSlot1.ClearSlot();
        subSlot2.ClearSlot();

        inventory.RemoveEquipment(subEquipmentOne);
        inventory.RemoveEquipment(subEquipmentTwo);

        mainSlot.UpdatePartUI();

        _inventoryPanel.Refresh();
    }

    //합성 성공률을 확인합니다.
    public int CheckFuseSuccessRate(Equipment mainEquipment)
    {
        int successRate = 0;
        switch (mainEquipment.equipmentRarity)
        {
            case EquipmentRarity.Normal:
                successRate = 90;
                break;
            case EquipmentRarity.Uncommon:
                successRate = 50;
                break;
            case EquipmentRarity.Rare:
                successRate = 25;
                break;
            case EquipmentRarity.Legendary:
                successRate = 5;
                break;
            case EquipmentRarity.Mythic:
                successRate = 0;
                break;
        }
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
