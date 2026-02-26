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
        //int equipLevel = CurrentSectionLevel(); ??

        //_inventory.AddEquipment(equipment);
    }

    private int CurrentSectionLevel()
    {
        return _stageManager.CurrentSection; // 현재 스테이지 섹션 정보
    }
}
