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
        StageManager.OnStageCleared += OnStageClear;
    }

    private void OnDisable()
    {
        _overdriveMode.OnClickOverdrive -= OnOverdrive;
        StageManager.OnStageCleared -= OnStageClear;
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

    private void OnStageClear(int stageId)
    {
        StartCoroutine(OnStageClearText(stageId));
    }

    IEnumerator OnStageClearText(int stageId)
    {
        //todo 스테이지 1-1이런식으로 출력되게해야함
        string stageNumber = _stageNumberText.text = "";
        _stageClearText.SetActive(true);
        yield return new WaitForSeconds(3f);
        _stageClearText.SetActive(false);
    }
}
