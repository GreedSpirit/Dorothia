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

    private void OnEnable()
    {
        DungeonManager.OnDungeonEQReward += UpdateEQRewardUI;
        DungeonManager.OnDungeonGSReward += UpdateGSRewardUI;
        DungeonManager.OnDungeonSKReward += UpdateSKRewardUI;
        DungeonManager.OnDungeonReward += UpdateRewardUI;
        DungeonManager.OnDungeonCleared += UpdateDungeonInfo;
        DungeonManager.OnDungeonFailed += UpdateDungeonFail;
    }

    private void OnDisable()
    {
        DungeonManager.OnDungeonEQReward -= UpdateEQRewardUI;
        DungeonManager.OnDungeonGSReward -= UpdateGSRewardUI;
        DungeonManager.OnDungeonSKReward -= UpdateSKRewardUI;
        DungeonManager.OnDungeonReward -= UpdateRewardUI;
        DungeonManager.OnDungeonCleared -= UpdateDungeonInfo;
        DungeonManager.OnDungeonFailed -= UpdateDungeonFail;
    }



    public void UpdateEQRewardUI(List<Equipment> list)
    {
        _dungeonRewardText.text = "";

        foreach (var EQ in list)
        {
            _dungeonRewardText.text += ($"{EQ.equip_name}\n");
        }
    }

    public void UpdateGSRewardUI(List<G_StoneData> list)
    {
        _dungeonRewardText.text = "";

        foreach (var GS in list)
        {
            _dungeonRewardText.text += ($"{GS.G_Stone_Name}\n");
        }
    }

    public void UpdateSKRewardUI(List<Sk_SclData> list)
    {
        _dungeonRewardText.text = "";

        //중복 체크 담아두기
        Dictionary<string, int> rewardCounts = new Dictionary<string, int>();

        foreach (var SK in list)
        {
            if (rewardCounts.ContainsKey(SK.Sk_Scl_Name))
            {
                // 이미 있는 이름이면 개수만 +1
                rewardCounts[SK.Sk_Scl_Name]++;
            }
            else
            {
                // 처음 나온 이름이면 사전에 추가
                rewardCounts[SK.Sk_Scl_Name] = 1;
            }
        }

        foreach (var item in rewardCounts)
        {
            //스킬주문서 x 중복수 출력
            _dungeonRewardText.text += $"{item.Key} x {item.Value}\n";
        }
    }

    public void UpdateRewardUI(BigInteger reward)
    {
        _dungeonRewardText.text = "";

        _dungeonRewardText.text = ($"{reward}");
    }

    public void UpdateDungeonInfo(string dungeonName, int stepId, float clearTime)
    {
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
}
