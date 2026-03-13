using UnityEngine;

/// <summary>
/// 던전 진행 규칙 인터페이스
/// DungeonManager는 공통 흐름만 담당하고,
/// 실제 "웨이브 시작 / 킬 처리 / 클리어 조건"은 룰이 담당
/// </summary>
public interface IDungeonRule
{
    void Initialize(DungeonManager manager);
    void OnPrepareStarted();
    void OnCombatStarted();
    void OnMonsterKilled(int monsterId);
}
