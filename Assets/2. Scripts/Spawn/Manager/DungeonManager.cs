using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using GameUtility;

public enum DungeonState
{
    None,
    Enter,
    Prepare,
    Combat,
    Clear,
    Fail,
    Exit
}

/// <summary>
/// 룰에서 사용할 간단한 웨이브 스폰 정보
/// </summary>
public readonly struct DungeonWaveSpawnEntry
{
    public readonly int monsterId;
    public readonly int spawnNum;

    public DungeonWaveSpawnEntry(int monsterId, int spawnNum)
    {
        this.monsterId = monsterId;
        this.spawnNum = spawnNum;
    }
}

/// <summary>
/// 던전 공통 진행 매니저
/// 입장 / 준비 / 전투 / 실패 / 클리어 / 복귀 공통 처리
/// 실제 던전별 진행 규칙은 IDungeonRule 구현체가 담당
/// </summary>
public class DungeonManager : MonoBehaviour
{
    public static DungeonManager Instance { get; private set; }

    //이벤트
    public static event System.Action<int> OnDungeonStarted;
    public static event System.Action<string, int, float> OnDungeonCleared;
    public static event System.Action<int> OnDungeonFailed;
    public static event System.Action<int> OnDungeonWaveChanged;

    //UI 연결용 이벤트
    public static event System.Action<DungeonState> OnDungeonStateChanged;
    public static event System.Action<int, int> OnDungeonStepStarted;              // dungeonId, stepId
    public static event System.Action<int, int, int> OnDungeonWaveProgressChanged; // currentWave, maxWave, alive
    public static event System.Action<int, int, int> OnDungeonEntryCountChanged;   // dungeonId, used, max
    public static event System.Action<float> OnDungeonTimeLimitChanged;            // timeLimit
    public static event System.Action<float> OnDungeonPrepareStarted;
    public static event System.Action<List<Equipment>> OnDungeonEQReward;
    public static event System.Action<List<G_StoneData>> OnDungeonGSReward;
    public static event System.Action<List<Sk_SclData>> OnDungeonSKReward;
    public static event System.Action<System.Numerics.BigInteger> OnDungeonReward;

    private bool _eventsRegistered;

    [SerializeField] private MonsterSpawnManager _spawnManager;
    [SerializeField] private MapManager _mapManager;
    [SerializeField] private StageManager _stageManager;
    [SerializeField] private MonoBehaviour _playerBehaviour;

    private IMonsterTarget _player;

    private DungeonData _dungeon;       // CSV
    private Dungeon_StepData _stepData; // CSV

    private List<Monster_GroupData> _monsterGroup;

    private DungeonState _state = DungeonState.None;

    private float _prepareStartTime;
    private float _combatStartTime;
    private float _timeLimit;

    private int _aliveMonsterCount;

    private bool _isDungeonRunning;

    private IDungeonRule _rule;

    private DungeonSpawnAreaProvider _dungeonSpawnProvider; // 현재 던전맵 전용 스폰 Provider

    private struct WaveEntry
    {
        public int wave;
        public int monsterId;
        public int spawnNum;
    }

    private WaveEntry[] _waveEntries; // 전체 Wave 정보
    private int _waveEntryCount;

    private int[] _waveStartIndex;  // wave 시작
    private int[] _waveEndIndex;    // wave 끝

    private int _maxWave;           // 최대 wave
    private int _currentWave;       // 현재 wave


    public bool IsDungeonRunning => _isDungeonRunning;
    public DungeonState CurrentState => _state;
    public int CurrentWave => _currentWave;
    public int MaxWave => _maxWave;
    public int AliveMonsterCount => _aliveMonsterCount;
    public int CurrentDungeonId => _dungeon != null ? _dungeon.Dungeon_Id : 0;
    public int CurrentDungeonStepId => _stepData != null ? _stepData.Dungeon_Step_Id : 0;
    public float CurrentTimeLimit => _timeLimit;
    public DungeonData CurrentDungeonData => _dungeon;
    public Dungeon_StepData CurrentStepData => _stepData;

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
            Debug.LogError("[DungeonManager] PlayerTarget 없음");

        if (_spawnManager == null)
            Debug.LogError("[DungeonManager] MonsterSpawnManager 없음");

        if (_mapManager == null)
            Debug.LogError("[DungeonManager] MapManager 없음");

