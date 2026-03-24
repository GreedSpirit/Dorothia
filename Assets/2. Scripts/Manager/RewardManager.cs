using System.Numerics;
using UnityEngine;

public class RewardManager : MonoBehaviour
{
    [SerializeField] private EquipmentInventory _inventory;
    [SerializeField] private StageManager _stageManager;    // 현재 스테이지 섹션 정보 가져와야함
    [SerializeField] private PlayerStats _playerStats;

    private void OnEnable()
    {
        MonsterController.OnMonsterKilled += HandleMonsterKilled;
    }

    private void OnDisable()
    {
        MonsterController.OnMonsterKilled -= HandleMonsterKilled;
    }

    private void HandleMonsterKilled(int monsterId, bool isBoss)
    {
        //경험치
        GiveExp(isBoss);

        //골드
        GiveGold(isBoss);

        //장비
        GetEquipment(monsterId, isBoss);
    }

    #region 공통 계산 함수
    /// <summary>
    /// 총 보상을 몬스터 / 보스로 분배 (7:3 비율 + remainder 보스 보정)
    /// </summary>
    /// <param name="total"></param>
    /// <returns></returns>
    private (BigInteger mob, BigInteger boss) CalculateReward(BigInteger total)
    {
        // 몬스터 1마리당
        BigInteger mob = (total * 7) / (10 * 120);

        // 보스 기본
        BigInteger bossBase = (total * 3) / 10;

        // 몬스터 총합
        BigInteger totalMob = mob * 120;

        // 현재 합계
        BigInteger current = totalMob + bossBase;

        // 오차
        BigInteger remainder = total - current;

        // 보스 보정
        BigInteger boss = bossBase + remainder;

        return (mob, boss);
    }
    #endregion

    #region 장비
    private void GetEquipment(int monsterId, bool isBoss)
    {
        //현재 섹션 번호(1, 2, --- 50, 51)
        int currentSection = _stageManager.CurrentSection;

        //현재 Stage_Section_Id(120001, 120002)
        int currentStageSectionId = _stageManager.CurrentStageSectionId;

        //Debug.Log($"드랍 섹션 확인 Section:{currentSection}, SectionId:{currentStageSectionId}");

        var DropData = DataManager.Instance.GetData<Stage_SectionData>(currentStageSectionId);

        //드랍율이 테이블에 있으며, float값으로 적용중이고 소수점 단위를 쓰므로 해당 값 적용
        int dropChance = Random.Range(0, 100);

        //Equip_Drop_Prob : 0.0X 단위이므로 100을 곱해 int값과 비교
        if(dropChance < DropData.Equip_Drop_Prob * 100)
        {
            TestWeaponGenerator.Instance.Test(DropData.Equip_Drop_Level);
        }
    }
    #endregion

    #region 경험치
    private void GiveExp(bool isBoss)
    {
        int currentSection = _stageManager.CurrentSection;

        var rewardData = DataManager.Instance.GetData<Stage_RewardData>(currentSection);

        if (rewardData == null)
        {
            Debug.LogWarning($"Stage_RewardData 없음: {currentSection}");
            return;
        }

        BigInteger totalExp = rewardData.Section_Exp;

        var (mobExp, bossExp) = CalculateReward(totalExp);

        BigInteger exp = isBoss ? bossExp : mobExp;

        _playerStats.AddExp(exp);
    }
    #endregion

    #region 골드
    private void GiveGold(bool isBoss)
    {
        int currentSection = _stageManager.CurrentSection;

        var rewardData = DataManager.Instance.GetData<Stage_RewardData>(currentSection);

        if (rewardData == null)
        {
            Debug.LogWarning($"Stage_RewardData 없음: {currentSection}");
            return;
        }

        BigInteger totalGold = rewardData.Section_Gold;

        var (mobGold, bossGold) = CalculateReward(totalGold);

        BigInteger gold = isBoss ? bossGold : mobGold;

        if (ExchangeManager.Instance != null)
        {
            ExchangeManager.Instance.GetMoney(MoneyType.Gold, gold);
        }
        else
        {
            Debug.LogError("ExchangeManager 없음");
        }
    }
    #endregion
}
