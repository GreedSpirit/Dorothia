using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

//파츠 별 가지게 될 옵션을 정의하기 위한, 추후 사용할 열거형.
public enum EquipmentStatus
{
    체력 = 1, 공격력, 마법공격력, 공격속도, 크리티컬확률, 크리티컬데미지, 방어력, 마법저항력, 체력재생력, 이동속도
}
public class Equipment
{
    //CSV 구조도 순서대로 작성합니다!
    //equip
    public int equip_id;                                       // 장비의 ID값입니다.
    public string equip_name;                                  // 장비의 이름입니다.
    public Dictionary<EquipmentStatus, float> equip_status;    // 장비의 주요 스텟 전반을 담고 있을 Dictionary입니다.
    public Equip_Type equip_type;                              // 장비의 타입입니다. int값을 받으면 EquipType 열거형으로 자동 치환하여 가독성을 높입니다.
    public int equip_price;                                    // 장비의 판매가입니다.
    public string equip_model;                                 // 장비의 모델입니다. 모델? 장착 시 외형 변화를 위한 것인가...
    public string equip_icon;                                  // 장비의 아이콘, 즉 인벤토리에 출력 시 사용될 스프라이트를 가져오기 위한 경로입니다.
    public Sprite icon;                                        // 장비의 실질적 아이콘입니다. equip_icon을 통해 가져온 스프라이트를 넣을 공간입니다.

    //equip_rank
    //▼ 일반 : 1, 희귀 : 2, 레어 : 3, 전설 : 4, 신화 : 5
    public Equip_Rank equipment_Rarity;               // 장비의 레어도입니다. 데이터에서 받아온 등급의 값의 가독성을 높이기 위해 열거형을 사용합니다.
    public EquipmentRank rankData;                    // 레어도에 따른 다른 데이터값을 저장한 클래스입니다.

    //equip_Upgrade


    public int equip_set_id;

    public int EquippedSlotIndex = -1; // 기본값은 -1, 장착 시 0, "반지 2 슬롯 한정" 1
    public bool isEquipped = false;    // 장착 시에만 true가 되는 장착 여부 확인용 bool형 매개변수


    /// <summary>
    /// 장착 슬롯에 해당 장비를 장착합니다.
    /// </summary>
    /// <param name="slotIndex">장착할 슬롯(반지 슬롯 대비)</param>
    public void SetEquipped(int slotIndex)
    {
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
    /// 데이터의 장비 id와 추가적인 값들을 기반으로, 장비를 생성합니다.
    /// </summary>
    /// <param name="equipData">id값을 통해 데이터 테이블로부터 빼온 장비 id값</param>
    /// <param name="rarity">해당 장비의 등급</param>
    public Equipment(EquipData equipData, Equip_Rank rarity)
    {

        #region equipData로부터 받아올 값
        equip_id = equipData.Equip_Id;                                            // 장비 id값
        equip_name = equipData.Equip_Name;                                        // 장비 이름
        equip_type = equipData.Equip_Type;                                        // 장착 부위
        equip_icon = equipData.Equip_Icon;                                        // 장비 아이콘
        equip_model = equipData.Equip_Model;                                      // ?

        equip_status = new Dictionary<EquipmentStatus, float>();
        AddEquipStatus(EquipmentStatus.체력, equipData.Equip_Hp);                           // 체력 스텟 존재할 시 스텟 추가
        AddEquipStatus(EquipmentStatus.공격력, equipData.Equip_Atk);                        // 공격력 스텟 존재할 시 스텟 추가
        AddEquipStatus(EquipmentStatus.마법공격력, equipData.Equip_Atk_M);                  // 마법공격력 스텟 존재할 시 스텟 추가
        AddEquipStatus(EquipmentStatus.공격속도, equipData.Equip_Dps);                      // 공격속도 스텟 존재할 시 스텟 추가
        AddEquipStatus(EquipmentStatus.크리티컬확률, equipData.Equip_Crt_Prob);             // 치명타 확률 스텟 존재할 시 스텟 추가
        AddEquipStatus(EquipmentStatus.크리티컬데미지, equipData.Equip_Crt_Dmg);            // 치명타 피해량 스텟 존재할 시 스텟 추가
        AddEquipStatus(EquipmentStatus.방어력, equipData.Equip_Def);                       // 방어력 스텟 존재할 시 스텟 추가
        AddEquipStatus(EquipmentStatus.마법저항력, equipData.Equip_Def_M);                 // 마법방어력 스텟 존재할 시 스텟 추가
        AddEquipStatus(EquipmentStatus.체력재생력, equipData.Equip_Hp_Regen);              // 체력 재생 스텟 존재할 시 스텟 추가
        AddEquipStatus(EquipmentStatus.이동속도, equipData.Equip_Agi);                     // 이동속도 스텟 존재할 시 스텟 추가

        equip_price = equipData.Equip_Price;                                      // 장비 판매 가격
        #endregion

        #region 등급에 따라 받아올 값
        equipment_Rarity = rarity;                                                                                       // 장비 등급
        rankData = new EquipmentRank(DataManager.Instance.GetData<Equip_RankData>((int)equipment_Rarity + 40000));       // 해당 장비 등급에 따른 속성값들
        #endregion

        
    }

    /// <summary>
    /// 장비 데이터로부터, 해당 장비에서 유효한 스텟만 가져오는 메서드입니다.
    /// </summary>
    /// <param name="equipStatus">장비의 스텟</param>
    /// <param name="equipStatusValue">해당 장비 스텟의 값</param>
    public void AddEquipStatus(EquipmentStatus equipStatus,float equipStatusValue)
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
}
