using System.Collections.Generic;

public static class EquipStatusStaticRule
{
    public static readonly Dictionary<Rarity, int> SubStatusCount = new Dictionary<Rarity, int>
    {
        { Rarity.Normal, 0 },
        { Rarity.Uncommon, 0 },
        { Rarity.Rare, 0 },
        { Rarity.Legendary, 1 },
        { Rarity.Mythtic, 2 }
    };

    public static readonly Dictionary<Equip_Type, EquipPartStatusRule> _rules = new Dictionary<Equip_Type, EquipPartStatusRule>
    {
        {
            Equip_Type.Weapon,
            new EquipPartStatusRule
            {
                //무기 주요 스테이터스 : 공격력, 마법공격력(미적용)
                MainStatus = new List<Status>
                {
                    Status.ATK,
                    Status.MagicATK
                },
                //무기 보조 스테이터스 : 공격속도, 크리티컬확률
                SubStatus = new List<Status>
                {
                    Status.AttackSpeed,
                    Status.CriticalChance
                }
            }
        },
        {
            Equip_Type.Clothes,
            new EquipPartStatusRule
            {
                //상의 주요 스테이터스 : 방어력, 마법저항력
                MainStatus = new List<Status>
                {
                    Status.DEF,
                    Status.MagicDEF
                },
                //상의 보조 스테이터스 : 체력, 체력재생력
                SubStatus = new List<Status>
                {
                    Status.HP,
                    Status.HPRegen
                },
            }
        },
        {
            Equip_Type.Pants,
            new EquipPartStatusRule
            {
                //하의 주요 스테이터스 : 체력, 방어력
                MainStatus = new List<Status>
                {
                    Status.HP,
                    Status.DEF
                },
                //하의 보조 스테이터스 : 마법저항력, 체력재생력
                SubStatus = new List<Status>
                {
                    Status.MagicDEF,
                    Status.HPRegen
                },
            }
        },
        {
            Equip_Type.Gloves,
            new EquipPartStatusRule
            {
                //장갑 주요 스테이터스 : 공격속도, 마법저항력
                MainStatus = new List<Status>
                {
                    Status.AttackSpeed,
                    Status.MagicDEF
                },
                //장갑 보조 스테이터스 : 공격력, 마법공격력(미적용)
                SubStatus = new List<Status>
                {
                    Status.ATK,
                    Status.MagicATK
                },
            }
        },
        {
            Equip_Type.Shoes,
            new EquipPartStatusRule
            {
                //신발 주요 스테이터스 : 이동속도, 체력재생력
                MainStatus = new List<Status>
                {
                    Status.HPRegen,
                    Status.MoveSpeed
                },
                //신발 보조 스테이터스 : 방어력, 체력
                SubStatus = new List<Status>
                {
                    Status.HP,
                    Status.DEF
                },
            }
        },
        {
            Equip_Type.Necklace,
            new EquipPartStatusRule
            {
                //목걸이 주요 스테이터스 : 크리티컬데미지, 이동속도
                MainStatus = new List<Status>
                {
                    Status.CriticalDamage,
                    Status.MoveSpeed
                },
                //목걸이 보조 스테이터스 : 마법저항력, 체력재생력
                SubStatus = new List<Status>
                {
                    Status.MagicDEF,
                    Status.HPRegen
                },
            }
        },
        {
            Equip_Type.Ring,
            new EquipPartStatusRule
            {
                //반지 주요 스테이터스 : 크리티컬확률, 크리티컬데미지
                MainStatus = new List<Status>
                {
                    Status.CriticalChance,
                    Status.CriticalDamage
                },
                //반지 보조 스테이터스 : 공격력, 마법공격력(미적용)
                SubStatus = new List<Status>
                {
                    Status.ATK,
                    Status.MagicATK
                },
            }
        }
    };
}
