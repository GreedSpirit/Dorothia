using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using GameUtility;
using System.Numerics;

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

public class DungeonManager : MonoBehaviour
{
    public static DungeonManager Instance { get; private set; }

    //이벤트
    public static event System.Action<int> OnDungeonStarted;
    public static event System.Action<int> OnDungeonCleared;
    public static event System.Action<int> OnDungeonFailed;
    public static event System.Action<int> OnDungeonWaveChanged;

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
        }

        _isDungeonRunning = true;
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

        OnDungeonStarted?.Invoke(_dungeon.Dungeon_Id);

        ChangeState(DungeonState.Prepare);
    }

    private void PrepareCombat()
    {
        Debug.Log("[Dungeon] 준비");

        _prepareStartTime = Time.time;
    }

    #region 전투 로직
    private void StartCombat()
    {
        Debug.Log("[Dungeon] 전투 시작");

        _combatStartTime = Time.time;

        _currentWave = 1;

        ChangeState(DungeonState.Combat);
        SpawnWave(_currentWave);
    }

    private void SpawnWave(int wave)
    {
        if (wave > _maxWave)
        {
            ChangeState(DungeonState.Clear);
            return;
        }

        int start = _waveStartIndex[wave];
        int end = _waveEndIndex[wave];

        if (start == -1)
        {
            Debug.LogError($"Wave 없음 {wave}");
            ChangeState(DungeonState.Fail);
            return;
        }

        _aliveMonsterCount = 0;

        OnDungeonWaveChanged?.Invoke(wave);

        for (int i = start; i <= end; i++)
        {
            int monsterId = _waveEntries[i].monsterId;
            int spawnNum = _waveEntries[i].spawnNum;

            for (int j = 0; j < spawnNum; j++)
            {
                SpawnSingle(monsterId);
            }
        }

        //0마리 스폰 보호처리
        if (_aliveMonsterCount <= 0)
        {
            Debug.LogError($"[Dungeon] Wave {wave} 스폰 결과가 0마리");
            ChangeState(DungeonState.Fail);
        }
    }

    private void SpawnSingle(int monsterId)
    {
        if (_spawnManager == null)
            return;

        if (!_spawnManager.TryGetSpawnPosition(out UnityEngine.Vector3 pos))
            pos = UnityEngine.Vector3.zero;

        if (_spawnManager.SpawnSingleDungeon(monsterId, pos))
        {
            _aliveMonsterCount++;
        }
        else
        {
            Debug.LogWarning($"[Dungeon] Spawn 실패 monsterId={monsterId}");
        }
    }
    #endregion

    private void HandleMonsterKilled(int monsterId, bool isBoss)
    {
        if (_state != DungeonState.Combat)
            return;

        _aliveMonsterCount = Mathf.Max(0, _aliveMonsterCount - 1);

        if (_aliveMonsterCount > 0)
            return;

        //마지막 몬스터를 잡았을 때 다음 웨이브 또는 클리어
        if (_currentWave < _maxWave)
        {
            _currentWave++;
            SpawnWave(_currentWave);
        }
        else
        {
            ChangeState(DungeonState.Clear);
        }
    }

    #region 성공 / 실패
    private void ClearDungeon()
    {
        Debug.Log("[Dungeon] 성공");

        GiveReward();

        OnDungeonCleared?.Invoke(_dungeon.Dungeon_Id);

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
        var reward =
            DataManager.Instance.GetData<Dungeon_RewardData>(_stepData.Reward_Group_Id);

        if (reward == null)
        {
            Debug.LogError("RewardData 없음");
            return;
        }

        //현재 Reward_Min / Max가 int 타입이라 소수/콤마가 들어가는 csv는 파싱 단계에서 정리 필요

        BigInteger amount = BigIntRandom.Range(reward.Reward_Min, reward.Reward_Max + 1);
        

        Debug.Log($"[Dungeon] Reward 지급 ConsumId={reward.Consum_Id}," +
            $" Amount={amount}, Rank={reward.Reward_Rank}");
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

    public void StartTestDungeon()  // 테스트용
    {
        //StartDungeon(150002, 160501);   //광신도
        //StartDungeon(150003, 161001);   //암살자
        //StartDungeon(150004, 161501);   //마법사
        StartDungeon(150005, 162001);   //실력자
    }
}