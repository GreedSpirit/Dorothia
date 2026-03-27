using System;

[Serializable]
public class GremlinSaveData
{
    public string guid;        // GUID
    public int petID;          // 펫의 ID값
    public int rarity;         // 펫의 등급
    public int level;          // 펫의 강화 단계
    public int enchantCount;   // 해당 강화 단계에서의 강화 시도 횟수
    public bool isEquipped;    // 장착 여부
}
