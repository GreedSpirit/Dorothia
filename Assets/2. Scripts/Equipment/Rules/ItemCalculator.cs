using System.Collections.Generic;
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
                    ItemCalculator.GetEnchantWeightByRarity((Rarity)DataManager.Instance.GetData<Equip_RankData>(equip.equipment_Rarity).Equip_Rank)
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
                    ItemCalculator.GetEnchantWeightByRarity((Rarity)DataManager.Instance.GetData<Equip_RankData>(equip.equipment_Rarity).Equip_Rank)
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

    /// <summary>
    /// 장비 등급에 맞는 배율을 반환합니다.
    /// </summary>
    /// <param name="rarity">장비 등급</param>
    /// <returns></returns>
    public static float RarityMultiplyerCalculation(int rarity)
    {
        float RarityMultiply = 1f;
        switch(rarity)
        {
            case 40002:
                RarityMultiply = 1.1f;
                break;
            case 40003:
                RarityMultiply = 1.2f;
                break;
            case 40004:
                RarityMultiply = 1.4f;
                break;
            case 40005:
                RarityMultiply = 1.7f;
                break;
            default:
                RarityMultiply = 1;
                break;
        }
        return RarityMultiply;
    }

    /// <summary>
    /// 장비가 가진 스탯의 정보를 기반으로 점수를 책정합니다.
    /// </summary>
    /// <param name="equip">점수를 책정할 장비</param>
    /// <returns></returns>
    public static int GetEquipScore(Equipment equip)
    {
        int score = 0;
        score += (int)(GetStatus(equip, Status.ATK) * 10);
        score += (int)(GetStatus(equip, Status.DEF) * 2);
        score += (int)(GetStatus(equip, Status.MagicDEF) * 2);
        score += (int)(GetStatus(equip, Status.HP) * 3);
        score += (int)(GetStatus(equip, Status.HPRegen) * 1);
        score += (int)(GetStatus(equip, Status.AttackSpeed) * 8);
        score += (int)(GetStatus(equip, Status.MoveSpeed) * 7);
        score += (int)(GetStatus(equip, Status.CriticalChance) * 9);
        score += (int)(GetStatus(equip, Status.CriticalDamage) * 9);

        return score;
    }
    public static float GetStatus(Equipment equip, Status equipStatus)
    {
        float multiply = 1;
        if (equip.equip_Upgrade != 0)
        {
            multiply += DataManager.Instance.GetData<Equip_UpgradeData>(equip.equip_Upgrade).Equip_Value;
        }

        //레벨에 따른 상승량 값이 존재함에도 굳이 레벨*비율을 사용하는 이유는 혹시 모를 예외 상황에 대비하기 위함.
        return equip.equip_status.TryGetValue(equipStatus, out float value) ?
            value * multiply * ItemCalculator.RarityMultiplyerCalculation(equip.equipment_Rarity)+ equip.equip_level * 1.5f
            : 0f;
    }

    /// <summary>
    /// Rarity 열겨형 기반으로 배율을 받아옵니다.
    /// </summary>
    /// <param name="Rarity">해당 장비의 레어도를 나타내는 Rarity 열거형 값</param>
    /// <returns>해당 배율과 일치하는 강화 배율값</returns>
    public static float GetEnchantWeightByRarity(Rarity Rarity)
    {
        switch (Rarity)
        {
            case Rarity.Normal:
                return 1;

            case Rarity.Uncommon:
                return 1.5f;

            case Rarity.Rare:
                return 3;

            case Rarity.Legendary:
                return 6;

            case Rarity.Mythtic:
                return 10;

            default:
                return 1;
        }
    }

    /// <summary>
    /// 장비 데이터로부터, 해당 장비에서 유효한 스텟만 가져오는 메서드입니다.
    /// </summary>
    /// <param name="equip">스텟을 가져올 장비</param>
    /// <param name="equipStatus">장비의 스텟</param>
    /// <param name="equipStatusValue">해당 장비 스텟의 값</param>
    public static void AddEquipStatus(Equipment equip, Status equipStatus, float equipStatusValue)
    {
        //이미 해당 스테이터스가 Dictionary에 존재한다면, 해당 값을 추가합니다.
        if (equip.equip_status.ContainsKey(equipStatus))
        {
            equip.equip_status[equipStatus] += equipStatusValue;
        }
        //Dictionary에 존재하지 않을 경우, 값이 0이 아닌 경우에만 포함시킵니다.
        else if (equipStatusValue != 0)
        {
            equip.equip_status.Add(equipStatus, equipStatusValue);
        }
    }

    /// <summary>
    /// 규칙으로부터 확인한 스테이터스에 따라, 데이터로부터 해당 스테이터스의 정보를 받아옵니다.
    /// </summary>
    /// <param name="equip">정보를 확인할 장비</param>
    /// <param name="status">정보를 확인해야 하는 스테이터스</param>
    /// <param name="data">그 스테이터스를 확인하기 위한 테이블상의 장비데이터</param>
    public static void AddStatusFromData(Equipment equip, Status status, EquipData data)
    {
        switch (status)
        {
            case Status.HP:
                AddEquipStatus(equip, status, data.Equip_Hp);
                break;

            case Status.ATK:
                AddEquipStatus(equip, status, data.Equip_Atk);
                break;

            case Status.MagicATK:
                AddEquipStatus(equip, status, data.Equip_Atk_M);
                break;

            case Status.AttackSpeed:
                AddEquipStatus(equip, status, data.Equip_Dps);
                break;

            case Status.CriticalChance:
                AddEquipStatus(equip, status, data.Equip_Crt_Prob);
                break;

            case Status.CriticalDamage:
                AddEquipStatus(equip, status, data.Equip_Crt_Dmg);
                break;

            case Status.DEF:
                AddEquipStatus(equip, status, data.Equip_Def);
                break;

            case Status.MagicDEF:
                AddEquipStatus(equip, status, data.Equip_Def_M);
                break;

            case Status.HPRegen:
                AddEquipStatus(equip, status, data.Equip_Hp_Regen);
                break;

            case Status.MoveSpeed:
                AddEquipStatus(equip, status, data.Equip_Agi);
                break;
        }
    }

    /// <summary>
    /// 장착 부위별로 지정된 주요 스테이터스와 보조 스테이터스의 규칙대로 스테이터스를 형성합니다.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="rarity"></param>
    public static void AddEquipStatusByType(Equipment equip, EquipData data, Rarity rarity)
    {
        //규칙에 정의되지 않은 Equip_Type가 들어온 경우 반환합니다.
        if (!EquipStatusStaticRule._rules.ContainsKey(data.Equip_Type))
        {
            return;
        }
        //static으로 선언한 규칙에서 만드려는 장비의 Dictionary를 받아옵니다.
        var rule = EquipStatusStaticRule._rules[data.Equip_Type];

        //해당 장비의 타입을 기반으로, 데이터를 확인하여 메인 스테이터스를 생성합니다.
        foreach (var main in rule.MainStatus)
        {
            AddStatusFromData(equip, main, data);
        }

        if (!EquipStatusStaticRule.SubStatusCount.ContainsKey(rarity))
        {
            return;
        }
        //장비의 등급에 따라, 스테이터스를 얼마나 만들지 확인합니다.
        int subCount = EquipStatusStaticRule.SubStatusCount[rarity];

        //장비의 장착 부위와 등급에 따라, 보조 스테이터스 리스트를 생성합니다.
        var selectedSubs = new List<Status>();

        //만에 하나 SubStatus에 아무 정보도 담겨있지 않다면 반환합니다.
        if (rule.SubStatus.Count == 0)
        {
            return;
        }

        //규칙으로 정한 보조 스테이터스의 수량까지 도달하거나(배열 칸 이탈 방지) 추가해야 하는 보조 스테이터스 수량이 될 때까지 아래 코드를 실행합니다.
        for (int i = 0; i < rule.SubStatus.Count && selectedSubs.Count < subCount; i++)
        {
            if (rule.SubStatus != null)
                //보조 스테이터스 규칙에 따라, 앞에 있는 것부터 순차적으로 추가합니다.
                selectedSubs.Add(rule.SubStatus[i]);
        }

        //그럼에도 여전히 스텟을 추가해야 하는 경우라면, 위 코드를 반복합니다.
        while (selectedSubs.Count < subCount)
        {
            //왼쪽 조건문이 초기화되었어도, 오른쪽 조건문은 그대로일 테니 필요하면 멈출 것입니다.
            for (int i = 0; i < rule.SubStatus.Count && selectedSubs.Count < subCount; i++)
            {
                //보조 스테이터스 규칙에 따라, 앞에 있는 것부터 순차적으로 추가합니다.
                selectedSubs.Add(rule.SubStatus[i]);
            }
        }

        //채워넣은 보조 스테이터스 리스트의 각 보조 스테이터스마다 데이터를 확인하여 스테이터스를 생성합니다.
        foreach (var sub in selectedSubs)
        {
            AddStatusFromData(equip, sub, data);
        }
    }

    /// <summary>
    /// 이름에 해당하는 세트효과가 있는지 확인하고 그 세트효과의 ID값을 가져옵니다.
    /// </summary>
    /// <param name="equipName">현 장비의 이름.</param>
    /// <returns></returns>
    public static int GetSetEffect(string equipName)
    {
        //세트효과를 찾습니다.
        Dictionary<int, List<Equip_SetData>> allSets = DataManager.Instance.GetListDict<Equip_SetData>();
        int set_id = 0;
        foreach (var Set in allSets.Values)
        {
            foreach (var item in Set)
            {
                if (equipName.Contains(item.Equip_Set_Need_Name))
                    set_id = item.Equip_Set_Id;
            }
        }
        return set_id;
    }
}
