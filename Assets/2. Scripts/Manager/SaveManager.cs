using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    [SerializeField] private float checkSaveSecond = 10f;             // 저장 조건 충족 확인할 시간 간격
    [SerializeField] private PlayerCtrl _playerCtrl;
    [SerializeField] private PlayerStats _playerStat;
    [SerializeField] private GameObject _rewardPanel;
    [SerializeField] private TextMeshProUGUI _rewardText;
    [SerializeField] private TextMeshProUGUI _rewardEquipText;        // 장비 전용 보상 텍스트

    private BigInteger _gold;
    private BigInteger _exp;

    public Action OnSave;
    public Action EquipName;

    DateTime lastQuitTime;
    DateTime lastSaveTime;
    double offlineSeconds;

    private bool _isLoaded = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        OnSave += SaveGame;
        TestWeaponGenerator.Instance._OnGetEquipment += ChangeUIInfo;
    }

    private void Start()
    {
        if(lastSaveTime == DateTime.MinValue)
        {
            lastSaveTime = DateTime.UtcNow;
        }    
        StartCoroutine(LoadNextFrame());
        OnClickStartGame();
    }

    //종료 시
    private void OnApplicationQuit()
    {
        SaveGame();
        SaveTime();
    }

    //홈 버튼 등으로 인한 중지 상태에서의 강제종료 시
    private void OnApplicationPause(bool pause)
    {
        if (pause == true)
        {
            SaveGame();
            SaveTime();
        }
        else
        {
            TrySave();
        }
    }

    private void TrySave()
    {
        if((DateTime.UtcNow - lastSaveTime).TotalMinutes >= 5)
        {
            SaveGame();
            lastSaveTime = DateTime.UtcNow;

        }
    }
    private IEnumerator LoadNextFrame()
    {
        yield return null;

        var task = SaveManager.Instance.LoadGame();
        yield return new WaitUntil(() => task.IsCompleted);

        // Task에서 예외 터졌을 때 무시 방지
        if (task.IsFaulted)
            Debug.LogError($"Load 실패: {task.Exception}");
    }

    public void OnClickStartGame()
    {
        StartCoroutine(StartAutoSaveAfterLoad());
    }

    private IEnumerator StartAutoSaveAfterLoad()
    {
        while (!_isLoaded) // 로딩체크
            yield return null;

        StartCoroutine(AutoSaveCoroutine());
    }

    private IEnumerator AutoSaveCoroutine()
    {
        while (true)
        {
            //정해진 시간만큼 대기한 후
            yield return new WaitForSeconds(checkSaveSecond);

            //게임 저장
            TrySave();
        }
    }

    private void SaveGame()
    {
        if (!_isLoaded)
            return;

        var saveData = CreateSaveData();

        //AES 암호화된 JSON 파일로 저장
        SaveUtility.SaveEncrypted("GameData", saveData);
        Debug.LogWarning("저장 완료!");
    }

    private SaveData CreateSaveData()
    {
        //Debug.Log(EquipmentInventory.Instance);
        //Debug.Log(GremlinInventory.Instance);
        return new SaveData
        {
            exchangeData = ExchangeManager.Instance.GetSaveData(),
            equipInv = EquipmentInventory.Instance.GetSaveData(),
            GremlinInv = GremlinInventory.Instance.GetSaveData(),
            stageData = StageManager.Instance.GetSaveData(),
            skillData = SkillManager.Instance.GetSaveData(),
            playerData = _playerStat.GetSaveData()
        };
    }

    public async Task LoadGame()
    {
        var data = await SaveUtility.LoadEncryptedAsync<SaveData>("GameData");

        if (data == null)
        {
            Debug.LogWarning("Load된 데이터가 없음! 첫 시작");
            StageManager.Instance.StartStage(110001); // 첫 시작시 110001로 시작하게

            // 스타터 스킬 지급
            int StartSkillId = 10001;
            SkillManager.Instance.GiveStarterSkill(StartSkillId, slotIndex: 0);

            lastSaveTime = DateTime.UtcNow;
            _isLoaded = true;

            return;
        }

        ExchangeManager.Instance.LoadFromSaveData(data.exchangeData);
        // 프레임 양보
        await Task.Yield(); 
        EquipmentInventory.Instance.LoadFromSaveData(data.equipInv);
        await Task.Yield();
        GremlinInventory.Instance.LoadFromSaveData(data.GremlinInv);
        await Task.Yield();
        StageManager.Instance.LoadFromSaveData(data.stageData);
        await Task.Yield();
        SkillManager.Instance.LoadFromSaveData(data.skillData);
        await Task.Yield();
        _playerStat.LoadFromSaveData(data.playerData);

        GiveOfflineReward();

        _isLoaded = true;
    }

    private void SaveTime()
    {
        TimeManager.Instance.SaveQuitTime();
    }

    private void GiveOfflineReward()
    {
        //Debug.LogError("DataManager: " + (DataManager.Instance != null));
        //Debug.LogError("StageManager: " + (StageManager.Instance != null));
        //Debug.LogError("ExchangeManager: " + (ExchangeManager.Instance != null));
        //Debug.LogError("TestWeaponGenerator: " + (TestWeaponGenerator.Instance != null));
        //Debug.LogError("_playerStat: " + (_playerStat != null));

        var sectionData = DataManager.Instance.GetData<Stage_RewardData>(StageManager.Instance.CurrentSection);

        if (sectionData == null)
        {
            Debug.LogError("sectionData가 null입니다! CurrentSection 확인 필요");
            return;
        }

        double offlineHours = offlineSeconds / 3600;

        if (offlineHours <= 0) return;

        double clearsPerHour = 3600 / 90;

        BigInteger sectionGold = sectionData.Section_Gold;
        BigInteger sectionExp = sectionData.Section_Exp;

        double goldCalc = (double)sectionGold * clearsPerHour * 0.7 * offlineHours;
        double expCalc = (double)sectionExp * clearsPerHour * 0.6 * offlineHours;
        double equipCount = offlineHours * 2;

        BigInteger gold = new BigInteger(Math.Floor(goldCalc));
        BigInteger exp = new BigInteger(Math.Floor(expCalc));

        _gold = gold;
        _exp = exp;

        //Debug.LogError($"방치보상 골드: {goldCalc}");
        //Debug.LogError($"방치보상 경험치: {expCalc}");
        //Debug.LogError($"방치 장비 수: {equipCount}");

        //실제 지급
        ExchangeManager.Instance.GetMoney(MoneyType.Gold, gold);
        _playerStat.AddExp(exp);

        var eqlevel = StageManager.Instance.CurrentSectionData;

        if (eqlevel == null)
        {
            Debug.LogError($"eqlevel가 null입니다! CurrentSection: {StageManager.Instance.CurrentSection}");
            return;
        }

        //장비보상은 없는경우
        if (equipCount < 1)
        {
            _rewardEquipText.enabled = false;
        
            _rewardText.text =
                    $"경험치 : {_exp}\n" +
                    $"골드 : {_gold}\n";
        
            _rewardPanel.SetActive(true);
            return;
        }


        TestWeaponGenerator.Instance.Test3(eqlevel.Equip_Drop_Level, equipCount);


    }

    private string GetRarityText(int rarity)
    {
        switch (rarity)
        {
            case 1: return "일반";
            case 2: return "고급";
            case 3: return "희귀";
            case 4: return "영웅";
            case 5: return "전설";
            default: return "알 수 없음";
        }
    }

    //TestWeaponGenerator에서 Test함수에서 생성된 장비 이벤트오면 실행 (팝업패널정보)
    private void ChangeUIInfo(List<Equipment> data)
    {
        //문장만들 리스트랑 문자열준비
        List<string> equipStrings = new List<string>();

        Dictionary<Rarity, int> rarity = new Dictionary<Rarity, int>();

        foreach (Rarity part in System.Enum.GetValues(typeof(Rarity)))
        {
            rarity.Add(part, 0);
        }

        if(data.Count > 0)
        {
            foreach (var equip in data)
            {
                rarity[(Rarity)equip.equipment_Rarity]++;
            }
        }


        _rewardEquipText.enabled = true;

        _rewardText.text =
            $"경험치 : {_exp}\n" +
            $"골드 : {_gold}\n";

        _rewardEquipText.text = 
            $"장비           신화 : {rarity[Rarity.Mythtic]:D3},\n" +
            $"전설 : {rarity[Rarity.Legendary]:D3}, 레어 : {rarity[Rarity.Rare]:D3},\n" +
            $"희귀 : {rarity[Rarity.Uncommon]:D3}, 일반 : {rarity[Rarity.Normal]:D3}";

        _rewardPanel.SetActive(true);

        /*
        int index = data.equipment_Rarity;

        switch (index)
        {
            //희귀
            case 1:
                _rewardText.text =
                    $"경험치 : {_exp}\n" +
                    $"골드 : {_gold}\n" +
                    $"장비 : {data.equip_name}";
                _rewardPanel.SetActive(true);
                break;

            case 2:
                _rewardText.text =
                    $"경험치 : {_exp}\n" +
                    $"골드 : {_gold}\n" +
                    $"장비 : {data.equip_name}";
                _rewardPanel.SetActive(true);
                break;

            case 3:
                _rewardText.text =
                    $"경험치 : {_exp}\n" +
                    $"골드 : {_gold}\n" +
                    $"장비 : {data.equip_name}";
                _rewardPanel.SetActive(true);
                break;

            case 4:
                _rewardText.text =
                    $"경험치 : {_exp}\n" +
                    $"골드 : {_gold}\n" +
                    $"장비 : {data.equip_name}";
                _rewardPanel.SetActive(true);
                break;

            case 5:
                _rewardText.text =
                    $"경험치 : {_exp}\n" +
                    $"골드 : {_gold}\n" +
                    $"장비 : {data.equip_name}";
                _rewardPanel.SetActive(true);
                break;
        }
        */
    }
}
