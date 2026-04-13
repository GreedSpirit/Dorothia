using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerLevelText : MonoBehaviour
{
    TextMeshProUGUI _text;

    private void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
    }
    private IEnumerator Start()
    {
        yield return new WaitUntil(() => PlayerStats.Instance != null);
        PlayerStats.Instance.OnLevelChanged += ChangeLevel;

        StartCoroutine(UpdateDelay());
    }

    private void OnDestroy()
    {
        PlayerStats.Instance.OnLevelChanged -= ChangeLevel;
    }

    //TODO : 플레이어 스탯 초기화 함수 Awake로 변경 후 코루틴 제거
    IEnumerator UpdateDelay()
    {
        yield return null;
        _text.text = ($" Level {PlayerStats.Instance.CurrentLevel}");
    }

    void ChangeLevel(int currentLevel)
    {
        _text.text = ($" Level {currentLevel}");
    }
}
