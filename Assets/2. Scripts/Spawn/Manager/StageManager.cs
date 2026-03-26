using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum StageState
{
    None,
    Enter,
    Spawning,
    BossFight,
    Clear,
    Failed
}

/// <summary>
/// 스테이지 진행 FSM
/// StageData(스테이지 테이블) -> Monster_SpawnData(스폰풀 테이블)로 연결
/// 일반 몬스터 처치수( Boss_Summon_Dead_Namber ) 도달 시 보스전 진입
/// 보스전 진입 시: 일반 스폰 중단 + 기존 몬스터 전부 제거 + 보스 1마리 소환
/// 보스가 죽으면 Clear
/// 보스 ID는 SpawnData의 특정 슬롯에 고정하지 않고
/// Monster_Data.Monster_Type == Boss로 해서 결정
/// </summary>
public class StageManager : MonoBehaviour
{
    public static StageManager Instance { get; private set; }

    //이벤트
    public static event System.Action<int> OnStageIdChanged;
    public static event System.Action<StageState> OnStageStateChanged;
    public static event System.Action<int, int> OnKillCountChanged;
    public static event System.Action<int> OnBossSpawned;
    public static event System.Action<int> OnStageCleared;
    public static event System.Action<int> OnSectionChanged;
    public static event System.Action<int> OnSectionCleard;
    public static event System.Action<int, int> OnScenarioTrigger;

    private bool _eventsRegistered;

    [SerializeField] private MonsterSpawnManager _spawnManager;
    [SerializeField] private int _startStageId = 110001;

    [SerializeField] private float _defaultBossTimeLimit = 60f;

    [SerializeField] private MonoBehaviour _playerBehaviour;
    private IMonsterTarget _player;

    private StageData _stage;               // CSV
    private Monster_SpawnData _spawnData;   // CSV
    private Stage_SectionData _sectionData; // CSV

    private int _currentSection;
    private int _maxClearedSection;

    private StageState _state;
    private int _killCount;
    private int _bossKillTarget;
    private bool _bossAlive;
    private int _bossMonsterId;

    private float _bossTimeLimit;
    private float _bossStartTime;
    private bool _bossTimerRunning;

    //RewardManager용
    public int CurrentSection => _currentSection;                   // 현재 섹션
    public int CurrentStageSectionId => 
        _sectionData != null ? _sectionData.Stage_Section_Id : 0;   // 현재 스테이지섹션 ID
    public Stage_SectionData CurrentSectionData => _sectionData;    // 현재 구간 데이터

    //UI용
    public int CurrentStageId => _stage != null ? _stage.Stage_Id : 0;
    public int CurrentProgressSection => _currentSection;
    public int MaxClearedSection => _maxClearedSection;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _player = _playerBehaviour as IMonsterTarget;

