using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class Equipment
{
    //CSV 구조도 순서대로 작성합니다!
    //equip
    public int equip_id;                                       // 장비의 ID값입니다.
    public string equip_name;                                  // 장비의 이름입니다.
    public Dictionary<Status, float> equip_status;    // 장비의 주요 스텟 전반을 담고 있을 Dictionary입니다.
    public Equip_Type equip_type;                              // 장비의 타입입니다. int값을 받으면 EquipType 열거형으로 자동 치환하여 가독성을 높입니다.
    public int equip_price;                                    // 장비의 판매가입니다.
    public string equip_model;                                 // 장비의 모델입니다. 모델? 장착 시 외형 변화를 위한 것인가...
    public string equip_icon;                                  // 장비의 아이콘, 즉 인벤토리에 출력 시 사용될 스프라이트를 가져오기 위한 경로입니다.
    public Sprite icon;                                        // 장비의 실질적 아이콘입니다. equip_icon을 통해 가져온 스프라이트를 넣을 공간입니다.

    //equip_rank
    //▼ 일반 : 1, 희귀 : 2, 레어 : 3, 전설 : 4, 신화 : 5
    public int equipment_Rarity;                      // 장비의 레어도입니다. 데이터에서 받아온 등급의 값의 가독성을 높이기 위해 열거형을 사용합니다.

    //equip_Upgrade
    public int equip_Upgrade_Value;

    public int equip_set_id;

    public int equip_level;

    public int equip_Upgrade;
    public int equip_Upgrade_Count;

    public int EquippedSlotIndex = -1; // 기본값은 -1, 장착 시 0, "반지 2 슬롯 한정" 1
    public bool isEquipped = false;    // 장착 시에만 true가 되는 장착 여부 확인용 bool형 매개변수
    public bool isFusing = false;


    /// <summary>
    /// 장착 슬롯에 해당 장비를 장착합니다.
    /// </summary>
    /// <param name="slotIndex">장착할 슬롯(반지 슬롯 대비)</param>
    public void SetEquipped(int slotIndex)
    {
        if(isEquipped == true)
        {
            Debug.Log("이미 장착한 장비입니다.");
            return;
        }
        //현재 장비를 장착하는 것이므로 장착 여부를 참으로 설정합니다.
        isEquipped = true;
        
        //현재 장착한 슬롯의 위치를 인자값으로 받아온 인덱스 값으로 설정합니다.
        EquippedSlotIndex = slotIndex;
    }

    /// <summary>
    /// 장비를 장착 해제합니다.
    /// </summary>
    public void UnEquip()
    {
        //현재 장비를 장착 해제한 것이므로 장착 여부를 거짓으로 설정합니다.
        isEquipped = false;

        //현재 장착한 슬롯 위치는 장착하지 않았을 때의 기본값으로 변경합니다.
        EquippedSlotIndex = -1;
    }

    /// <summary>
    /// 합성 칸에 등록했던 장비를 합성 칸에서 빼냅니다.
    /// </summary>
    public void CancelFuseMaterial()
    {
        isFusing = false;
    }

    /// <summary>
    /// 데이터의 장비 id와 추가적인 값들을 기반으로, 장비를 생성합니다.
    /// </summary>
    /// <param name="equipData">id값을 통해 데이터 테이블로부터 빼온 장비 id값</param>
    /// <param name="rarity">해당 장비의 등급</param>
    public Equipment(EquipData equipData, int rarity, int equipLevel)
    {

        #region equipData로부터 받아올 값
        equip_id = equipData.Equip_Id;                                            // 장비 id값
        equip_name = equipData.Equip_Name;                                        // 장비 이름
        equip_type = equipData.Equip_Type;                                        // 장착 부위
        equip_icon = equipData.Equip_Icon;                                        // 장비 아이콘
        equip_model = equipData.Equip_Model;                                      // ?

        equip_status = new Dictionary<Status, float>();
        AddEquipStatus(Status.HP, equipData.Equip_Hp);                           // 체력 스텟 존재할 시 스텟 추가
        AddEquipStatus(Status.ATK, equipData.Equip_Atk);                        // 공격력 스텟 존재할 시 스텟 추가
        AddEquipStatus(Status.MagicATK, equipData.Equip_Atk_M);                  // 마법공격력 스텟 존재할 시 스텟 추가
        AddEquipStatus(Status.AttackSpeed, equipData.Equip_Dps);                      // 공격속도 스텟 존재할 시 스텟 추가
        AddEquipStatus(Status.CriticalChance, equipData.Equip_Crt_Prob);             // 치명타 확률 스텟 존재할 시 스텟 추가
        AddEquipStatus(Status.CriticalDamage, equipData.Equip_Crt_Dmg);            // 치명타 피해량 스텟 존재할 시 스텟 추가
        AddEquipStatus(Status.DEF, equipData.Equip_Def);                       // 방어력 스텟 존재할 시 스텟 추가
        AddEquipStatus(Status.MagicDEF, equipData.Equip_Def_M);                 // 마법방어력 스텟 존재할 시 스텟 추가
        AddEquipStatus(Status.HPRegen, equipData.Equip_Hp_Regen);              // 체력 재생 스텟 존재할 시 스텟 추가
        AddEquipStatus(Status.MoveSpeed, equipData.Equip_Agi);                     // 이동속도 스텟 존재할 시 스텟 추가

        equip_price = equipData.Equip_Price;                                      // 장비 판매 가격
        #endregion

        #region 등급에 따라 받아올 값
        equipment_Rarity = rarity;                                                                                       // 장비 등급
        #endregion

        equip_set_id = GetSetEffect(equip_name);

        equip_level = equipLevel;

        equip_Upgrade = 0;
        equip_Upgrade_Count = 0;

    }

    /// <summary>
    /// 장비 데이터로부터, 해당 장비에서 유효한 스텟만 가져오는 메서드입니다.
    /// </summary>
    /// <param name="equipStatus">장비의 스텟</param>
    /// <param name="equipStatusValue">해당 장비 스텟의 값</param>
    public void AddEquipStatus(Status equipStatus,float equipStatusValue)
    {
        //0이 아닌 경우에만 포함시킵니다.
        if(equipStatusValue != 0)
        {
            equip_status.Add(equipStatus, equipStatusValue);
        }
    }

    public string GetEquipStatusString()
    {
        StringBuilder stringBuilder = new StringBuilder();
        int i = 0;
        foreach(var stat in equip_status)
        {
            stringBuilder.Append($"{stat.Key} + {stat.Value} ");
            if (i == 1)
                stringBuilder.Append("\n");
            i++;
        }
        return stringBuilder.ToString();
    }

    public float GetStatus(Status equipStatus)
    {
        return equip_status.TryGetValue(equipStatus, out float value) ? value : 0f ;
    }

    private int GetSetEffect(string equipName)
    {
        Dictionary<int, List<Equip_SetData>> allSets = DataManager.Instance.GetListDict<Equip_SetData>();
        int set_id = 0;
        foreach(var Set in allSets.Values)
        {
            foreach(var item in Set)
            {
                if (equipName.Contains(item.Equip_Set_Need_Name))
                    set_id = item.Equip_Set_Id;
            }
        }
        return set_id;
    }
}
