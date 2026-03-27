using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;

public class OverDriveMode : MonoBehaviour
{
    [Header("오버드라이브 관련")]
    [SerializeField] private Button button;
    [SerializeField] private float overdriveTime = 600;
    [SerializeField] private float maxGauge = 60000;

    private float gauge = 0;
    public float Gauge
    {
        get => gauge;
        set
        {
            if (IsModeOn && value > gauge) return;
            gauge = value;
            if (gauge >= maxGauge)
            {
                gauge = maxGauge;
                button.interactable = true;
            }
            OnOverdriveGaugeChanged.Invoke(gauge, maxGauge);
        }
    }

    public event Action<float, float> OnOverdriveGaugeChanged;
    public event Action OnClickOverdrive;

    [Header("비주얼")]
    [SerializeField] private PlayerVisual playerVisual;
    [SerializeField] private VisualEffect aura;
    [SerializeField] private AfterImageGenerator afterImage;

    private Coroutine overdriveRoutine;
    public bool IsModeOn { get; set; }

    public void Click_Overdrive()
    {
        if (IsModeOn) return;
        button.interactable = false;
        overdriveRoutine = StartCoroutine(OverdriveTimer(overdriveTime));
        OnClickOverdrive?.Invoke();
    }

    private IEnumerator OverdriveTimer(float duration)
    {
        IsModeOn = true;
        StatManager.Instance.RefreshStats();
        aura.enabled = true;
        afterImage.StartAfterImage();
        playerVisual.ApplyOverdriveVisual(); 

        float timer = 0f;
        float startGauge = Gauge;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            startGauge -= (maxGauge / duration) * Time.deltaTime;
            Gauge = Mathf.Max(0, startGauge);
            yield return null;
        }

        EndOverdrive();
    }

    private void EndOverdrive()
    {
        Gauge = 0;
        aura.enabled = false;
        afterImage.StopAfterImage();
        playerVisual.ApplyOriginVisual(); 
        IsModeOn = false;
        StatManager.Instance.RefreshStats();
        overdriveRoutine = null;
    }
}