using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectStage : BaseUI
{
    [Header("References")]
    [SerializeField] private StageManager _stageManager;            // 스테이지매니저 연결
    [SerializeField] private Transform _stageGridRoot;              // 스테이지 버튼 담을 그리드
    [SerializeField] private StageButtonItem _stageButtonPrefab;    // 스테이지 버튼 프리팹
    [SerializeField] private Button _goToButton;                    // 바로가기 버튼
    [SerializeField] private TMP_Text _chapterTitleText;            // 챕터 제목
    [SerializeField] private TMP_Text _progressText;                // 진행도
    [SerializeField] private ScrollRect _scrollRect;

    private readonly List<StageButtonItem> _spawnedButtons = new();

    private int _selectedSection = -1;  // 현재 선택된 섹션 저장용
    private int _previewStageId;    // UI용 현재 선택 챕터

    //캐시용
    private readonly Dictionary<int, List<Stage_SectionData>> _sectionCache = new();
    private ChapterButtonItem[] _chapterButtons;

    //캐시 빌드 여부 플래그
    private bool _isCacheBuilt = false;

    private void OnEnable()
    {
        StageManager.OnStageIdChanged += HandleStageChanged;
        StageManager.OnStageCleared += HandleStageCleared;
        StageManager.OnSectionChanged += HandleSectionChanged;
    }

    private void OnDisable()
    {
        StageManager.OnStageIdChanged -= HandleStageChanged;
        StageManager.OnStageCleared -= HandleStageCleared;
        StageManager.OnSectionChanged -= HandleSectionChanged;
    }

    private void HandleStageChanged(int newStageId)
    {
        _previewStageId = newStageId;
        _selectedSection = -1;

        SyncChapterSelection();

        GenerateStageButtons();

        HighlightCurrentSection();
        StartCoroutine(CoScrollNextFrame());

        UpdateProgressText();
    }

    private void HandleStageCleared(int clearedStageId)
    {
        //현재 보고 있는 챕터면 갱신
        if (clearedStageId == _previewStageId)
        {
            GenerateStageButtons();
            UpdateProgressText();
        }
    }

    private void HandleSectionChanged(int newSection)
    {
        if (_previewStageId == _stageManager.CurrentStageId)
        {
            GenerateStageButtons();

            HighlightCurrentSection();
            StartCoroutine(CoScrollNextFrame());

            UpdateProgressText();
        }
    }

    protected override void OnOpen()
    {
        if (_stageManager == null)
        {
            Debug.LogError("[SelectStage] StageManager NULL");
            return;
        }

        if (_chapterButtons == null)
        {
            _chapterButtons = GetComponentsInChildren<ChapterButtonItem>(true);
        }

        //캐시는 최초 1회만 빌드
        if (!_isCacheBuilt)
            BuildSectionCache();

        Debug.Log($"현재 섹션: {_stageManager.CurrentSection}");

        _previewStageId = _stageManager.CurrentStageId; // 현재 진행 챕터 기준
        _selectedSection = -1; // 초기화

        SyncChapterSelection();
        SyncChapterHeaderWithCurrentStage();

        GenerateStageButtons();

        HighlightCurrentSection();
        StartCoroutine(CoScrollNextFrame());

        UpdateProgressText();

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

    #region Scroll 안정화 코루틴
    private IEnumerator CoScrollNextFrame()
    {
        yield return null; // UI Layout 완료 대기
        ScrollToCurrentSection();
    }
    #endregion

    #region 캐시
    /// <summary>
    /// Stage_SectionData를 Stage_Id 기준으로 캐싱
    /// LINQ Where / OrderBy / ToList 사용을 제거하기 위한 사전 작업
    /// </summary>
    private void BuildSectionCache()
    {
        _sectionCache.Clear();

        if (DataManager.Instance == null)
        {
            Debug.LogError("[SelectStage] DataManager 없음");
            return;
        }

        var dict = DataManager.Instance.GetDict<Stage_SectionData>();
        if (dict == null)
        {
            Debug.LogError("[SelectStage] Stage_SectionData Dict 없음");
            return;
        }

        foreach (var pair in dict)
        {
            Stage_SectionData data = pair.Value;
            if (data == null)
                continue;

            if (!_sectionCache.TryGetValue(data.Stage_Id, out var list))
            {
                list = new List<Stage_SectionData>();
                _sectionCache.Add(data.Stage_Id, list);
            }

            list.Add(data);
        }

        //Stage_Id별 Section_Start 오름차순 정렬
        foreach (var pair in _sectionCache)
        {
            pair.Value.Sort((a, b) => a.Section_Start.CompareTo(b.Section_Start));
        }

        _isCacheBuilt = true;
    }

    /// <summary>
    /// 캐시된 섹션 리스트 반환
    /// </summary>
    /// <param name="stageId"></param>
    /// <param name="sections"></param>
    /// <returns></returns>
    private bool TryGetSections(int stageId, out List<Stage_SectionData> sections)
    {
        return _sectionCache.TryGetValue(stageId, out sections) &&
               sections != null &&
               sections.Count > 0;
    }
    #endregion

    #region 스테이지
    private void GenerateStageButtons()
    {
        DeactivateAllButtons(); // 재사용

        if (_stageManager == null)
            return;

        int stageId = _previewStageId; // 우선은 preview로 사용 (추후 _stageManager.CurrentStageId로 바뀔수도)
        int currentSection = _stageManager.CurrentSection;

        //캐시 사용
        if (!TryGetSections(stageId, out var sections))
        {
            Debug.LogWarning($"[SelectStage] Stage_Id={stageId} 에 해당하는 Section 데이터 없음");
            return;
        }

        //전체 범위 계산
        int totalStart = sections[0].Section_Start;
        int totalEnd = sections[0].Section_End;

        //LINQ Min / Max 제거
        for (int i = 1; i < sections.Count; i++)
        {
            if (sections[i].Section_Start < totalStart)
                totalStart = sections[i].Section_Start;

            if (sections[i].Section_End > totalEnd)
                totalEnd = sections[i].Section_End;
        }

        //버튼 재사용 인덱스
        int buttonIndex = 0;

        for (int realSection = totalStart; realSection <= totalEnd; realSection++)
        {
            StageButtonItem btn;

            //기존 버튼 재사용
            if (buttonIndex < _spawnedButtons.Count)
            {
                btn = _spawnedButtons[buttonIndex];
            }
            else
            {
                //부족한 경우에만 새로 생성
                btn = Instantiate(_stageButtonPrefab, _stageGridRoot);
                _spawnedButtons.Add(btn);
            }

            btn.gameObject.SetActive(true);

            StageStateType stateType = GetStageState(realSection, currentSection);
            int displayNumber = realSection - totalStart + 1; // UI 표시용 번호 계산

            btn.Initialize(displayNumber, realSection, stateType, OnClickStageButton);

            buttonIndex++;
        }
    }

    private StageStateType GetStageState(int sectionNumber, int currentSection)
    {
        int maxCleared = _stageManager.MaxClearedSection;

        if (sectionNumber <= maxCleared)
        {
            if (sectionNumber == currentSection)
                return StageStateType.Current;

            return StageStateType.Cleared;
        }

        return StageStateType.Locked;
    }

    private void OnClickStageButton(int realSectionNumber)
    {
        _selectedSection = realSectionNumber;

        //모든 버튼 선택 해제
        for (int i = 0; i < _spawnedButtons.Count; i++)
        {
            if (_spawnedButtons[i].gameObject.activeSelf)
                _spawnedButtons[i].SetSelected(false);
        }

        //FirstOrDefault 제거 -> for문으로 직접 탐색
        StageButtonItem selectedBtn = null;

        for (int i = 0; i < _spawnedButtons.Count; i++)
        {
            StageButtonItem btn = _spawnedButtons[i];

            if (!btn.gameObject.activeSelf)
                continue;

            if (btn.RealSectionNumber == realSectionNumber)
            {
                selectedBtn = btn;
                break;
            }
        }

        if (selectedBtn != null)
            selectedBtn.SetSelected(true);

        if (_goToButton != null)
            _goToButton.interactable = true;

        UpdateProgressText();
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

    private void HighlightCurrentSection()
    {
        int currentSection = _stageManager.CurrentSection;

        for (int i = 0; i < _spawnedButtons.Count; i++)
        {
            var btn = _spawnedButtons[i];

            if (!btn.gameObject.activeSelf)
                continue;

            bool isCurrent = (btn.RealSectionNumber == currentSection);
            btn.SetSelected(isCurrent);
        }
    }

    private void ScrollToCurrentSection()
    {
        int currentSection = _stageManager.CurrentSection;

        for (int i = 0; i < _spawnedButtons.Count; i++)
        {
            var btn = _spawnedButtons[i];

            if (!btn.gameObject.activeSelf)
                continue;

            if (btn.RealSectionNumber == currentSection)
            {
                RectTransform content = _scrollRect.content;
                RectTransform target = btn.GetComponent<RectTransform>();

                Canvas.ForceUpdateCanvases();

                float contentHeight = content.rect.height;
                float viewportHeight = _scrollRect.viewport.rect.height;

                float targetPosY = Mathf.Abs(target.anchoredPosition.y);

                float normalized =
                    Mathf.Clamp01(targetPosY / (contentHeight - viewportHeight));

                _scrollRect.verticalNormalizedPosition = 1f - normalized;

                break;
            }
        }
    }
    #endregion

    #region 챕터
    //ChapterButtonItem에서 호출
    public void OnClickChapter(int stageId, string chapterNumber, string chapterName)
    {
        _previewStageId = stageId;

        SyncChapterSelection();

        UpdateChapterHeader($"{chapterNumber} : {chapterName}");

        GenerateStageButtons();

        HighlightCurrentSection();
        StartCoroutine(CoScrollNextFrame());

        UpdateProgressText();
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
        if (_chapterButtons == null)
            return;

        for (int i = 0; i < _chapterButtons.Length; i++)
        {
            var chapter = _chapterButtons[i];

            if (chapter.StageId == _previewStageId)
            {
                UpdateChapterHeader(
                    $"{chapter.ChapterNumberText.text} : {chapter.ChapterNameText.text}"
                );
                return;
            }
        }

        UpdateChapterHeader(null); // 못 찾으면 비워둠
    }

    private void SyncChapterSelection()
    {
        if (_chapterButtons == null)
            return;

        for (int i = 0; i < _chapterButtons.Length; i++)
        {
            bool isSelected = (_chapterButtons[i].StageId == _previewStageId);
            _chapterButtons[i].SetSelected(isSelected);
        }
    }
    #endregion

    private void UpdateProgressText()
    {
        if (_progressText == null || _stageManager == null)
            return;

        int stageId = _previewStageId;

        //캐시 사용
        if (!TryGetSections(stageId, out var sections))
        {
            _progressText.text = "";
            return;
        }

        int totalStart = sections[0].Section_Start;
        int totalEnd = sections[0].Section_End;

        for (int i = 1; i < sections.Count; i++)
        {
            if (sections[i].Section_Start < totalStart)
                totalStart = sections[i].Section_Start;

            if (sections[i].Section_End > totalEnd)
                totalEnd = sections[i].Section_End;
        }

        int totalCount = totalEnd - totalStart + 1;

        //선택한 섹션이 있으면 그걸 기준으로 표시
        int referenceSection = (_selectedSection > 0)
            ? _selectedSection
            : _stageManager.MaxClearedSection;

        //현재 진행 중인 챕터가 아닌 경우
        if (stageId != _stageManager.CurrentStageId)
        {
            _progressText.text = $"현재진행도: 0 / {totalCount}";
            return;
        }

        int currentIndex = Mathf.Clamp(
            referenceSection - totalStart + 1,
            1,
            totalCount
        );

        _progressText.text = $"현재진행도: {currentIndex} / {totalCount}";
    }

    /// <summary>
    /// 버튼 재사용
    /// 다음 GenerateStageButtons에서 필요한 만큼 재활성화해서 재사용
    /// </summary>
    private void DeactivateAllButtons()
    {
        for (int i = 0; i < _spawnedButtons.Count; i++)
        {
            if (_spawnedButtons[i] != null)
            {
                _spawnedButtons[i].SetSelected(false);
                _spawnedButtons[i].gameObject.SetActive(false);
            }
        }
    }
}