        if (_stageManager == null)
            Debug.LogError("[DungeonManager] StageManager 없음");
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

    private void Update()
    {
        if (_state == DungeonState.Prepare)
        {
            if (Time.time - _prepareStartTime >= 3f)
            {
                StartCombat();
            }
        }

        if (_state == DungeonState.Combat)
        {
            if (Time.time - _combatStartTime >= _timeLimit)
            {
                Debug.Log("[Dungeon] 시간 초과");
                ChangeState(DungeonState.Fail);
            }
        }
    }

    #region 던전 입장
    public void StartDungeon(int dungeonId, int stepId)
    {
        //TODO : 테스트용 횟수초기화 삭제 예정
        DungeonEntryTracker.ForceSetUsedCount(150001, 0);
        DungeonEntryTracker.ForceSetUsedCount(150002, 0);
        DungeonEntryTracker.ForceSetUsedCount(150003, 0);
        DungeonEntryTracker.ForceSetUsedCount(150004, 0);
        DungeonEntryTracker.ForceSetUsedCount(150005, 0);

        if (_isDungeonRunning)
        {
            Debug.LogWarning("[Dungeon] 이미 던전 진행중");
            return;
        }

        if (DataManager.Instance == null)
        {
            Debug.LogError("[Dungeon] DataManager 없음");
            return;
        }

        _dungeon = DataManager.Instance.GetData<DungeonData>(dungeonId);
        if (_dungeon == null)
        {
            Debug.LogError("[Dungeon] DungeonData 없음");
            return;
        }

        _stepData = DataManager.Instance.GetData<Dungeon_StepData>(stepId);
        if (_stepData == null)
        {
            Debug.LogError("[Dungeon] DungeonStepData 없음");
            return;
        }

        //stepId와 dungeonId 일치 검증
        if (_stepData.Dungeon_Id != dungeonId)
        {
            Debug.LogError($"[Dungeon] stepId={stepId} 는 dungeonId={dungeonId} 해당이 아님");
            return;
        }

        _monsterGroup = DataManager.Instance.GetList<Monster_GroupData>(_stepData.Monster_Group_Id);
        if (_monsterGroup == null || _monsterGroup.Count == 0)
        {
            Debug.LogError("[Dungeon] MonsterGroup 없음");
            return;
        }

        

        //wave 정렬 안정화
        _monsterGroup.Sort((a, b) =>
        {
            int waveCompare = a.Monster_Wave.CompareTo(b.Monster_Wave);
            if (waveCompare != 0)
                return waveCompare;

            return a.Monster_Id.CompareTo(b.Monster_Id);
        });

        //문자열 안전 파싱
        if (!float.TryParse(_stepData.Time_Limit, NumberStyles.Any, 
            CultureInfo.InvariantCulture, out _timeLimit))
        {
            Debug.LogWarning($"[Dungeon] Time_Limit 파싱 실패: {_stepData.Time_Limit}, 기본 60초 사용");
            _timeLimit = 60f;
        }

        OnDungeonTimeLimitChanged?.Invoke(_timeLimit);

        //현재 스테이지 진행 상태 저장
        if (_stageManager != null)
        {
            DungeonReturnContext.Save(
                _stageManager.CurrentStageId,
                _stageManager.CurrentSection
            );
        }

        //스테이지 자동 스폰 정지 및 필드 정리
        if (_spawnManager != null)
        {
            _spawnManager.StopAllSpawnForDungeon();
            _spawnManager.ForceClearAll();
        }

        //던전용 웨이브 테이블 생성
        BuildWaveTable();

        //던전 맵 로드
        if (_mapManager != null)
        {
            _mapManager.LoadDungeonMap(_dungeon.Dungeon_Id);
            _dungeonSpawnProvider = _mapManager.CurrentDungeonSpawnProvider;
        }

        _rule = DungeonRuleFactory.CreateRule(_dungeon.Dungeon_Id);
        _rule.Initialize(this);

        _isDungeonRunning = true;

        OnDungeonStepStarted?.Invoke(dungeonId, stepId);

        ChangeState(DungeonState.Enter);
    }
    #endregion

