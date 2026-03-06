using UnityEngine;
using UnityEngine.UI;

public class PlayerOverDriveGauge : MonoBehaviour
{
    [SerializeField] private OverDriveMode odm;
    private Slider _slider;

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
        odm.OnOverdriveGaugeChanged += ChangeOverDriveBar;
    }

    private void OnDisable()
    {
        odm.OnOverdriveGaugeChanged -= ChangeOverDriveBar;
    }

    void ChangeOverDriveBar(float currentGauge, float maxGauge)
    {
        _slider.value = (currentGauge / maxGauge);
    }
}
