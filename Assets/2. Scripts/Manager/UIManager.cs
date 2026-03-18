using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using TMPro;
using System.Numerics;

public class UIManager : MonoBehaviour
{
    private static UIManager instance;
    public static UIManager Instance => instance;

    [SerializeField] GameObject _dungeonRewardPanel;
    [SerializeField] TextMeshProUGUI _dungeonInfoText;
    [SerializeField] TextMeshProUGUI _dungeonRewardText;
    [SerializeField] Button _dungeonClear;
    [SerializeField] Button _nextDungeon;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    private void OnEnable()
    {
        DungeonManager.OnDungeonEQReward += UpdateEQRewardUI;
        DungeonManager.OnDungeonGSReward += UpdateGSRewardUI;
        DungeonManager.OnDungeonSKReward += UpdateSKRewardUI;
        DungeonManager.OnDungeonReward += UpdateRewardUI;
        DungeonManager.OnDungeonCleared += UpdateDungeonInfo;
    }

    private void OnDisable()
    {
        DungeonManager.OnDungeonEQReward -= UpdateEQRewardUI;
        DungeonManager.OnDungeonGSReward -= UpdateGSRewardUI;
        DungeonManager.OnDungeonSKReward -= UpdateSKRewardUI;
        DungeonManager.OnDungeonReward -= UpdateRewardUI;
        DungeonManager.OnDungeonCleared -= UpdateDungeonInfo;
    }
    private Stack<BaseUI> uiStack = new Stack<BaseUI>();

    private void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CloseTopPanel();
        }
    }


    public void OpenPanel(BaseUI baseUI)
    {
        if (baseUI == null) return;

        if (baseUI.IsOpen) return;

        baseUI.Open();
        uiStack.Push(baseUI);
        Debug.Log(uiStack.Count);
    }

    public void CloseTopPanel()
    {
        Debug.Log(uiStack.Count);
        if (uiStack.Count > 0)
        {
            var top = uiStack.Pop();
            top.Close();
        }
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

        foreach (var SK in list)
        {
            _dungeonRewardText.text += ($"{SK.Sk_Scl_Name}\n");
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
}