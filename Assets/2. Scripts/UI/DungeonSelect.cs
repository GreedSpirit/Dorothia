using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DungeonSelect : BaseUI
{
    [SerializeField] GameObject[] _dungeons;
    [SerializeField] GameObject[] _lockDungeons;
    [SerializeField] TextMeshProUGUI[] _clearMessage;
    [SerializeField] int[] _dungeonsLevel;
    [SerializeField] int[] _targetDungeonIds;

    [SerializeField] Button[] _selectBtns;

    int _maxCurrent = 3;

    protected override void OnOpen()
    {
        //플레이어 현재 레벨 값 가져오기
        UpdateLevel(PlayerStats.Instance.CurrentLevel);
    }

    protected override void OnClose()
    {
        
    }

    private void OnEnable()
    {
        PlayerStats.Instance.OnLevelChanged += UpdateLevel;
    }

    private void OnDisable()
    {
        PlayerStats.Instance.OnLevelChanged -= UpdateLevel;
    }

    void UpdateLevel(int currentLevel)
    {
        //던전갯수만큼 반복
        for (int i = 0; i < _dungeons.Length; i++)
        {
            //플레이어 레벨이랑 던전별 레벨 조건 체크하고
            bool isUnlocked = currentLevel >= _dungeonsLevel[i];

            //레벨조건이 만족하면 각오브젝트들 활성/비활성
            _dungeons[i].SetActive(isUnlocked);
            _lockDungeons[i].SetActive(!isUnlocked);

            // DataManager 통해 조회
            int dungeonId = _targetDungeonIds[i];
            int used = DataManager.Instance.GetUsedEntryCount(dungeonId);
            _clearMessage[i].text = $"클리어 횟수 {used}/{_maxCurrent}";

        }
    }

    public void OnAllBtns()
    {
        foreach (var btn in _selectBtns)
        {
            btn.interactable = true;
        }
    }

    public void OffAllBtns()
    {
        foreach (var btn in _selectBtns)
        {
            btn.interactable = false;
        }
    }
}
