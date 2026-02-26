using System.Collections.Generic;
using UnityEngine;

public class EquipmentSlotManager : MonoBehaviour
{
    public static EquipmentSlotManager Instance;

    [SerializeField] List<EquipSlot> equipSlots;    // 장비를 장착할 슬롯들의 리스트입니다. 장비 장착 칸의 모든 슬롯을 넣어 주십시오.
    private Dictionary<int, int> SetDictionary;     // 장비의 세트 ID값을 Key로, 해당 ID값을 갖는 장비의 수를 Value로 갖는 Dictionary입니다.

    public Dictionary<Status, float> EquipmentStatus = new Dictionary<Status, float>();
    public Dictionary<Status, float> SetStatus = new Dictionary<Status, float>();

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
                Debug.Log($"장비 세트 ID : {SetEffect.Equip_Set_Id}, 요구 장비 수 : {SetEffect.Equip_Set_Need_Number}, 장착 장비 수 : {set.Value}");
                //현재 장착 중인 해당 세트의 장비 수가 세트효과를 받기 위해 요구하는 장비 수보다 많을 경우 아래 코드를 실행합니다.
                if(set.Value >= SetEffect.Equip_Set_Need_Number)
                {
                    Debug.Log($"세트 효과 {SetEffect.Equip_Set_Need_Number}셋 적용, {SetEffect.Affection_Equip_Set} {SetEffect.Affection_Equip_Set_Value} 증가");
                    //세트 효과로 인해 받는 스텟 증가를 적용합니다.
                    ApplySetStats(SetEffect.Affection_Equip_Set, SetEffect.Affection_Equip_Set_Value);
                }
            }
        }

        //세트효과를 통해 얻는 스텟을 확인합니다. (세트효과는 여기서 끝)
        foreach(var a in SetStatus.Keys)
        {
            Debug.Log($"{a} = {SetStatus[a]}");
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
        foreach(var a in EquipmentStatus.Keys)
        {
            Debug.Log($"{a} = {EquipmentStatus[a]}");
        }
        //StatManager.Instance.RefreshStats();
    }

    /// <summary>
    /// 세트효과를 통해 얻는 효과를 Manager에 적용합니다.
    /// </summary>
    /// <param name="stat">세트효과를 통해 영향을 받게 되는 스텟</param>
    /// <param name="value">해당 스텟의 값(증가량)</param>
    private void ApplySetStats(Status stat, float value)
    {
        SetStatus[stat] += value;
        Debug.Log($"{stat} {value}만큼 증가 성공, 현재 스탯 {SetStatus[stat]}");
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
                foreach(var a in slot.equipped.equip_status.Keys)
                {
                    EquipmentStatus[a] += slot.equipped.GetStatus(a);
                }
            }
        }
    }
}
