using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHp : MonoBehaviour
{
    [SerializeField] PlayerStats _playerStats;
    Slider _slider;

    private void Awake()
    {
        _slider = GetComponent<Slider>();
    }

    private void Start()
    {
        StartCoroutine(UpdateDelay());
    }

    private void OnEnable()
    {
        _playerStats.OnHpChanged += ChangeHpBar;
    }

    private void OnDisable()
    {
        _playerStats.OnHpChanged -= ChangeHpBar;
    }

    //TODO : 플레이어 스탯 초기화 함수 Awake로 변경 후 코루틴 제거
    IEnumerator UpdateDelay()
    {
        yield return null;
        _slider.value = _playerStats._currentHp;
    }

    void ChangeHpBar(float currentHp, float maxHp)
    {
        _slider.value = (currentHp / maxHp);
    }
}
