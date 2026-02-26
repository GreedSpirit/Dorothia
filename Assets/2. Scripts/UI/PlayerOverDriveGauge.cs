using UnityEngine;
using UnityEngine.UI;

public class PlayerOverDriveGauge : MonoBehaviour
{
    [SerializeField] PlayerStats _playerStats;
    Slider _slider;

    private void Awake()
    {
        _slider = GetComponent<Slider>();
    }

    private void Start()
    {
        //TODO : 저장되어있던 오버드라이브게이지값으로 적용해야함
        _slider.value = 0f;
    }

    private void OnEnable()
    {
        //_playerStats.OnOverDriveGaugeChanged += ChangeOverDriveBar;
    }

    private void OnDisable()
    {
        //_playerStats.OnOverDriveGaugeChanged -= ChangeOverDriveBar;
    }

    void ChangeOverDriveBar(float currentGauge, float maxGauge)
    {
        _slider.value = (currentGauge / maxGauge);
    }
}
