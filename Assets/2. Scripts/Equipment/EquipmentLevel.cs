using UnityEngine;

public class EquipmentLevel
{
    public int equip_level;          // 장비의 레벨입니다.
    public float equip_level_value;  // 장비 레벨에 따른 스테이터스 상승값입니다.

    /// <summary>
    /// 레벨데이터 테이블로부터 정보를 받아 해당 정보 기반으로 생성합니다.
    /// </summary>
    /// <param name="equip_LevelData"></param>
    public EquipmentLevel(Equip_LevelData equip_LevelData)
    {
        equip_level = equip_LevelData.Equip_Level;
        equip_level_value = equip_LevelData.Equip_Level_Value;
    }
}
