using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class ScenarioManager : MonoBehaviour
{
    //대사창
    [SerializeField] private GameObject _dialoguePanel;
    //이름
    [SerializeField] private TextMeshProUGUI _nameText;
    //대사내용
    [SerializeField] private TextMeshProUGUI _dialogueText;

    //출력할 대사들을 담아둘 큐
    private Queue<Talk_TableData> _dialogueQueue = new Queue<Talk_TableData>();

    private void OnEnable()
    {
        // StageManager의 이벤트 구독
        StageManager.OnScenarioTrigger += HandleDialogueTrigger;
    }

    private void OnDisable()
    {
        StageManager.OnScenarioTrigger -= HandleDialogueTrigger;
    }

    private void HandleDialogueTrigger(int sectionId, int outputTime)
    {
        Debug.LogError($"대사호출 {sectionId} , {outputTime}");
        
        //전체대사 가져오기
        var dict = DataManager.Instance.GetDict<Talk_TableData>();

        //조건필터링용 링큐
        var targetDialogues = dict.Values
            //섹션아이디와 같으면서 출력시점이 같은걸
            .Where(x => x.Section_id == sectionId && x.output_time == outputTime)
            //아이디순서대로 정렬
            .OrderBy(x => x.id)
            //리스트로 전환
            .ToList();

        //큐에 필터된대사들 넣어주기
        foreach (var dialogue in targetDialogues)
        {
            _dialogueQueue.Enqueue(dialogue);
        }

        //대사출력
        StartDialogue();
    }

    private void StartDialogue()
    {
        Time.timeScale = 0f;
        _dialoguePanel.SetActive(true);
        DisplayNextSentence();
    }

    //버튼클릭함수 (대사패널을 버튼으로 화면덮음)
    public void DisplayNextSentence()
    {
        //큐에 남은 대사가 없다면 대사 종료 처리
        if (_dialogueQueue.Count == 0)
        {
            EndDialogue();
            return;
        }

        //다음대사 꺼내기
        Talk_TableData currentData = _dialogueQueue.Dequeue();

        //대사 적용
        _nameText.text = currentData.name;
        _dialogueText.text = currentData.line_desc;

        //TODO: 스탠딩이미지 변경로직 추가
    }

    private void EndDialogue()
    {
        _dialoguePanel.SetActive(false);
        Time.timeScale = 1f;
    }
}