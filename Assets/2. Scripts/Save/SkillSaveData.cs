using System;
using System.Collections.Generic;

[Serializable]
public class SerializableSkillKey
{
    public int sid;
    public Skill_Type type;
    public Rarity rarity;
    public bool isScroll;

    public SerializableSkillKey() { }

    public SerializableSkillKey(SkillKey key)
    {
        sid = key.sid;
        type = key.type;
        rarity = key.rarity;
        isScroll = key.isScroll;
    }

    public SkillKey ToSkillKey() => new SkillKey(sid, type, rarity, isScroll);
}

// 인벤토리 한 칸 (Key + 수량)
[Serializable]
public class InventoryEntry
{
    public SerializableSkillKey key;
    public int count;
}

// 언락된 스킬 한 칸 (Key + 레벨)
[Serializable]
public class UnlockedSkillEntry
{
    public SerializableSkillKey key;
    public int level;
}

// 슬롯 한 칸 (null 여부 + Key)
[Serializable]
public class SlotEntry
{
    public bool isEmpty;
    public SerializableSkillKey key;
}

[Serializable]
public class SkillSaveData
{
    public List<InventoryEntry> inventory = new();
    public List<UnlockedSkillEntry> unlockedSkills = new();

    public SlotEntry[] activeSlots = new SlotEntry[SkillManager.ACTIVE_SLOT_MAX];
    public SlotEntry[] passiveSlots = new SlotEntry[SkillManager.PASSIVE_SLOT_MAX];
    public SlotEntry ultimateSlot;

    public float mysteryGauge;
}