using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class FusePanel : BaseUI
{
    [SerializeField] EquipmentInventory inventory;      // 실질적인 인벤토리입니다.

    [Header("합성 슬롯")]
    [SerializeField] EquipSlot mainSlot;                // 합성으로 등급 상승을 노릴, 사라지지 않는 메인 장비를 놓을 공간입니다.
    [SerializeField] EquipSlot subSlot1;                // 합성으로 인해 소모될, 재료 장비를 놓을 공간입니다.
    [SerializeField] EquipSlot subSlot2;                // 합성으로 인해 소모될, 재료 장비를 놓을 공간입니다.

    [SerializeField] Button fuseButton;                 // 합성 준비가 되었을 때 합성을 진행하도록 해줄 버튼입니다.
    [SerializeField] InventoryPanel _inventoryPanel;    // 인벤토리를 열고 닫기 위한 패널입니다.
    [SerializeField] Toggle _useWeightToggle;           // 가중치를 사용할지 결정하기 위한 토글입니다.
    [SerializeField] TextMeshProUGUI _toggleText;

    private bool _isUsingWeight;

    private void Awake()
    {
        //합성 버튼에 합성 기능을 추가합니다.
        fuseButton.onClick.AddListener(OnClickFuse);
        mainSlot.OnSlotClicked +=_inventoryPanel.SetTargetSlot;
        mainSlot.gameObject.GetComponent<Button>().onClick.AddListener(() =>
        {
            mainSlot.OnClickSlot();
        });
        subSlot1.OnSlotClicked += _inventoryPanel.SetTargetSlot;
        subSlot1.gameObject.GetComponent<Button>().onClick.AddListener(() =>
        {
            subSlot1.OnClickSlot();
        });
        subSlot2.OnSlotClicked += _inventoryPanel.SetTargetSlot;
        subSlot2.gameObject.GetComponent<Button>().onClick.AddListener(() =>
        {
            subSlot2.OnClickSlot();
        });
        _useWeightToggle.onValueChanged.AddListener(UseWeight);
    }

    private void Start()
    {
        Close();
    }

    private void UseWeight(bool value)
    {
        _isUsingWeight = value;
        _toggleText.text = value == true ? "On" : "Off";
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

        //신화 등급의 장비를 합성에 사용하려고 시도할 경우 불가능하다는 안내를 띄웁니다.
        if((Rarity)mainEquipment.equipment_Rarity == Rarity.Mythtic ||
            (Rarity)subEquipmentOne.equipment_Rarity == Rarity.Mythtic ||
            (Rarity)subEquipmentTwo.equipment_Rarity == Rarity.Mythtic)
        {
            Debug.Log("신화 장비는 합성할 수 없습니다.");
            return;
        }

        //이름도 동일하게!
        if(mainEquipment.equip_name != subEquipmentOne.equip_name ||
            mainEquipment.equip_name != subEquipmentTwo.equip_name)
        {
            Debug.Log("합성하기 위한 세 장비의 이름이 일치하지 않습니다.");
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
        float successNumber = Mathf.RoundToInt(DataManager.Instance.GetData<Equip_RankData>(mainEquipment.equipment_Rarity + 1).Equip_Success_Prob * 100);
        float failNumber = 100 - successNumber;

        if(_isUsingWeight == true)
        {
            //보정값을 전부 사용하였을 때 100을 초과하지 않는다면 그냥 그 값을 그대로 더합니다.
            if (successNumber + mainEquipment.equip_Fuse_Weight <= 100)
            {
                successNumber += Mathf.RoundToInt(mainEquipment.equip_Fuse_Weight);
            }
            //초과하는 경우라면, 100을 달성할 값까지만 사용합니다.
            else
            {
                successNumber = 100;
            }
        }

        //디버그를 위한, 확인용 출력입니다.
        Debug.Log($"{randomNumber} / {successNumber}");

        //랜덤으로 뽑은 숫자가 성공을 결정할 숫자보다 작거나 같은 경우, 합성에 성공합니다.
        if (randomNumber <= successNumber)
        {
            Debug.Log("합성에 성공하였습니다! 장비의 레어도가 상승합니다.");
            //가중치를 사용한 것인 경우, 체크하고 감소시킵니다.
            if(_isUsingWeight == true)
            {
                mainEquipment.equip_Fuse_Weight = mainEquipment.equip_Fuse_Weight >= failNumber?
                    mainEquipment.equip_Fuse_Weight - failNumber : 0;
            }

            //장비의 레어도를 1 올려, 메인 장비의 레어도를 1 올립니다.
            int rarity = mainEquipment.equipment_Rarity;
            rarity += 1;
            mainEquipment.equipment_Rarity = rarity;
            mainEquipment.AddSubStatusOnUpgrade(DataManager.Instance.GetData<EquipData>(mainEquipment.equip_id),
                (Rarity)DataManager.Instance.GetData<Equip_RankData>(mainEquipment.equipment_Rarity - 1).Equip_Rank,
                (Rarity)DataManager.Instance.GetData<Equip_RankData>(mainEquipment.equipment_Rarity).Equip_Rank);

            //합성에 성공하여 장비의 등급이 신화까지 올라간 경우, 가지고 있는 가중치를 일괄 삭제합니다. (만에 하나 방지 코드가 작동 안할 경우를 대비)
            if((Rarity)mainEquipment.equipment_Rarity == Rarity.Mythtic)
            {
                mainEquipment.equip_Fuse_Weight = 0;
            }
        }

        //랜덤으로 뽑은 숫자가 성공을 결정할 숫자를 넘어갔을 경우, 합성에 실패합니다.
        else
        {
            Debug.Log($"합성에 실패하였습니다. 가중치를 {DataManager.Instance.GetData<Equip_RankData>(mainEquipment.equipment_Rarity + 1).Equip_Rank_Failure * 100}만큼 획득합니다.");
            //만약 가중치를 사용했다면, 가중치를 0으로 초기화합니다.
            if(_isUsingWeight == true)
            {
                mainEquipment.equip_Fuse_Weight = mainEquipment.equip_Fuse_Weight >= failNumber?
                    mainEquipment.equip_Fuse_Weight - failNumber : 0;
            }

            //실패한 등급 기준 가중치를 획득합니다.
            mainEquipment.equip_Fuse_Weight += DataManager.Instance.GetData<Equip_RankData>(mainEquipment.equipment_Rarity + 1).Equip_Rank_Failure * 100;
            Debug.Log($"현재 합성 가중치는 {mainEquipment.equip_Fuse_Weight}입니다.");
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
        _inventoryPanel.onInventoryChanged.Invoke();
    }

    //합성 성공률을 확인합니다.
    public int CheckFuseSuccessRate(Equipment mainEquipment)
    {
        //확인용 숫자를 초기화합니다.
        int successRate = 0;

        //인자값으로 받은 장비의 레어도에 따라 성공률을 결정합니다. (하드코딩)
        switch ((Rarity)mainEquipment.equipment_Rarity)
        {
            case Rarity.Normal:
                successRate = 90;
                break;

            case Rarity.Uncommon:
                successRate = 50;
                break;

            case Rarity.Rare:
                successRate = 25;
                break;

            case Rarity.Legendary:
                successRate = 5;
                break;

            case Rarity.Mythtic:
                successRate = 0;
                break;
        }

        //성공률을 계산하기 위한 숫자를 반환합니다.
        return successRate;
    }

    protected override void OnOpen()
    {
        
    }

    protected override void OnClose()
    {
        
    }
}
