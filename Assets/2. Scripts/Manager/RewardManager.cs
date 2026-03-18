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

        //장비
        GetEquipment(monsterId, isBoss);
    }

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

    private void GiveExp(bool isBoss)
    {
        int sectionId = _stageManager.CurrentSection;

        var rewardData = DataManager.Instance.GetData<Stage_RewardData>(sectionId);

        if (rewardData == null)
        {
            Debug.LogWarning($"Stage_RewardData 없음: {sectionId}");
            return;
        }

        //테이블의 총 경험치
        BigInteger totalExp = rewardData.Section_Exp;

        //기본 분배
        BigInteger mobExp = (totalExp * 7) / (10 * 120); // 몬스터 1마리당
        BigInteger bossBaseExp = (totalExp * 3) / 10;    // 보스 기본

        //몬스터 총합
        BigInteger totalMobExp = mobExp * 120;

        //현재 합계
        BigInteger currentTotal = totalMobExp + bossBaseExp;

        //오차 계산
        BigInteger remainder = totalExp - currentTotal;

        //보스에 오차 추가
        BigInteger bossExp = bossBaseExp + remainder;

        //지급
        BigInteger exp = isBoss ? bossExp : mobExp;

        //Debug.Log($"[RewardManager] Exp 지급 - Section:{sectionId}, IsBoss:{isBoss}, Exp:{exp}");

        _playerStats.AddExp(exp);
    }
}
