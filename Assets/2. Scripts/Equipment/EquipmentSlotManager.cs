using System.Collections.Generic;
using UnityEngine;

public class EquipmentSlotManager : MonoBehaviour
{
    [SerializeField] List<EquipSlot> equipSlots;    // 장비를 장착할 슬롯들의 리스트입니다. 장비 장착 칸의 모든 슬롯을 넣어 주십시오.
    private Dictionary<int, int> SetDictionary;     // 장비의 세트 ID값을 Key로, 해당 ID값을 갖는 장비의 수를 Value로 갖는 Dictionary입니다.

    private float HP;                               // 체력
    private float ATK;                              // 공격력
    private float MagicATK;                         // 마법 공격력
    private float AttackSpeed;                      // 공격속도
    private float Critical_Chance;                  // 크리티컬 확률
    private float Critical_Damage;                  // 크리티컬 데미지
    private float DEF;                              // 방어력
    private float MagicDEF;                         // 마법 방어력
    private float HPRegen;                          // 체력 재생률
    private float MoveSpeed;                        // 이동속도

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

        //장비를 모두 살펴본 후, Dictionary를 다시 확인합니다.
        foreach(var set in SetDictionary)
        {
            //세트 테이블로부터 현재 활성화된 세트효과의 ID값을 통해 데이터를 받아옵니다.
            var SetData = DataManager.Instance.GetList<Equip_SetData>(set.Key);

            //해당 데이터는 리스트이므로, 리스트 내의 각각의 데이터를 확인합니다.
            foreach(var SetEffect in SetData)
            {
                //현재 장착 중인 해당 세트의 장비 수가 세트효과를 받기 위해 요구하는 장비 수보다 많을 경우 아래 코드를 실행합니다.
                if(set.Value >= SetEffect.Equip_Set_Need_Number)
                {
                    //세트 효과로 인해 받는 스텟 증가를 적용합니다.
                    ApplySetStats(SetEffect.Affection_Equip_Set, SetEffect.Affection_Equip_Set_Value);
                }
            }
        }

        //세트효과를 통해 얻는 스텟을 확인합니다. (세트효과는 여기서 끝)
        Debug.Log($"체력 : {HP}, 공격력 : {ATK}, 마법공격력 : {MagicATK}, 공격속도 : {AttackSpeed}, 치명타율 : {Critical_Chance}, 치명타피해 : {Critical_Damage}, 방어력 : {DEF}, 마법방어력 : {MagicDEF}, 체력재생 : {HPRegen}, 이동속도 : {MoveSpeed}");

        //세트효과 적용 후, 현재 장비의 효과를 처리합니다.
        GetAllStatusFromEquipment();
        Debug.Log($"체력 : {HP}, 공격력 : {ATK}, 마법공격력 : {MagicATK}, 공격속도 : {AttackSpeed}, 치명타율 : {Critical_Chance}, 치명타피해 : {Critical_Damage}, 방어력 : {DEF}, 마법방어력 : {MagicDEF}, 체력재생 : {HPRegen}, 이동속도 : {MoveSpeed}");
    }

    /// <summary>
    /// 세트효과를 통해 얻는 효과를 Manager에 적용합니다.
    /// </summary>
    /// <param name="stat">세트효과를 통해 영향을 받게 되는 스텟</param>
    /// <param name="value">해당 스텟의 값(증가량)</param>
    private void ApplySetStats(Status stat, float value)
    {
        switch (stat)
        {
            case Status.HP:
                HP += value;
                break;

            case Status.ATK:
                ATK += value;
                break;

            case Status.MagicATK:
                MagicATK += value;
                break;

            case Status.AttackSpeed:
                AttackSpeed += value;
                break;

            case Status.CriticalChance:
                Critical_Chance += value;
                break;

            case Status.CriticalDamage:
                Critical_Damage += value;
                break;

            case Status.DEF:
                DEF += value;
                break;

            case Status.MagicDEF:
                MagicDEF += value;
                break;

            case Status.HPRegen:
                HPRegen += value;
                break;

            case Status.MoveSpeed:
                MoveSpeed += value;
                break;

        }
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
                //해당 장비가 가지고 있는 스탯을 가져와 현재 이 클래스가 담당하는 해당 스텟에 넣어둡니다.
                //(연산식이 필요한 경우 아래 코드를 확인해주세요)
                HP += slot.equipped.GetStatus(Status.HP);
                ATK += slot.equipped.GetStatus(Status.ATK);
                MagicATK += slot.equipped.GetStatus(Status.MagicATK);
                AttackSpeed += slot.equipped.GetStatus(Status.AttackSpeed);
                Critical_Chance += slot.equipped.GetStatus(Status.CriticalChance);
                Critical_Damage += slot.equipped.GetStatus(Status.CriticalDamage);
                DEF += slot.equipped.GetStatus(Status.DEF);
                MagicDEF += slot.equipped.GetStatus(Status.MagicDEF);
                HPRegen += slot.equipped.GetStatus(Status.HPRegen);
                MoveSpeed += slot.equipped.GetStatus(Status.MoveSpeed);
            }
        }
    }
}
