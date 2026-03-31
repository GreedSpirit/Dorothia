using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using TMPro;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    [SerializeField] private float autoSaveSecond = 300f;
    [SerializeField] private PlayerCtrl _playerCtrl;
    [SerializeField] private PlayerStats _playerStat;
    [SerializeField] private GameObject _rewardPanel;
    [SerializeField] private TextMeshProUGUI _rewardText;
    [SerializeField] private TextMeshProUGUI _rewardEquipText;

    private BigInteger _gold;
    private BigInteger _exp;

    public Action OnSave;
    public Action EquipName;

    DateTime lastQuitTime;
    double offlineSeconds;

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
        OnClickStartGame();
    }

    //종료 시
    private void OnApplicationQuit()
    {
        SaveGame();
        SaveTime();
        PlayerPrefs.Save();
    }

    //홈 버튼 등으로 인한 중지 상태에서의 강제종료 시
    private void OnApplicationPause(bool pause)
    {
        if (pause == true)
        {
            SaveGame();
            SaveTime();
            PlayerPrefs.Save();
        }
    }

    private IEnumerator AutoSaveCoroutine()
    {
        while (true)
        {
            //정해진 시간만큼 대기한 후
            yield return new WaitForSeconds(autoSaveSecond);

            //게임 저장
            SaveGame();
        }
    }

    public void OnClickStartGame()
    {
        StartCoroutine(AutoSaveCoroutine());
    }

    private void SaveGame()
    {
        var saveData = CreateSaveData();
        SaveManagement.Save("GameData", saveData);
        Debug.LogWarning("저장 완료!");
    }

    private SaveData CreateSaveData()
    {
        //Debug.Log(EquipmentInventory.Instance);
        //Debug.Log(GremlinInventory.Instance);
        return new SaveData
        {
            equipInv = EquipmentInventory.Instance.GetSaveData(),
            GremlinInv = GremlinInventory.Instance.GetSaveData(),
            stageData = StageManager.Instance.GetSaveData(),
            skillData = SkillManager.Instance.GetSaveData(),
            playerData = _playerStat.GetSaveData()
        };
    }

    public async void LoadGame()
    {
        //종료시각 불러오기
        lastQuitTime = TimeManager.Instance.LoadQuitTime();

        //현재 시각
        DateTime now = DateTime.UtcNow;

        //방치 시간 계산
        offlineSeconds = (now - lastQuitTime).TotalSeconds;

        //12시간까지만
        offlineSeconds = Math.Clamp(offlineSeconds, 0, 12 * 3600);

        var data = SaveManagement.Load<SaveData>("GameData");

        if (data == null) return;
        EquipmentInventory.Instance.LoadFromSaveData(data.equipInv);
        GremlinInventory.Instance.LoadFromSaveData(data.GremlinInv);
        StageManager.Instance.LoadFromSaveData(data.stageData);
        SkillManager.Instance.LoadFromSaveData(data.skillData);
        _playerStat.LoadFromSaveData(data.playerData);

        GiveOfflineReward();
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
        string result;

        foreach (var equip in data)
        {
            string rarityText = GetRarityText(equip.equipment_Rarity);
            equipStrings.Add($"{rarityText} : {equip.equip_name}");
        }

        //들어온 장비가 있으면
        if (equipStrings.Count > 0)
        {
            result = string.Join(", ", equipStrings);
        }
        else
        {
            result = "없음";
        }

        _rewardEquipText.enabled = true;

        _rewardText.text =
            $"경험치 : {_exp}\n" +
            $"골드 : {_gold}\n";

        _rewardEquipText.text = $"장비  {result}";

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
