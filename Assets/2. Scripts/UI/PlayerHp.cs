using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHp : MonoBehaviour
{
    [SerializeField] PlayerStats _playerStats;
    Image _image;

    private void Awake()
    {
        _image = GetComponent<Image>();
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
        _image.fillAmount = _playerStats._currentHp;
    }

    void ChangeHpBar(float currentHp, float maxHp)
    {
        _image.fillAmount = (currentHp / maxHp);
    }
}
