using System.Collections;
using System.Numerics;
using UnityEngine;
using UnityEngine.UI;

public class PlayerExp : MonoBehaviour
{
    Slider _slider;

    private void Awake()
    {
        _slider = GetComponent<Slider>();
    }
    private IEnumerator Start()
    {
        yield return new WaitUntil(() => PlayerStats.Instance != null);
        PlayerStats.Instance.OnExpChanged += ChangeExpBar;

        StartCoroutine(UpdateDelay());
    }


    private void OnDestroy()
    {
        PlayerStats.Instance.OnExpChanged -= ChangeExpBar;
    }

    //TODO : 플레이어 스탯 초기화 함수 Awake로 변경 후 코루틴 제거
    IEnumerator UpdateDelay()
    {
        yield return null;
        _slider.value = 0f;
    }

    void ChangeExpBar(BigInteger currentExp, BigInteger maxExp)
    {
        _slider.value = (float)((double)currentExp / (double)maxExp);
    }
}
