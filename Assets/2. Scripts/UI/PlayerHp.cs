using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHp : MonoBehaviour
{
    Slider _slider;

    private void Awake()
    {
        _slider = GetComponent<Slider>();
    }

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => PlayerStats.Instance != null);
        PlayerStats.Instance.OnHpChanged += ChangeHpBar;

        StartCoroutine(UpdateDelay());
    }

    private void OnDestroy()
    {
        PlayerStats.Instance.OnHpChanged -= ChangeHpBar;
    }

    //TODO : 플레이어 스탯 초기화 함수 Awake로 변경 후 코루틴 제거
    IEnumerator UpdateDelay()
    {
        yield return null;
        _slider.value = PlayerStats.Instance.CurrentHp;
    }

    void ChangeHpBar(float currentHp, float maxHp)
    {
        _slider.value = (currentHp / maxHp);
    }
}
