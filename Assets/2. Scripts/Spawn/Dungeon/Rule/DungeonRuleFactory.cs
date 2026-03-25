using UnityEngine;

/// <summary>
/// 던전 ID별 룰 생성
/// 150001 우상의 제단
/// 150002 종말의 장
/// 150003 여정의 날개
/// 150004 침묵의 성역
/// </summary>
public static class DungeonRuleFactory
{
    public static IDungeonRule CreateRule(int dungeonId)
    {
        switch (dungeonId)
        {
            case 150001: // 우상의 제단
                return new StatueDungeonRule();

            case 150002: // 종말의 장
                return new StandardWaveDungeonRule(useOrderedSpawn: true);

            case 150003: // 여정의 날개
                return new StandardWaveDungeonRule(useOrderedSpawn: true);

            case 150004: // 침묵의 성역
                return new StandardWaveDungeonRule(useOrderedSpawn: true);

            case 150006: // 아폴론 (레이드)
                return new RaidDungeonRule();

            default:
                // 미구현 던전은 기본 웨이브 룰 사용
                return new StandardWaveDungeonRule(useOrderedSpawn: true);
        }
    }
}
