using UnityEngine;
using System.Collections.Generic;

public class EquipmentSet
{
    public int equip_Set_Id;
    public int set_Manager_Id;                      // 세트효과 구분용 아이디입니다.
    public string equip_Set_Need_Name;              // 세트효과 적용을 위해 해당 장비가 포함하는 것을 필요로 하는 문자열입니다.
    public int equip_Set_Need_Number;               // 세트효과 적용을 위해 필요한 세트 장비 장착 수입니다.
    public Status affection_Equip_Set;              // 세트효과가 적용될 경우 영향을 받게 될 스텟의 종류입니다.
    public float affection_Equip_Set_Value;         // 세트효과가 적용될 경우 영향을 받게 될 스텟의 변동값입니다.


    /// <summary>
    /// 세트효과의 관리용 아이디와 그 내부 구성 요소들을 전부 받아 하나의 세트효과를 저장하는 생성자입니다.
    /// 동일한 세트효과이나 장착 개수가 다르거나 하는 이유로 동일 ID를 공유하는 세트효과를 분류하기 위함입니다.
    /// </summary>
    /// <param name="SetManagerId">동일 ID 내 세트효과 관리용 ID</param>
    /// <param name="EquipSetNeedName">세트효과에 포함되기 위해 해당 장비의 이름에 포함되어야 하는 문자열</param>
    /// <param name="EquipSetNeedNumber">세트효과가 충족되기 위해 장착해야 하는 세트 장비의 수</param>
    /// <param name="AffectionEquipSet">세트 효과가 적용되었을 때 영향을 받게 될 스텟</param>
    /// <param name="AffectionEquipSetValue">세트 효과가 적용되었을 때 영향 받는 스텟의 변화 값</param>
    public EquipmentSet(Equip_SetData equip_SetData)
    {
        set_Manager_Id = equip_SetData.Set_Manager_Id;
        equip_Set_Need_Name = equip_SetData.Equip_Set_Need_Name;
        equip_Set_Need_Number = equip_SetData.Equip_Set_Need_Number;
        affection_Equip_Set = equip_SetData.Affection_Equip_Set;
        affection_Equip_Set_Value = equip_SetData.Affection_Equip_Set_Value;
    }

}
