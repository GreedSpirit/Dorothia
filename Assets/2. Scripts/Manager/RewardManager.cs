using UnityEngine;

public class RewardManager : MonoBehaviour
{
    [SerializeField] private EquipmentInventory _inventory;
    [SerializeField] private StageManager _stageManager;    // 현재 스테이지 섹션 정보 가져와야함

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

        //골드

        //장비
        //GetEquipment(monsterId, isBoss);
    }

    private void GetEquipment(int monsterId, bool isBoss)
    {
        //현재 섹션 번호(1, 2, --- 50, 51)
        int currentSection = _stageManager.CurrentSection;

        //현재 Stage_Section_Id(120001, 120002)
        int currentStageSectionId = _stageManager.CurrentStageSectionId;

        Debug.Log($"드랍 섹션 확인 Section:{currentSection}, SectionId:{currentStageSectionId}");

        ////드랍율이 테이블에 있으며, float값으로 적용중이고 소수점 단위를 쓰므로 해당 값 적용
        //int dropChance = Random.Range(0, 100);

        ////Equip_Drop_Prob : 0.0X 단위이므로 100을 곱해 int값과 비교
        //if(dropChance < id.Equip_Drop_Prob * 100)
        //{
        //    //TestWeaponGenerator.Instance.Test(id.Equip_Drop_Level);
        //}
    }
}
