using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerLevelText : MonoBehaviour
{
    [SerializeField] PlayerStats _playerStats;
    TextMeshProUGUI _text;

    private void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
    }
    private void Start()
    {
        StartCoroutine(UpdateDelay());
    }

    private void OnEnable()
    {
        _playerStats.OnLevelChanged += ChangeLevel;
    }

    private void OnDisable()
    {
        _playerStats.OnLevelChanged -= ChangeLevel;
    }

    //TODO : 플레이어 스탯 초기화 함수 Awake로 변경 후 코루틴 제거
    IEnumerator UpdateDelay()
    {
        yield return null;
        _text.text = ($" Level {_playerStats.CurrentLevel}");
    }

    void ChangeLevel(int currentLevel)
    {
        _text.text = ($" Level {currentLevel}");
    }
}
