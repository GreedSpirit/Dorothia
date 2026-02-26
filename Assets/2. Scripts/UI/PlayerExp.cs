using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerExp : MonoBehaviour
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
        _playerStats.OnExpChanged += ChangeExpBar;
    }

    private void OnDisable()
    {
        _playerStats.OnExpChanged -= ChangeExpBar;
    }

    //TODO : 플레이어 스탯 초기화 함수 Awake로 변경 후 코루틴 제거
    IEnumerator UpdateDelay()
    {
        yield return null;
        _image.fillAmount = 0f;
    }

    void ChangeExpBar(float currentExp, double maxExp)
    {
        _image.fillAmount = (float)(currentExp / maxExp);
    }
}
