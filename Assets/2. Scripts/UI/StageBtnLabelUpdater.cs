using TMPro;
using UnityEngine;

public class StageBtnLabelUpdater : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;

    private void OnEnable()
    {
        StageManager.OnStageIdChanged += HandleStageChanged;
        StageManager.OnSectionChanged += HandleSectionChanged;

        UpdateText();
    }

    private void OnDisable()
    {
        StageManager.OnStageIdChanged -= HandleStageChanged;
        StageManager.OnSectionChanged -= HandleSectionChanged;
    }

    private void HandleStageChanged(int _)
    {
        UpdateText();
    }

    private void HandleSectionChanged(int _)
    {
        UpdateText();
    }

    private void UpdateText()
    {
        if (StageManager.Instance == null)
        {
            Debug.Log("[StageBtnLabelUpdater] StageManager 없음");
            return;
        }

        int stageId = StageManager.Instance.CurrentStageId;
        int sectionId = StageManager.Instance.CurrentSection;

        int chapter = stageId % 1000;
        int section = sectionId % 1000;

        _text.text = $"스테이지 변경 \n {chapter} - {section}";
    }
}
