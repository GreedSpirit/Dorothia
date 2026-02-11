using System.Collections.Generic;
using UnityEngine;

public class InventoryPanel : MonoBehaviour
{
    [SerializeField] EquipmentUI equipmentUI;                   // 장착 중인 장비를 보여주는 UI
    [SerializeField] private List<InventorySlot> slots;         // 인벤토리 슬롯의 배열
    [SerializeField] EquipmentInventory equipmentInventory;     // 인벤토리

    private EquipmentPart currentPart;                          // 현재 열람하고자 하는 인벤토리의 장착 부위 정보

    private void Awake()
    {
        //인스펙터상의 실수 확인용
        if (slots.Count != 16)
            Debug.Log("InventoryPanel - 슬롯의 수가 맞지 않습니다.");
    }

    //아이템이 들어있는 칸을 눌렀을 때의 동작입니다.
    public void OnClickItem(Equipment equip)
    {
        //해당 부위 칸에 아이템을 장착합니다.
        equipmentUI.EquipPart(equip);
    }

    /// <summary>
    /// 인자값으로 받은 장착 부위에 맞는 인벤토리를 엽니다.
    /// </summary>
    /// <param name="part">인벤토리를 확인하고자 하는 장착 부위</param>
    public void Open(EquipmentPart part)
    {
        //현재 장착 부위를 인자값으로 받아온 값과 일치시킵니다.
        currentPart = part;

        //인벤토리를 다시 불러옵니다.
        Refresh();
    }

    /// <summary>
    /// 인벤토리 내부를 새로고침하는 메서드입니다.
    /// </summary>
    private void Refresh()
    {
        //장착 부위에 맞는 인벤토리를 가져옵니다.
        List<Equipment> list = equipmentInventory.GetInventory(currentPart);

        //인벤토리 슬롯 길이만큼 다음 동작을 실행합니다.
        for(int i = 0; i<slots.Count; i++)
        {
            //리스트에 있는 총 수보다 i가 적으면 그 리스트에서 장비 정보를 가져옵니다.
            if(i < list.Count)
            {
                SetSlot(slots[i], list[i]);
            }

            //리스트에 있는 총 개수보다 i가 같거나 크면 비웁니다.
            else
            {
                ClearSlot(slots[i]);
            }
        }
    }

    /// <summary>
    /// 인벤토리 슬롯에 장비를 지정합니다.
    /// </summary>
    /// <param name="slot">인벤토리 슬롯</param>
    /// <param name="equip">해당 슬롯과 index가 일치하는 인벤토리 내 장비</param>
    private void SetSlot(InventorySlot slot, Equipment equip)
    {
        //스프라이트를 동일하게 맞추고, 그 스프라이트를 활성화합니다.
        slot.icon.sprite = equip.icon;
        slot.icon.enabled = true;

        //장착 중이라는 것을 볼 수 있도록 장착 표기를 활성화합니다.
        slot.equipMark.SetActive(equip.isEquipped);

        //해당 슬롯 버튼을 눌렀을 때의 동작을 전부 지우고 새로 추가합니다.
        slot.button.onClick.RemoveAllListeners();
        slot.button.onClick.AddListener(() =>
        {
            //기존 장비가 있다면 해제하여, 현재 장비를 장착합니다.
            OnClickItem(equip);
        });
    }

    /// <summary>
    /// 해당 인벤토리 슬롯을 비웁니다.
    /// </summary>
    /// <param name="slot">비우고자 하는 슬롯</param>
    private void ClearSlot(InventorySlot slot)
    {
        //스프라이트를 없애고, 스프라이트의 활성 여부를 거짓으로 설정합니다.
        slot.icon.sprite = null;
        slot.icon.enabled = false;

        //장착 중이 아니므로 장착 표기를 비활성화합니다.
        slot.equipMark.SetActive(false);

        //해당 슬롯 버튼을 눌렀을 때 동작을 전부 지웁니다. 이 버튼을 눌렀을 때는 어떤 동작도 일어나면 안 됩니다.
        slot.button.onClick.RemoveAllListeners();
    }
}
