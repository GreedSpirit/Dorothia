using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 장비 관련입니다.
/// </summary>
public class InventoryEquipFunction : MonoBehaviour
{
    [Header("인벤토리 패널로부터 받아와야 하는 목록")]
    [SerializeField] InventoryPanel _inventoryPanel;
    [SerializeField] EquipmentInventory _equipmentInventory;

    [Header("기타 버튼")]
    [SerializeField] Button _autoEquipButtons;                   // 자동장착 버튼입니다.
    [SerializeField] Button _lockButton;                         // 잠금 버튼입니다.
    [SerializeField] TextMeshProUGUI _lockButtonText;            // 잠금 버튼의 텍스트입니다.

    private void Awake()
    {
        //자동장착 기능 추가 - 현재 인벤토리 기준 장비 장착
        _autoEquipButtons.onClick.AddListener(() =>
        {
            AutoEquip(_inventoryPanel.currentPart);
            _inventoryPanel.onInventoryChanged?.Invoke();
        });
    }

    /// <summary>
    /// 자동 장착 시의 동작입니다.
    /// </summary>
    /// <param name="part">장착 부위</param>
    public void AutoEquip(Equip_Type part)
    {
        //인벤토리로부터 해당 장착 부위의 장비 리스트를 가져옵니다.
        List<Equipment> list = _equipmentInventory.GetInventory(part);

        //리스트 안에 들어있는 게 없다면?
        if (list.Count == 0)
        {
            //아무것도 없다는 것이므로 그냥 반환시킵니다.
            return;
        }

        //장착 중인 장비를 확인합니다.
        int currentEquipWeight = _inventoryPanel.targetSlot.equipped != null ? ItemCalculator.GetEquipScore(_inventoryPanel.targetSlot.equipped) : 0;
        //장착할 장비를 선언하고, 장착 중인 장비가 있다면 해당 장비를 넣습니다. (없어도 null이 들어갈 것입니다.)
        Equipment equipmentToEquip = _inventoryPanel.targetSlot.equipped;

        for (int i = 0; i < list.Count; i++)
        {
            //이미 장착 중인 장비라면, 해당 칸에 이미 장착되었거나, 반지의 경우 다른 칸에 이미 장착된 경우입니다.
            //그러니 다음 단계로 넘어갑니다.
            if (list[i].isEquipped == true)
            {
                continue;
            }
            //현재 칸의 장비 점수를 체크합니다.
            int score = ItemCalculator.GetEquipScore(list[i]);

            //해당 장비 점수가 현재의 가중치보다 높을 경우
            if (score > currentEquipWeight)
            {
                //가중치를 해당 점수로 두고
                currentEquipWeight = score;
                //해당 장비를 장착할 장비로 선언한 후
                equipmentToEquip = list[i];
                //다음 단계로 넘어갑니다.
                continue;
            }
            //해당 장비 점수가 현재의 가중치보다 낮을 경우
            else if (score < currentEquipWeight)
            {
                //바로 다음 단계로 넘어갑니다.
                continue;
            }

            //여기 도착했다는 건 장비 점수가 같다는 이야기입니다.
            //점수가 같은데 장비가 없다는 건 가중치 0, 점수 0의 장비라는 것.
            if (equipmentToEquip == null)
            {
                //장비가 없다는 뜻이니 우선 장착할 장비로 둡니다.
                equipmentToEquip = list[i];
                //다음 단계로 넘어갑니다.
                continue;
            }
            //장착할 장비가 존재한다면, 점수가 같을 때 처음 봐야 하는 것은 등급입니다.
            //등급이 서로 다를 경우, 장착할 장비를 결정합니다.
            if (equipmentToEquip.equipment_Rarity != list[i].equipment_Rarity)
            {
                //등급이 높은 쪽이 장착할 대상이 됩니다.
                equipmentToEquip = equipmentToEquip.equipment_Rarity > list[i].equipment_Rarity ?
                    equipmentToEquip : list[i];
                //다음 단계로 넘어갑니다.
                continue;
            }
            //장비가 존재하고, 등급도 같다면 다음에 봐야 하는 것은 강화도입니다.
            if (equipmentToEquip.equip_Upgrade != list[i].equip_Upgrade)
            {
                //강화도가 높은 쪽이 장착할 대상이 됩니다.
                equipmentToEquip = equipmentToEquip.equip_Upgrade > list[i].equip_Upgrade ?
                    equipmentToEquip : list[i];
                //다음 단계로 넘어갑니다.
                continue;
            }
            //장비가 존재하고, 등급도 같으며, 강화도마저 같으면 획득한 순서를 살펴봅니다.
            //다만, GUID가 같으면 같은 장비인 것이고, GUID가 다르면 list[i]쪽이 더 나중에 획득한 장비입니다. ( 현재 배치순서 변경 불가 )
            //따라서, 여기까지 왔으면 다음 단계로 넘어갑니다.
        }
        //전부 진행했다면, 해당 장비를 장착합니다.
        _inventoryPanel.AddToSlot(equipmentToEquip);
    }
    public void ChangeLockButtonState(Equipment equip)
    {
        if (equip.isLocked == true)
        {
            _lockButtonText.text = "Unlock";
            _lockButton.onClick.RemoveAllListeners();
            _lockButton.onClick.AddListener(() =>
            {
                UnlockEquipment(equip);
            });
        }
        else
        {
            _lockButtonText.text = "Lock";
            _lockButton.onClick.RemoveAllListeners();
            _lockButton.onClick.AddListener(() =>
            {
                LockEquipment(equip);
            });
        }
    }
    /// <summary>
    /// 장비를 잠금 상태로 변경합니다.
    /// </summary>
    /// <param name="equip">잠글 장비</param>
    public void LockEquipment(Equipment equip)
    {
        //반환되지 않았다면 고른 장비가 존재한다는 것.
        //잠겨있는 경우에는 잠금을 해제합니다.
        if (equip.isLocked == false)
        {
            equip.isLocked = true;
        }
        _inventoryPanel.onInventoryChanged?.Invoke();
        _inventoryPanel.OnClickItem(equip);
    }

    /// <summary>
    /// 장비를 잠금 해제 상태로 변경합니다.
    /// </summary>
    /// <param name="equip">잠금 해제할 장비</param>
    public void UnlockEquipment(Equipment equip)
    {
        //반환되지 않았다면 고른 장비가 존재한다는 것.
        //잠겨있는 경우에는 잠금을 해제합니다.
        if (equip.isLocked == true)
        {
            equip.isLocked = false;
        }
        _inventoryPanel.onInventoryChanged?.Invoke();
        _inventoryPanel.OnClickItem(equip);
    }
}
