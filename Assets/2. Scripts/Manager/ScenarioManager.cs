using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScenarioManager : MonoBehaviour
{
    //대사창
    [SerializeField] private GameObject _dialoguePanel;
    //이름
    [SerializeField] private TextMeshProUGUI _nameText;
    //대사내용
    [SerializeField] private TextMeshProUGUI _dialogueText;
    //배경
    [SerializeField] private CanvasGroup _backGroundImage;
    //캐릭터스탠딩이미지
    [SerializeField] private Image _portraitImage;
    //CG
    [SerializeField] private Image _cgImage;

    //출력할 대사들을 담아둘 큐
    private Queue<Talk_TableData> _dialogueQueue = new Queue<Talk_TableData>();
    //메모리 해제용 변수
    private string _currentPortraitKey;
    private string _currentCgKey;

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
        //Debug.LogError($"대사호출 {sectionId} , {outputTime}");
        
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
        
        //페이드인 완료 콜백받으면 대사창활성화
        StartCoroutine(FadeInBackground(() =>
        {
            _dialoguePanel.SetActive(true);
            DisplayNextSentence(); 
        }));
        
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
        //이름이 없을경우도 처리
        _nameText.text = string.IsNullOrEmpty(currentData.name) ? "" : currentData.name;
        _dialogueText.text = currentData.line_desc;

        //TODO: 스탠딩이미지 변경로직 추가
        UpdatePortrait(currentData.portrait);

        UpdateCG(currentData.cg);
    }

    private void EndDialogue()
    {
        _dialoguePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    private IEnumerator FadeInBackground(Action onComplete)
    {
        _backGroundImage.alpha = 0f;
        _backGroundImage.gameObject.SetActive(true);

        float timer = 0f;
        //1.5초까지 알파값 늘리기 반복
        while (timer < 1.5f)
        {            
            timer += Time.unscaledDeltaTime;
            //0~1범위에서만
            _backGroundImage.alpha = Mathf.Clamp01(timer / 1.5f);
            yield return null;
        }

        _backGroundImage.alpha = 1f;
        onComplete?.Invoke();
    }


    private void UpdatePortrait(string portraitKey)
    {
        Debug.LogError(portraitKey);
        //이미 로드된 이미지가 있다면 메모리에서 해제
        if (!string.IsNullOrEmpty(_currentPortraitKey))
        {
            AddressableManager.Instance.ReleaseAsset(portraitKey);
            _currentPortraitKey = null;
        }

        //이미지없는경우는 숨기기
        if (string.IsNullOrEmpty(portraitKey))
        {
            _portraitImage.gameObject.SetActive(false);
            Debug.LogError(portraitKey);
            return;
        }

        //새이미지 저장
        _currentPortraitKey = portraitKey;

        //어드레서블 로드 (어드레서블 그룹에 있는 이름과 CSV의 portrait 이름같아야함)
        AddressableManager.Instance.LoadAsset<Sprite>(portraitKey, (sprite) =>
        {
            _portraitImage.gameObject.SetActive(true);
            _portraitImage.sprite = sprite;
        });

    }

    private void UpdateCG(string cgKey)
    {
        //이미 로드된 이미지가 있다면 메모리에서 해제
        if (!string.IsNullOrEmpty(_currentCgKey))
        {
            AddressableManager.Instance.ReleaseAsset(cgKey);
            _currentCgKey = null;
        }

        //이미지없는경우는 숨기기
        if (string.IsNullOrEmpty(cgKey))
        {
            _cgImage.gameObject.SetActive(false);
            return;
        }

        //어드레서블 로드 (어드레서블 그룹에 있는 이름과 CSV의 portrait 이름같아야함)
        AddressableManager.Instance.LoadAsset<Sprite>(cgKey, (sprite) =>
        {
            _cgImage.gameObject.SetActive(true);
            _cgImage.sprite = sprite;
        });

    }
}