    #region 웨이브 빌드
    private void BuildWaveTable()
    {
        _waveEntryCount = _monsterGroup.Count;
        _waveEntries = new WaveEntry[_waveEntryCount];
        _maxWave = 0;

        for (int i = 0; i < _waveEntryCount; i++)
        {
            var data = _monsterGroup[i];

            _waveEntries[i].wave = data.Monster_Wave;
            _waveEntries[i].monsterId = data.Monster_Id;
            _waveEntries[i].spawnNum = data.Spawn_Num;

            if (data.Monster_Wave > _maxWave)
                _maxWave = data.Monster_Wave;
        }

        //wave index 생성
        _waveStartIndex = new int[_maxWave + 1];
        _waveEndIndex = new int[_maxWave + 1];

        for (int i = 0; i < _waveStartIndex.Length; i++)
        {
            _waveStartIndex[i] = -1;
            _waveEndIndex[i] = -1;
        }

        for (int i = 0; i < _waveEntryCount; i++)
        {
            int wave = _waveEntries[i].wave;

            if (_waveStartIndex[wave] == -1)
                _waveStartIndex[wave] = i;

            _waveEndIndex[wave] = i;
        }

        if (_maxWave <= 0)
            Debug.LogWarning("[Dungeon] 웨이브 수가 0이하임");
    }
    #endregion

    #region 던전 상태
    private void ChangeState(DungeonState newState)
    {
        _state = newState;

        OnDungeonStateChanged?.Invoke(_state);

        switch (_state)
        {
            case DungeonState.Enter:
                EnterDungeon();
                break;

            case DungeonState.Prepare:
                PrepareCombat();
                break;

            case DungeonState.Combat:
                break;

            case DungeonState.Clear:
                ClearDungeon();
                break;

            case DungeonState.Fail:
                FailDungeon();
                break;

            case DungeonState.Exit:
                ExitDungeon();
                break;
        }
    }
    #endregion

    private void EnterDungeon()
    {
        Debug.Log($"[Dungeon] 진입 {_dungeon.Dungeon_Name}");

        MovePlayerToSpawn();

        OnDungeonStarted?.Invoke(_dungeon.Dungeon_Id);

        ChangeState(DungeonState.Prepare);
    }

    private void PrepareCombat()
    {
        Debug.Log("[Dungeon] 준비");

        _prepareStartTime = Time.time;

        OnDungeonPrepareStarted?.Invoke(3f);

        _rule?.OnPrepareStarted();
    }

    #region 전투 로직
    private void StartCombat()
    {
        Debug.Log("[Dungeon] 전투 시작");

        _combatStartTime = Time.time;
        ChangeState(DungeonState.Combat);

        _rule?.OnCombatStarted(); // 실제 스폰은 룰이 담당

        //혹시 룰 구현 오류로 0마리 스폰이면 실패 처리
        if (_aliveMonsterCount <= 0)
        {
            Debug.LogError("[Dungeon] 룰 실행 후 스폰된 몬스터가 없음");
            ChangeState(DungeonState.Fail);
        }
    }
    #endregion

    //private void SpawnWave(int wave)
    //{
    //    if (wave > _maxWave)
    //    {
    //        ChangeState(DungeonState.Clear);
    //        return;
    //    }

    //    int start = _waveStartIndex[wave];
    //    int end = _waveEndIndex[wave];

    //    if (start == -1)
    //    {
    //        Debug.LogError($"Wave 없음 {wave}");
    //        ChangeState(DungeonState.Fail);
    //        return;
    //    }

    //    _aliveMonsterCount = 0;

    //    OnDungeonWaveChanged?.Invoke(wave);

    //    for (int i = start; i <= end; i++)
    //    {
    //        int monsterId = _waveEntries[i].monsterId;
    //        int spawnNum = _waveEntries[i].spawnNum;

    //        for (int j = 0; j < spawnNum; j++)
    //        {
    //            SpawnSingle(monsterId);
    //        }
    //    }

    //    //0마리 스폰 보호처리
    //    if (_aliveMonsterCount <= 0)
    //    {
    //        Debug.LogError($"[Dungeon] Wave {wave} 스폰 결과가 0마리");
    //        ChangeState(DungeonState.Fail);
    //    }
    //}

    //private void SpawnSingle(int monsterId)
    //{
    //    if (_spawnManager == null)
    //        return;

    //    if (!_spawnManager.TryGetSpawnPosition(out UnityEngine.Vector3 pos))
    //        pos = UnityEngine.Vector3.zero;

