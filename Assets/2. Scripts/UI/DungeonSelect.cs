using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DungeonSelect : BaseUI
{
    [SerializeField] PlayerStats _playerStats;

    [SerializeField] GameObject[] _dungeons;
    [SerializeField] GameObject[] _lockDungeons;
    //[SerializeField] TextMeshProUGUI[] _levelTexts;
    [SerializeField] TextMeshProUGUI[] _clearMessage;
    [SerializeField] int[] _dungeonsLevel;



    int _currentLevel;

    private void OnEnable()
    {
        _playerStats.OnLevelChanged += UpdateLevel;
    }

    private void OnDisable()
    {
        _playerStats.OnLevelChanged -= UpdateLevel;
    }

    protected override void OnOpen()
    {
        //플레이어 현재 레벨 값 가져오기
        UpdateLevel(_playerStats._level);
    }

    protected override void OnClose()
    {
        
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
            //_levelTexts[i].enabled = !isUnlocked;

            //TODO : 던전클리어횟수변수 받아서 해당 값으로 텍스트입력
            //던전매니저에도 클리어횟수를 배열로 저장해두고
            //int clearCount = 던전매니저카운트[i];
            _clearMessage[i].text = ($"클리어 횟수 3/3");

        }
    }
}
