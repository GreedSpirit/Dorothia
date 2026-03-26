using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class EquipmentSlotManager : MonoBehaviour
{
    public static EquipmentSlotManager Instance;

    [SerializeField] List<EquipSlot> equipSlots;    // 장비를 장착할 슬롯들의 리스트입니다. 장비 장착 칸의 모든 슬롯을 넣어 주십시오.
    private Dictionary<int, int> SetDictionary;     // 장비의 세트 ID값을 Key로, 해당 ID값을 갖는 장비의 수를 Value로 갖는 Dictionary입니다.

    public Dictionary<Status, float> EquipmentStatus = new Dictionary<Status, float>();
    public Dictionary<Status, float> SetStatus = new Dictionary<Status, float>();

    //플레이어스탯에게 알림용 이벤트
    public event Action OnEquipChanged;

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }

        Instance = this;
    }

    public void ApplyEquipmentSet()
    {
        //Dictionary를 초기화시킵니다. (장비는 초기 상태입니다.)
        SetDictionary = new Dictionary<int, int>();

        //모든 장비 슬롯들을 기준으로 아래 코드를 실행합니다.
        foreach (var slot in equipSlots)
        {
            //슬롯에 장비가 존재하며 그 장비의 세트 아이디가 0이 아닐 경우
            if(slot.equipped != null && slot.equipped.equip_set_id != 0)
            {
                //Dictionary가 해당 세트효과 ID값을 Key로 가지고 있지 않다면 아래 코드를 실행합니다.
                if(!SetDictionary.ContainsKey(slot.equipped.equip_set_id))
                {
                    //Dictionary에 1(해당 장비)의 Value를 갖는 Key를 등록합니다.
                    SetDictionary.Add(slot.equipped.equip_set_id, 1);
                }

                //Dictionary가 해당 세트효과 ID값을 이미 Key로 가지고 있다면 아래 코드를 실행합니다.
                else
                {
                    //해당 Key의 Value를 1 증가시킵니다.
                    SetDictionary[slot.equipped.equip_set_id]++;
                }
            }
        }

        SetStatus = new Dictionary<Status, float>
        {
            { Status.HP, 0 },
            { Status.ATK, 0 },
            { Status.MagicATK, 0 },
            { Status.AttackSpeed, 0 },
            { Status.CriticalChance, 0 },
            { Status.CriticalDamage, 0 },
            { Status.DEF, 0 },
            { Status.MagicDEF, 0 },
            { Status.HPRegen, 0 },
            { Status.MoveSpeed, 0 }
        };

        //장비를 모두 살펴본 후, Dictionary를 다시 확인합니다.
        foreach(var set in SetDictionary)
        {
            //세트 테이블로부터 현재 활성화된 세트효과의 ID값을 통해 데이터를 받아옵니다.
            var SetData = DataManager.Instance.GetList<Equip_SetData>(set.Key);
            //해당 데이터는 리스트이므로, 리스트 내의 각각의 데이터를 확인합니다.
            foreach (var SetEffect in SetData)
            {
                //현재 장착 중인 해당 세트의 장비 수가 세트효과를 받기 위해 요구하는 장비 수보다 많을 경우 아래 코드를 실행합니다.
                if(set.Value >= SetEffect.Equip_Set_Need_Number)
                {
                    //세트 효과로 인해 받는 스텟 증가를 적용합니다.
                    ApplySetStats(SetEffect.Affection_Equip_Set, SetEffect.Affection_Equip_Set_Value);
                }
            }
        }

        EquipmentStatus = new Dictionary<Status, float>
        {
            { Status.HP, 0 },
            { Status.ATK, 0 },
            { Status.MagicATK, 0 },
            { Status.AttackSpeed, 0 },
            { Status.CriticalChance, 0 },
            { Status.CriticalDamage, 0 },
            { Status.DEF, 0 },
            { Status.MagicDEF, 0 },
            { Status.HPRegen, 0 },
            { Status.MoveSpeed, 0 }
        };
        //세트효과 적용 후, 현재 장비의 효과를 처리합니다.
        GetAllStatusFromEquipment();
        //스탯계산 후 적용
        StatManager.Instance.RefreshStats();
        //플레이어스탯에게 알림
        OnEquipChanged?.Invoke();
    }

    /// <summary>
    /// 세트효과를 통해 얻는 효과를 Manager에 적용합니다.
    /// </summary>
    /// <param name="stat">세트효과를 통해 영향을 받게 되는 스텟</param>
    /// <param name="value">해당 스텟의 값(증가량)</param>
    private void ApplySetStats(Status stat, float value)
    {
        SetStatus[stat] += value;
    }

    /// <summary>
    /// 모든 슬롯으로부터 장착 중인 장비의 스텟을 가져옵니다.
    /// </summary>
    private void GetAllStatusFromEquipment()
    {
        
        //슬롯 리스트의 모든 슬롯을 대상으로 아래 코드를 실행합니다.
        foreach(EquipSlot slot in equipSlots)
        {
            //해당 슬롯에 장착 중인 장비가 존재한다면 아래 코드를 실행합니다.
            if(slot.equipped != null)
            {
                foreach(var status in slot.equipped.equip_status.Keys)
                {
                    EquipmentStatus[status] += ItemCalculator.GetStatus(slot.equipped, status);
                }
            }
        }
    }

    /// <summary>
    /// 현재 장비가 가진 모든 스탯의 정보를 출력합니다.
    /// 인벤토리 정보 패널에서 선택된 장비의 스탯을 출력하기 위함입니다.
    /// </summary>
    /// <returns></returns>
    public string GetEquipStatusString(Equipment equip)
    {
        StringBuilder stringBuilder = new StringBuilder();
        //몇 개의 정보를 연달아 적었는지 나타냅니다.
        int i = 0;
        foreach (var stat in equip.equip_status)
        {
            //(스탯 값) 형태로 출력되도록 합니다. ex) HP 5
            stringBuilder.Append($"{stat.Key} + {stat.Value} ");

            //i가 1이면 두 가지 정보를 출력한 것이므로 일단 내립니다.
            //최대 4개의 스탯을 가질 수 있으므로 1에서 내리고 난 뒤 추가 작업은 진행하지 않습니다.
            if (i == 1)
            {
                stringBuilder.Append("\n");
            }

            //i값 상승.
            i++;
        }
        return stringBuilder.ToString();
    }
}
