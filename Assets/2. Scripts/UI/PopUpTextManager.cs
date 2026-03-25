using System.Collections;
using TMPro;
using UnityEngine;

public class PopUpTextManager : MonoBehaviour
{
    [SerializeField] OverDriveMode _overdriveMode;
    [SerializeField] GameObject _overdiveText;
    [SerializeField] GameObject _stageClearText;
    [SerializeField] TextMeshProUGUI _stageNumberText;

    private void OnEnable()
    {
        _overdriveMode.OnClickOverdrive += OnOverdrive;
        StageManager.OnSectionCleard += HandleSectionChanged;
    }

    private void OnDisable()
    {
        _overdriveMode.OnClickOverdrive -= OnOverdrive;
        StageManager.OnSectionCleard -= HandleSectionChanged;
    }
    private void OnOverdrive()
    {
        StartCoroutine(OnOverdriveText());
    }

    IEnumerator OnOverdriveText()
    {
        _overdiveText.SetActive(true);
        yield return new WaitForSeconds(3f);
        _overdiveText.SetActive(false);
    }

    private void HandleSectionChanged(int _)
    {
        StartCoroutine(UpdateText());
    }

    IEnumerator UpdateText()
    {
        int stageId = StageManager.Instance.CurrentStageId;
        int sectionId = StageManager.Instance.CurrentSection;

        int chapter = stageId % 1000;
        int section = sectionId % 1000;

        _stageNumberText.text = ($"{chapter}-{section} Clear!");

        _stageClearText.SetActive(true);
        yield return new WaitForSeconds(3f);
        _stageClearText.SetActive(false);
    }
}