    //    if (_spawnManager.SpawnSingleDungeon(monsterId, pos))
    //    {
    //        _aliveMonsterCount++;
    //    }
    //    else
    //    {
    //        Debug.LogWarning($"[Dungeon] Spawn 실패 monsterId={monsterId}");
    //    }
    //}

    private void HandleMonsterKilled(int monsterId, bool isBoss)
    {
        if (!_isDungeonRunning)
            return;

        if (_state != DungeonState.Combat)
            return;

        _aliveMonsterCount = Mathf.Max(0, _aliveMonsterCount - 1);
        NotifyWaveProgressChanged();

        _rule?.OnMonsterKilled(monsterId);
    }

    #region 성공 / 실패
    private void ClearDungeon()
    {
        float clearTime = (Time.time - _combatStartTime);

        Debug.Log("[Dungeon] 성공");

        GiveReward();

        DataManager.Instance.TryConsumeEntry(_dungeon.Dungeon_Id, _dungeon.Daily_Entry, out int usedCount);

        OnDungeonEntryCountChanged?.Invoke(_dungeon.Dungeon_Id, usedCount, _dungeon.Daily_Entry);

        OnDungeonCleared?.Invoke(_dungeon.Dungeon_Name, _stepData.Dungeon_Step_Id, clearTime);

        ChangeState(DungeonState.Exit);
    }

    private void FailDungeon()
    {
        Debug.Log("[Dungeon] 실패");

        OnDungeonFailed?.Invoke(_dungeon.Dungeon_Id);

        ChangeState(DungeonState.Exit);
    }
    #endregion

    #region 던전 보상
    private void GiveReward()
    {
        //던전 보상 조회
        var reward =
            DataManager.Instance.GetData<Dungeon_RewardData>(_stepData.Reward_Group_Id);

        if (reward == null)
        {
            Debug.LogError("RewardData 없음");
            return;
        }

        //클리어한 던전 보상이 골드, 경험치, 강자의증표
        if (reward.Dungeon_Type == Dungeon_Type.Gold || reward.Dungeon_Type == Dungeon_Type.Exp || reward.Dungeon_Type == Dungeon_Type.TokenOfStrong)
        {
            System.Numerics.BigInteger amount =
                BigIntRandom.Range(reward.Reward_Min, reward.Reward_Max + 1);

            Debug.Log($"[Dungeon] Reward 지급 ConsumId={reward.Consum_Id}," +
                $" Amount={amount}, Rank={reward.Reward_Rank}");

            //보상값 이벤트 보내기
            OnDungeonReward?.Invoke(amount);
        }

        //클리어한 던전 보상이 스킬주문서
        if (reward.Dungeon_Type == Dungeon_Type.SkillScroll)
        {
            //스킬주문서 데이터가져오고
            var SData = DataManager.Instance.GetDict<Sk_SclData>();

            var RandomList = new List<Sk_SclData>();

            int a = (int)BigIntRandom.Range(reward.Reward_Min, reward.Reward_Max + 1);

            for (int i = 0; i < a; i++)
            {
                //값들 리스트 만들고
                var list = new List<Sk_SclData>(SData.Values);
                //랜덤돌리기
                var random = list[Random.Range(0, list.Count)];
                //선택된값 추가
                RandomList.Add(random);
            }

            //랜덤값 이벤트 보내기
            OnDungeonSKReward?.Invoke(RandomList);
        }

        //클리어한 던전 보상이 요정석
        if (reward.Dungeon_Type == Dungeon_Type.FairyStone)
        {
            //요정석 데이터 가져오기
            var GSData = DataManager.Instance.GetDict<G_StoneData>();

            var RandomList = new List<G_StoneData>();

            int a = (int)BigIntRandom.Range(reward.Reward_Min, reward.Reward_Max + 1);

            for (int i = 0; i < a; i++)
            {                
                //값들 list에 넣고
                var list = new List<G_StoneData>(GSData.Values);
                //랜덤선택
                var random = list[Random.Range(0, list.Count)];
                //선택된값 추가
                RandomList.Add(random);
            }

            //랜덤값 이벤트 보내기
            OnDungeonGSReward?.Invoke(RandomList);
        }

        //클리어한 던전 보상이 장비
        if (reward.Dungeon_Type == Dungeon_Type.Equipment)
        {
            //기존에 있던 스테이지 섹션 아이디 조회
            var Ddata = DataManager.Instance.GetData<Stage_SectionData>(DungeonReturnContext.ReturnSection);

            //장비담아둘 리스트
            var Equipmentlist = new List<Equipment>();

            //반복할 횟수 랜덤 뽑기
            int a = (int)BigIntRandom.Range(reward.Reward_Min, reward.Reward_Max + 1);

            //랜덤으로 뽑은 횟수만큼 장비생성
            for (int i = 0; i < a; i++)
            {
                var Equipment = TestWeaponGenerator.Instance.Test2(Ddata.Equip_Drop_Level, (int)reward.Reward_Rank);

                Equipmentlist.Add(Equipment);
            }

            //생성된 장비 이벤트 보내기
            OnDungeonEQReward?.Invoke(Equipmentlist);
        }


        
    }
    #endregion

