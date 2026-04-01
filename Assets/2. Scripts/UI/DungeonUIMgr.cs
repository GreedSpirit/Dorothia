using System.Collections.Generic;
using System.Numerics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DungeonUIMgr : MonoBehaviour
{
    [SerializeField] GameObject _dungeonRewardPanel;
    [SerializeField] GameObject _dungeonFailPanel;
    [SerializeField] TextMeshProUGUI _dungeonInfoText;
    [SerializeField] TextMeshProUGUI _dungeonRewardText;
    [SerializeField] TextMeshProUGUI _dungeonFailText;
    [SerializeField] Button _dungeonClear;
    [SerializeField] Button _nextDungeon;

    [SerializeField] GameObject _dungeonInfoPanel;
    [SerializeField] TextMeshProUGUI _dungeonName;
    [SerializeField] TextMeshProUGUI _dungeonTime;
    bool _isEnterDungeon;

    private void Update()
    {
        if (_isEnterDungeon == false) return;

        float elapsed = TimeManager.Instance.GetElapsed(TimerType.Dungeon);

        int minutes = (int)(elapsed / 60);
        int seconds = (int)(elapsed % 60);

        //00:00 표시
        _dungeonTime.text = ($"{minutes:00} : {seconds:00}");
    }

    private void OnEnable()
    {
        DungeonManager.OnDungeonEQReward += UpdateEQRewardUI;
        DungeonManager.OnDungeonGSReward += UpdateGSRewardUI;
        DungeonManager.OnDungeonSKReward += UpdateSKRewardUI;
        DungeonManager.OnDungeonReward += UpdateRewardUI;
        DungeonManager.OnDungeonCleared += UpdateDungeonInfo;
        DungeonManager.OnDungeonCleared += CloseDungeonInfo;
        DungeonManager.OnDungeonFailed += UpdateDungeonFail;
        DungeonManager.OnDungeonFailed += CloseDungeonInfo;

        DungeonManager.OnDungeonStarted += UpdateDungeonInfo;
        DungeonManager.OnDungeonStateChanged += HandleStateChanged;
    }

    private void OnDisable()
    {
        DungeonManager.OnDungeonEQReward -= UpdateEQRewardUI;
        DungeonManager.OnDungeonGSReward -= UpdateGSRewardUI;
        DungeonManager.OnDungeonSKReward -= UpdateSKRewardUI;
        DungeonManager.OnDungeonReward -= UpdateRewardUI;
        DungeonManager.OnDungeonCleared -= UpdateDungeonInfo;
        DungeonManager.OnDungeonCleared -= CloseDungeonInfo;
        DungeonManager.OnDungeonFailed -= UpdateDungeonFail;
        DungeonManager.OnDungeonFailed -= CloseDungeonInfo;

        DungeonManager.OnDungeonStarted -= UpdateDungeonInfo;
        DungeonManager.OnDungeonStateChanged -= HandleStateChanged;
    }

    public void UpdateDungeonInfo(int currentDungeonId)
    {
        var DungeonName = DataManager.Instance.GetData<DungeonData>(currentDungeonId);

        _dungeonInfoPanel.SetActive(true);

        _dungeonName.text = "";
        _dungeonTime.text = "";

        _isEnterDungeon = true;

        _dungeonName.text = ($"{DungeonName.Dungeon_Name}");
    }

    public void CloseDungeonInfo(string dungeonName, int stepId, float clearTime)
    {
        _dungeonInfoPanel.SetActive(false);

        _isEnterDungeon = false;
    }



    public void UpdateEQRewardUI(List<Equipment> list)
    {
        _dungeonRewardText.text = "";

        foreach (var EQ in list)
        {
            _dungeonRewardText.text += ($"{EQ.equip_name}\n");
        }
    }

    public void UpdateGSRewardUI(Dictionary<int, int> rewardMap)
    {
        _dungeonRewardText.text = "";
        int maxVisibleLines = 5;
        int lineCount = 0;

        foreach (var GS in rewardMap)
        {
            if (lineCount >= maxVisibleLines)
            {
                _dungeonRewardText.text += "...";
                break;
            }
            var data = DataManager.Instance.GetData<G_StoneData>(GS.Key);
            _dungeonRewardText.text += $"{data.G_Stone_Name} x {GS.Value}\n";
            lineCount++;
        }
    }

    public void UpdateSKRewardUI(List<SkillData> list)
    {
        _dungeonRewardText.text = "";
        int maxVisibleLines = 5;
        int lineCount = 0;

        //중복 체크 담아두기
        Dictionary<string, int> rewardCounts = new Dictionary<string, int>();


        foreach (var SK in list)
        {
            if (lineCount >= maxVisibleLines)
            {
                _dungeonRewardText.text += "...";
                break;
            }

            string name = SK.Skill_Name + " 주문서";

            if (rewardCounts.ContainsKey(name))
            {
                // 이미 있는 이름이면 개수만 +1
                rewardCounts[name]++;
            }
            else
            {
                // 처음 나온 이름이면 사전에 추가
                rewardCounts[name] = 1;
            }
        }

        foreach (var item in rewardCounts)
        {
            //스킬주문서 x 중복수 출력
            _dungeonRewardText.text += $"{item.Key} x {item.Value}\n";
            lineCount++;
        }
    }

    public void UpdateRewardUI(Dungeon_Type type, BigInteger reward)
    {
        _dungeonRewardText.text = "";

        switch (type)
        {
            case Dungeon_Type.Gold:
                _dungeonRewardText.text = ($"골드 : {reward}");
                break;

            case Dungeon_Type.Exp:
                _dungeonRewardText.text = ($"경험치 : {reward}");
                break;
        }
    }

    public void UpdateDungeonInfo(string dungeonName, int stepId, float clearTime)
    {
        _isEnterDungeon = false;

        int minutes = Mathf.FloorToInt(clearTime / 60F);
        int seconds = Mathf.FloorToInt(clearTime - minutes * 60);

        string time = string.Format("{0:00}:{1:00}", minutes, seconds);

        _dungeonInfoText.text = "";

        _dungeonInfoText.text = ($"{dungeonName}\n{stepId}단계 Clear!\n진행시간 {time}");

        _dungeonRewardPanel.SetActive(true);
    }

    public void UpdateDungeonFail(string dungeonName, int stepId, float clearTime)
    {
        int minutes = Mathf.FloorToInt(clearTime / 60F);
        int seconds = Mathf.FloorToInt(clearTime - minutes * 60);

        string time = string.Format("{0:00}:{1:00}", minutes, seconds);

        _dungeonFailText.text = "";

        _dungeonFailText.text = ($"{dungeonName}\n{stepId}단계 Fail\n진행시간 {time}");

        _dungeonFailPanel.SetActive(true);
    }

    private void HandleStateChanged(DungeonState state)
    {
        if (state == DungeonState.Prepare)
        {
            _isEnterDungeon = false; // 준비 중에는 타이머 OFF
        }

        if (state == DungeonState.Combat)
        {
            _isEnterDungeon = true; // 전투 시작시 시작
        }

        if (state == DungeonState.Clear)
        {
            _isEnterDungeon = false; // 타이머 멈춤
        }

        if (state == DungeonState.Exit)
        {
            _dungeonInfoPanel.SetActive(false);
            _isEnterDungeon = false;
        }
    }
}