        if (_player == null)
            Debug.Log("플레이어 넣어야함");
    }

    private void OnEnable()
    {
        if (_eventsRegistered)
            return;

        MonsterController.OnMonsterKilled -= HandleMonsterKilled;
        MonsterController.OnMonsterKilled += HandleMonsterKilled;

        if (_player != null)
        {
            _player.OnDead -= HandlePlayerDead;
            _player.OnDead += HandlePlayerDead;
        }

        _eventsRegistered = true;
    }

    private void OnDisable()
    {
        if (!_eventsRegistered)
            return;

        MonsterController.OnMonsterKilled -= HandleMonsterKilled;

        if (_player != null)
            _player.OnDead -= HandlePlayerDead;

        _eventsRegistered = false;
    }

    private void Start()
    {
        StartStage(_startStageId);
    }

    private void Update()
    {
        if (!_bossTimerRunning)
            return;

        if (Time.time - _bossStartTime >= _bossTimeLimit)
        {
            Debug.Log("보스 시간 초과");
            HandleBossFail();
        }
    }

    #region 스테이지 진입점
    /// <summary>
    /// 스테이지 시작/재시작 진입점
    /// StageData 로드 -> Monster_SpawnData 로드
    /// SpawnManager에 현재 스테이지 스폰 설정 주입
    /// </summary>
    /// <param name="stageId"></param>
    public void StartStage(int stageId)
    {
        InternalStartStage(stageId, -1);
    }

    public void StartStageFromSection(int stageId, int sectionNumber)
    {
        InternalStartStage(stageId, sectionNumber);
    }

    //던전 복귀용
    public void ResumeStageFromSavedContext()
    {
        if (!DungeonReturnContext.HasContext)
        {
            Debug.Log("[StageManager] 저장된 던전 복귀 컨텍스트 없음");
            return;
        }

        if (_player is IResettable resettable)
        {
            resettable.ResetState();
        }

        StartStageFromSection(
            DungeonReturnContext.ReturnStageId,
            DungeonReturnContext.ReturnSection
        );

        DungeonReturnContext.Clear();
    }

    private void InternalStartStage(int stageId, int startSection)
    {
        //기존 몬스터 완전 초기화
        if (_spawnManager != null)
        {
            _spawnManager.ResetSpawnState();
            _spawnManager.StopNormalSpawn();   // 기존 스폰 루틴 중지
            _spawnManager.ForceClearAll();     // 필드 몬스터 전부 제거
        }

        if (DataManager.Instance == null)
        {
            Debug.Log("[StageManager] DataManager NULL");
            return;
        }

        _stage = DataManager.Instance.GetData<StageData>(stageId);
        if (_stage == null)
        {
            Debug.Log("StageData 없음");
            return;
        }

        _spawnData = DataManager.Instance.GetData<Monster_SpawnData>(_stage.Monster_Spawn_Id);
        if (_spawnData == null)
        {
            Debug.Log("SpawnData 없음");
            return;
        }

        var dict = DataManager.Instance.GetDict<Stage_SectionData>();
        if (dict == null)
        {
            Debug.Log("SectionData Dict NULL");
            return;
        }

        var sections = dict.Values
            .Where(x => x.Stage_Id == stageId)
            .OrderBy(x => x.Section_Start)
            .ToList();

        if (sections.Count == 0)
        {
            Debug.Log("해당 Stage에 Section 없음");
            return;
        }

        if (startSection <= 0)
        {
            _sectionData = sections.First();
            _currentSection = _sectionData.Section_Start; // 현재 섹션은 현재 구간의 start로 확정
        }
        else
        {
            //선택한 섹션이 포함된 구간 찾기
            var matched = sections.FirstOrDefault(x =>
                startSection >= x.Section_Start &&
                startSection <= x.Section_End);

            if (matched == null)
            {
                Debug.Log("선택 섹션에 맞는 구간 없음");
                return;
            }

            _sectionData = matched;
            _currentSection = startSection;
        }

        if (_maxClearedSection < _currentSection)
            _maxClearedSection = _currentSection;

        OnSectionChanged?.Invoke(_currentSection);

        _killCount = 0;
        _bossKillTarget = _stage.Boss_Summon_Dead_Namber;
        _bossAlive = false;

        _bossMonsterId = ResolveBossId(_spawnData);

        OnStageIdChanged?.Invoke(_stage.Stage_Id);

        //스폰매니저 초기화 (보스 ID도 같이 넘김)
        _spawnManager.InitializeStageSpawn(
            _spawnData, _stage.Same_Spawn_Max, _bossMonsterId
        );

        ChangeState(StageState.Enter);
    }
    #endregion

    #region 섹션 관리 로직
    /// <summary>
    /// 현재 섹션 번호가 현재 구간 범위를 넘어가면 다음 Stage_Section_Id로 이동
    /// </summary>
    /// <returns></returns>
    private bool TryAdvanceSectionDataIfNeeded()
    {
        if (_sectionData == null)
        {
            Debug.LogError("[StageManager] _sectionData NULL");
            return false;
        }

        //아직 현재 구간 범위 안이면
        if (_currentSection >= _sectionData.Section_Start &&
            _currentSection <= _sectionData.Section_End)
        {
            return true;
        }

        //범위를 벗어난 경우 -> 다음 구간 로드 시도
        //50->51 넘어갈 때 120001 -> 120002 같은 식으로 연결
        int nextSectionId = _sectionData.Stage_Section_Id + 1;

        var next = DataManager.Instance.GetData<Stage_SectionData>(nextSectionId);
        if (next == null)
        {
            Debug.LogError($"[StageManager] 다음 SectionData 없음 nextId={nextSectionId}");
            return false;
        }

        _sectionData = next;

        //새 구간 로드 후에도 현재 섹션이 범위에 들어오는지 검증
        bool valid = (_currentSection >= _sectionData.Section_Start &&
                      _currentSection <= _sectionData.Section_End);

        if (!valid)
        {
            Debug.LogError($"[StageManager] 섹션/구간 불일치 section={_currentSection}, range={_sectionData.Section_Start}~{_sectionData.Section_End}, id={_sectionData.Stage_Section_Id}");
        }

        return valid;
    }

    /// <summary>
    /// 감소 시 이전 구간으로 이동
    /// </summary>
    /// <returns></returns>
    private bool TryRetreatSectionDataIfNeeded()
    {
        if (_sectionData == null)
        {
            Debug.LogError("[StageManager] _sectionData NULL (Retreat)");
            return false;
        }

        // 아직 현재 구간 범위 안이면 문제 없음
        if (_currentSection >= _sectionData.Section_Start &&
            _currentSection <= _sectionData.Section_End)
        {
            return true;
        }

        // 이전 구간으로 이동 시도
        int prevSectionId = _sectionData.Stage_Section_Id - 1;

        var prev = DataManager.Instance.GetData<Stage_SectionData>(prevSectionId);
        if (prev == null)
        {
            Debug.LogError($"[StageManager] 이전 SectionData 없음 prevId={prevSectionId}");
            return false;
        }

        _sectionData = prev;

        bool valid = (_currentSection >= _sectionData.Section_Start &&
                      _currentSection <= _sectionData.Section_End);

        if (!valid)
        {
            Debug.LogError($"[StageManager] 감소 후 섹션/구간 불일치 section={_currentSection}, range={_sectionData.Section_Start}~{_sectionData.Section_End}, id={_sectionData.Stage_Section_Id}");
        }

        return valid;
    }
    #endregion

    /// <summary>
    /// 스테이지 FSM 전환
    /// </summary>
    /// <param name="newState"></param>
    private void ChangeState(StageState newState)
    {
        _state = newState;
        OnStageStateChanged?.Invoke(_state);
        Debug.Log($"현재상태{_state}");
        switch (_state)
        {
            case StageState.Enter:
                //진입 연출이 생기면 여기서
                OnScenarioTrigger?.Invoke(CurrentSection, 1);
                ChangeState(StageState.Spawning);
                break;

            case StageState.Spawning:
                //일반 및 앨리트 몬스터 스폰 시작
                _spawnManager.StartNormalSpawn();
                
                break;

            case StageState.BossFight:
                //보스전 진입 처리
                SpawnBoss();
                break;

            case StageState.Clear:
                //스테이지 클리어 처리
                _spawnManager.StopNormalSpawn();
                break;

            case StageState.Failed:
                //실패시 재시작
                _spawnManager.StopNormalSpawn();
                StartStage(_stage.Stage_Id);
                break;
        }
    }

    /// <summary>
    /// MonsterController 사망 이벤트 처리
    /// 몬스터 킬카운트 누적
    /// 목표치 도달시 BossFight로 전환
    /// 보스가 죽으면 Clear
    /// </summary>
    /// <param name="monsterId"></param>
    /// <param name="isBoss"></param>
    private void HandleMonsterKilled(int monsterId, bool isBoss)
    {
        //던전 진행 중에는 스테이지 킬카운트/보스 진행을 절대 처리하지 않음
        if (DungeonManager.Instance != null && DungeonManager.Instance.IsDungeonRunning)
            return;

        if (_state == StageState.Clear)
            return;

        if (isBoss)
        {
            OnScenarioTrigger?.Invoke(CurrentSection, 3);
            OnSectionCleard?.Invoke(_currentSection);

            _bossAlive = false;
            _bossTimerRunning = false;

            _spawnManager.EndBossFight(); // 스폰 다시 가능하게

            _currentSection++;

            //증가 직후 현재 구간 범위 체크 -> 필요시 다음 Stage_Section_Id로 이동
            if (!TryAdvanceSectionDataIfNeeded())
            {
                Debug.Log("[StageManager] 섹션 구간 갱신 실패");
                return;
            }

            if (_currentSection > _maxClearedSection)
                _maxClearedSection = _currentSection;

            OnSectionChanged?.Invoke(_currentSection);

            Debug.Log($"섹션 확인 {_currentSection} (SectionId:{CurrentStageSectionId})");

            //현재 Stage의 Section 끝에 도달했는가?
            if (_currentSection > _sectionData.Section_End)
            {
                int nextStageId = _stage.Stage_Id + 1;

                StageData nextStage = DataManager.Instance.GetData<StageData>(nextStageId);

                if (nextStage != null)
                {
                    Debug.Log($"Stage 구간 바뀜 -> {nextStageId}");
                    StartStage(nextStageId);
                }
                else
                {
                    //다음 스테이지가 없으면 전체 클리어
                    ChangeState(StageState.Clear);
                }

                return;
            }

            //다음 섹션 시작
            _killCount = 0;
            _spawnManager.EndBossFight();
            ChangeState(StageState.Enter);
            return;
        }

        //보스전 들어가면 일반 킬카운트는 더 이상 누적할 필요 없음
        if (_bossAlive)
            return;

        _killCount++;
        OnKillCountChanged?.Invoke(_killCount, _bossKillTarget);

        //목표치 도달 -> 보스전 진입
        if (!_bossAlive && _bossKillTarget > 0 && _killCount >= _bossKillTarget)
        {
            ChangeState(StageState.BossFight);
        }
    }

    /// <summary>
    /// 보스전 진입시 처리
    /// 일반 스폰 중단
    /// 현재 필드의 몬스터 전부 제거
    /// 보스 1마리 소환
    /// </summary>
    private void SpawnBoss()
    {
        if (_bossMonsterId <= 0)
        {
            Debug.Log("Boss ID 무효");
            return;
        }

        _bossAlive = true;

        //일반 스폰 완전 정지
        //_spawnManager.StopNormalSpawn();

        //기존 몬스터 제거
        //_spawnManager.ForceClearAll();

        //보스 1마리 소환
        _spawnManager.SpawnBoss(_bossMonsterId);

        //제한시간
        _bossStartTime = Time.time;
        _bossTimeLimit = _defaultBossTimeLimit;
        _bossTimerRunning = true;

        OnBossSpawned?.Invoke(_bossMonsterId);
        OnScenarioTrigger?.Invoke(CurrentSection, 2);
    }

    private void HandleBossFail()
    {
        _bossTimerRunning = false;
        _bossAlive = false;

        _spawnManager.EndBossFight();

        ResetSectionAndRespawn();
    }

    /// <summary>
    /// SpawnData 어디 슬롯에 보스가 들어있든 Monster_Data.Monster_Type == Boss 로 찾기
    /// (CSV가 Monster_Id_5에 넣든, Monster_Id_8에 넣든 안전)
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    private int ResolveBossId(Monster_SpawnData data)
    {
        int[] ids =
        {
            data.Monster_Id_1, data.Monster_Id_2, data.Monster_Id_3, data.Monster_Id_4,
            data.Monster_Id_5, data.Monster_Id_6, data.Monster_Id_7, data.Monster_Id_8
        };

        for (int i = 0; i < ids.Length; i++)
        {
            int id = ids[i];
            if (id <= 0) continue;

            var md = DataManager.Instance.GetData<Monster_Data>(id);
            if (md != null && md.Monster_Type == Monster_Type.Boss)
                return id;
        }

        //fallback (데이터가 없거나 Monster_Type 세팅이 비정상일 때)
        if (data.Monster_Id_8 > 0) return data.Monster_Id_8;

        return 0;
    }

    private void ResetSectionAndRespawn()
    {
        Debug.Log($"[Stage] 실패 -> 이전 섹션 이동 전: {_currentSection}");

        _spawnManager.StopNormalSpawn();
        _spawnManager.ForceClearAll();

        if (_currentSection > _sectionData.Section_Start)
            _currentSection--;
        else
            _currentSection = _sectionData.Section_Start;

        if (!TryRetreatSectionDataIfNeeded())
        {
            Debug.Log("[StageManager] 이전 구간 이동 실패");
            return;
        }

        OnSectionChanged?.Invoke(_currentSection);

        Debug.Log($"[Stage] 실패 -> 현재 섹션: {_currentSection}");

        _killCount = 0;

        if (_player is IResettable resettable)
            resettable.ResetState();

        Debug.Log("[Stage] Spawning 재시작");

        ChangeState(StageState.Spawning);
    }

    private void HandlePlayerDead()
    {
        //던전 진행 중 플레이어 사망은 DungeonManager가 처리하므로 StageManager는 무시
        if (DungeonManager.Instance != null && DungeonManager.Instance.IsDungeonRunning)
        {
            Debug.LogWarning("던전 진행중이라 StageManager 처리 스킵");
            return;
        }

        if (_bossAlive)
        {
            Debug.Log("플레이어 사망 -> 보스전 실패");
            HandleBossFail();
        }
        else
        {
            Debug.Log("플레이어 사망 -> 섹션 리셋");

            ResetSectionAndRespawn();
        }
    }

    #region 테스트용
    public void JumpSection(int amount)
    {
        int targetSection = _currentSection + amount;

        Debug.Log($"[Cheat] Section Jump: {_currentSection} -> {targetSection}");

        //현재 Stage 기준으로 Section 재계산
        var dict = DataManager.Instance.GetDict<Stage_SectionData>();

        if (dict == null)
        {
            Debug.Log("[StageManager] SectionData Dict 없음");
            return;
        }

        //전체 Section 중 targetSection이 포함된 구간 찾기
        var matched = dict.Values
            .Where(x => targetSection >= x.Section_Start && targetSection <= x.Section_End)
            .FirstOrDefault();

        if (matched == null)
        {
            Debug.Log($"[Cheat] 해당 Section 없음: {targetSection}");
            return;
        }

        //Stage 변경 필요 여부 체크
        if (_stage.Stage_Id != matched.Stage_Id)
        {
            Debug.Log($"[Cheat] Stage 변경 필요: {_stage.Stage_Id} -> {matched.Stage_Id}");

            //Stage 자체를 다시 시작 (맵 포함 전체 리셋)
            StartStageFromSection(matched.Stage_Id, targetSection);
            return;
        }

        //같은 Stage면 내부 값만 변경
        _currentSection = targetSection;
        _sectionData = matched;

        OnSectionChanged?.Invoke(_currentSection);

        Debug.Log($"[Cheat] Section 이동 완료: {_currentSection} (SectionId:{_sectionData.Stage_Section_Id})");
    }
    #endregion
}