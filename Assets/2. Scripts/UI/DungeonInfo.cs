using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DungeonInfo : MonoBehaviour
{
    [SerializeField] DungeonLevelSelect[] _levelButtons;

    [SerializeField] TextMeshProUGUI _dungeonName;
    [SerializeField] TextMeshProUGUI _dungeonPower;
    [SerializeField] TextMeshProUGUI _dungeonTime;
    [SerializeField] Image _dungeonImage;
    [SerializeField] TextMeshProUGUI _dungeonMessage;

    //이미지 들고있는 SO
    [SerializeField] DungeonUIData _dungeonUIData;

    //단계버튼 리스트
    Dictionary<int, List<Dungeon_StepData>> _dungeonStep;

    

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
        Debug.LogError($"[DungeonInfo] 지금 작동 중인 객체: {gameObject.name}", gameObject);

        //패널 활성화하고
        gameObject.SetActive(true);

        Init();
        Debug.LogError(dungeonId);
        Debug.LogError(startLevel);
        Debug.LogError(_dungeonStep == null ? "_dungeonStep이 null" : "_dungeonStep 정상");
        Debug.LogError(_levelButtons == null ? "_levelButtons가 null" : "_levelButtons 정상");



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
    }

    public void DungeonLevelClick(int dungeonId, int level)
    {
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

            //1단계는 0이니깐 
            int index = level;
        if (index < 0 || index >= steps.Count) return;

        Dungeon_StepData stepData = steps[level];
        

        //SO이미지 가져오기
        _dungeonImage.sprite = _dungeonUIData.GetSprite(dungeonId);

        //테이블값으로 UI갱신
        _dungeonName.text = dungeonData.Dungeon_Name;   
        _dungeonPower.text = stepData.Rec_Cp;
        _dungeonTime.text = stepData.Time_Limit;

        //TODO 메세지 테이블값으로 바꿔야함
        _dungeonMessage.text = level.ToString();      
    }
}