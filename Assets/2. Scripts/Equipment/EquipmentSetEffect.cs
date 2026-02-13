using UnityEngine;

public class EquipmentSetEffect
{
    public int set_Manager_Id;                      // 세트효과 구분용 아이디입니다.
    public string equip_Set_Need_Name;              // 세트효과 적용을 위해 해당 장비가 포함하는 것을 필요로 하는 문자열입니다.
    public int equip_Set_Need_Number;               // 세트효과 적용을 위해 필요한 세트 장비 장착 수입니다.
    public Status affection_Equip_Set;                 // 세트효과가 적용될 경우 영향을 받게 될 스텟의 종류입니다.
    public float affection_Equip_Set_Value;         // 세트효과가 적용될 경우 영향을 받게 될 스텟의 변동값입니다.

    public void Initialize(Equip_SetData equip_SetData)
    {
        set_Manager_Id = equip_SetData.Set_Manager_Id;
        equip_Set_Need_Name = equip_SetData.Equip_Set_Need_Name;
        equip_Set_Need_Number = equip_SetData.Equip_Set_Need_Number;
        affection_Equip_Set = equip_SetData.Affection_Equip_Set;
        affection_Equip_Set_Value = equip_SetData.Affection_Equip_Set_Value;
    }
}
