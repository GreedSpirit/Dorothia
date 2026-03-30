using UnityEngine;
using UnityEditor;
using System.Numerics;

public class CheatEditorWindow : EditorWindow
{
    private long goldAmount = 10000;
    private long scrapAmount = 10000;
    private long skillScrollAmount = 3000;
    private long expAmount = 3000;

    private float damageMultiplier = 2.0f;

    private int jumpSection = 4;
    private int levelAmount = 1;

    private int shardCount = 1;

    [MenuItem("Tools/Cheat Editor")]
    public static void ShowWindow()
    {
        GetWindow<CheatEditorWindow>("치트 에디터");
    }

    private void OnGUI()
    {
        GUILayout.Label("게임 내 실시간 치트 설정", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // --- 골드 치트 섹션 ---
        GUILayout.Label("재화 치트", EditorStyles.label);
        goldAmount = EditorGUILayout.LongField("추가할 골드 양", goldAmount);
        if (GUILayout.Button($"{goldAmount} 골드 추가"))
        {
            AddGoldCheat();
        }

        // --- 스크랩 치트 섹션 ---
        scrapAmount = EditorGUILayout.LongField("추가할 스크랩 양", scrapAmount);
        if (GUILayout.Button($"{scrapAmount} 스크랩 추가"))
        {
            AddScrapCheat();
        }

        // --- 그렘린 조각 치트 섹션 ---
        shardCount = EditorGUILayout.IntField("추가할 그렘린 조각 양", shardCount);
        if(GUILayout.Button($"{shardCount} 플린트 조각 추가"))
        {
            AddGremlinShardCheat(210001);
        }
        if(GUILayout.Button($"{shardCount} 피니언 조각 추가"))
        {
            AddGremlinShardCheat(210002);
        }
        if(GUILayout.Button($"{shardCount} 코어 조각 추가"))
        {
            AddGremlinShardCheat(210003);
        }
        if(GUILayout.Button($"{shardCount} 징크 조각 추가"))
        {
            AddGremlinShardCheat(210004);
        }

        EditorGUILayout.Space();
        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1)); // 구분선
        EditorGUILayout.Space();

        // --- 스킬 치트 섹션 ---
        GUILayout.Label("스킬 치트", EditorStyles.label);
        skillScrollAmount = EditorGUILayout.LongField("추가할 랜덤 스킬 주문서 양", skillScrollAmount);
        if (GUILayout.Button($"{skillScrollAmount} 주문서 추가"))
        {
            AddSkillScrollCheat();
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("모든 스킬 쿨타임 초기화"))
        {
            ResetCooldownCheat();
        }

        EditorGUILayout.Space();
        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1)); // 구분선
        EditorGUILayout.Space();

        // --- 캐릭터 상태 섹션 ---
        GUILayout.Label("캐릭터 조작", EditorStyles.label);

        //if (GUILayout.Button("캐릭터 무적 (On/Off)"))
        //{
        //    ToggleGodModeCheat();
        //}

        expAmount = EditorGUILayout.LongField("추가할 경험치 양", expAmount);
        if (GUILayout.Button($"{expAmount} 경험치 추가"))
        {
            AddExpCheat();
        }

        EditorGUILayout.Space();

        levelAmount = EditorGUILayout.IntField("레벨 업 할 단위", levelAmount);
        if (GUILayout.Button($"{levelAmount} 레벨 추가"))
        {
            PlayerStats.Instance.CheatLevelUp(levelAmount);
        }

        EditorGUILayout.Space();
        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1)); // 구분선
        EditorGUILayout.Space();

        // --- 스테이지 섹션 ---
        GUILayout.Label("스테이지 조작", EditorStyles.label);
        jumpSection = EditorGUILayout.IntField("이동할 섹션", jumpSection);
        if (GUILayout.Button($"{jumpSection}섹션으로 이동"))
        {
            JumpSection();
        }
    }

    private void AddGoldCheat()
    {
        if (!Application.isPlaying) { Debug.LogWarning("게임 실행 중에만 가능합니다."); return; }

        ExchangeManager.Instance.GetMoney(MoneyType.Gold, goldAmount);
        Debug.Log($"[Cheat] {goldAmount} 골드 추가됨");
    }

    private void AddScrapCheat()
    {
        if (!Application.isPlaying) { Debug.LogWarning("게임 실행 중에만 가능합니다."); return; }

        ExchangeManager.Instance.GetMoney(MoneyType.Scrap, scrapAmount);
        Debug.Log($"[Cheat] {goldAmount} 스크랩 추가됨");
    }

    private void AddSkillScrollCheat()
    {
        if (!Application.isPlaying) { Debug.LogWarning("게임 실행 중에만 가능합니다."); return; }

        SkillManager.Instance.GetRandomScroll(3000);
        Debug.Log($"[Cheat] {skillScrollAmount} 랜덤 주문서 추가됨");
    }
    private void AddExpCheat()
    {
        if (!Application.isPlaying) { Debug.LogWarning("게임 실행 중에만 가능합니다."); return; }

        PlayerStats stat = FindAnyObjectByType<PlayerStats>();

        stat.AddExp(expAmount);

        Debug.Log($"[Cheat] {expAmount} 경험치 추가됨");
    }

    //private void ToggleGodModeCheat()
    //{
    //    if (!Application.isPlaying) return;

    //    PlayerCtrl player = FindAnyObjectByType<PlayerCtrl>();

    //    player.IsInvincible = !player.IsInvincible;
    //    Debug.Log($"[Cheat] 캐릭터 무적{player.IsInvincible} 상태 토글됨" +
    //    $"\n<color=$ff0000>주의 : 스킬 시전 후 무적 상태 풀림</color>");
    //}
    private void JumpSection()
    {
        if (!Application.isPlaying) { Debug.LogWarning("게임 실행 중에만 가능합니다."); return; }

        if (StageManager.Instance == null) return;

        StageManager.Instance.JumpSection(jumpSection);
        Debug.Log($"[Cheat] {jumpSection}섹션으로 이동 됨");
    }

    private void ResetCooldownCheat()
    {
        if (!Application.isPlaying) return;

        SkillManager.Instance.ResetAllCooldown();
        Debug.Log("[Cheat] 모든 스킬 쿨타임 초기화됨");
    }

    private void AddGremlinShardCheat(int id)
    {
        if (!Application.isPlaying) return;
        ExchangeManager.Instance.SetCurrentShardID(id);
        ExchangeManager.Instance.AddGremlinPiece(id, shardCount);
        Debug.Log($"{shardCount} 조각 추가 완료");
    }
}