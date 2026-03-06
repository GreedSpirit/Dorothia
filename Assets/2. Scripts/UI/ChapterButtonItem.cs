using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ChapterButtonItem : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text _chapterNumberText; // 챕터 1
    [SerializeField] private TMP_Text _chapterNameText;   // 론도니아
    [SerializeField] private Button _button;

    [Header("Data")]
    [SerializeField] private int _stageId;   // 이 챕터에 해당하는 Stage_Id

    private SelectStage _selectStage;

    public int StageId => _stageId;
    public TMP_Text ChapterNumberText => _chapterNumberText;
    public TMP_Text ChapterNameText => _chapterNameText;

    private void Awake()
    {
        //SelectStage 자동 탐색
        if (_selectStage == null)
            _selectStage = GetComponentInParent<SelectStage>();

        if (_button != null)
        {
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(OnClick);
        }
    }

    private void OnClick()
    {
        if (_selectStage == null)
            return;

        _selectStage.OnClickChapter(
            _stageId,
            _chapterNumberText.text,
            _chapterNameText.text
        );
    }
}