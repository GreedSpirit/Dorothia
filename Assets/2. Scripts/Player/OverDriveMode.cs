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

    [Header("무기 오브젝트")]
    [SerializeField] private GameObject leftWeapon;
    [SerializeField] private GameObject rightWeapon;

    [Header("기본 상태 리소스 (Origin)")]
    [SerializeField] private Material originWeaponMat;
    [SerializeField] private Mesh originWeaponMesh;

    [Header("오버드라이브 리소스 (Overdrive)")]
    [SerializeField] private VisualEffect aura;
    [SerializeField] private AfterImageGenerator afterImage;
    [SerializeField] private Material overDriveWeaponMat;
    [SerializeField] private Mesh overDriveWeaponMesh;

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
        Debug.Log("오버드라이브 시작");
        IsModeOn = true;

        StatManager.Instance.RefreshStats();

        aura.enabled = true;
        afterImage.StartAfterImage();
        ApplyWeaponSettings(overDriveWeaponMesh, overDriveWeaponMat);

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
        ApplyWeaponSettings(originWeaponMesh, originWeaponMat);
        Debug.Log("오버드라이브 종료");

        IsModeOn = false;

        StatManager.Instance.RefreshStats();

        overdriveRoutine = null;
    }

    private void ApplyWeaponSettings(Mesh targetMesh, Material targetMat)
    {
        SetWeaponState(leftWeapon, targetMesh, targetMat);
        SetWeaponState(rightWeapon, targetMesh, targetMat);
    }

    private void SetWeaponState(GameObject weapon, Mesh mesh, Material mat)
    {
        if (weapon == null) return;

        if (weapon.TryGetComponent<MeshFilter>(out MeshFilter mf))
        {
            mf.mesh = mesh;
        }

        if (weapon.TryGetComponent<MeshRenderer>(out MeshRenderer mr))
        {
            mr.material = mat;
        }
    }
}