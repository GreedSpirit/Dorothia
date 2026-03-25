using UnityEngine;

/// <summary>
/// 던전 입장 전 스테이지 진행 상태를 임시 저장
/// Stage -> Dungeon -> Stage 복귀용
/// </summary>
public static class DungeonReturnContext
{
    public static bool HasContext { get; private set; }

    public static int ReturnStageId { get; private set; }
    public static int ReturnSection { get; private set; }
    public static int ReturnEqsection { get; private set; }

    public static void Save(int stageId, int section, int eqSection)
    {
        ReturnStageId = stageId;
        ReturnSection = section;
        ReturnEqsection = eqSection;
        HasContext = true;

        Debug.Log($"[DungeonReturnContext] Save stageId={stageId}, section={section}");
    }

    public static void Clear()
    {
        HasContext = false;
        ReturnStageId = 0;
        ReturnSection = 0;
        ReturnEqsection = 0;
    }
}