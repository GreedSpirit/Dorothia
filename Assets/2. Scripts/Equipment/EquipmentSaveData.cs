[System.Serializable]
public class EquipmentSaveData
{
    public Equip_Type equipType;            // 장비 타입
    public int slotIndex;                   // 장비의 칸 위치

    public string instanceGUID;             // 해당 장비의 GUID
    public int equipID;                     // 해당 장비의 테이블상 ID
    public int equipLevel;                  // 해당 장비의 장착레벨
    public int equipEnchant;                // 해당 장비의 강화단계
    public int equipRarity;                 // 해당 장비의 등급
    public float enchantWeight;             // 해당 장비의 강화 가중치
    public float fuseWeight;                // 해당 장비의 합성 가중치
}
