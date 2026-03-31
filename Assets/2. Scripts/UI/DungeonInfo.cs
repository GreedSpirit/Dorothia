using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DungeonInfo : MonoBehaviour
{
    [SerializeField] DungeonLevelSelect[] _levelButtons;
    [SerializeField] DungeonSelect _dungeonSelect;
    [SerializeField] private GameObject _cantClearCountPanel;
    [SerializeField] private GameObject _cantClearPanel;
    [SerializeField] private GameObject _clearPanel;


    [SerializeField] TextMeshProUGUI _dungeonName;
    [SerializeField] TextMeshProUGUI _dungeonPower;
    [SerializeField] TextMeshProUGUI _dungeonTime;
    [SerializeField] Image _dungeonImage;
    //[SerializeField] TextMeshProUGUI _dungeonMessage;
    [SerializeField] TextMeshProUGUI _clearCount;

    //이미지 들고있는 SO
    [SerializeField] DungeonUIData _dungeonUIData;

    //단계버튼 리스트
    Dictionary<int, List<Dungeon_StepData>> _dungeonStep;


    private int _currentStepIndex = 0;
    private int _currentDungeonId = 0;
    int _maxCount = 3;

    private void Init()
    {
        //딕셔너리 초기화
        _dungeonStep = new Dictionary<int, List<Dungeon_StepData>>();

        //던전스탭 데이터 불러오기
        var stepList = DataManager.Instance.GetDict<Dungeon_StepData>();

        //순회돌면서 값 step에 값들 넣기
        foreach (var step in stepList.Values)
        {
            //id없으면 리스트생성
            if (!_dungeonStep.TryGetValue(step.Dungeon_Id, out var list))
            {
                //150001에 해당하는 값들 리스트로 묶음
                list = new List<Dungeon_StepData>();
                _dungeonStep.Add(step.Dungeon_Id, list);
            }
            //단계값 추가
            list.Add(step);
        }
    }

    //패널 열릴때 호출
    public void Open(int dungeonId, int startLevel = 0)
    {       

        //패널 활성화하고
        gameObject.SetActive(true);
        _currentDungeonId = dungeonId;
        _currentStepIndex = startLevel;

        Init();



        //던전id에 해당하는 단계수
        int stepCount = _dungeonStep.TryGetValue(dungeonId, out var steps) ? steps.Count : 0;

        //버튼수만큼 순회돌고
        for (int i = 0; i < _levelButtons.Length; i++)
        {
            //단계수이하이면 참
            bool hasStep = (i < stepCount);
            _levelButtons[i].gameObject.SetActive(hasStep);

            if (hasStep)
            {
                _levelButtons[i]._dungeonId = dungeonId;
                _levelButtons[i]._dungeonLevel = i;
            }
        }

        //초기 버튼들 알파값 셋팅
        DungeonLevelClick(dungeonId, startLevel);
        //클리어카운트
        int used = DataManager.Instance.GetUsedEntryCount(_currentDungeonId);
        _clearCount.text = $"클리어 횟수 {used}/{_maxCount}";
    }

    public void DungeonLevelClick(int dungeonId, int level)
    {
        _currentStepIndex = level;
        _currentDungeonId = dungeonId;

        //던전이름 정보 가져오고
        var dungeonData = DataManager.Instance.GetData<DungeonData>(dungeonId);

        //버튼들 순회돌면서
        for (int i = 0; i < _levelButtons.Length; i++)
        {
            //버튼들 캔버스그룹가져오고
            CanvasGroup cg = _levelButtons[i].GetComponent<CanvasGroup>();

            //선택된 버튼 외 알파값 줄이기
            cg.alpha = (_levelButtons[i]._dungeonLevel == level) ? 1f : 0.5f;
        }

        //던전단계데이터에 키가있으면 steps에 값 넘기기
        if (!_dungeonStep.TryGetValue(dungeonId, out var steps)) return;

            
        if (level < 0 || level >= steps.Count) return;

        Dungeon_StepData stepData = steps[level];
        

        //SO이미지 가져오기
        _dungeonImage.sprite = _dungeonUIData.GetSprite(dungeonId);

        //테이블값으로 UI갱신
        _dungeonName.text = dungeonData.Dungeon_Name;   
        _dungeonPower.text = stepData.Rec_Cp;
        _dungeonTime.text = stepData.Time_Limit;

        //TODO 메세지 테이블값으로 바꿔야함
        //_dungeonMessage.text = level.ToString();      
    }


    //소탕 버튼 클릭 로직
    public void OnDungeonClearBtcClick()
    {
        int count = DataManager.Instance.GetUsedEntryCount(_currentDungeonId);
        //일일 입장 횟수 체크
        if (count > 3)
        {
            _cantClearCountPanel.SetActive(true);
            return;
        }

        //한번도 클리어안함
        if (count < 1)
        {
            _cantClearPanel.SetActive(true);
        }
        else
        {
            _clearPanel.SetActive(true);
        }
    }

    //실제 소탕 실행
    public void DungeonClear()
    {
        int count = DataManager.Instance.GetUsedEntryCount(_currentDungeonId);
        Debug.Log($"클리어횟수 = {count}");
        if (count >= 3)
        {
            _cantClearCountPanel.SetActive(true);
            return;
        }
        if (DataManager.Instance.TryConsumeEntry(_currentDungeonId, _maxCount, out int used))
        {            
            _clearCount.text = $"클리어 횟수 {used}/{_maxCount}";
            Debug.Log($"[소탕 성공] 오늘 {_currentDungeonId} 사용 횟수: {used}");
        }
        _clearPanel.SetActive(false);
    }

    // 입장 버튼 클릭
    public void OnDungeonEnterBtnClick()
    {
        if (!DataManager.Instance.CanEnterDungeon(_currentDungeonId, _maxCount))
        {
            _cantClearCountPanel.SetActive(true);
            return;
        }

        if (_dungeonStep.TryGetValue(_currentDungeonId, out var steps))
        {
            if (_currentStepIndex >= 0 && _currentStepIndex < steps.Count)
            {
                // 리스트에서 현재 선택된 인덱스의 실제 고유 ID를 가져옴
                int StepId = steps[_currentStepIndex].Dungeon_Step_Id;

                Debug.Log($"[Dungeon] 입장 시도: {_currentDungeonId} / 실제 단계ID: {StepId}");
                DungeonManager.Instance.StartDungeon(_currentDungeonId, StepId);

                Invoke(nameof(ClosePanel), 0.5f);
                return;
            }
        }
    }

    private void ClosePanel()
    {
        gameObject.SetActive(false);
        _dungeonSelect.Close();
    }
}