    #region 던전 나가기
    private void ExitDungeon()
    {
        Debug.Log("[Dungeon] Exit -> Stage 복귀");

        if (_spawnManager != null)
        {
            _spawnManager.StopAllSpawnForDungeon();
            _spawnManager.ForceClearAll();
        }

        if (_mapManager != null && DungeonReturnContext.HasContext)
        {
            _mapManager.LoadStageMap(DungeonReturnContext.ReturnStageId);
        }

        //저장된 스테이지 진행도로 복귀
        if (_stageManager != null && DungeonReturnContext.HasContext)
        {
            _stageManager.ResumeStageFromSavedContext();
        }

        ResetDungeonRuntime();
    }

    private void ResetDungeonRuntime()
    {
        _dungeon = null;
        _stepData = null;
        _monsterGroup = null;

        _waveEntries = null;
        _waveStartIndex = null;
        _waveEndIndex = null;

        _aliveMonsterCount = 0;
        _currentWave = 0;
        _maxWave = 0;
        _timeLimit = 0f;

        _rule = null;
        _dungeonSpawnProvider = null;

        _isDungeonRunning = false;
        _state = DungeonState.None;
    }
    #endregion

    private void HandlePlayerDead()
    {
        if (_state == DungeonState.Combat || _state == DungeonState.Prepare)
        {
            ChangeState(DungeonState.Fail);
        }
    }

    private void MovePlayerToSpawn()
    {
        if (_player == null)
            return;

        if (TryGetDungeonSpecialPoint(
            DungeonSpecialPointType.PlayerSpawn,
            out Vector3 pos))
        {
            Transform playerTransform = ((MonoBehaviour)_player).transform;
            playerTransform.position = pos;
        }
        else
        {
            Debug.LogWarning("[Dungeon] PlayerSpawnPoint 없음");
        }
    }

    #region Rule 전용
    public void SetCurrentWave(int Wave)
    {
        _currentWave = Wave;
    }

    public void NotifyWaveChanged()
    {
        OnDungeonWaveChanged?.Invoke(_currentWave);
        NotifyWaveProgressChanged();
    }

    public void NotifyWaveProgressChanged()
    {
        OnDungeonWaveProgressChanged?.Invoke(_currentWave, _maxWave, _aliveMonsterCount);
    }

    public void AdvanceToNextWave()
    {
        _currentWave++;
        NotifyWaveChanged();

        //기본 룰은 다음 웨이브를 ordered 방식으로 스폰
        //실제 wave spawn 방식은 current rule에 의해 재사용되는 helper를 통해 제어
        if (_rule is StandardWaveDungeonRule)
        {
            // StandardWaveDungeonRule 내부 로직과 맞추기 위해
            // 다시 룰 쪽 spawn helper를 쓰도록 간단히
            // current wave combat started와 동일하게 처리하지 않고,
            // DungeonRuleBase가 직접 manager helpers를 호출하는 구조를 유지함.
            // 여기서는 Rule instance가 다음 웨이브 스폰까지 담당하도록 명시 호출 대신 아래 방식 사용.
            SpawnCurrentWaveByRuleFallback();
        }
        else
        {
            SpawnCurrentWaveByRuleFallback();
        }
    }

    /// <summary>
    /// 룰이 웨이브 전환 후 별도 로직 없이 ordered spawn을 쓰는 기본 fallback
    /// StandardWaveDungeonRule를 위해 준비한 helper
    /// </summary>
    private void SpawnCurrentWaveByRuleFallback()
    {
        if (_dungeon == null)
            return;

        //150002, 150003, 150004는 ordered spawn 기반 공통 처리
        switch (_dungeon.Dungeon_Id)
        {
            case 150002:
            case 150003:
            case 150004:
                SpawnWaveOrderedInternal(_currentWave);
                break;

            default:
                SpawnWaveOrderedInternal(_currentWave);
                break;
        }
    }

