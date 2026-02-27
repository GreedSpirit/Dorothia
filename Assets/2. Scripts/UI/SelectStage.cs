using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using TMPro;

public class SelectStage : BaseUI
{
    [Header("References")]
    [SerializeField] private StageManager _stageManager;            // 스테이지매니저 연결
    [SerializeField] private Transform _stageGridRoot;              // 스테이지 버튼 담을 그리드
    [SerializeField] private StageButtonItem _stageButtonPrefab;    // 스테이지 버튼 프리팹
    [SerializeField] private Button _goToButton;                    // 바로가기 버튼
    [SerializeField] private TMP_Text _chapterTitleText;            // 챕터 제목

    private readonly List<StageButtonItem> _spawnedButtons = new();

    private int _selectedSection = -1;  // 현재 선택된 섹션 저장용
    private int _previewStageId;    // UI용 현재 선택 챕터

    protected override void OnOpen()
    {
        Debug.Log($"현재 섹션: {_stageManager.CurrentSection}");

        _previewStageId = _stageManager.CurrentStageId; // 현재 진행 챕터 기준
        _selectedSection = -1; // 초기화

        SyncChapterHeaderWithCurrentStage();

        GenerateStageButtons();

        if (_goToButton != null)
        {
            _goToButton.onClick.RemoveAllListeners();
            _goToButton.onClick.AddListener(OnClickGoToStage);
            _goToButton.interactable = false; // 처음엔 비활성화
        }
    }

    protected override void OnClose()
    {
    }

    #region 스테이지
    private void GenerateStageButtons()
    {
        ClearButtons();

        if (_stageManager == null)
            return;

        int stageId = _previewStageId; // 우선은 preview로 사용 (추후 _stageManager.CurrentStageId로 바뀔수도)
        int currentSection = _stageManager.CurrentSection;

        //Stage_Id 기준으로 정확히 필터링
        var dict = DataManager.Instance.GetDict<Stage_SectionData>();

        if (dict == null)
        {
            Debug.LogError("Stage_SectionData Dict NULL");
            return;
        }

        List<Stage_SectionData> sections = dict
            .Values
            .Where(x => x.Stage_Id == stageId)
            .OrderBy(x => x.Section_Start)
            .ToList();

        if (sections.Count == 0)
            return;

        //전체 범위 계산
        int totalStart = sections.Min(x => x.Section_Start);
        int totalEnd = sections.Max(x => x.Section_End);

        //버튼 생성
        for (int realSection = totalStart; realSection <= totalEnd; realSection++)
        {
            var btn = Instantiate(_stageButtonPrefab, _stageGridRoot);

            StageStateType stateType = GetStageState(realSection, currentSection);

            int displayNumber = realSection - totalStart + 1; // UI표시용 번호 계산

            //표시번호 + 실제번호 둘 다 전달
            btn.Initialize(displayNumber, realSection, stateType, OnClickStageButton);

            _spawnedButtons.Add(btn);

            //Debug.Log($"Create display={displayNumber} real={realSection} state={stateType} current={currentSection}");
        }
    }

    private StageStateType GetStageState(int sectionNumber, int currentSection)
    {
        if (sectionNumber < currentSection)
            return StageStateType.Cleared;

        if (sectionNumber == currentSection)
            return StageStateType.Current;

        return StageStateType.Locked;
    }

    private void OnClickStageButton(int realSectionNumber)
    {
        _selectedSection = realSectionNumber;

        //모든 버튼 선택 해제
        foreach (var btn in _spawnedButtons)
        {
            btn.SetSelected(false);
        }

        //선택된 버튼만 강조
        var selectedBtn = _spawnedButtons
            .FirstOrDefault(b => b.RealSectionNumber == realSectionNumber);

        if (selectedBtn != null)
            selectedBtn.SetSelected(true);

        if (_goToButton != null)
            _goToButton.interactable = true;
    }

    //실제 이동은 여기서
    private void OnClickGoToStage()
    {
        if (_selectedSection <= 0)
            return;

        Debug.Log($"바로가기 이동: {_selectedSection}");
        
        //선택한 스테이지에서 시작
        _stageManager.StartStageFromSection(
            _previewStageId, _selectedSection //일단 preview 사용
        );

        Close();
    }
#endregion

    #region 챕터
    //ChapterButtonItem에서 호출
    public void OnClickChapter(int stageId, string chapterNumber, string chapterName)
    {
        _previewStageId = stageId;

        UpdateChapterHeader($"{chapterNumber} : {chapterName}");

        GenerateStageButtons();
    }

    //상단 텍스트 갱신
    private void UpdateChapterHeader(string title)
    {
        if (_chapterTitleText == null)
            return;

        if (string.IsNullOrEmpty(title))
        {
            _chapterTitleText.text = "";
            return;
        }

        _chapterTitleText.text = title;
    }

    private void SyncChapterHeaderWithCurrentStage()
    {
        var chapterButtons = GetComponentsInChildren<ChapterButtonItem>(true);

        foreach (var chapter in chapterButtons)
        {
            if (chapter.StageId == _previewStageId)
            {
                UpdateChapterHeader(
                    $"{chapter.ChapterNumberText.text} : {chapter.ChapterNameText.text}"
                );
                return;
            }
        }

        //못 찾으면 그냥 비워둠
        UpdateChapterHeader(null);
    }
#endregion

    private void ClearButtons()
    {
        foreach (var btn in _spawnedButtons)
        {
            if (btn != null)
                Destroy(btn.gameObject);
        }

        _spawnedButtons.Clear();
    }
}