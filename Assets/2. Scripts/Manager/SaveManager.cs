using System;
using System.Collections;
using System.Numerics;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    [SerializeField] private float autoSaveSecond = 60f;
    [SerializeField] private PlayerStats _playerStat;

    public Action OnSave;

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
        Instance.LoadGame();
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
            skillData = SkillManager.Instance.GetSaveData()
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
        //Debug.Log(EquipmentInventory.Instance);
        //Debug.Log(GremlinInventory.Instance);
        EquipmentInventory.Instance.LoadFromSaveData(data.equipInv);
        GremlinInventory.Instance.LoadFromSaveData(data.GremlinInv);
        StageManager.Instance.LoadFromSaveData(data.stageData);
        SkillManager.Instance.LoadFromSaveData(data.skillData);

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

        for (int i = 0; i < (int)equipCount; i++)
        {
            TestWeaponGenerator.Instance.Test(eqlevel.Equip_Drop_Level);
        }
    }
}