    public void RequestClear()
    {
        ChangeState(DungeonState.Clear);
    }

    public void RequestFail()
    {
        ChangeState(DungeonState.Fail);
    }

    public bool SpawnDungeonMonster(int monsterId, Vector3 pos)
    {
        if (_spawnManager == null)
            return false;

        if (_spawnManager.SpawnSingleDungeon(monsterId, pos))
        {
            _aliveMonsterCount++;
            NotifyWaveProgressChanged();
            return true;
        }

        return false;
    }

    public bool TryGetDungeonOrderedSpawnPoint(int orderIndex, out Vector3 pos)
    {
        if (_dungeonSpawnProvider != null &&
            _dungeonSpawnProvider.TryGetOrderedPointOnNavMesh(orderIndex, out pos))
        {
            return true;
        }

        pos = Vector3.zero;
        return false;
    }

    public bool TryGetDungeonRandomSpawnPoint(out Vector3 pos)
    {
        if (_dungeonSpawnProvider != null &&
            _dungeonSpawnProvider.TryGetRandomPointOnNavMesh(out pos))
        {
            return true;
        }

        //던전 provider가 없으면 기존 랜덤 필드 provider fallback
        if (_spawnManager != null && _spawnManager.TryGetSpawnPosition(out pos))
            return true;

        pos = Vector3.zero;
        return false;
    }

    public bool TryGetDungeonSpecialPoint(DungeonSpecialPointType type, out Vector3 pos)
    {
        if (_dungeonSpawnProvider != null &&
            _dungeonSpawnProvider.TryGetSpecialPointOnNavMesh(type, out pos))
        {
            return true;
        }

        pos = Vector3.zero;
        return false;
    }

    public List<DungeonWaveSpawnEntry> GetWaveEntries(int wave)
    {
        List<DungeonWaveSpawnEntry> result = new();

        if (_waveStartIndex == null || _waveEndIndex == null)
            return result;

        if (wave <= 0 || wave >= _waveStartIndex.Length)
            return result;

        int start = _waveStartIndex[wave];
        int end = _waveEndIndex[wave];

        if (start == -1 || end == -1)
            return result;

        for (int i = start; i <= end; i++)
        {
            result.Add(new DungeonWaveSpawnEntry(
                _waveEntries[i].monsterId,
                _waveEntries[i].spawnNum
            ));
        }

        return result;
    }

    /// <summary>
    /// DungeonRuleBase fallback용 내부 ordered spawn
    /// </summary>
    /// <param name="wave"></param>
    private void SpawnWaveOrderedInternal(int wave)
    {
        List<DungeonWaveSpawnEntry> entries = GetWaveEntries(wave);
        int pointOrder = 0;

        for (int i = 0; i < entries.Count; i++)
        {
            for (int j = 0; j < entries[i].spawnNum; j++)
            {
                if (TryGetDungeonOrderedSpawnPoint(pointOrder, out Vector3 pos))
                {
                    if (SpawnDungeonMonster(entries[i].monsterId, pos))
                        pointOrder++;
                }
                else if (TryGetDungeonRandomSpawnPoint(out Vector3 fallbackPos))
                {
                    if (SpawnDungeonMonster(entries[i].monsterId, fallbackPos))
                        pointOrder++;
                }
            }
        }

        if (_aliveMonsterCount <= 0)
        {
            Debug.LogError($"[Dungeon] Wave {wave} 스폰 결과가 0마리");
            ChangeState(DungeonState.Fail);
        }
    }
    #endregion

    public void StartTestDungeon()  // 테스트용
    {
        DungeonEntryTracker.ForceSetUsedCount(150002, 0);
        DungeonEntryTracker.ForceSetUsedCount(150003, 0);
        DungeonEntryTracker.ForceSetUsedCount(150004, 0);
        DungeonEntryTracker.ForceSetUsedCount(150005, 0);

        //StartDungeon(150002, 160501);     //광신도
        //StartDungeon(150003, 161001);     //암살자
        StartDungeon(150004, 161501);       //마법사, 침묵의 성역 1단계 버튼
        //StartDungeon(150005, 162001);     //실력자
    }
}