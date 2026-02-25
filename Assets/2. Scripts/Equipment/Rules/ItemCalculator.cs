using UnityEngine;

public static class ItemCalculator
{
    public static int SalvageScrapCalculate(Equipment equip)
    {
        var breakData = DataManager.Instance.GetData<Equip_BreakData>(equip.equipment_Rarity);
        return ((equip.equip_level + breakData.Equip_Break_Gold_Scrap) / 10);
    }
    public static int SalvageGoldCalculate(Equipment equip)
    {
        int gold = 0;
        var breakData = DataManager.Instance.GetData<Equip_BreakData>(equip.equipment_Rarity);
        //강화 수치가 0이 아닐 경우, 테이블로부터 강화 수치 기준 데이터를 받아와 골드에 공식을 적용합니다.
        if (equip.equip_Upgrade > 0)
        {
            var upgradeData = DataManager.Instance.GetData<Equip_UpgradeData>(equip.equip_Upgrade);
            gold = Mathf.RoundToInt
                (
                    Mathf.RoundToInt(equip.equip_price * Mathf.Pow(equip.equip_Upgrade, DataManager.Instance.GetData<Equip_Upgrade_GoldData>(equip.equip_Upgrade).Equip_Upgrade_Value)) *
                    breakData.Equip_Break_Gold / DataManager.Instance.GetData<Equip_UpgradeData>(equip.equip_Upgrade).Equip_Success_Prob * 0.2f
                );
        }
        //강화 수치가 0인 경우, 골드는 기본값으로 적용하고 스크랩만 계산하여 지급합니다.
        if (equip.equip_Upgrade == 0)
        {
            gold = equip.equip_price;
        }
        return gold;
    }

    public static int SellCalculate(Equipment equip)
    {
        //해당 장비의 강화 단계를 기준으로 데이터를 먼저 불러옵니다.
        var upgradeData = DataManager.Instance.GetData<Equip_UpgradeData>(equip.equip_Upgrade);

        //판매가격인 골드 지역변수를 선언합니다.
        int equipGold = 0;

        //강화 단계가 50(현재 최대치)이거나, 모종의 툴을 사용하여 그 이상이 나왔을 경우
        //오류를 방지하기 위해 49단계 기준으로 진행합니다.
        if (equip.equip_Upgrade > 50)
        {
            //공식 : (기본 판매가격 * 장비 장착 레벨 * 장비 등급에 따른 가중치) + (강화 평균 소모 골드 * 0.2)
            //강화 평균 소모 골드 : 해당 단계 기준 1회 강화 비용(장비 기본 판매가격 * (현재 강화단계 + 1)값의 (골드데이터 상의 배율)제곱 * 등급에 따른 가중치) / 성공 확률
            int firstGold = Mathf.RoundToInt
                (
                    equip.equip_price * equip.equip_level *
                    GetRarityWeight((Rarity)DataManager.Instance.GetData<Equip_RankData>(equip.equipment_Rarity).Equip_Rank)
                );
            int secondGold = Mathf.RoundToInt
                (
                    Mathf.RoundToInt(equip.equip_price * Mathf.Pow(50, DataManager.Instance.GetData<Equip_Upgrade_GoldData>(50).Equip_Upgrade_Value)) *
                    equip.GetEnchantWeightByRarity((Rarity)DataManager.Instance.GetData<Equip_RankData>(equip.equipment_Rarity).Equip_Rank)
                    / DataManager.Instance.GetData<Equip_UpgradeData>(50).Equip_Success_Prob * 0.2f
                );

            equipGold = firstGold + secondGold;
        }
        else if(equip.equip_Upgrade == 0)
        {
            //공식 : (기본 판매가격 * 장비 장착 레벨 * 장비 등급에 따른 가중치) + (강화 평균 소모 골드 * 0.2)
            //강화 평균 소모 골드 : 해당 단계 기준 1회 강화 비용(장비 기본 판매가격 * (현재 강화단계 + 1)값의 (골드데이터 상의 배율)제곱 * 등급에 따른 가중치) / 성공 확률
            

            equipGold = Mathf.RoundToInt
                (
                    equip.equip_price * equip.equip_level *
                    GetRarityWeight((Rarity)DataManager.Instance.GetData<Equip_RankData>(equip.equipment_Rarity).Equip_Rank)
                );
        }
        else
        {
            //공식 : (기본 판매가격 * 장비 장착 레벨 * 장비 등급에 따른 가중치) + (강화 평균 소모 골드 * 0.2)
            //강화 평균 소모 골드 : 해당 단계 기준 1회 강화 비용(장비 기본 판매가격 * (현재 강화단계 + 1)값의 (골드데이터 상의 배율)제곱 * 등급에 따른 가중치) / 성공 확률
            int firstGold = Mathf.RoundToInt
                (
                    equip.equip_price * equip.equip_level *
                    GetRarityWeight((Rarity)DataManager.Instance.GetData<Equip_RankData>(equip.equipment_Rarity).Equip_Rank)
                );
            int secondGold = Mathf.RoundToInt
                (
                    Mathf.RoundToInt(equip.equip_price * Mathf.Pow(equip.equip_Upgrade, DataManager.Instance.GetData<Equip_Upgrade_GoldData>(equip.equip_Upgrade).Equip_Upgrade_Value)) *
                    equip.GetEnchantWeightByRarity((Rarity)DataManager.Instance.GetData<Equip_RankData>(equip.equipment_Rarity).Equip_Rank)
                    / DataManager.Instance.GetData<Equip_UpgradeData>(equip.equip_Upgrade).Equip_Success_Prob * 0.2f
                );

            equipGold = firstGold + secondGold;
        }

        //계산된 만큼 골드를 획득합니다.
        return equipGold;
    }

    /// <summary>
    /// 등급에 따른 판매 시의 가중치를 구합니다.
    /// </summary>
    /// <param name="rarity">판매하려는 장비의 등급</param>
    /// <returns></returns>
    private static float GetRarityWeight(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Normal:
                return 1;
            case Rarity.Uncommon:
                return 1.5f;
            case Rarity.Rare:
                return 2.5f;
            case Rarity.Legendary:
                return 5;
            case Rarity.Mythtic:
                return 10;
            default:
                return 1;
        }
    }

    public static int RarityCalculator()
    {
        int value = Random.Range(1, 10001);
        int rarity = 40001;
        if(value <= 9400)
        {
            rarity = 40001;
        }
        else if(value > 9400 && value <= 9900)
        {
            rarity = 40002;
        }
        else if(value > 9900 && value <= 9990)
        {
            rarity = 40003;
        }
        else if(value > 9990 && value <= 9999)
        {
            rarity = 40004;
        }
        else if(value == 10000)
        {
            rarity = 40005;
        }
        return rarity;
    }
}
